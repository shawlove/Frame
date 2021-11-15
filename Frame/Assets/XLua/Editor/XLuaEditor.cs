using CSObjectWrapEditor;

public static class XLuaEditor
{
    [GenPath]
    public static string customGenPath = "Assets/_RestoryWork/Script/XLua/Gen/";

    public static void GenCode()
    {
        Generator.GenAll();
    }
    
    public static void ClearCode()
    {
        Generator.ClearAll();
    }
}