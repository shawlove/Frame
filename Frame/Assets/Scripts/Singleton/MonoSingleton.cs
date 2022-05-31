using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static          T      _instance;
        private static readonly object objLock = new object();

        public static bool IsInstanceEqualsNull()
        {
            return ReferenceEquals(_instance, null);
        }

        public static async Task PreCreateInstance()
        {
            if (ReferenceEquals(_instance, null) == false) return;

            T singleton = SingletonManager.Instance.CreateGlobalManagerMono<T>();
            _instance = singleton;

            if (singleton is ISyncInitalizeManager syncInitalizeManager)
            {
                try
                {
                    await syncInitalizeManager.Initialize();
                    Debug.Log($"Sync创建GlobalManager {typeof(T).Name}");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            else if (singleton is ILazyInitializeManager lazyInitializeManager)
            {
                try
                {
                    lazyInitializeManager.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public static T Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                {
                    lock (objLock)
                    {
                        if (ReferenceEquals(_instance, null))
                        {
                            T singleton = SingletonManager.Instance.CreateGlobalManagerMono<T>();
                            _instance = singleton;

                            if (singleton is ISyncInitalizeManager syncInitalizeManager)
                            {
                                Debug.LogError($"ISyncInitalizeManager 不能Lazy创建，只能事先创建好后使用{typeof(T).Name}");
                            }
                            else if (singleton is ILazyInitializeManager initializeManager)
                            {
                                try
                                {
                                    initializeManager.Initialize();
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e);
                                }
                            }

                            Debug.Log($"Lazy创建GlobalManager {typeof(T).Name}");
                        }
                    }
                }

                return _instance;
            }
        }
    }
}