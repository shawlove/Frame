using System;
using UnityEngine;

namespace GameFrame.Config
{
    public class ExcelReaderException : Exception
    {
        private readonly string    _tip;
        private readonly Exception _e;

        public ExcelReaderException(string tip)
        {
            _tip = tip;
        }

        public ExcelReaderException(string tip,Exception e)
        {
            _tip = tip;
            _e   = e;
        }

        public string GetTip()
        {
            string result = $"{_tip}\n{_tip}\n{_tip}\n{_tip}\n{_tip}\n{_tip}\n";
            if (_e == null)
            {
                return result;
            }
            else
            {
                Debug.LogError($"ExcelReader报错信息 : {_e}");
                return result + "报错信息见打印";
            }
        }
    }
}