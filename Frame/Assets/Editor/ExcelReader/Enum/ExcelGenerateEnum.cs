using System;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Config
{
    public static class ExcelGenerateEnum
    {
        private static EnumReader _reader;

        private static EnumReader Reader
        {
            get
            {
                if (_reader == null)
                    _reader = new EnumReader();
                return _reader;
            }
        }

        public static void Generate()
        {
            try
            {
                foreach (ExcelConfig config in Reader.configs)
                {
                    if (config is IExcelGenerateCode generateCode)
                    {
                        generateCode.GenerateLuaCode();

#if !HOT_UPDATE
                        generateCode.GenerateCSharpCode();
#endif
                    }
                }

                EditorUtility.DisplayDialog("成功", "生成完毕", "确认");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError(e);
                EditorUtility.DisplayDialog("错误", e.Message, "确认");
            }
        }
    }
}