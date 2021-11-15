using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Config
{
    public class ConstExcelConfig : ExcelConfig, IExcelGenerateCode, IExcelGenerateAsset
    {
        public override string ExcelName { get; }

        private readonly List<ExcelConfigItem> _items;

        public ConstExcelConfig(string excelName, List<ExcelConfigItem> items)
        {
            ExcelName = excelName;
            _items    = items;

            CombineList();
            DealEnum();
            DealLocalize();
            DealComment();
        }

        public void CreateAsset()
        {
            //CreateScriptObject();
            CreateJson();
        }

        public string CheckAsset()
        {
            string clsName = ExcelName + "ConstConfig";
            Type   clsType = assembly.GetType(clsName);
            if (clsType == null)
            {
                return $"{clsName} : 未生成C#";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{clsName} :");

            List<FieldInfo> fields = new List<FieldInfo>(clsType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            foreach (FieldInfo fieldInfo in fields)
            {
                //找到table中是否有此项
                var item = _items.Find((configItem => configItem.DataName == fieldInfo.Name));
                if (item == null)
                {
                    sb.AppendLine($"Excel表中缺少{fieldInfo.Name}");
                    continue;
                }

                if (item.DataType != TypeToString(fieldInfo.FieldType))
                    sb.AppendLine($"Excel表中{fieldInfo.Name} 类型为：{item.DataType}  C#中为{fieldInfo.FieldType}");
            }

            foreach (ExcelConfigItem item in _items)
            {
                FieldInfo fieldInfo = fields.Find((info => TypeToString(info.FieldType) == item.DataType));
                if (fieldInfo == null)
                {
                    sb.AppendLine($"Excel表有多余的项{item.DataName}");
                }
            }

            return sb.ToString();
        }

        private void CreateScriptObject()
        {
            int dataCount = _items[0].Contents.Count;

            if (dataCount == 0)
            {
                throw new ExcelReaderException($"常数表{ExcelName}中未包含任何数据");
            }

            if (dataCount > 1)
            {
                Debug.LogWarning($"常数表{ExcelName}中数据超过一行，仅第一行有效");
            }

            //---------------------------------------------反射Config类
            string className = $"{ExcelName}ConstConfig";
            Type   classType = assembly.GetType(className);

            if (classType == null) throw new ExcelReaderException($"{ExcelName}未生成代码");

            //---------------------------------------------创建Config的ScriptObject
            ScriptableObject asset = ScriptableObject.CreateInstance(classType);

            //---------------------------------------------遍历field，赋值
            FieldInfo[] fields = classType.GetFields();
            for (int j = 0; j < fields.Length; j++)
            {
                FieldInfo field      = fields[j];
                int       fieldIndex = _items.FindIndex(e => e.DataName == field.Name);
                if (fieldIndex < 0) continue;

                ExcelConfigItem excelConfigItem = _items[fieldIndex];

                string fieldValue = excelConfigItem.Contents[0];
                Type   fieldType  = StringToType(excelConfigItem.DataType);
                object value      = null;
                value = ParseValue(fieldValue, fieldType);
                field.SetValue(asset, value);
            }

            //---------------------------------------------更新修改时间
            ExcelAssetUpdateTime.UpdateExcelTime(ExcelName + "ConstConfig");

            //---------------------------------------------保存
            string assetPath = $"{ExcelPathDefine.CONST_ASSET_FOLDER}/{ExcelName}ConstConfig.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        private void CreateJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            ExcelConfigItem idItem = null;
            foreach (ExcelConfigItem item in _items)
            {
                if (item.IsID)
                {
                    idItem = item;
                }
            }

            if (idItem == null)
                throw new ExcelReaderException($"{ExcelName}表未找到ID项");

            if (idItem.Contents.Count == 0)
            {
                throw new ExcelReaderException($"常数表{ExcelName}中未包含任何数据");
            }

            if (idItem.Contents.Count > 1)
            {
                Debug.LogWarning($"常数表{ExcelName}中数据超过一行，仅第一行有效");
            }

            for (int j = 0; j < _items.Count; j++)
            {
                ExcelConfigItem item = _items[j];

                if (item == idItem) continue;

                sb.Append($"\t\t\"{item.DataName}\" : {ParseValueToJson(item.DataType, item.Contents[0])}");

                if (j == _items.Count - 1)
                    sb.AppendLine();
                else
                    sb.AppendLine(",");
            }

            sb.AppendLine("}");

            using (StreamWriter sw = new StreamWriter(
                new FileStream($"{Directory.GetCurrentDirectory()}/{ExcelPathDefine.CONST_ASSET_FOLDER}/{ExcelName}ConstConfig.json", FileMode.Create)))
            {
                sw.Write(sb.ToString());
            }

            //-----------更新修改时间
            ExcelAssetUpdateTime.UpdateExcelTime(ExcelName + "ConstConfig");

            AssetDatabase.Refresh();
        }

        public void GenerateCSharpCode()
        {
            string folderPath  = Path.Combine(Application.dataPath, ExcelPathDefine.CONST_CSHARP_FOLDER);
            string filePath    = Path.Combine(folderPath, ExcelName + "ConstConfig.cs");
            string codeContent = CodeContent();

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (StreamWriter sw =
                new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.GetEncoding("utf-8")))
            {
                sw.Write(codeContent);
            }

            string CodeContent()
            {
                StringBuilder strBuilder = new StringBuilder();

                strBuilder
                   .AppendLine("using System;")
                   .AppendLine("using System.Collections.Generic;")
                   .AppendLine("using UnityEngine;")
                   .AppendLine("using RestoryStudio.Config;")
                   .AppendLine()
                   .AppendFormat("public class {0}ConstConfig : ConfigBase_Excel\r\n", ExcelName)
                   .AppendLine("{");

                // 忽略第一列ID列
                for (int i = 1; i < _items.Count; i++)
                {
                    strBuilder.AppendFormat("    /// <summary>\r\n");
                    string[] desc = _items[i].DataComment.Split('\n');
                    for (int x = 0; x < desc.Length; x++)
                    {
                        //--生成注释
                        strBuilder.AppendFormat("    /// {0}\r\n", desc[x]);
                    }

                    strBuilder.AppendFormat("    /// </summary>\r\n");
                    strBuilder.AppendFormat("    public {0} {1};\r\n", _items[i].DataType, _items[i].DataName);
                }

                strBuilder.AppendLine("}");

                return strBuilder.ToString();
            }
        }

        public void GenerateLuaCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"---@class X{ExcelName}ConstConfig");
            foreach (ExcelConfigItem item in _items)
            {
                if (item.IsID) continue;
                sb.AppendLine(item.ToLuaComment());
            }

            FileInfo fileInfo = new FileInfo(ExcelPathDefine.CONFIG_LUA_PATH);

            string content = "";
            using (var sr = new StreamReader(fileInfo.OpenRead()))
            {
                content = sr.ReadToEnd();
            }

            string regexStr = $@"(?<=--start X{ExcelName}ConstConfig)[\s\S]*?(?=--end)";
            var    match    = Regex.Match(content, regexStr);
            if (match.Success)
            {
                sb.Insert(0, "\n");
                content = Regex.Replace(content, regexStr, sb.ToString());
            }
            else
            {
                sb.Insert(0, $"--start X{ExcelName}ConstConfig\n");
                sb.AppendLine("--end");
                content = content + "\n" + sb;
            }

            using (var sw = new StreamWriter(fileInfo.OpenWrite()))
            {
                sw.Write(content);
            }
        }

        //处理本地化
        private void DealLocalize()
        {
            int itemCount = _items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                if (_items[i].DealLocalize(out ExcelConfigItem localizeItem))
                {
                    _items.Add(localizeItem);
                }
            }
        }

        //处理枚举
        private void DealEnum()
        {
            foreach (ExcelConfigItem item in _items)
            {
                item.DealEnum();
            }
        }

        // _1 , _2 形式的配置合并为 List
        private void CombineList()
        {
            List<ExcelConfigItem> needClearItems = new List<ExcelConfigItem>();

            foreach (ExcelConfigItem item in _items)
            {
                if (item.IsCombineList)
                {
                    //需要合并
                    bool exist = false;
                    foreach (ExcelConfigItem configItem in _items)
                    {
                        if (configItem.DataName == item.CombineListName)
                        {
                            //合并到这一项
                            configItem.CombineList(item);
                            exist = true;
                            break;
                        }
                    }

                    if (exist == false)
                    {
                        //自己作为第一项
                        item.ChangeIntoCombineList();
                        needClearItems.Add(item);
                    }
                }
            }

            //删除残余的项
            _items.RemoveAll((item => item.IsCombineList));

            //清理后面合并的空字符串
            foreach (ExcelConfigItem needClearItem in needClearItems)
            {
                needClearItem.ClearAfterCombine();
            }
        }

        //处理注释
        private void DealComment()
        {
            _items.RemoveAll((item => item.DealComment()));
        }
    }
}