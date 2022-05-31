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

        private readonly List<IDeInitializeSingleton> _deInitializeManagers = new List<IDeInitializeSingleton>();
        private readonly List<IUpdateSingleton>       _updateManagers       = new List<IUpdateSingleton>();

        public T CreateSingleton<T>() where T : Singleton<T>, new()
        {
            T singleton = new T();

            if (singleton is IDeInitializeSingleton deInitializeManager)
            {
                _deInitializeManagers.Add(deInitializeManager);
            }

            if (singleton is IUpdateSingleton updateManager)
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

            if (singleton is IDeInitializeSingleton deInitializeManager)
            {
                _deInitializeManagers.Add(deInitializeManager);
            }

            if (singleton is IUpdateSingleton updateManager)
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
                if (l.updateOrder > r.updateOrder)
                {
                    return 1;
                }
                else if (l.updateOrder < r.updateOrder)
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
            foreach (IDeInitializeSingleton deInitializeManager in _deInitializeManagers)
            {
                deInitializeManager.DeInitialize();
            }
        }
    }
}