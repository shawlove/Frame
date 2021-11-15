using System;
using System.Collections.Generic;

namespace GameFrame.Config
{
    public struct ExcelAttributeItem
    {
        public int    attributeID;
        public string attributeChinese;
        public string attributeEn;
        public double maxValue;
        public double minValue;
        public string suffix;
        public int    point;

        public ExcelAttributeItem(List<string> row)
        {
            try
            {
                attributeID = int.Parse(row[0]);
            }
            catch (Exception e)
            {
                throw new Exception($"{row[0]} 无法转换成int");
            }

            attributeChinese = row[1];
            attributeEn   = row[2];
            try
            {
                minValue = row[3] == "" ? 0 : double.Parse(row[3]);
            }
            catch (Exception e)
            {
                throw new Exception($"{row[3]} 无法转换成double");
            }

            try
            {
                maxValue = row[4] == "" ? 0 : double.Parse(row[4]);
            }
            catch (Exception e)
            {
                throw new Exception($"{row[4]} 无法转换成double");
            }

            suffix = row[5];

            try
            {
                point = row[6] == "" ? 0 : int.Parse(row[6]);
            }
            catch (Exception e)
            {
                throw new Exception($"{row[6]} 无法转换成int");
            }
        }
    }
}