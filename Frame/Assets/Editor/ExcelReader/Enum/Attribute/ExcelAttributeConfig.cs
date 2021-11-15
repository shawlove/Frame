using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace GameFrame.Config
{
    public class ExcelAttributeConfig : ExcelConfig, IExcelGenerateCode
    {
        public override string ExcelName { get; }

        private readonly List<ExcelAttributeItem> _items;

        private static string[] _roundPower10Double =
        {
            "1.0", "10.0", "100.0", "1000.0", "10000.0", "100000.0", "1000000.0", "10000000.0", "100000000.0", "1000000000.0", "10000000000.0", "100000000000.0",
            "1000000000000.0", "10000000000000.0", "100000000000000.0", "1E+15"
        };

        public ExcelAttributeConfig(string excelName, List<ExcelAttributeItem> items)
        {
            ExcelName = excelName;
            _items    = items;
        }

        public void GenerateCSharpCode()
        {
            //----------------------------------------枚举部分
            StringBuilder sb = new StringBuilder();

            // sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public enum E{ExcelName}");
            sb.AppendLine("{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine("\t/// <summary>");
                sb.AppendLine($"\t/// {item.attributeChinese}");
                sb.AppendLine("\t/// </summary>");
                sb.AppendLine($"\t{item.attributeEn} = {item.attributeID},");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            //----------------------------------------AttributeExt拓展方法
            sb.AppendLine($"public static class {ExcelName}Ext");
            sb.AppendLine("{");

            //--------------------------------------------------------------------------------数值范围
            sb.AppendLine($"\tprivate static Dictionary<E{ExcelName},double[]> _valueRangeDic = new Dictionary<E{ExcelName}, double[]>");
            sb.AppendLine("\t{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t\t{{E{ExcelName}.{item.attributeEn},new double[]{{{item.minValue},{item.maxValue}}}}},");
            }

            sb.AppendLine("\t};");
            sb.AppendLine();

            //--------------------------------------------------------------------------------获取最小值
            sb.AppendLine($"\tpublic static double GetMinValue(this E{ExcelName} type)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\treturn _valueRangeDic[type][0];");
            sb.AppendLine("\t}");
            sb.AppendLine();

            //--------------------------------------------------------------------------------获取最大值
            sb.AppendLine($"\tpublic static double GetMaxValue(this E{ExcelName} type)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\treturn _valueRangeDic[type][1];");
            sb.AppendLine("\t}");
            sb.AppendLine();

            //--------------------------------------------------------------------------------字符串转枚举
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// 从表中所填的属性枚举字符串转为对应的枚举");
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine($"\tpublic static E{ExcelName} To{ExcelName}(this string str)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tswitch(str)");
            sb.AppendLine("\t\t{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t\t\tcase \"{item.attributeEn}\": return E{ExcelName}.{item.attributeEn};");
            }

            sb.AppendLine($"\t\t\tdefault:Debug.LogError(\"错误的属性枚举！\");return default(E{ExcelName});");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine();

            //--------------------------------------------------------------------------------中文名称
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// 获取属性枚举的中文名称");
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine($"\tpublic static string GetChineseName(this E{ExcelName} type)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tswitch(type)");
            sb.AppendLine("\t\t{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t\t\tcase E{ExcelName}.{item.attributeEn}: return \"{item.attributeChinese}\";");
            }

            sb.AppendLine("\t\t\tdefault: return \"无法获取中文名称\";");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine();

            //--------------------------------------------------------------------------------获取显示文本
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// 获取枚举数值文本（添加后缀）");
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine($"\tpublic static string GetValueText(this double value, E{ExcelName} type)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tdouble displayValue;");
            sb.AppendLine("\t\tswitch (type)");
            sb.AppendLine("\t\t{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t\t\tcase E{ExcelName}.{item.attributeEn}:");
                if (item.suffix == "")
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\");");
                }
                else if (item.suffix == "%")
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point + 2]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\") + \"%\";");
                }
                else
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\") + \"{item.suffix}\";");
                }
            }

            sb.AppendLine("\t\t\tdefault:");
            sb.AppendLine("\t\t\t\tDebug.LogError(\"获取枚举数值文本（添加后缀）失败\");");
            sb.AppendLine("\t\t\t\treturn \"\";");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine();

            //--------------------------------------------------------------------------------获取显示文本out计算后的值
            sb.AppendLine("\t/// <summary>");
            sb.AppendLine("\t/// 获取枚举数值文本（添加后缀）并返回计算后的显示值");
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine($"\tpublic static string GetValueText(this double value, E{ExcelName} type, out double displayValue)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tdisplayValue = 0;");
            sb.AppendLine("\t\tswitch (type)");
            sb.AppendLine("\t\t{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t\t\tcase E{ExcelName}.{item.attributeEn}:");
                if (item.suffix == "")
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\");");
                }
                else if (item.suffix == "%")
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point + 2]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\") + \"%\";");
                }
                else
                {
                    sb.AppendLine($"\t\t\t\tdisplayValue = ((int) (value * {_roundPower10Double[item.point]})) / {_roundPower10Double[item.point]};");
                    sb.AppendLine($"\t\t\t\treturn displayValue.ToString(\"N{item.point}\") + \"{item.suffix}\";");
                }
            }

            sb.AppendLine("\t\t\tdefault:");
            sb.AppendLine("\t\t\t\tDebug.LogError(\"获取枚举数值文本（添加后缀）失败\");");
            sb.AppendLine("\t\t\t\treturn \"\";");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("}");

            using (var sw = new StreamWriter(new FileStream($"{ExcelPathDefine.ENUM_CSHARP_FOLDER}/{ExcelName}Config.cs", FileMode.Create)))
            {
                sw.Write(sb.ToString());
            }

            AssetDatabase.Refresh();
        }

        public void GenerateLuaCode()
        {
            //----------------------------------------枚举部分
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"---@class E{ExcelName}");
            sb.AppendLine($"E{ExcelName} = {{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t---{item.attributeChinese}");
                sb.AppendLine($"\t{item.attributeEn} = {item.attributeID},");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            //----------------------------------------中文名称表
            sb.AppendLine("---@private");
            sb.AppendLine($"E{ExcelName}._chineseList = {{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t[{item.attributeID}] = \"{item.attributeChinese}\",");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            //----------------------------------------数值范围表
            sb.AppendLine("---@private");
            sb.AppendLine($"E{ExcelName}._rangeList = {{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t[{item.attributeID}] = {{ min = {item.minValue}, max = {item.maxValue} }},");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            //----------------------------------------获取数值文本及显示数值方法表
            sb.AppendLine("---@private");
            sb.AppendLine($"E{ExcelName}._getValueTextFunList = {{");
            foreach (ExcelAttributeItem item in _items)
            {
                sb.AppendLine($"\t[{item.attributeID}] = function(value)");
                if (item.suffix == "")
                {
                    sb.AppendLine($"\t\tlocal displayValue = math.floor(value * {_roundPower10Double[item.point]}) / {_roundPower10Double[item.point]}");
                    sb.AppendLine($"\t\treturn displayValue, displayValue");
                }
                else if (item.suffix == "%")
                {
                    sb.AppendLine($"\t\tlocal displayValue = math.floor(value * {_roundPower10Double[item.point + 2]}) / {_roundPower10Double[item.point]}");
                    sb.AppendLine($"\t\treturn displayValue .. \"%\", displayValue");
                }
                else
                {
                    sb.AppendLine($"\t\tlocal displayValue = math.floor(value * {_roundPower10Double[item.point]}) / {_roundPower10Double[item.point]}");
                    sb.AppendLine($"\t\treturn displayValue .. \"{item.suffix}\", displayValue");
                }

                sb.AppendLine("\tend,");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            sb.AppendLine();

            //----------------------------------------获取中文
            sb.AppendLine("---@return string");
            sb.AppendLine("---@param type int 枚举值");
            sb.AppendLine($"function E{ExcelName}:GetChinese(type)");
            sb.AppendLine("\treturn self._chineseList[type]");
            sb.AppendLine("end");
            sb.AppendLine();

            //----------------------------------------获取最大值
            sb.AppendLine("---@return double");
            sb.AppendLine("---@param type int 枚举值");
            sb.AppendLine($"function E{ExcelName}:GetMaxValue(type)");
            sb.AppendLine("\treturn self._rangeList[type].max");
            sb.AppendLine("end");
            sb.AppendLine();

            //----------------------------------------获取最小值
            sb.AppendLine("---@return double");
            sb.AppendLine("---@param type int 枚举值");
            sb.AppendLine($"function E{ExcelName}:GetMinValue(type)");
            sb.AppendLine("\treturn self._rangeList[type].min");
            sb.AppendLine("end");
            sb.AppendLine();

            //----------------------------------------获取数值文本及显示数值
            sb.AppendLine("---@return string|double");
            sb.AppendLine("---@param type int 枚举值");
            sb.AppendLine("---@param value double 属性值");
            sb.AppendLine($"function E{ExcelName}:GetValueText(type, value)");
            sb.AppendLine("\treturn self._getValueTextFunList[type](value)");
            sb.AppendLine("end");

            string path = $"{ExcelPathDefine.ENUM_LUA_FOLDER}/{ExcelName}Config.lua.txt";
            if (File.Exists(path) == false)
            {
                using (new FileStream($"{ExcelPathDefine.ENUM_LUA_FOLDER}/{ExcelName}Config.lua.txt", FileMode.CreateNew))
                {
                }
            }

            using (var sw = new StreamWriter(new FileStream($"{ExcelPathDefine.ENUM_LUA_FOLDER}/{ExcelName}Config.lua.txt", FileMode.Truncate)))
            {
                sw.Write(sb.ToString());
            }

            AssetDatabase.Refresh();
        }
    }
}