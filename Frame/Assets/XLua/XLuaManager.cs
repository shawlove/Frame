using System.Text;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

namespace GameFrame.XLua
{
    public class XLuaManager : MonoBehaviour
    {
        private LuaEnv _luaenv;

        private void CompileLuaScripts()
        {
            _luaenv?.Dispose();
            _luaenv = new LuaEnv();

            _luaenv.AddLoader(CustomLoader);

            //第三方库
            _luaenv.AddBuildin("rapidjson", Lua.LoadRapidJson);
            _luaenv.AddBuildin("pb", Lua.LoadLuaProfobuf);

#if UNITY_EDITOR

            //调试
            _luaenv.DoString(@"    package.cpath = package.cpath .. ';' .. CS.UnityEngine.Application.dataPath .. '/_RestoryWork/Script/XLua/Editor/?.dll'
            local dbg = require('emmy_core')
            pcall(dbg.tcpConnect, 'localhost', 9966)");
#endif

            try
            {
                _luaenv.DoString("require('LuaMain')");
            }
            catch (LuaException e)
            {
                Debug.LogError($"lua出错：{e}");
            }
        }

        private byte[] CustomLoader(ref string luaName)
        {
#if UNITY_EDITOR
            if (luaName == "emmy_core")
                return null;
#endif
            switch (luaName)
            {
                case "xlua.util":
                case "perf.memory":
                case "perf.profiler":
                case "tdr.tdr":
                    return null;
            }

            TextAsset text = null;

            return Encoding.UTF8.GetBytes(text.text);
        }

        public void Update()
        {
            _luaenv?.Tick();
        }
    }
}