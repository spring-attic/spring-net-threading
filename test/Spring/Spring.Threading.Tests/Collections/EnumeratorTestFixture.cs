using System;
using System.Collections;
using NUnit.Framework;

namespace Spring.Collections
{
    /* Example usage of EnumeratorTestFixture
 
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ListEnumeratorTest<T> : EnumeratorTestFixture
        {
            protected override IEnumerator NewEnumerator()
            {
                return new ArrayList(TestData<T>.MakeTestArray(55)).GetEnumerator();
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ArrayEnumeratorTest<T> : EnumeratorTestFixture
        {
            [SetUp] public void SetUp()
            {
                AntiHangingLimit = 600;
            }
            protected override IEnumerator NewEnumerator()
            {
                return TestData<T>.MakeTestArray(555).GetEnumerator();
            }
        }

     */

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
            object value;
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
