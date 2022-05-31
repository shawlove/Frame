using System.Collections.Generic;

namespace GameFrame
{
    public class RepeatChecker<TValue>
    {
        private bool _isFirst;

        public RepeatChecker()
        {
            _isFirst = true;
        }

        private TValue _curValue;

        /// <summary>
        /// Returns true if the value differs from the last time , except the first time
        /// </summary>
        public bool Check(TValue value)
        {
            if (_isFirst)
            {
                _isFirst  = false;
                _curValue = value;
                return true;
            }
            else
            {
                return EqualsValue(_curValue, value) == false;
            }
        }

        private bool EqualsValue(TValue l, TValue r)
        {
            return EqualityComparer<TValue>.Default.Equals(l, r);
        }
    }
}