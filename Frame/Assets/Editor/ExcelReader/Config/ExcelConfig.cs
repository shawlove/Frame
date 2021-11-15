using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using UnityEngine;

namespace GameFrame.Config
{
    /// <summary>
    /// 一类配置表
    /// </summary>
    public abstract class ExcelConfig
    {
        public const char   CODE_SPLIT_LIST       = '|';
        public const char   EXCEL_SPLIT_LIST      = ',';
        public const string LOCALIZED_TEXT_PREFIX = "LTextID_";

        protected static readonly Assembly assembly = typeof(IConfig).Assembly;

        protected static readonly CSharpCodeProvider provider = new CSharpCodeProvider();

        public abstract string ExcelName { get; }

        /// <summary>
        /// string转对应类型
        /// </summary>
        /// <returns></returns>
        protected static object ParseValue(string content, Type type)
        {
            if (type.IsGenericType)
            {
                object list = Activator.CreateInstance(type);

                if (string.IsNullOrEmpty(content) == false)
                {
                    string[]   arrayValue    = content.Split(CODE_SPLIT_LIST);
                    MethodInfo listAddMethod = type.GetMethod("Add");
                    Type       basicType     = type.GetGenericArguments()[0];

                    for (int i = 0; i < arrayValue.Length; i++)
                    {
                        listAddMethod.Invoke(list, new object[] {ParseBasicValue(arrayValue[i], basicType)});
                    }
                }

                return list;
            }
            else
            {
                return ParseBasicValue(content, type);
            }

            object ParseBasicValue(string value, Type basicType)
            {
                try
                {
                    if (basicType == typeof(bool))
                    {
                        return bool.Parse(value);
                    }
                    else if (basicType == typeof(int))
                    {
                        return int.Parse(value);
                    }
                    else if (basicType == typeof(uint))
                    {
                        return uint.Parse(value);
                    }
                    else if (basicType == typeof(float))
                    {
                        return float.Parse(value);
                    }
                    else if (basicType == typeof(double))
                    {
                        return double.Parse(value);
                    }
                    else if (basicType == typeof(ulong))
                    {
                        return ulong.Parse(value);
                    }
                    else if (basicType == typeof(string))
                    {
                        return value;
                    }
                    else
                    {
                        //处理枚举
                        if (int.TryParse(value, out int enumValue))
                        {
                            if (Enum.IsDefined(basicType, enumValue))
                            {
                                return Enum.ToObject(basicType, enumValue);
                            }
                            else
                            {
                                throw new ExcelReaderException($"{basicType} 没有定义 {value}");
                            }
                        }

                        try
                        {
                            return Enum.Parse(basicType, value);
                        }
                        catch (ExcelReaderException e)
                        {
                            throw new ExcelReaderException($"{value} 无法转换为枚举 {basicType}   格式为EnumType_XX");
                        }
                    }
                }
                catch
                {
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        Debug.LogError($"值转换异常 类型={basicType.FullName} 值={value}");
                    }

                    //手动处理一下转换异常  相当于不填的默认值
                    if (basicType == typeof(string))
                    {
                        return value;
                    }
                    else
                    {
                        return default;
                    }
                }
            }
        }

        /// <summary>
        /// 字符串转Type
        /// </summary>
        protected static Type StringToType(string dataType)
        {
            if (dataType.StartsWith("List<") && dataType.EndsWith(">"))
            {
                dataType = dataType.Substring(5);
                dataType = dataType.Remove(dataType.Length - 1);
                Type type     = ParseBasicType(dataType);
                Type listType = typeof(List<>);
                return listType.MakeGenericType(type);
            }
            else
            {
                return ParseBasicType(dataType);
            }

            Type ParseBasicType(string str)
            {
                switch (str)
                {
                    case "int":
                        return typeof(int);
                    case "uint":
                        return typeof(uint);
                    case "float":
                        return typeof(float);
                    case "string":
                        return typeof(string);
                    case "bool":
                        return typeof(bool);
                    case "double":
                        return typeof(double);
                    case "ulong":
                        return typeof(ulong);
                    default:
                        if ((str.EndsWith("Type") || str.EndsWith("Attribute")) && str.StartsWith("E"))
                        {
                            //枚举
                            try
                            {
                                Type type = assembly.GetType(str);

                                // Debug.Log(type.Name);
                                if (type == null || !type.IsEnum)
                                {
                                    throw new ExcelReaderException($"{str}枚举不存在！，请先生成枚举");
                                }

                                return type;
                            }
                            catch (ExcelReaderException e)
                            {
                                throw new ExcelReaderException($"{str}枚举不存在！，请先生成枚举");
                            }
                        }

                        throw new ExcelReaderException($"{str}类型不符合要求！只支持int、uint、float、double、string、bool、ulong、ExxType、ExxAttribute格式的枚举");
                }
            }
        }

        /// <summary>
        /// Json相关处理
        /// </summary>
        protected static string ParseValueToJson(string dataType, string content)
        {
            if (dataType.StartsWith("List<") && dataType.EndsWith(">"))
            {
                string basicType = dataType.Replace("List<", "");
                basicType = basicType.Replace(">", "");

                string result = content;

                if (string.IsNullOrEmpty(content) == false)
                {
                    string[] arrayValue = content.Split(CODE_SPLIT_LIST);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < arrayValue.Length; i++)
                    {
                        sb.Append($"{ParseValueToJson(basicType, arrayValue[i])},");
                    }

                    result = sb.ToString().TrimEnd(',');
                }

                return $"[{result}]";
            }
            else
            {
                return ParseBasicValue(dataType, content);
            }

            string ParseBasicValue(string typeStr, string str)
            {
                string result = str;

                switch (typeStr)
                {
                    case "int":
                    case "uint":
                    case "float":
                    case "double":
                    case "ulong":
                        if (str == "")
                            result = "0";
                        break;
                    case "string":
                        result = Regex.Replace(result, @"\\(?!(n|f|b|t|r))", "\\\\");
                        result = Regex.Replace(result, "\"", "\\\"");
                        result = $"\"{result}\"";
                        break;
                    case "bool":
                        result = str == "" ? "false" : result.ToLower();
                        break;
                    default:
                        if ((typeStr.EndsWith("Type") || typeStr.EndsWith("Attribute")) && typeStr.StartsWith("E"))
                        {
                            //枚举
                            try
                            {
                                Type type = assembly.GetType(typeStr);

                                // Debug.Log(type.Name);
                                if (type == null || !type.IsEnum)
                                {
                                    throw new ExcelReaderException($"{typeStr}枚举不存在！，请先生成枚举");
                                }

                                //lua中所有枚举处理成int  xx==XXENUM.XXX 形式来判断
                                if (int.TryParse(str, out int enumValue))
                                {
                                    if (Enum.IsDefined(type, enumValue))
                                    {
                                        return enumValue.ToString();
                                    }
                                    else
                                    {
                                        throw new ExcelReaderException($"{type} 没有定义 {str}");
                                    }
                                }

                                try
                                {
                                    if (str == "") str = "None";
                                    object enumObj     = Enum.Parse(type, str);
                                    if (type.IsEnumDefined(enumObj))
                                    {
                                        return enumObj.GetHashCode().ToString();
                                    }
                                }
                                catch (ExcelReaderException e)
                                {
                                    Debug.LogError(e);
                                    throw new ExcelReaderException($"{str} 无法转换为枚举 {type}   格式为EnumType_XX");
                                }
                            }
                            catch (ExcelReaderException e)
                            {
                                Debug.LogError(e);
                                throw new ExcelReaderException($"{typeStr}枚举不存在！，请先生成枚举");
                            }
                        }
                        else
                        {
                            throw new ExcelReaderException($"{typeStr}类型不符合要求！只支持int、uint、float、double、string、bool、ulong、ExxType、ExxAttribute格式的枚举");
                        }

                        break;
                }

                return result;
            }
        }

        /// <summary>
        /// Type转string 入Int32 -》 int
        /// </summary>
        protected static string TypeToString(Type type)
        {
            string typeStr;
            Type   basicType;
            if (type.IsGenericType)
            {
                basicType = type.GenericTypeArguments[0];
                typeStr   = "List<{0}>";
            }
            else
            {
                basicType = type;
                typeStr   = "{0}";
            }

            if (basicType == typeof(bool))
            {
                typeStr = string.Format(typeStr, "bool");
            }
            else if (basicType == typeof(int))
            {
                typeStr = string.Format(typeStr, "int");
            }
            else if (basicType == typeof(uint))
            {
                typeStr = string.Format(typeStr, "uint");
            }
            else if (basicType == typeof(float))
            {
                typeStr = string.Format(typeStr, "float");
            }
            else if (basicType == typeof(double))
            {
                typeStr = string.Format(typeStr, "double");
            }
            else if (basicType == typeof(ulong))
            {
                typeStr = string.Format(typeStr, "ulong");
            }
            else if (basicType == typeof(string))
            {
                typeStr = string.Format(typeStr, "string");
            }
            else
            {
                Debug.LogError(basicType.Name);
                typeStr = string.Format(typeStr, basicType.Name);
            }

            return typeStr;
        }
    }
}