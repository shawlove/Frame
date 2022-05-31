using System.Collections.Generic;

namespace GameFrame.Flux
{
    public class StoreCenter : Singleton<StoreCenter>
    {
        //factories
        private readonly HashSet<IStoreFactory> _factories = new HashSet<IStoreFactory>();

        //proxies
        private readonly Dictionary<string, StoreProxy> _uiData = new Dictionary<string, StoreProxy>();

        //callback
        private readonly Dictionary<string, PriorityDelegate> _uiCallbacks = new Dictionary<string, PriorityDelegate>();

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

            public void AddData(IStore data)
            {
                _store?.DeInit();
                _store = data;
                _store.Init();
            }

            public void RemoveData()
            {
                _store?.DeInit();
                _store = null;
            }

            public void Undo()
            {
                if (_undoStack.Count <= 0) return;

                _prevStore?.DeInit();
                _store?.OnAbort();
                _prevStore = _store;
                _store     = _undoStack.Pop();
                _store?.OnUndo(_prevStore);
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
                    newUIData?.Init();
                    _store.OnUndoable();
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

            public bool GetPrevData(out IStore prevData)
            {
                prevData = _prevStore;
                return prevData != null;
            }

            public bool GetData(out IStore data)
            {
                data = _store;
                return data != null;
            }

            public void Receive(IAction action)
            {
                _store?.ReceiveAction(action);
            }
        }
    }
}