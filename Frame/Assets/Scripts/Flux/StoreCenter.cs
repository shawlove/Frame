using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Flux
{
    public class StoreCenter : Singleton<StoreCenter>, IUpdateSingleton
    {
        //factories
        private readonly HashSet<IStoreFactory> _factories = new HashSet<IStoreFactory>();

        //proxies
        private readonly Dictionary<string, StoreProxy> _stores = new Dictionary<string, StoreProxy>();

        //callback
        private readonly Dictionary<string, PriorityDelegate> _callbacks = new Dictionary<string, PriorityDelegate>();

        //storeConfigs
        private readonly Dictionary<string, StoreConfig> _storeConfigs = new Dictionary<string, StoreConfig>();

        //receive priority
        private readonly List<string> _storeReceivePriority = new List<string>();

        //callback priority
        private readonly List<string> _storeCallbackPriority = new List<string>();

        //check store dirty tool
        private readonly HashSet<StoreProxy> _dirtyStore = new HashSet<StoreProxy>();

        private bool _banRefresh;

        public async Task Middleware(IAction action)
        {
            switch (action.ActionType)
            {
                case ActionType.UNDO:
                    foreach (KeyValuePair<string, StoreProxy> pair in _stores)
                    {
                        pair.Value.Undo();
                    }

                    break;
                case ActionType.UNDOABLE:
                    foreach (KeyValuePair<string, StoreProxy> pair in _stores)
                    {
                        pair.Value.Undoable();
                    }

                    break;
            }

            foreach (string storeName in _storeReceivePriority)
            {
                if (_stores[storeName].GetStore(out IStore store))
                {
                    Task task = store.ReceiveAction(action);
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
        }

        public void SetupStoreConfig(IEnumerable<StoreConfig> configs)
        {
            foreach (StoreConfig config in configs)
            {
                string storeName = config.storeName;
                _storeConfigs[storeName] = config;
                _stores[storeName]       = new StoreProxy(storeName);
                _storeCallbackPriority.Add(storeName);
                _storeCallbackPriority.Add(storeName);
            }

            //sort by priority
            _storeReceivePriority.Sort(((s,  s1) => _storeConfigs[s].storePriority.CompareTo(_storeConfigs[s1].storePriority)));
            _storeCallbackPriority.Sort(((s, s1) => _storeConfigs[s].storeCallbackPriority.CompareTo(_storeConfigs[s1].storeCallbackPriority)));
        }

        public void RegisterUIDataFactory(IStoreFactory factory)
        {
            _factories.Add(factory);
        }

        public void UnRegisterUIDataFactory(IStoreFactory factory)
        {
            _factories.Remove(factory);
        }

        public void AddStore(string storeName)
        {
            foreach (IStoreFactory factory in _factories)
            {
                IStore data = null;
                try
                {
                    data = factory.CreateStore(storeName);
                }
                catch (Exception e)
                {
                    Debug.LogError($"create store fail ：{factory} {storeName} {e}");
                }

                if (data != null)
                {
                    _stores[storeName].AddStore(data);
                    break;
                }
            }
        }

        public void RemoveStore(string storeName)
        {
            _stores[storeName].RemoveStore();
        }

        public bool GetStore(string storeName, out IStore store)
        {
            if (_stores.TryGetValue(storeName, out StoreProxy storeProxy))
            {
                return storeProxy.GetStore(out store);
            }

            store = null;
            return false;
        }

        public bool GetStore<T>(out T store) where T : class, IStore
        {
            if (GetStore(typeof(T).Name, out IStore storeBase))
            {
                store = storeBase as T;
                return true;
            }

            store = default;
            return false;
        }

        public bool GetPrevUIData(string storeName, out IStore store)
        {
            return _stores[storeName].GetPrevStore(out store);
        }

        public bool GetPrevUIData<T>(out T store) where T : class, IStore
        {
            if (GetPrevUIData(typeof(T).Name, out IStore storeBase))
            {
                store = storeBase as T;
                return true;
            }

            store = null;
            return false;
        }

        public void AddListener<T>(Action listener, int priority = MsgPriorityDefine.NORMAL) where T : IStore
        {
            string dataName = typeof(T).Name;

            AddListener(dataName, listener, priority);
        }

        public void AddListener(string dataName, Action listener, int priority = MsgPriorityDefine.NORMAL)
        {
            if (!_callbacks.TryGetValue(dataName, out PriorityDelegate callbacks))
            {
                callbacks = new PriorityDelegate();
                _callbacks.Add(dataName, callbacks);
            }

            callbacks.AddListener(listener, priority);
        }

        public void RemoveListener<T>(Action listener)
        {
            string dataName = typeof(T).Name;
            RemoveListener(dataName, listener);
        }

        public void RemoveListener(string dataName, Action listener)
        {
            if (_callbacks.TryGetValue(dataName, out PriorityDelegate callbacks))
            {
                callbacks.RemoveListener(listener);
            }
        }

        public void ForceFresh()
        {
            CustomUpdate();
        }

        public void BanRefresh(bool isBan)
        {
            _banRefresh = isBan;
        }

        public EUpdateSingletonOrder updateOrder => EUpdateSingletonOrder.StoreCenter;

        public void CustomUpdate()
        {
            if (_banRefresh) return;

            foreach (string storeName in _storeCallbackPriority)
            {
                var proxy = _stores[storeName];
                if (proxy.GetStore(out IStore store) && store.GetDirty())
                {
                    _dirtyStore.Add(proxy);
                }
            }

            if (_dirtyStore.Count == 0) return;

            foreach (var proxy in _dirtyStore)
            {
                try
                {
                    if (proxy.GetStore(out IStore store))
                    {
                        store.BeforeClearDirty();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"UIData({proxy.storeName})BeforeClearDirty出错 ：{e}");
                }

                if (_callbacks.TryGetValue(proxy.storeName, out PriorityDelegate listeners))
                {
                    listeners.Invoke();
                }

                try
                {
                    if (proxy.GetStore(out IStore store))
                    {
                        store.ClearDirty();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"UIData({proxy.storeName})ClearDirty出错 ：{e}");
                }
            }

            _dirtyStore.Clear();
        }

        private class StoreProxy
        {
            public readonly string storeName;
            private         IStore _store;

            private          IStore        _prevStore;
            private readonly Stack<IStore> _undoStack = new Stack<IStore>();

            public StoreProxy(string storeName)
            {
                this.storeName = storeName;
            }

            public void AddStore(IStore data)
            {
                try
                {
                    _store?.DeInit();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                _store = data;

                try
                {
                    _store.Init();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            public void RemoveStore()
            {
                try
                {
                    _store?.DeInit();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                _store = null;
            }

            public void Undo()
            {
                if (_undoStack.Count <= 0) return;

                try
                {
                    _prevStore?.DeInit();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                try
                {
                    _store?.OnAbort();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                _prevStore = _store;
                _store     = _undoStack.Pop();

                try
                {
                    _store?.OnUndo(_prevStore);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            public void Undoable()
            {
                if (_store == null)
                {
                    _undoStack.Push(null);
                }
                else
                {
                    IStore newUIData = _store.Clone() as IStore;

                    try
                    {
                        newUIData?.Init();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    try
                    {
                        _store.OnUndoable();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    _undoStack.Push(newUIData);
                }
            }

            // public void OpenMainUI()
            // {
            //     foreach (IStore uiData in _undoStack)
            //     {
            //         uiData?.DeInit();
            //     }
            //
            //     _undoStack.Clear();
            //
            //     if (ConfigManager.Instance.UINameInfoConfig.dontDestroyOnGoHomeUIData.Contains(storeName) == false)
            //     {
            //         _store?.DeInit();
            //         _store = null;
            //     }
            // }

            public bool GetPrevStore(out IStore prevStore)
            {
                prevStore = _prevStore;
                return prevStore != null;
            }

            public bool GetStore(out IStore store)
            {
                store = _store;
                return store != null;
            }

            public void Receive(IAction action)
            {
                try
                {
                    _store?.ReceiveAction(action);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}