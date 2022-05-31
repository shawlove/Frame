using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Recycle
{
    public class RecycleList<T> where T : class, IRecycle, new()
    {
        private static RecycleList<T> _instance;
        private static RecycleList<T> Instance => _instance ??= new RecycleList<T>();

        private readonly List<T> _list = new List<T>();

        /// <summary>
        /// get value
        /// </summary>
        public static T Get()
        {
            return Instance.Get_Inner();
        }

        /// <summary>
        /// manual recycle
        /// </summary>
        public static void Recycle(T t)
        {
            Instance.Recycle_Inner(t);
        }

        //element that index less than _useIndex is using
        private int _useIndex = -1;

        private T Get_Inner()
        {
            if (_useIndex == _list.Count - 1)
            {
                _list.Add(new T());
            }

            T result = _list[_list.Count - 1];
            _list[_list.Count - 1] = _list[++_useIndex];
            _list[_useIndex]       = result;

            _list[_list.Count - 1].Index = _list.Count - 1;
            _list[_useIndex].Index       = _useIndex;

            return result;
        }

        private void Recycle_Inner(T t)
        {
            try
            {
                t.Reset();
                _list[t.Index]     = _list[_useIndex];
                _list[_useIndex--] = t;

                _list[t.Index].Index = t.Index;
                if (_useIndex >= 0)
                    _list[_useIndex].Index = _useIndex;
            }
            catch (Exception e)
            {
                Debug.LogError($"{t.GetType()}recycle fail, value：{t} exception：{e}");
            }
        }
    }
}