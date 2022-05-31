using System;

namespace GameFrame.UI
{
    [Serializable]
    public class UIConfig
    {
        public EuiType  uiType;
        public string   uiName;
        public string   uiDataName;
        public string[] childDataName;
        public string[] preloadAtlas;
        public string[] preloadConfig;
    }
}