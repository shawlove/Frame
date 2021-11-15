using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using UnityEditor;

namespace GameFrame.Config
{
    public abstract class ExcelReader
    {
        public readonly List<ExcelConfig> configs = new List<ExcelConfig>();

        protected abstract List<FileInfo> FindTargetExcelFile();
        protected abstract ExcelConfig    Parser(FileInfo file);

        public ExcelReader()
        {
            List<FileInfo> fileInfos = FindTargetExcelFile();

            List<ExcelConfig> configList = new List<ExcelConfig>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                try
                {
                    EditorUtility.DisplayProgressBar($"{GetType().Name}", $"加载文件{fileInfo.Name}", configList.Count / (float) fileInfos.Count);
                    configList.Add(Parser(fileInfo));
                }
                catch (Exception e)
                {
                    throw new Exception($"{fileInfo} 错误：\n\n\n\n{e}");
                }
            }

            int index = 0;

            //合并相同的项
            foreach (ExcelConfig excelConfig in configList)
            {
                EditorUtility.DisplayProgressBar($"{GetType().Name}", $"合并表{excelConfig.ExcelName}", index++ / (float) configList.Count);
                bool shouldCombine = false;
                foreach (ExcelConfig config in configs)
                {
                    if (config.ExcelName == excelConfig.ExcelName)
                    {
                        if (config is IExcelCombineConfig combineConfig && excelConfig is IExcelCombineConfig excelCombineConfig)
                        {
                            combineConfig.Combine(excelCombineConfig);
                            shouldCombine = true;
                            break;
                        }
                        else
                        {
                            EditorUtility.ClearProgressBar();
                            throw new Exception($"{config.ExcelName} 和 {excelConfig.ExcelName} 合并错误 ，两张表不能合并，但是表名相同");
                        }
                    }
                }

                if (shouldCombine == false)
                {
                    configs.Add(excelConfig);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        public static ExcelTable FileToExcelTable(FileInfo fileInfo)
        {
            try
            {
                using (FileStream stream = fileInfo.OpenRead())
                {
                    IExcelDataReader excelReader;
                    if (fileInfo.Extension == ".xls")
                    {
                        excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (fileInfo.Extension == ".csv")
                    {
                        excelReader = ExcelReaderFactory.CreateCsvReader(stream);
                    }
                    else
                    {
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }

                    //IExcelDataReader excelReader = file.Extension == ".xls"
                    //    ? ExcelReaderFactory.CreateBinaryReader(stream)
                    //    : ExcelReaderFactory.CreateOpenXmlReader(stream);
                    DataTable table = excelReader.AsDataSet().Tables[0];
                    stream.Close();

                    return new ExcelTable(table);
                }
            }
            catch (IOException e)
            {
                throw new Exception($"关闭excel表{fileInfo.Name}再试");
            }
        }
    }
}