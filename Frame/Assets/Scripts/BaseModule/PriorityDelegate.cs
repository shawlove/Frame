using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public abstract class PriorityDelegateBase<TDelegate> where TDelegate : Delegate
    {
        protected readonly List<TDelegate> actions  = new List<TDelegate>();
        protected readonly List<int>       priority = new List<int>();
        protected          int             count    = 0;

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
            else if (actions.Contains(rCallback))
            {
                Debug.LogWarning($"Do not register delegate callbacks twice => {rCallback.Target.GetType().Name}.{rCallback.Method.Name}");
                return;
            }

            int insertIndex = 0;
            while (insertIndex < count)
            {
                if (priority[insertIndex] > sPriority)
                {
                    break;
                }

                insertIndex++;
            }

            actions.Insert(insertIndex, rCallback);
            priority.Insert(insertIndex, sPriority);
            count++;
        }

        public void RemoveListener(TDelegate rCallback)
        {
            for (int i = 0; i < count; i++)
            {
                if (actions[i].Equals(rCallback))
                {
                    actions.RemoveAt(i);
                    priority.RemoveAt(i);
                    count--;
                    break;
                }
            }
        }

        public void RemoveAll()
        {
            actions.Clear();
            priority.Clear();
            count = 0;
        }
    }

    public class PriorityDelegate : PriorityDelegateBase<Action>
    {
        public void Invoke()
        {
            for (int i = 0; i < count; i++)
            {
                try
                {
                    actions[i].Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {actions[i].Target.GetType().Name}.{actions[i].Method.Name} : {e}");
                }
            }
        }
    }

    public class PriorityDelegate<T> : PriorityDelegateBase<Action<T>>
    {
        public void Invoke(T param)
        {
            for (int i = 0; i < count; i++)
            {
                try
                {
                    actions[i].Invoke(param);
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {actions[i].Target.GetType().Name}.{actions[i].Method.Name} : {e}");
                }
            }
        }
    }

    public class PriorityDelegate<T1, T2> : PriorityDelegateBase<Action<T1, T2>>
    {
        public void Invoke(T1 param1, T2 param2)
        {
            for (int i = 0; i < count; i++)
            {
                try
                {
                    actions[i].Invoke(param1, param2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"PriorityDelegate invoke fail => {actions[i].Target.GetType().Name}.{actions[i].Method.Name} : {e}");
                }
            }
        }
    }
}