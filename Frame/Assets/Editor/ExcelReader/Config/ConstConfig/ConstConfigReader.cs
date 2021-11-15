using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameFrame.Config
{
    public class ConstConfigReader : ExcelReader
    {
        protected override List<FileInfo> FindTargetExcelFile()
        {
            return Directory.CreateDirectory(ExcelPathDefine.CONST_EXCEL_FOLDER).GetFiles("*", SearchOption.AllDirectories).ToList();
        }

        protected override ExcelConfig Parser(FileInfo file)
        {
            ExcelTable table = FileToExcelTable(file);

            string excelName = file.Name.Split('.')[0];

            List<string> dataType    = table.GetRow(0);
            List<string> dataName    = table.GetRow(1);
            List<string> dataMeta    = table.GetRow(2);
            List<string> dataComment = table.GetRow(3);

            if (dataType.Count != dataName.Count)
                throw new ExcelReaderException("类型和名字数量不对应");

            //------------------------------------------创建DataConfigItem
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

            //------------------------------------------用第五行赋值
            List<string> rowFive = table.GetRow(4, true);
            for (int j = 0; j < items.Count; j++)
            {
                items[j].AddContent(rowFive[j]);
            }

            return new ConstExcelConfig(excelName, items);
        }
    }
}