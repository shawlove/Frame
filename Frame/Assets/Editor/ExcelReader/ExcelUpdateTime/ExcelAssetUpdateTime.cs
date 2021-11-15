using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Config
{
    public static class ExcelAssetUpdateTime
    {
        [Serializable]
        private class JsonClassList
        {
            public List<JsonClass> list;

            public void UpdateTime(string name)
            {
                DateTime  now  = DateTime.Now;
                string    time = $"{now.Month}月{now.Day}日{now.Hour}:{now.Minute}";
                JsonClass cls  = list.Find((json => json.name == name));
                if (cls == null)
                {
                    list.Add(new JsonClass() {name = name, time = time});
                }
                else
                {
                    cls.time = time;
                }
            }

            public string GeTime(string name)
            {
                JsonClass cls = list.Find((json => json.name == name));
                if (cls == null)
                    return "无版本";
                else
                    return cls.time;
            }
        }

        [Serializable]
        private class JsonClass
        {
            public string name;
            public string time;
        }

        private const  string        UPDATE_TIME_JSON_PATH = "Assets/_RestoryWork/Editor/ExcelReader/ExcelUpdateTime/ExcelAssetUpdateTimeJson.json";
        private static JsonClassList _;

        private static JsonClassList JsonList
        {
            get
            {
                if (_ == null)
                {
                    using (var sr = new StreamReader(new FileStream(UPDATE_TIME_JSON_PATH, FileMode.OpenOrCreate)))
                    {
                        string content = sr.ReadToEnd();
                        _ = JsonUtility.FromJson<JsonClassList>(content);
                    }
                }

                return _;
            }
        }

        public static void UpdateExcelTime(string name)
        {
            try
            {
                JsonList.UpdateTime(name);

                using (var sw = new StreamWriter(new FileStream(UPDATE_TIME_JSON_PATH, FileMode.Truncate)))
                {
                    sw.Write(JsonUtility.ToJson(JsonList));
                }

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public static string GetUpdateTime(string name)
        {
            try
            {
                return JsonList.GeTime(name);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }
}