using System;
using System.Collections;
using NUnit.Framework;

namespace Spring.TestFixtures.Collections.NonGeneric
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IEnumerator"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class EnumeratorTestFixture
    {
        private int _antiHangingLimit = 512;
        protected int AntiHangingLimit
        {
            get { return _antiHangingLimit; }
            set { _antiHangingLimit = value; }
        }

        protected abstract IEnumerator NewEnumerator();

        [Test] public void IteratingThroughEnumeratorOnce()
        {
            Iterate(NewEnumerator());
        }

        [Test] public void IterateEnumeratorResetAndIterateAgain()
        {
            IEnumerator e = NewEnumerator();
            int count = Iterate(e);
            try
            {
                e.Reset();
            }
            catch(NotSupportedException)
            {
                return;
            }
            Assert.That(Iterate(e), Is.EqualTo(count));
        }

        private int Iterate(IEnumerator enumerator)
        {
            int count = 0;
#pragma warning disable 219
            object value;
#pragma warning restore 219
            Assert.Throws<InvalidOperationException>(delegate { value = enumerator.Current; });
            while(enumerator.MoveNext())
            {
                value = enumerator.Current;
                if (++count >= _antiHangingLimit)
                {
                    Assert.Fail("Endless enumerator? reached the {0} iteration limit set by AntiHangingLimit property.", _antiHangingLimit);
                }
            }
            Assert.Throws<InvalidOperationException>(delegate { value = enumerator.Current; });
            return count;
        }
    }
}