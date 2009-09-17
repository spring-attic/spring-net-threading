using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.TestFixture.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class EnumeratorTestFixture<T> : ThreadingTestFixture
    {
        private int _antiHangingLimit = 512;
        protected int AntiHangingLimit
        {
            get { return _antiHangingLimit; }
            set { _antiHangingLimit = value; }
        }

        protected abstract IEnumerator<T> NewEnumerator();

        [Test]
        public void IteratingThroughEnumeratorOnce()
        {
            Iterate(NewEnumerator());
        }

        [Test]
        public void IterateEnumeratorResetAndIterateAgain()
        {
            IEnumerator<T> e = NewEnumerator();
            int count = Iterate(e);
            try
            {
                e.Reset();
            }
            catch (NotSupportedException)
            {
                return;
            }
            Assert.That(Iterate(e), Is.EqualTo(count));

        }

        private int Iterate(IEnumerator<T> enumerator)
        {
            int count = 0;
            while (enumerator.MoveNext())
            {
#pragma warning disable 168
                T value = enumerator.Current;
#pragma warning restore 168
                if (++count >= _antiHangingLimit)
                {
                    Assert.Fail("Endless enumerator? reached the {0} iteration limit set by AntiHangingLimit property.", _antiHangingLimit);
                }
            }
            return count;
        }
    }
}