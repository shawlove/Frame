using System;
using System.Collections.Generic;
using System.Text;

namespace GameFrame.Config
{
    public struct ExcelEnumItem
    {
        public string enumName;
        public string enumContent;
        public int    enumValue;
        public string enumChinese;
        public string enumComment;

        public bool HasChinese => string.IsNullOrEmpty(enumChinese) == false;

        public ExcelEnumItem(List<string> row)
        {
            enumName    = row[0];
            enumContent = row[1];
            try
            {
                enumValue = int.Parse(row[2]);
            }
            catch (Exception e)
            {
                throw new Exception($"枚举 {enumName} {enumContent} 的枚举值填写错误，必须为数字");
            }

            enumChinese = row[3];
            enumComment = row[4];
        }
    }
}