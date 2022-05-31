using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class SingletonManager
    {
        public static SingletonManager Instance { get; private set; }

        public static SingletonManager CreateSingletonManager(Transform owner)
        {
            Instance = new SingletonManager {_owner = owner};
            return Instance;
        }

        private Transform _owner;

        private readonly List<IDeInitializeManager> _deInitializeManagers = new List<IDeInitializeManager>();
        private readonly List<IUpdateManager>       _updateManagers       = new List<IUpdateManager>();

        public T CreateSingleton<T>() where T : Singleton<T>, new()
        {
            T singleton = new T();

            if (singleton is IDeInitializeManager deInitializeManager)
            {
                _deInitializeManagers.Add(deInitializeManager);
            }

            if (singleton is IUpdateManager updateManager)
            {
                _updateManagers.Add(updateManager);
                SortUpdateManager();
            }

            return singleton;
        }

        public T CreateGlobalManagerMono<T>() where T : MonoSingleton<T>
        {
            GameObject obj = new GameObject($"[{typeof(T).Name}]");
            obj.transform.SetParent(_owner);
            T singleton = obj.AddComponent<T>();

            if (singleton is IDeInitializeManager deInitializeManager)
            {
                _deInitializeManagers.Add(deInitializeManager);
            }

            if (singleton is IUpdateManager updateManager)
            {
                _updateManagers.Add(updateManager);
                SortUpdateManager();
            }

            return singleton;
        }

        private void SortUpdateManager()
        {
            _updateManagers.Sort((l, r) =>
            {
                if (l.UpdateOrder > r.UpdateOrder)
                {
                    return 1;
                }
                else if (l.UpdateOrder < r.UpdateOrder)
                {
                    return -1;
                }

                return 0;
            });
        }

        public void Update()
        {
            for (int i = 0; i < _updateManagers.Count; i++)
            {
                try
                {
                    _updateManagers[i].CustomUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void OnDestroy()
        {
            foreach (IDeInitializeManager deInitializeManager in _deInitializeManagers)
            {
                deInitializeManager.DeInitialize();
            }
        }
    }
}