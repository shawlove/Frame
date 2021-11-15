using System.Collections.Generic;
using System.Data;

namespace GameFrame.Config
{
    /// <summary>
    /// 代表一张Excel表
    /// </summary>
    public class ExcelTable
    {
        //行，列
        private readonly string[,] _content;

        public readonly DataTable dataTable;

        public int ColumnCount => _content.GetLength(1);

        public int RowCount => _content.GetLength(0);

        public ExcelTable(DataTable table)
        {
            dataTable = table;

            _content = new string[table.Rows.Count, table.Columns.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    _content[i, j] = table.Rows[i][j].ToString();
                }
            }
        }

        /// <summary>
        /// 获取一行数据
        /// </summary>
        /// <param name="row"></param>
        /// <param name="retainEmpty">是否保留末端空字符串</param>
        /// <returns></returns>
        public List<string> GetRow(int row, bool retainEmpty = false)
        {
            List<string> result = new List<string>();
            bool         start  = false;
            for (int i = _content.GetLength(1) - 1; i >= 0; i--)
            {
                string str = _content[row, i];
                if (retainEmpty)
                {
                    result.Insert(0, str);
                }
                else
                {
                    if (start == false)
                    {
                        if (string.IsNullOrEmpty(str))
                        {
                            continue;
                        }
                        else
                        {
                            result.Insert(0, str);
                            start = true;
                        }
                    }
                    else
                    {
                        result.Insert(0, str);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取一列数据
        /// </summary>
        /// <param name="column"></param>
        /// <param name="retainEmpty">是否保留末端空字符串</param>
        /// <returns></returns>
        public List<string> GetColumn(int column, bool retainEmpty = false)
        {
            List<string> result = new List<string>();
            bool         start  = false;
            for (int i = _content.GetLength(0) - 1; i >= 0; i--)
            {
                string str = _content[i, column];
                if (retainEmpty)
                {
                    result.Insert(0, str);
                }
                else
                {
                    if (start == false)
                    {
                        if (string.IsNullOrEmpty(str))
                        {
                            continue;
                        }
                        else
                        {
                            result.Insert(0, str);
                            start = true;
                        }
                    }
                    else
                    {
                        result.Insert(0, str);
                    }
                }
            }

            return result;
        }
    }
}