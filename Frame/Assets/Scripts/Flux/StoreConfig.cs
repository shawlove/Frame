using System;

namespace GameFrame.Flux
{
    [Serializable]
    public class StoreConfig
    {
        public string   storeName;
        public int      storePriority;
        public int      storeCallbackPriority;
        public string[] childStoreName;
    }
}