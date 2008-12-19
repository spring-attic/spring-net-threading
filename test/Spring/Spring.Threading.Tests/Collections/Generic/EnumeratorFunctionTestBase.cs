using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Base unit test that tests the core functionality of the implementation
    /// of <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Below are a few examples.
    /// <example language="c#">
    ///    [TestFixture]
    ///    public class IntArrayTest : EnumeratorFunctionTestBase&lt;int>
    ///    {
    ///        private IEnumerator&lt;int> _testee;
    ///
    ///        private IList&lt;int> _list;
    ///
    ///        protected override IEnumerator&lt;int> Testee { get { return _testee; } }
    ///
    ///        [SetUp]
    ///        public void SetUp()
    ///        {
    ///            _list = new int[10];
    ///            for(int i=0; i&lt;10; i++) _list[i] = i;
    ///            _testee = _list.GetEnumerator();
    ///        }
    ///
    ///        protected override IEnumerator&lt;int> GetExpectedEnumerator()
    ///        {
    ///            return _list.GetEnumerator();
    ///        }
    ///    }
    /// </example>
    /// 
    /// <example language="c#">
    ///    [TestFixture]
    ///    public class ListTest : EnumeratorFunctionTestBase&lt;string>
    ///    {
    ///        private IEnumerator&lt;string> _testee;
    ///
    ///        private IList&lt;string> _list;
    ///
    ///        protected override IEnumerator&lt;string> Testee { get { return _testee; } }
    ///
    ///        [SetUp]
    ///        public void SetUp()
    ///        {
    ///            _list = new List&lt;string>(10);
    ///            for (int i = 0; i &lt; 10; i++) _list.Add(i.ToString());
    ///            _testee = _list.GetEnumerator();
    ///        }
    ///
    ///        protected override IEnumerator&lt;string> GetExpectedEnumerator()
    ///        {
    ///            return _list.GetEnumerator();
    ///        }
    ///
    ///        protected override bool ConcurrentUpdate()
    ///        {
    ///            _list[0] = "test string";
    ///            return true;
    ///        }
    ///
    ///        protected override bool ConcurrentAdd()
    ///        {
    ///            _list.Add("test string");
    ///            return true;
    ///        }
    ///
    ///        protected override bool ConcurrentRemove()
    ///        {
    ///            _list.RemoveAt(0);
    ///            return true;
    ///        }
    ///    }
    /// </example>
    /// <example language="c#">
    /// </example>
    /// </para>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    public abstract class EnumeratorFunctionTestBase<T>
    {
        protected abstract IEnumerator<T> Testee { get; }

        private IEnumerator TesteeAsNonGeneric
        {
            get { return Testee; }
        }

        //[Test, ExpectedException(typeof(InvalidOperationException))]
        public void CurrentBeforeMoveNext()
        {
            // The behavior was not specified in the API document and is not
            // consistent within .Net Framework Classes. i.e, enumerator from
            // int[] throws InvalidOperationException but List<int> doesn't.
            T t = Testee.Current;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ChokesOnNonGenericCurrentBeforeMoveNext()
        {
            object o = TesteeAsNonGeneric.Current;
        }

        //[Test, ExpectedException(typeof(InvalidOperationException))]
        public void CurrentAfterMoveNextFalse()
        {
            // The behavior was not specified in the API document and is not
            // consistent within .Net Framework Classes. i.e, enumerator from
            // int[] throws InvalidOperationException but List<int> doesn't.
            IEnumerator<T> testee = Testee;
            while (testee.MoveNext());
            object o = testee.Current;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ChokesOnNonGenericCurrentAfterMoveNextFalse()
        {
            IEnumerator testeeAsNonGeneric = TesteeAsNonGeneric;
            while (testeeAsNonGeneric.MoveNext());
            object o = testeeAsNonGeneric.Current;
        }

        [Test]
        public void ChokesOnConcurrentUpdate()
        {
            IEnumerator<T> testee = Testee;
            if (ConcurrentUpdate()) ConcurrentModification(testee);
        }

        [Test]
        public void ChokesOnConcurrentAdd()
        {
            IEnumerator<T> testee = Testee;
            if (ConcurrentAdd()) ConcurrentModification(testee);
        }

        [Test]
        public void ChokesOnConcurrentRemove()
        {
            IEnumerator<T> testee = Testee;
            if (ConcurrentRemove()) ConcurrentModification(testee);
        }

        private void ConcurrentModification(IEnumerator<T> testee)
        {
            TestHelper.AssertException<InvalidOperationException>(
                delegate
                {
                    while (testee.MoveNext())
                    {
                        T t = testee.Current;
                    }
                });
        }

        [Test]
        public void IterateOnce()
        {
            TestHelper.AssertEnumeratorEquals(GetExpectedEnumerator(), Testee);
        }

        [Test]
        public void IterateOnceGeneric()
        {
            IEnumerator<T> expected = GetExpectedEnumerator();
            IEnumerator<T> testee = Testee;
            while (expected.MoveNext())
            {
                Assert.IsTrue(testee.MoveNext(), "actual has too less elements.");
                Assert.AreEqual(expected.Current, testee.Current);
            }
            Assert.IsFalse(testee.MoveNext(), "actual has too many elements.");
            testee.Dispose();
        }


        [Test]
        public void IterateResetIterate()
        {
            IEnumerator<T> testee = Testee;
            TestHelper.AssertEnumeratorEquals(GetExpectedEnumerator(), testee);

            try
            {
                testee.Reset();
            }
            catch (NotSupportedException)
            {
                return; // fine, not supported.
            }

            TestHelper.AssertEnumeratorEquals(GetExpectedEnumerator(), testee);
        }

        protected abstract IEnumerator<T> GetExpectedEnumerator();

        protected virtual bool ConcurrentUpdate()
        {
            return false;
        }

        protected virtual bool ConcurrentAdd()
        {
            return false;
        }

        protected virtual bool ConcurrentRemove()
        {
            return false;
        }

    }
}
