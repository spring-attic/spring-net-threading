using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="AbstractEnumerator{T}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class AbstractEnumeratorTest
    {
        private const string _errorMessage = "Enumeration has either not started or has already finished.";

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void ResetIsNotSupported()
        {
            new MockEnumerator<string>().Reset();
        }

        [Test]
        public void DisposeIsImplemented()
        {
            new MockEnumerator<string>().Dispose();
        }

        [Test]
        public void GenericCurrentBeforeStartReferenceType()
        {
            MockEnumerator<string> e = GetEnumeratorBeforeStart<string>();
            Assert.IsNull(e.Current);
        }

        public void GenericCurrentBeforeStartValueType()
        {
            MockEnumerator<int> e = GetEnumeratorBeforeStart<int>();
            Assert.AreEqual(0, e.Current);
        }

        [Test]
        public void ChokesOnNonGenericCurrentBeforeStartReferenceType()
        {
            IEnumerator e = GetEnumeratorBeforeStart<string>();
            TestHelper.AssertException<InvalidOperationException>(
                delegate { object o = e.Current; },
                MessageMatch.Exact, _errorMessage);
        }

        [Test]
        public void ChokesOnNonGenericCurrentBeforeStartValueType()
        {
            IEnumerator e = GetEnumeratorBeforeStart<int>();
            TestHelper.AssertException<InvalidOperationException>(
                delegate { object o = e.Current; },
                MessageMatch.Exact, _errorMessage);
        }

        [Test]
        public void GenericCurrentAfterFinishedReferenceType()
        {
            MockEnumerator<string> e = GetEnumeratorAfterFinished<string>();
            Assert.IsNull(e.Current);
        }

        public void GenericCurrentAfterFinishedValueType()
        {
            MockEnumerator<int> e = GetEnumeratorAfterFinished<int>();
            Assert.AreEqual(0, e.Current);
        }

        [Test]
        public void ChokesOnNonGenericCurrentAfterFinishedReferenceType()
        {
            IEnumerator e = GetEnumeratorAfterFinished<string>();
            TestHelper.AssertException<InvalidOperationException>(
                delegate { object o = e.Current; },
                MessageMatch.Exact, _errorMessage);
        }

        [Test]
        public void ChokesOnNonGenericCurrentAfterFinishedValueType()
        {
            IEnumerator e = GetEnumeratorAfterFinished<int>();
            TestHelper.AssertException<InvalidOperationException>(
                delegate { object o = e.Current; },
                MessageMatch.Exact, _errorMessage);
        }

        [Test]
        public void GenericCurrentReferenceType()
        {
            string s = "hello";
            MockEnumerator<string> es = GetEnumeratorInProgress(s);
            es.MoveNext();
            Assert.AreSame(s, es.Current);
        }

        [Test]
        public void GenericCurrentValueType()
        {
            int i = 123;
            MockEnumerator<int> ei = GetEnumeratorInProgress(i);
            ei.MoveNext();
            Assert.AreEqual(i, ei.Current);
        }


        [Test]
        public void IEnumeratorCurrentReferenceType()
        {
            string s = "hello";
            MockEnumerator<string> es = GetEnumeratorInProgress(s);
            es.MoveNext();
            Assert.AreSame(s, ((IEnumerator)es).Current);
        }


        [Test]
        public void IEnumeratorCurrentValueType()
        {
            int i = 123;
            MockEnumerator<int> ei = GetEnumeratorInProgress(i);
            ei.MoveNext();
            Assert.AreEqual(i, ((IEnumerator)ei).Current);
        }

        [Test]
        public void IsAlsoEnumerable()
        {
            MockEnumerator<string> e = new MockEnumerator<string>();
            IEnumerable<string> genericEnumerable = e;
            IEnumerable nonGenericEnumerable = e;

            Assert.AreSame(e, genericEnumerable.GetEnumerator());
            Assert.AreSame(e, nonGenericEnumerable.GetEnumerator());
        }

        private MockEnumerator<T> GetEnumeratorBeforeStart<T>()
        {
            return new MockEnumerator<T>();
        }

        private MockEnumerator<T> GetEnumeratorInProgress<T>(T element)
        {
            MockEnumerator<T> e = GetEnumeratorBeforeStart<T>();
            e.ToBeReturn = element;
            e.HasNext = true;
            return e;
        }

        private MockEnumerator<T> GetEnumeratorAfterFinished<T>()
        {
            MockEnumerator<T> e = GetEnumeratorInProgress<T>(default(T));
            e.MoveNext();
            e.HasNext = false;
            e.MoveNext();
            return e;
        }

        internal class MockEnumerator<T> : AbstractEnumerator<T>
        {
            public T ToBeReturn;
            public bool HasNext;

            protected override T FetchCurrent()
            {
                return ToBeReturn;
            }

            protected override bool GoNext()
            {
                return HasNext;
            }
        }

        internal class SimpleEnumerator<T> : AbstractEnumerator<T>
        {
            private int _index = -1;
            private readonly IList<T> _list;

            public SimpleEnumerator(IList<T> list)
            {
                this._list = list;
            }

            protected override bool GoNext()
            {
                return ++_index < _list.Count;
            }

            protected override T FetchCurrent()
            {
                return _list[_index];
            }
        }

        [TestFixture] public class ValueTypeFunctionTest : EnumeratorFunctionTestBase<int>
        {
            private IEnumerator<int> _testee;
            private IList<int> _list;

            protected override IEnumerator<int> Testee
            {
                get { return _testee; }
            }

            [SetUp]
            public void SetUp()
            {
                _list = CollectionTestUtils.MakeTestArray<int>(10);
                _testee = new SimpleEnumerator<int>(_list);
            }

            protected override IEnumerator<int> GetExpectedEnumerator()
            {
                return _list.GetEnumerator();
            }

        }

        [TestFixture]
        public class ReferenceTypeFunctionTest : EnumeratorFunctionTestBase<string>
        {
            private IEnumerator<string> _testee;
            private IList<string> _list;

            protected override IEnumerator<string> Testee
            {
                get { return _testee; }
            }

            [SetUp]
            public void SetUp()
            {
                _list = CollectionTestUtils.MakeTestArray<string>(10);
                _testee = new SimpleEnumerator<string>(_list);
            }

            protected override IEnumerator<string> GetExpectedEnumerator()
            {
                return _list.GetEnumerator();
            }

        }
    
    }

    
}
