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
    public class DataExcelConfig : ExcelConfig, IExcelCombineConfig, IExcelGenerateCode, IExcelGenerateAsset
    {
        public override string ExcelName { get; }

        private List<ExcelConfigItem> _items;

        public DataExcelConfig(string excelName, List<ExcelConfigItem> items)
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
            string clsName = ExcelName + "DataConfigItem";
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
            //---------------------------------------------检查重复ID
            ExcelConfigItem idConfigItem = _items[0];
            if (idConfigItem.DataName != "ID" && idConfigItem.DataName != "id")
            {
                throw new ExcelReaderException($"{ExcelName} 第一列不是ID");
            }

            HashSet<string> check = new HashSet<string>();
            for (int i = 0; i < idConfigItem.Contents.Count; i++)
            {
                if (check.Add(idConfigItem.Contents[i]) == false)
                {
                    throw new ExcelReaderException($"{ExcelName} 有重复ID：{idConfigItem.Contents[i]}");
                }
            }

            //---------------------------------------------反射Config类
            string className = $"{ExcelName}DataConfig";
            Type   classType = assembly.GetType(className);

            if (classType == null) throw new ExcelReaderException($"{ExcelName}表未生成代码");

            MethodInfo addMethod = classType.GetMethod("AddItem");

            //---------------------------------------------创建Config的ScriptObject
            ScriptableObject asset = ScriptableObject.CreateInstance(classType);

            //---------------------------------------------反射ConfigItem类
            string itemName = $"{ExcelName}DataConfigItem";
            Type   itemType = assembly.GetType(itemName);

            //---------------------------------------------创建赋值ConfigItem并添加到Config
            int dataCount = _items[0].Contents.Count;
            for (int i = 0; i < dataCount; i++)
            {
                object      item   = Activator.CreateInstance(itemType);
                FieldInfo[] fields = itemType.GetFields();

                //---------------------------------------------遍历field，赋值
                for (int j = 0; j < fields.Length; j++)
                {
                    FieldInfo field      = fields[j];
                    int       fieldIndex = _items.FindIndex(e => e.DataName == field.Name);
                    if (fieldIndex < 0) continue;

                    ExcelConfigItem excelConfigItem = _items[fieldIndex];

                    object value = null;

                    Type fieldType = StringToType(excelConfigItem.DataType);

                    value = ParseValue(excelConfigItem.Contents[i], fieldType);

                    field.SetValue(item, value);
                }

                addMethod.Invoke(asset, new object[] {item});
            }

            //---------------------------------------------更新修改时间
            ExcelAssetUpdateTime.UpdateExcelTime(ExcelName + "DataConfig");

            //---------------------------------------------保存
            string assetPath = $"{ExcelPathDefine.DATA_ASSET_FOLDER}/{ExcelName}DataConfig.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        private void CreateJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"DataList\": [");

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

            for (int i = 0; i < idItem.Contents.Count; i++)
            {
                sb.AppendLine("    {");
                for (int j = 0; j < _items.Count; j++)
                {
                    ExcelConfigItem item = _items[j];

                    sb.Append($"      \"{item.DataName}\" : {ParseValueToJson(item.DataType, item.Contents[i])}");

                    if (j == _items.Count - 1)
                        sb.AppendLine();
                    else
                        sb.AppendLine(",");
                }

                sb.Append("    }");

                if (i == idItem.Contents.Count - 1)
                    sb.AppendLine();
                else
                    sb.AppendLine(",");
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            using (StreamWriter sw = new StreamWriter(
                new FileStream($"{Directory.GetCurrentDirectory()}/{ExcelPathDefine.DATA_ASSET_FOLDER}/{ExcelName}DataConfig.json", FileMode.Create)))
            {
                sw.Write(sb.ToString());
            }

            //-----------更新修改时间
            ExcelAssetUpdateTime.UpdateExcelTime(ExcelName + "DataConfig");

            AssetDatabase.Refresh();
        }

        public void GenerateCSharpCode()
        {
            string folderPath = Path.Combine(Application.dataPath, ExcelPathDefine.DATA_CSHARP_FOLDER);
            string filePath   = Path.Combine(folderPath, ExcelName + "DataConfig.cs");

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            List<string> customPart = new List<string>(2) {"#region Custom\n\t#endregion", "#region Custom\n\t#endregion"};

            using (StreamReader sr =
                new StreamReader(new FileStream(filePath, FileMode.OpenOrCreate), Encoding.GetEncoding("utf-8")))
            {
                string content = sr.ReadToEnd();
                var    custom  = Regex.Matches(content, @"#region Custom[\s\S]*?#endregion");
                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i].Success)
                    {
                        customPart[i] = custom[i].Value;
                    }
                }
            }

            using (StreamWriter sw =
                new StreamWriter(new FileStream(filePath, FileMode.Truncate), Encoding.GetEncoding("utf-8")))
            {
                sw.Write(CodeContent());
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
                   .AppendLine("[System.Serializable]")
                   .AppendFormat("public class {0}DataConfigItem\r\n", ExcelName)
                   .AppendLine("{");
                for (int i = 0; i < _items.Count; i++)
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

                strBuilder.Append("\t");
                strBuilder.AppendLine(customPart[0]);

                strBuilder.AppendLine("}");

                strBuilder.AppendFormat("public class {0}DataConfig : ConfigBase_Excel\r\n", ExcelName)
                          .AppendLine("{");

                strBuilder.AppendLine("    [SerializeField]")
                          .AppendFormat("    public List<{0}DataConfigItem> DataList = new List<{0}DataConfigItem>();\r\n", ExcelName);

                strBuilder.AppendFormat(
                    "    private Dictionary<int,{0}DataConfigItem> DataDictionary = new Dictionary<int,{1}DataConfigItem>();\r\n",
                    ExcelName, ExcelName);

                strBuilder.AppendLine();

                strBuilder.AppendFormat("    public void AddItem({0}DataConfigItem item)\r\n", ExcelName)
                          .AppendLine("    {")
                          .AppendLine("        DataList.Add(item);")
                          .AppendLine("    }");

                strBuilder.AppendLine();

                strBuilder.AppendLine("    public override void Initialize()")
                          .AppendLine("    {")
                          .AppendLine("        for (int i = 0; i < DataList.Count; i++)")
                          .AppendLine("        {")
                          .AppendLine("            DataDictionary.Add(DataList[i].ID, DataList[i]);")
                          .AppendLine("        }")
                          .AppendLine("    }");

                strBuilder.AppendLine();

                /*strBuilder
                    .AppendLine("    /// <summary>")
                    .AppendLine("    /// 返回配置ID对应的元素，若不存在返回null")
                    .AppendLine("    /// </summary>")
                    .AppendFormat("    public {0}DataConfigItem GetValueByID(string ID)\r\n", _configName)
                    .AppendLine("    {")
                    .AppendFormat("        {0}DataConfigItem result;\r\n", _configName)
                    .AppendLine("        if (DataDictionary.TryGetValue(ID, out result) == false)")
                    .AppendLine("        {")
                    .AppendFormat("            Debug.LogError(\"{0}表中没有对应ID:\" + ID);\r\n", _configName)
                    .AppendLine("        }")
                    .AppendLine("        return result;")
                    .AppendLine("    }");
    
                strBuilder.AppendLine();*/

                strBuilder
                   .AppendLine("    /// <summary>")
                   .AppendLine("    /// 是否拥有对应ID的配置")
                   .AppendLine("    /// </summary>")
                   .AppendLine("    public bool ContainsID(int ID)")
                   .AppendLine("    {")
                   .AppendLine("        return DataDictionary.ContainsKey(ID);")
                   .AppendLine("    }");

                strBuilder.AppendLine();

                strBuilder
                   .AppendLine("    /// <summary>")
                   .AppendLine("    /// 返回是否存在配置ID对应的元素，若存在，则out该数据")
                   .AppendLine("    /// </summary>")
                   .AppendFormat("    public bool TryGetValueByID(int ID, out {0}DataConfigItem item)\r\n", ExcelName)
                   .AppendLine("    {")
                   .AppendLine("        if (DataDictionary.TryGetValue(ID, out item))")
                   .AppendLine("        {")
                   .AppendLine("            return true;")
                   .AppendLine("        }")
                   .AppendLine($"        Debug.LogError(\"{ExcelName}表中没有对应ID:\" + ID);")
                   .AppendLine("        return false;")
                   .AppendLine("    }");

                strBuilder.Append("\t");
                strBuilder.AppendLine(customPart[1]);

                strBuilder.AppendLine("}");

                return strBuilder.ToString();
            }
        }

        public void GenerateLuaCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"---@class X{ExcelName}DataConfig");
            sb.AppendLine($"---@field public GetItem fun(id : int):X{ExcelName}DataConfigItem");
            sb.AppendLine($"---@class X{ExcelName}DataConfigItem");
            foreach (ExcelConfigItem item in _items)
            {
                sb.AppendLine(item.ToLuaComment());
            }

            FileInfo fileInfo = new FileInfo(ExcelPathDefine.CONFIG_LUA_PATH);

            string content = "";
            using (var sr = new StreamReader(fileInfo.OpenRead()))
            {
                content = sr.ReadToEnd();
            }

            string regexStr = $@"(?<=--start X{ExcelName}DataConfig)[\s\S]*?(?=--end)";
            var    match    = Regex.Match(content, regexStr);
            if (match.Success)
            {
                sb.Insert(0, "\n");
                content = Regex.Replace(content, regexStr, sb.ToString());
            }
            else
            {
                sb.Insert(0, $"--start X{ExcelName}DataConfig\n");
                sb.AppendLine("--end");
                content = content + "\n" + sb;
            }

            using (var sw = new StreamWriter(fileInfo.OpenWrite()))
            {
                sw.Write(content);
            }
        }

        public void Combine(IExcelCombineConfig config)
        {
            if (config is DataExcelConfig another)
            {
                if (another.ExcelName != ExcelName)
                    throw new ExcelReaderException($"{ExcelName}表尝试和{another.ExcelName}表合并，失败");

                List<ExcelConfigItem> otherItems = another._items;

                //item是否一一对应，顺序没关系
                if (otherItems.Count != _items.Count) throw new ExcelReaderException($"{ExcelName}表合并失败，内部不一致");

                foreach (ExcelConfigItem item in _items)
                {
                    bool exist = false;
                    foreach (ExcelConfigItem otherItem in otherItems)
                    {
                        if (otherItem == item)
                        {
                            item.AddDataConfigItem(otherItem);
                            exist = true;
                            break;
                        }
                    }

                    if (exist == false) throw new ExcelReaderException($"{ExcelName}表合并失败，内部不一致");
                }
            }
            else
            {
                throw new ExcelReaderException($"{ExcelName} Type:{typeof(DataExcelConfig)}不能合并表 Type:{config.GetType()}");
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