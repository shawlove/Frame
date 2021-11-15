using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameFrame.Config
{
    public class EnumReader : ExcelReader
    {
        private static EnumReader _reader;

        public static EnumReader Instance
        {
            get
            {
                if (_reader == null)
                    _reader = new EnumReader();
                return _reader;
            }
        }

        protected override List<FileInfo> FindTargetExcelFile()
        {
            return Directory.CreateDirectory(ExcelPathDefine.ENUM_EXCEL_FOLDER).GetFiles("*", SearchOption.AllDirectories).ToList();
        }

        protected override ExcelConfig Parser(FileInfo file)
        {
            string     excelName = file.Name.Split('.')[0];
            ExcelTable table     = FileToExcelTable(file);
            if (file.Name.Contains("Attribute"))
            {
                return ParserAttribute(excelName, table);
            }
            else
            {
                return ParserEnum(excelName, table);
            }
        }

        private ExcelAttributeConfig ParserAttribute(string excelName, ExcelTable table)
        {
            List<ExcelAttributeItem> items = new List<ExcelAttributeItem>();

            //忽略第一行
            for (int i = 1; i < table.RowCount; i++)
            {
                items.Add(new ExcelAttributeItem(table.GetRow(i, true)));
            }

            return new ExcelAttributeConfig(excelName, items);
        }

        private ExcelEnumsConfig ParserEnum(string excelName, ExcelTable table)
        {
            Dictionary<string, List<ExcelEnumItem>> items = new Dictionary<string, List<ExcelEnumItem>>();

            List<ExcelEnum> enums = new List<ExcelEnum>();

            //忽略第一行
            for (int i = 1; i < table.RowCount; i++)
            {
                ExcelEnumItem item = new ExcelEnumItem(table.GetRow(i, true));
                if (items.TryGetValue(item.enumName, out var list) == false)
                {
                    list = new List<ExcelEnumItem>();
                    items.Add(item.enumName, list);
                }

                list.Add(item);
            }

            foreach (KeyValuePair<string, List<ExcelEnumItem>> pair in items)
            {
                bool hasChinese = false;
                foreach (ExcelEnumItem item in pair.Value)
                {
                    if (item.HasChinese)
                    {
                        hasChinese = true;
                        break;
                    }
                }

                enums.Add(new ExcelEnum(pair.Key, pair.Value, hasChinese));
            }

            return new ExcelEnumsConfig(excelName, enums);
        }
    }
}