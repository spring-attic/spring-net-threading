using System.Collections.Generic;

namespace Spring.Collections.Generic
{
    class TestEnumerator : IEnumerator<int>
    {
        public int Value = -1;
        public bool IsDisposed;
        private int _max;

        public TestEnumerator(int max)
        {
            _max = max;
        }

        #region IEnumerator<int> Members

        public int Current
        {
            get { return Value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            IsDisposed = true;
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return ++Value < _max;
        }

        public void Reset()
        {
            Value = -1;
        }

        #endregion
    }
}
