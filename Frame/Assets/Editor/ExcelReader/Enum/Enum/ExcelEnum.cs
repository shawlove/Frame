using System.Collections.Generic;
using System.Text;

namespace GameFrame.Config
{
    public struct ExcelEnum
    {
        public string              enumName;
        public List<ExcelEnumItem> items;
        public bool                hasChinese;

        public ExcelEnum(string name, List<ExcelEnumItem> items, bool hasChinese)
        {
            enumName        = name;
            this.items      = items;
            this.hasChinese = hasChinese;
        }
    }
}