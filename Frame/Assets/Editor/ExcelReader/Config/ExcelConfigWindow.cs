using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameFrame.Config
{
    public class ExcelConfigWindow : EditorWindow
    {
        private class ExcelHelper
        {
            public readonly string group;
            public readonly string description;

            public ExcelHelper(string group, string description)
            {
                this.group       = group;
                this.description = description;
            }
        }

        private bool[]                          selectionStates;
        private bool                            selectAll;
        private Vector2                         size;
        private Vector2                         pos;
        private List<ExcelConfig>               selectedFiles         = new List<ExcelConfig>();
        private Dictionary<string, ExcelHelper> excelHelperDictionary = new Dictionary<string, ExcelHelper>();
        private string[]                        groups;
        private int                             selectedGroupIndex = 0;
        private bool                            isData;
        private ExcelReader                     _reader;
        private ExcelReader                     _dataConfigReader;
        private ExcelReader                     _constConfigReader;

        //搜索框用
        private SearchField search;
        private string      searchStr = "";

        private static ExcelConfigWindow myWindow;

        [MenuItem("RestoryTools/策划工具/配置数据/1.配置表工具窗", false, 101)]
        public static void CreateAndShowReader()
        {
            if (myWindow != null)
            {
                return;
            }

            myWindow = GetWindow(typeof(ExcelConfigWindow), false, "配置表工具", true) as ExcelConfigWindow; //创建窗口
            myWindow.Init();
            myWindow.Show(); //展示
        }

        private void Init()
        {
            try
            {
                minSize = new Vector2(550, 900);
                maxSize = new Vector2(1000, 900);
                search  = new SearchField();
                EditorUtility.DisplayProgressBar("加载中", "初始化Data表", 0);
                _dataConfigReader = new DataConfigReader();
                EditorUtility.DisplayProgressBar("加载中", "初始化Const表", 0.5f);
                _constConfigReader = new ConstConfigReader();
                EditorUtility.DisplayProgressBar("加载中", "完成", 1f);
                isData = false;
                SwitchDataOrConst();
            }
            catch (ExcelReaderException e)
            {
                EditorUtility.DisplayDialog("初始化失败", e.GetTip(), "确认");
            }
            catch (Exception e)
            {
                Debug.LogError($"导表窗口未知错误打印：{e}");
                EditorUtility.DisplayDialog("初始化失败", "未知错误！\n见打印", "确认");
            }

            EditorUtility.ClearProgressBar();
        }

        private void SwitchDataOrConst()
        {
            try
            {
                if (isData)
                {
                    isData          = false;
                    _reader         = _constConfigReader;
                    selectionStates = new bool[_reader.configs.Count];
                    selectAll       = false;
                    searchStr       = "";
                    LoadHelper(false);
                }
                else
                {
                    isData          = true;
                    _reader         = _dataConfigReader;
                    selectionStates = new bool[_reader.configs.Count];
                    selectAll       = false;
                    searchStr       = "";
                    LoadHelper(true);
                }
            }
            catch (ExcelReaderException e)
            {
                EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
            }
            catch (Exception e)
            {
                Debug.LogError($"导表窗口未知错误打印：{e}");
                EditorUtility.DisplayDialog("错误", "未知错误！\n见打印", "确认");
                Close();
            }
        }

        private void LoadHelper(bool isData)
        {
            excelHelperDictionary.Clear();
            using (FileStream stream = File.OpenRead(ExcelPathDefine.EXCEL_HELPER_ASSET_PATH))
            {
                IExcelDataReader  excelReader   = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataRowCollection data          = isData ? excelReader.AsDataSet().Tables[0].Rows : excelReader.AsDataSet().Tables[1].Rows;
                HashSet<string>   groupsHashset = new HashSet<string>();
                string            excel, group, description;
                for (int i = 1; i < data.Count; i++)
                {
                    excel = isData ? $"Data_{data[i][0]}" : $"Const_{data[i][0]}";
                    if (isData)
                    {
                        group       = data[i][1].ToString();
                        description = data[i][2].ToString();
                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            groupsHashset.Add(group);
                        }
                    }
                    else
                    {
                        group       = String.Empty;
                        description = data[i][1].ToString();
                    }

                    excelHelperDictionary.Add(excel, new ExcelHelper(group, description));
                }

                groups    = new string[groupsHashset.Count + 1];
                groups[0] = "全部分组";
                int groupIndex = 1;
                foreach (string item in groupsHashset)
                {
                    groups[groupIndex] = item;
                    groupIndex++;
                }
            }
        }

        private void OnGUI()
        {
            try
            {
                // 表示unity代码有变动，需要重新加载
                if (myWindow == null)
                {
                    CreateAndShowReader();
                }

                size = myWindow.position.size;

                //自动排布区
                GUILayout.BeginArea(new Rect(0, 10, size.x, 890));


                #region 功能注释和分组筛选

                // GUILayout.Box("-----上方搜索栏目-----适用精准查找\n建议：功能二选一\n-----下方分组筛选-----适用模块查找", GUILayout.Height(50), GUILayout.ExpandWidth(true));

                Color switchButtonColor = isData ? new Color(0.54f, 1f, 0.65f) : new Color(0.62f, 0.8f, 1f);
                GUI.color = switchButtonColor;
                string switchButtonText = isData ? "当前为数据表   [点击切换至常数表]" : "当前为常数表   [点击切换至数据表]";
                if (GUILayout.Button(switchButtonText, GUILayout.Height(20)))
                {
                    SwitchDataOrConst();
                }

                GUI.color = Color.white;

                GUILayout.Space(30);

                GUILayout.BeginArea(new Rect(5, 25, size.x - 5, 35));
                if (isData)
                {
                    //分组选择枚举
                    selectedGroupIndex = EditorGUILayout.Popup(selectedGroupIndex, groups, GUILayout.Height(20), GUILayout.Width(150));
                }
                else
                {
                    selectedGroupIndex = 0;
                }

                GUILayout.BeginArea(new Rect(200, 0, size.x - 90, 30));
                GUILayout.Label("搜索：", GUILayout.Width(50));

                //搜索框GUI,手动排布区 
                searchStr = search.OnGUI(new Rect(50, 0, size.x - 245, 20), searchStr);
                searchStr = searchStr.ToLower();
                GUILayout.EndArea();

                GUILayout.EndArea();

                #endregion


                #region 全选和栏目分类

                GUILayout.BeginHorizontal();
                bool cancelAll = selectAll;
                if (selectAll = EditorGUILayout.Toggle("全选", selectAll, GUILayout.Width(200)))
                {
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        if ((string.IsNullOrEmpty(searchStr) || _reader.configs[i].ExcelName.ToLower().Contains(searchStr)) && IsInThisGroup(_reader.configs[i].ExcelName))
                        {
                            selectionStates[i] = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else if (cancelAll)
                {
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        selectionStates[i] = false;
                    }
                }

                EditorGUILayout.LabelField("更新时间", GUILayout.Width(100));
                EditorGUILayout.LabelField("中文释意");
                GUILayout.EndHorizontal();

                #endregion


                pos = EditorGUILayout.BeginScrollView(pos);


                #region Excel文件Toggle及文件信息显示

                int selectCount = 0;
                if (_reader.configs.Count == 0)
                {
                    EditorGUILayout.LabelField("无配置表！");
                }
                else
                {
                    for (int i = 0; i < _reader.configs.Count; i++)
                    {
                        //在勾选表格右侧写上中文释义以及更新时间
                        GUILayout.BeginHorizontal();
                        if ((string.IsNullOrEmpty(searchStr) || _reader.configs[i].ExcelName.ToLower().Contains(searchStr)) && IsInThisGroup(_reader.configs[i].ExcelName))
                        {
                            selectionStates[i] = EditorGUILayout.Toggle(_reader.configs[i].ExcelName, selectionStates[i], GUILayout.Width(200));
                            EditorGUILayout.LabelField(GetUpdateTimeVersion(_reader.configs[i].ExcelName), GUILayout.Width(100));
                            EditorGUILayout.LabelField(GetChineseDescription(_reader.configs[i].ExcelName));
                        }

                        GUILayout.EndHorizontal();

                        if (selectionStates[i] == false)
                        {
                            selectAll = false;
                        }
                        else
                        {
                            selectCount++;
                        }
                    }
                }

                #endregion


                if (selectCount == selectionStates.Length)
                {
                    selectAll = true;
                }

                EditorGUILayout.EndScrollView();


                #region 导出按钮和文件勾选数

#if !HOT_UPDATE
                if (GUILayout.Button("生成代码(C#)", GUILayout.Height(30)))
                {
                    selectedFiles.Clear();
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        if (selectionStates[i])
                        {
                            selectedFiles.Add(_reader.configs[i]);
                        }
                    }

                    if (selectedFiles.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n", "确认");
                        return;
                    }

                    bool isSuc = true;
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("导出配置数据代码", selectedFiles[i].ExcelName, i / (float) selectedFiles.Count);
                        try
                        {
                            if (selectedFiles[i] is IExcelGenerateCode generateCode)
                                generateCode.GenerateCSharpCode();

                            //同时也创建一个资源文件
                            //    selectedFiles[i].CreateAsset();
                        }
                        catch (ExcelReaderException e)
                        {
                            EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
                            isSuc = false;
                            break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"导表窗口未知错误打印：{e}");
                            EditorUtility.DisplayDialog("导表失败", "未知错误！\n见打印", "确认");
                            isSuc = false;
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    if (isSuc)
                    {
                        EditorUtility.DisplayDialog("导表完毕", "代码生成完毕", "确认");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    //更新ConfigManager
                    SyncConfigManager.Sync();
                }
#endif
                GUILayout.Space(10);
                if (GUILayout.Button("生成代码(Lua)", GUILayout.Height(30)))
                {
                    selectedFiles.Clear();
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        if (selectionStates[i])
                        {
                            selectedFiles.Add(_reader.configs[i]);
                        }
                    }

                    if (selectedFiles.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n", "确认");
                        return;
                    }

                    bool isSuc = true;
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("导出配置数据代码", selectedFiles[i].ExcelName, i / (float) selectedFiles.Count);
                        try
                        {
                            if (selectedFiles[i] is IExcelGenerateCode generateCode)
                                generateCode.GenerateLuaCode();

                            //同时也创建一个资源文件
                            //    selectedFiles[i].CreateAsset();
                        }
                        catch (ExcelReaderException e)
                        {
                            EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
                            isSuc = false;
                            break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"导表窗口未知错误打印：{e}");
                            EditorUtility.DisplayDialog("导表失败", "未知错误！\n见打印", "确认");
                            isSuc = false;
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    if (isSuc)
                    {
                        EditorUtility.DisplayDialog("导表完毕", "代码生成完毕", "确认");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    //更新ConfigManager
                    SyncConfigManager.Sync();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("导出资源文件(json)", GUILayout.Height(30)))
                {
                    selectedFiles.Clear();
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        if (selectionStates[i])
                        {
                            selectedFiles.Add(_reader.configs[i]);
                        }
                    }

                    if (selectedFiles.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n", "确认");
                        return;
                    }

                    bool isSuc = true;
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("导出配置数据资源", selectedFiles[i].ExcelName, i / (float) selectedFiles.Count);
                        try
                        {
                            if (selectedFiles[i] is IExcelGenerateAsset generateAsset)
                                generateAsset.CreateAsset();
                        }
                        catch (ExcelReaderException e)
                        {
                            EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
                            isSuc = false;
                            break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"导表窗口未知错误打印：{e}");
                            EditorUtility.DisplayDialog("导表失败", "未知错误！\n见打印", "确认");
                            isSuc = false;
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    if (isSuc)
                    {
                        EditorUtility.DisplayDialog("导表完毕", "资源导出完毕", "确认");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                GUILayout.Space(10);
                if (GUILayout.Button("检查资源(程序用)", GUILayout.Height(30)))
                {
                    selectedFiles.Clear();
                    for (int i = 0; i < selectionStates.Length; i++)
                    {
                        if (selectionStates[i])
                        {
                            selectedFiles.Add(_reader.configs[i]);
                        }
                    }

                    if (selectedFiles.Count == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n当前未勾选任何配置表\n", "确认");
                        return;
                    }

                    bool isSuc = true;
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("检查配置资源", selectedFiles[i].ExcelName, i / (float) selectedFiles.Count);
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("检查打印：");
                            if (selectedFiles[i] is IExcelGenerateAsset generateAsset)
                                sb.AppendLine(generateAsset.CheckAsset());
                            Debug.LogError(sb);
                        }
                        catch (ExcelReaderException e)
                        {
                            EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
                            isSuc = false;
                            break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"导表窗口未知错误打印：{e}");
                            EditorUtility.DisplayDialog("检查失败", "未知错误！\n见打印", "确认");
                            isSuc = false;
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    if (isSuc)
                    {
                        EditorUtility.DisplayDialog("检查完毕", "见打印", "确认");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                EditorGUILayout.LabelField(selectCount + "/" + _reader.configs.Count);

                #endregion


                GUILayout.EndArea();
            }
            catch (ExcelReaderException e)
            {
                EditorUtility.DisplayDialog("错误", e.GetTip(), "确认");
            }
            catch (Exception e)
            {
                Debug.LogError($"导表窗口未知错误打印：{e}");
                EditorUtility.DisplayDialog("导表失败", "未知错误！\n见打印", "确认");
                Close();
            }
        }

        //全部表格的信息描述
        private string GetChineseDescription(string excelName)
        {
            excelName = isData ? "Data_" + excelName : "Const_" + excelName;
            ExcelHelper helper;
            excelHelperDictionary.TryGetValue(excelName, out helper);
            return helper == null ? null : helper.description;
        }

        //分组筛选，返回布尔
        private bool IsInThisGroup(string excelName)
        {
            if (selectedGroupIndex == 0)
            {
                return true;
            }

            excelName = isData ? "Data_" + excelName : "Const_" + excelName;
            ExcelHelper helper;
            excelHelperDictionary.TryGetValue(excelName, out helper);
            if (helper != null)
            {
                return helper.group == groups[selectedGroupIndex];
            }
            else
            {
                return false;
            }
        }

        //读取出object的Version时间,传进来的名字是带有.xlsx的文件名
        private string GetUpdateTimeVersion(string excelName)
        {
            return ExcelAssetUpdateTime.GetUpdateTime(isData ? excelName + "DataConfig" : excelName + "ConstConfig");
        }
    }
}