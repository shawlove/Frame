using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public abstract class PriorityDelegateBase<TDelegate> where TDelegate : MulticastDelegate
    {
        private readonly List<TDelegate> _actions  = new List<TDelegate>();
        private readonly List<int>       _priority = new List<int>();
        private          int             _count    = 0;

        private readonly List<TDelegate> _invokeList = new List<TDelegate>();

        ~PriorityDelegateBase()
        {
            RemoveAll();
        }

        /// <summary>
        /// A smaller number indicates a higher priority
        /// </summary>
        public void AddListener(TDelegate rCallback, int sPriority)
        {
            if (rCallback == null)
            {
                Debug.LogWarning("Do not register empty delegate callbacks");
                return;
            }
            else if (_actions.Contains(rCallback))
            {
                Debug.LogWarning($"Do not register delegate callbacks twice => {rCallback.Target.GetType().Name}.{rCallback.Method.Name}");
                return;
            }

            int insertIndex = 0;
            while (insertIndex < _count)
            {
                if (_priority[insertIndex] > sPriority)
                {
                    break;
                }

                insertIndex++;
            }

            _actions.Insert(insertIndex, rCallback);
            _priority.Insert(insertIndex, sPriority);
            _count++;
        }

        public void RemoveListener(TDelegate rCallback)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_actions[i].Equals(rCallback))
                {
                    _actions.RemoveAt(i);
                    _priority.RemoveAt(i);
                    _count--;
                    break;
                }
            }
        }

        public void RemoveAll()
        {
            _actions.Clear();
            _priority.Clear();
            _count = 0;
        }

        protected IEnumerable<TDelegate> GetInvokeList()
        {
            _invokeList.Clear();
            _invokeList.AddRange(_actions);
            return _invokeList;
        }
    }

    public class PriorityDelegate : PriorityDelegateBase<Action>
    {
        public void Invoke()
        {
            foreach (Action action in GetInvokeList())
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {action.Target.GetType().Name}.{action.Method.Name} : {e}");
                }
            }
        }
    }

    public class PriorityDelegate<T> : PriorityDelegateBase<Action<T>>
    {
        public void Invoke(T param)
        {
            foreach (Action<T> action in GetInvokeList())
            {
                try
                {
                    action.Invoke(param);
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {action.Target.GetType().Name}.{action.Method.Name} : {e}");
                }
            }
        }
    }

    public class PriorityDelegate<T1, T2> : PriorityDelegateBase<Action<T1, T2>>
    {
        public void Invoke(T1 param1, T2 param2)
        {
            foreach (Action<T1, T2> action in GetInvokeList())
            {
                try
                {
                    action.Invoke(param1, param2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {action.Target.GetType().Name}.{action.Method.Name} : {e}");
                }
            }
        }
    }
}