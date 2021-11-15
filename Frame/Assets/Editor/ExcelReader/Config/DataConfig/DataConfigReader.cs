using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameFrame.Config
{
    public class DataConfigReader : ExcelReader
    {
        protected override List<FileInfo> FindTargetExcelFile()
        {
            return Directory.CreateDirectory(ExcelPathDefine.DATA_EXCEL_FOLDER).GetFiles("*", SearchOption.AllDirectories).ToList();
        }

        protected override ExcelConfig Parser(FileInfo file)
        {
            ExcelTable table = FileToExcelTable(file);

            string excelName = file.Name.Split('.')[0];

            string[] excelNameSplits = excelName.Split('_');
            if (excelNameSplits.Length > 1)
            {
                return ParseVertical(excelNameSplits[0], excelNameSplits[1], table);
            }
            else
            {
                return ParseHorizontal(excelName, table);
            }
        }

        private ExcelConfig ParseVertical(string excelName, string id, ExcelTable table)
        {
            List<string> dataType    = table.GetRow(0);
            List<string> dataName    = table.GetRow(1);
            List<string> dataMeta    = table.GetRow(2);
            List<string> dataComment = table.GetRow(3);

            if (dataType.Count != dataName.Count)
                throw new ExcelReaderException("类型和名字数量不对应");

            //------------------------------------------创建DataConfigItem
            List<ExcelConfigItem> items = CreateDataConfigItem(dataType, dataName, dataMeta, dataComment);

            //------------------------------------------用第五行赋值
            List<string> rowFive = table.GetRow(4, true);
            for (int j = 0; j < items.Count; j++)
            {
                items[j].AddContent(rowFive[j]);
            }

            //------------------------------------------手动创建ID项添加到表头
            ExcelConfigItem idItem = new ExcelConfigItem("int", "ID", "", "");
            idItem.AddContent(id);
            items.Insert(0, idItem);

            //------------------------------------------读第6-9行
            dataType    = table.GetRow(5);
            dataName    = table.GetRow(6);
            dataMeta    = table.GetRow(7);
            dataComment = table.GetRow(8);

            if (dataType.Count != dataName.Count)
                throw new ExcelReaderException("类型和名字数量不对应");

            //------------------------------------------创建DataConfigItem
            List<ExcelConfigItem> verticalItems = CreateDataConfigItem(dataType, dataName, dataMeta, dataComment);

            foreach (ExcelConfigItem verticalItem in verticalItems)
            {
                verticalItem.ChangeIntoCombineList();
            }

            string[] combineStr = new string[verticalItems.Count];
            for (int i = 9; i < table.RowCount; i++)
            {
                List<string> row = table.GetRow(i, true);
                for (int j = 0; j < combineStr.Length; j++)
                {
                    combineStr[j] += row[j] + ExcelConfig.CODE_SPLIT_LIST;
                }
            }

            for (int i = 0; i < combineStr.Length; i++)
            {
                verticalItems[i].AddContent(combineStr[i].TrimEnd(ExcelConfig.CODE_SPLIT_LIST));
            }

            items.AddRange(verticalItems);

            DataExcelConfig config = new DataExcelConfig(excelName, items);
            return config;
        }

        private ExcelConfig ParseHorizontal(string excelName, ExcelTable table)
        {
            List<string> dataType    = table.GetRow(0);
            List<string> dataName    = table.GetRow(1);
            List<string> dataMeta    = table.GetRow(2);
            List<string> dataComment = table.GetRow(3);

            if (dataType.Count != dataName.Count)
                throw new ExcelReaderException("类型和名字数量不对应");

            //------------------------------------------创建DataConfigItem
            List<ExcelConfigItem> items = CreateDataConfigItem(dataType, dataName, dataMeta, dataComment);

            //------------------------------------------赋值
            for (int i = 4; i < table.RowCount; i++)
            {
                List<string> row = table.GetRow(i, true);
                for (int j = 0; j < items.Count; j++)
                {
                    items[j].AddContent(row[j]);
                }
            }

            DataExcelConfig config = new DataExcelConfig(excelName, items);
            return config;
        }

        private List<ExcelConfigItem> CreateDataConfigItem(List<string> dataType, List<string> dataName, List<string> dataMeta, List<string> dataComment)
        {
            List<ExcelConfigItem> items = new List<ExcelConfigItem>();
            for (int i = 0; i < dataType.Count; i++)
            {
                string type, name, meta, comment;
                type = dataType[i];
                name = dataName[i];
                if (dataMeta.Count > i)
                {
                    meta = dataMeta[i];
                }
                else
                {
                    meta = "";
                }

                if (dataComment.Count > i)
                {
                    comment = dataComment[i];
                }
                else
                {
                    comment = "";
                }

                ExcelConfigItem item = new ExcelConfigItem(type, name, meta, comment);

                items.Add(item);
            }

            return items;
        }
    }
}