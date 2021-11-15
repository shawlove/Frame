using System.Collections.Generic;
using System.Text;

namespace GameFrame.Config
{
    /// <summary>
    /// content会包含一列末尾的空字符串，保证每一列当数据长度相同
    /// </summary>
    public class ExcelConfigItem
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; private set; }
        /// <summary>
        /// 数据名称
        /// </summary>
        public string DataName { get; private set; }
        /// <summary>
        /// 元数据 ： Comment、Enum、Localize等
        /// </summary>
        public string DataMeta { get; private set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string DataComment { get; private set; }
        /// <summary>
        /// 表内容
        /// </summary>
        public IReadOnlyList<string> Contents => _contents;
        private List<string> _contents = new List<string>();

        public ExcelConfigItem(string type, string name, string meta, string comment)
        {
            DataType    = type;
            DataName    = name;
            DataMeta    = meta;
            DataComment = comment;
        }

        public bool IsID => DataName == "ID";

        /// <summary>
        /// 数据类型是否List
        /// </summary>
        public bool IsList => DataType.StartsWith("List<") && DataType.EndsWith(">");

        public void AddContent(string content)
        {
            //--------------------Excel中用,分割List。 代码中用-分割List
            if (IsList)
            {
                content = content.Replace(ExcelConfig.EXCEL_SPLIT_LIST, ExcelConfig.CODE_SPLIT_LIST);
            }

            _contents.Add(content);
        }


        #region 合并List

        /// <summary>
        /// 是否是_1 _2 形式的需要合并的项
        /// </summary>
        public bool IsCombineList => DataName.Split('_').Length > 1;

        /// <summary>
        /// 作为合并项的名字
        /// </summary>
        public string CombineListName => DataName.Split('_')[0] + "List";

        /// <summary>
        /// 将这一项变成合并的一项
        /// </summary>
        public void ChangeIntoCombineList()
        {
            DataName = CombineListName;
            DataType = $"List<{DataType}>";
        }

        /// <summary>
        /// 合并其他_1 , _2形式的项
        /// </summary>
        public void CombineList(ExcelConfigItem item)
        {
            if (item._contents.Count != _contents.Count) throw new ExcelReaderException($"合并{DataName}时出错，和{item.DataName} 的数据数量不匹配");

            for (int i = 0; i < item._contents.Count; i++)
            {
                _contents[i] = $"{_contents[i]}{ExcelConfig.CODE_SPLIT_LIST}{item._contents[i]}";
            }
        }

        /// <summary>
        /// 清理后面合并的空字符串
        /// </summary>
        public void ClearAfterCombine()
        {
            for (int i = 0; i < _contents.Count; i++)
            {
                // while (_contents[i].Length>0 && _contents[i][_contents[i].Length-1] == ExcelConfig.CODE_SPLIT_LIST)
                // {
                //     
                // }
                _contents[i] = _contents[i].TrimEnd(new char[] {ExcelConfig.CODE_SPLIT_LIST});
            }
        }

        #endregion


        #region 处理枚举

        public void DealEnum()
        {
            if (string.IsNullOrEmpty(DataMeta)) return;

            string[] splits = DataMeta.Split(',');
            foreach (string split in splits)
            {
                string[] s = split.Split('#');
                if (s[0] == "Enum" || s[0] == "enum")
                {
                    string enumName = "E" + s[1];

                    //DataType 改为对应枚举  内容去掉 枚举名_
                    if (IsList)
                    {
                        DataType = $"List<{enumName}>";
                    }
                    else
                    {
                        DataType = enumName;
                    }

                    for (int i = 0; i < _contents.Count; i++)
                    {
                        _contents[i] = _contents[i].Replace($"{s[1]}_", "");
                    }
                }
            }
        }

        #endregion


        #region 处理本地化

        public bool DealLocalize(out ExcelConfigItem localizeItem)
        {
            if (string.IsNullOrEmpty(DataMeta))
            {
                localizeItem = null;
                return false;
            }

            string[] splits = DataMeta.Split(',');
            foreach (string split in splits)
            {
                string[] s = split.Split('#');
                if (s[0] == "Localize")
                {
                    string type;
                    string name    = $"{ExcelConfig.LOCALIZED_TEXT_PREFIX}{DataName}";
                    string meta    = DataMeta;
                    string comment = $"{DataComment} - 本地化ID";

                    if (IsList)
                    {
                        type         = "List<ulong>";
                        localizeItem = new ExcelConfigItem(type, name, meta, comment);

                        foreach (string content in _contents)
                        {
                            if (string.IsNullOrEmpty(content))
                            {
                                localizeItem._contents.Add("");
                                continue;
                            }

                            string[] contentItems = content.Split(ExcelConfig.CODE_SPLIT_LIST);

                            //----------------计算hash，然后连接
                            StringBuilder sb = new StringBuilder();
                            foreach (string contentItem in contentItems)
                            {
                                sb.Append(contentItem.StringHashFnv1a());
                                sb.Append(ExcelConfig.CODE_SPLIT_LIST);
                            }

                            localizeItem._contents.Add(sb.ToString().TrimEnd(ExcelConfig.CODE_SPLIT_LIST));
                        }
                    }
                    else
                    {
                        type         = "ulong";
                        localizeItem = new ExcelConfigItem(type, name, meta, comment);

                        foreach (string content in _contents)
                        {
                            if (string.IsNullOrEmpty(content))
                            {
                                localizeItem._contents.Add("");
                                continue;
                            }

                            //----------------计算hash
                            localizeItem._contents.Add(content.StringHashFnv1a().ToString());
                        }
                    }

                    return true;
                }
            }

            localizeItem = null;
            return false;
        }

        #endregion


        #region 处理注释

        public bool DealComment()
        {
            if (string.IsNullOrEmpty(DataMeta)) return false;

            string[] splits = DataMeta.Split(',');
            foreach (string split in splits)
            {
                string[] s = split.Split('#');
                if (s[0] == "Comment" || s[0] == "comment")
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region 生成对应LuaComment

        public string ToLuaComment()
        {
            string fieldName = DataName;
            string fieldType;
            if (IsList)
            {
                //List<xx>  ->  xx[]
                fieldType = DataType.Substring(5, DataType.Length - 6) + "[]";
            }
            else
            {
                fieldType = DataType;
            }

            string fieldComment;
            if (IsList)
            {
                fieldComment = "@一个原生table数组  " + DataComment;
            }
            else
            {
                fieldComment = "@" + DataComment;
            }
            
            fieldComment = fieldComment.Replace("\n", " ");

            return $"---@field public {fieldName} {fieldType} {fieldComment}";
        }

        #endregion


        public void AddDataConfigItem(ExcelConfigItem item)
        {
            _contents.AddRange(item._contents);
        }

        public static bool operator ==(ExcelConfigItem a, ExcelConfigItem b)
        {
            if (ReferenceEquals(b, null))
                return ReferenceEquals(a, null);
            return a.DataType == b.DataType && b.DataName == a.DataName;
        }

        public static bool operator !=(ExcelConfigItem a, ExcelConfigItem b)
        {
            return (a == b) == false;
        }
    }
}