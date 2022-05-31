using System;

namespace GameFrame.UI
{
    public class UITag<T1>
    {
        private readonly RepeatChecker<T1> _checker1 = new RepeatChecker<T1>();

        private readonly Action<T1> _callback;

        public UITag(Action<T1> callback)
        {
            _callback = callback;
        }

        public void Set(T1 value1)
        {
            bool flag = _checker1.Check(value1);

            if (flag)
                _callback(value1);
        }
    }

    public class UITag<T1, T2>
    {
        private readonly RepeatChecker<T1> _checker1 = new RepeatChecker<T1>();
        private readonly RepeatChecker<T2> _checker2 = new RepeatChecker<T2>();

        private readonly Action<T1, T2> _callback;

        public UITag(Action<T1, T2> callback)
        {
            _callback = callback;
        }

        public void Set(T1 value1, T2 value2)
        {
            bool flag = _checker1.Check(value1);
            flag = _checker2.Check(value2) || flag;

            if (flag)
                _callback(value1, value2);
        }
    }
}