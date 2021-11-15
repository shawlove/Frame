using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace GameFrame.Config
{
    public class ExcelEnumsConfig : ExcelConfig, IExcelGenerateCode
    {
        public override string ExcelName { get; }

        private readonly List<ExcelEnum> _enums;

        public ExcelEnumsConfig(string excelName, List<ExcelEnum> enums)
        {
            ExcelName = excelName;
            _enums    = enums;
        }

        public void GenerateCSharpCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");

            // //----------------------------------------枚举部分
            foreach (ExcelEnum excelEnum in _enums)
            {
                sb.AppendLine($"public enum E{excelEnum.enumName}");
                sb.AppendLine("{");
                sb.AppendLine("\tNone = 0,");
                foreach (ExcelEnumItem item in excelEnum.items)
                {
                    if (string.IsNullOrEmpty(item.enumComment) == false || string.IsNullOrEmpty(item.enumChinese) == false)
                    {
                        sb.AppendLine("\t/// <summary>");
                        sb.AppendLine($"\t///{item.enumChinese} {item.enumComment}");
                        sb.AppendLine("\t/// </summary>");
                    }

                    sb.AppendLine($"\t{item.enumContent} = {item.enumValue},");
                }

                sb.AppendLine("}");

                sb.AppendLine();
            }

            //----------------------------------------拓展方法
            sb.AppendLine($"public static class {ExcelName}Parser");
            sb.AppendLine("{");
            foreach (ExcelEnum excelEnum in _enums)
            {
                //-----------------------------------------------------------获取中文
                if (excelEnum.hasChinese)
                {
                    sb.AppendLine($"\tpublic static string ToChinese(this E{excelEnum.enumName} enumType)");
                    sb.AppendLine("\t{");
                    sb.AppendLine("\t\tswitch((int)enumType)");
                    sb.AppendLine("\t\t{");
                    foreach (ExcelEnumItem item in excelEnum.items)
                    {
                        sb.AppendLine($"\t\t\tcase {item.enumValue}: return \"{item.enumChinese}\";");
                    }

                    sb.AppendLine($"\t\t\tdefault: return \"无法转换枚举E{excelEnum.enumName}！\";");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine("\t}");
                    sb.AppendLine();
                }

                //-----------------------------------------------------------string转枚举
                sb.AppendLine($"\tpublic static E{excelEnum.enumName} ParseToE{excelEnum.enumName}(this string rEnumValueString)");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tswitch (rEnumValueString)");
                sb.AppendLine("\t\t{");
                foreach (ExcelEnumItem item in excelEnum.items)
                {
                    sb.AppendLine($"\t\t\tcase \"{item.enumName}_{item.enumContent}\": return E{item.enumName}.{item.enumContent};");
                }

                sb.AppendLine("\t\t\tdefault:");
                sb.AppendLine("\t\t\t\tDebug.LogError($\"无法转化{rEnumValueString}为" + "E" + excelEnum.enumName + "。\");");
                sb.AppendLine($"\t\t\t\treturn E{excelEnum.enumName}.None;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            using (var sw = new StreamWriter(new FileStream($"{ExcelPathDefine.ENUM_CSHARP_FOLDER}/{ExcelName}Config.cs", FileMode.Create)))
            {
                sw.Write(sb.ToString());
            }

            AssetDatabase.Refresh();
        }

        public void GenerateLuaCode()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ExcelEnum excelEnum in _enums)
            {
                sb.AppendLine($"---START E{excelEnum.enumName}");

                //----------------------------------------枚举部分
                sb.AppendLine($"---@class E{excelEnum.enumName}");
                sb.AppendLine($"E{excelEnum.enumName} = {{");
                sb.AppendLine($"\tNone = 0,");
                foreach (ExcelEnumItem item in excelEnum.items)
                {
                    if (string.IsNullOrEmpty(item.enumComment) == false || string.IsNullOrEmpty(item.enumChinese) == false)
                    {
                        sb.AppendLine($"\t---{item.enumChinese}  {item.enumComment}");
                    }

                    sb.AppendLine($"\t{item.enumContent} = {item.enumValue},");
                }

                sb.AppendLine("}");

                if (excelEnum.hasChinese)
                {
                    sb.AppendLine();

                    //----------------------------------------中文表
                    sb.AppendLine("---@private");
                    sb.AppendLine($"E{excelEnum.enumName}._chineseList = {{");
                    foreach (ExcelEnumItem item in excelEnum.items)
                    {
                        sb.AppendLine($"\t[{item.enumValue}] = \"{item.enumChinese}\",");
                    }

                    sb.AppendLine("}");
                    sb.AppendLine();

                    //----------------------------------------获取中文
                    sb.AppendLine("---@return string");
                    sb.AppendLine("---@param type int 枚举值");
                    sb.AppendLine($"function E{excelEnum.enumName}:GetChinese(type)");
                    sb.AppendLine("\treturn self._chineseList[type]");
                    sb.AppendLine("end");
                }

                sb.AppendLine($"---END");
                sb.AppendLine();
                sb.AppendLine();
            }

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