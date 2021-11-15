using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace GameFrame.Config
{
    public class SyncConfigManager
    {
        private const string LUA_FILE_PATH = "Assets/_RestoryWork/AddressableResources/Lua/Common/Config/XConfigComment.lua.txt";
        private const string FILE_PATH     = "Assets/_RestoryWork/Script/BasicModule/Runtime/BasicFunctionManagers/Config/ConfigManager_Auto.cs";

        private static readonly List<FileInfo> Configs = new List<FileInfo>();

        private static Assembly _assembly;

        public static void Sync()
        {
            _assembly = typeof(IConfig).Assembly;

            Configs.Clear();
            Configs.AddRange(Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + ExcelPathDefine.DATA_ASSET_FOLDER)
                                      .GetFiles("*.json", SearchOption.AllDirectories));
            Configs.AddRange(Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + ExcelPathDefine.CONST_ASSET_FOLDER)
                                      .GetFiles("*.json", SearchOption.AllDirectories));
            Configs.AddRange(Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + ExcelPathDefine.CONFIG_ASSET_SCRIPTABLE_OBJECTS_DIRECTORY)
                                      .GetFiles("*.asset", SearchOption.AllDirectories));

            string codeContent = GenerateCode();

            if (File.Exists(FILE_PATH))
            {
                File.Delete(FILE_PATH);
            }

            using (StreamWriter sw =
                new StreamWriter(new FileStream(FILE_PATH, FileMode.Create), Encoding.GetEncoding("utf-8")))
            {
                sw.Write(codeContent);
            }

            FileInfo luaFile = new FileInfo(LUA_FILE_PATH);
            string   content;
            using (var reader = new StreamReader(luaFile.OpenRead()))
            {
                content = reader.ReadToEnd();
            }

            using (var writer = new StreamWriter(luaFile.OpenWrite()))
            {
                writer.Write(GenerateLuaConfigManager(content));
            }
        }

        private static string GenerateCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("namespace RestoryStudio.Config")
              .AppendLine("{")
              .AppendLine("    public partial class ConfigManager ")
              .AppendLine("\t{");

            foreach (FileInfo fileInfo in Configs)
            {
                string configName = fileInfo.Name.Split('.')[0];
                Type   csharpType = _assembly.GetType(configName);

                //未生成C#端配置
                if (csharpType == null) continue;
                sb.AppendLine("\t\t[AutoConfig]");
                sb.AppendLine("\t\tpublic " + GetTypeName(csharpType) + " " + configName + " { get; private set; }");
            }

            sb.AppendLine("\t}").Append("}");

            return sb.ToString();
        }

        private static CSharpCodeProvider _provider = new CSharpCodeProvider();

        private static string GetTypeName(Type t)
        {
            CodeTypeReference reference = new CodeTypeReference(t.ToString());
            return _provider.GetTypeOutput(reference);
        }

        private static string GenerateLuaConfigManager(string content)
        {
            StringBuilder sb = new StringBuilder();

            foreach (FileInfo fileInfo in Configs)
            {
                //lua掉ScriptableObject直接用CS调
                if (fileInfo.Name.EndsWith(".asset")) continue;

                //---@field public TestConfig XTestConfig @调用时自动加载
                string configName = fileInfo.Name.Split('.')[0];

                sb.AppendLine($"---@field public {configName} X{configName} @调用时自动加载");
            }

            return Regex.Replace(content, @"(?<=---start[\s]*?XConfigManager[\s]*?\n)[\s\S]*?(?=---end)", sb.ToString());
        }
    }
}