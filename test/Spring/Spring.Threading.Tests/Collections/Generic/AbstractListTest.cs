using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test case for <see cref="AbstractList{T}"/> class
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class AbstractListTest : AbstractListTestBase<int>
    {
        private int _theTestItem1 = 999;
        private int _theTestItem2 = 3;

        private IList<int> _backList;
        private MockList<int> _readOnlyTestee;
        private MockListMutable<int> _mutableTestee;

        private IList _readOnlyNonGeneric;
        private IList _mutableNonGeneric;

        protected override IList<int> BackList
        {
            get { return _backList; }
        }

        protected override AbstractList<int> ReadOnlyTestee
        {
            get { return _readOnlyTestee; }
        }

        protected override AbstractList<int> MutableTestee
        {
            get { return _mutableTestee; }
        }

        protected override int TheTestItem1
        {
            get { return _theTestItem1; }
        }

        protected override int MakeTestItem(int i)
        {
            return i;
        }

        [SetUp]
        public void SetUp()
        {
            _readOnlyTestee = new MockList<int>();
            _mutableTestee = new MockListMutable<int>();
            _readOnlyNonGeneric = _readOnlyTestee;
            _mutableNonGeneric = _mutableTestee;
            _backList = CollectionTestUtils.MakeTestList<int>(_size);
            _readOnlyTestee.TrueList = _backList;
            _mutableTestee.TrueList = _backList;
        }

        [Test]
        public void CheckToString()
        {
            Assert.AreEqual("MockList`1(0, 1, 2, 3, 4)", _readOnlyTestee.ToString());
        }

        #region IList Members

        [Test] public void NonGenricAdd()
        {
            TestHelper.AssertException<NotSupportedException>(
                delegate { _readOnlyNonGeneric.Add(_theTestItem1); }
                );
            Assert.AreEqual(_size, _mutableNonGeneric.Add(_theTestItem1));
            Assert.AreEqual(_theTestItem1, _mutableTestee.ParameterItem);
            Assert.IsTrue(_mutableTestee.IsInsertCalled);
        }

        [Test] public void NonGenericContains()
        {
            Assert.IsFalse(_readOnlyNonGeneric.Contains(_theTestItem1));
            Assert.IsTrue(_readOnlyNonGeneric.Contains(_theTestItem2));
            Assert.IsFalse(_readOnlyNonGeneric.Contains(new object()));
        }

        [Test] public void NonGenericIndexOf()
        {
            int expected = _backList.IndexOf(_theTestItem2);

            Assert.AreEqual(expected, _readOnlyNonGeneric.IndexOf(_theTestItem2));
            Assert.AreEqual(-1, _readOnlyNonGeneric.IndexOf(_theTestItem1));
            Assert.AreEqual(-1, _readOnlyNonGeneric.IndexOf(new object()));
        }

        [Test] public void NonGenericInsert()
        {
            TestHelper.AssertException<NotSupportedException>(
                delegate { _readOnlyNonGeneric.Insert(0, _theTestItem1); }
                );
            _mutableNonGeneric.Insert(1, _theTestItem1);
            Assert.AreEqual(_theTestItem1, _mutableTestee.ParameterItem);
            Assert.AreEqual(1, _mutableTestee.ParameterIndex);
            Assert.IsTrue(_mutableTestee.IsInsertCalled);
        }

        [Test] public void NonGenericRemove()
        {
            TestHelper.AssertException<NotSupportedException>(
                delegate { _readOnlyNonGeneric.Remove(_theTestItem1); }
                );
            _mutableTestee.ReturnBool = true;
            _mutableNonGeneric.Remove(_theTestItem1);
            Assert.AreEqual(_theTestItem1, _mutableTestee.ParameterItem);
            Assert.IsTrue(_mutableTestee.IsRemoveCalled);
        }

        [Test] public void NonGenericIndexerGet()
        {
            int index = _size/2;
            Assert.AreEqual(index, _readOnlyNonGeneric[index]);
        }

        [Test] public void NonGenericIndexerSet()
        {
            int index = _size / 2;
            TestHelper.AssertException<NotSupportedException>(
                delegate { _readOnlyNonGeneric[index] = _theTestItem1; }
                );
            _mutableNonGeneric[index] = _theTestItem1;
            Assert.AreEqual(_theTestItem1, _mutableTestee.ParameterItem);
            Assert.AreEqual(index, _mutableTestee.ParameterIndex);
            Assert.IsTrue(_mutableTestee.IsIndexerSetCalled);
        }

        [Test] public void IsFixedSize()
        {
            Assert.IsFalse(_readOnlyNonGeneric.IsFixedSize);
        }

        #endregion

        private class MockList<T> : AbstractList<T>
        {
            public IList<T> TrueList;

            public override IEnumerator<T> GetEnumerator()
            {
                return TrueList.GetEnumerator();
            }
        }

        private class MockListMutable<T> : MockList<T>
        {
            public T ParameterItem;
            public int ParameterIndex;
            public bool ReturnBool;

            public bool IsInsertCalled;
            public bool IsIndexerSetCalled;
            public bool IsRemoveCalled;

            public override void Add(T item)
            {
                ParameterItem = item;
            }

            public override void Insert(int index, T item)
            {
                ParameterItem = item;
                ParameterIndex = index;
                IsInsertCalled = true;
            }

            public override bool Remove(T item)
            {
                ParameterItem = item;
                IsRemoveCalled = true;
                return ReturnBool;
            }

            public override T this[int index]
            {
                get
                {
                    return base[index];
                }
                set
                {
                    ParameterItem = value;
                    ParameterIndex = index;
                    IsIndexerSetCalled = true;
                }
            }
        }
    }

    public abstract class AbstractListTestBase<T> : BaseAbstractCollectionTest<T>
    {
        protected int _size = 5;

        protected abstract IList<T> BackList { get; }
        protected abstract AbstractList<T> ReadOnlyTestee { get; }
        protected abstract AbstractList<T> MutableTestee { get; }

        protected sealed override ICollection<T> BackCollection
        {
            get { return BackList; }
        }

        protected sealed override AbstractCollection<T> Testee
        {
            get { return ReadOnlyTestee; }
        }

        protected abstract T MakeTestItem(int i);

        #region IList<T> Members

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void RemoveAtIsNotSupported()
        {
            ReadOnlyTestee.RemoveAt(0);
        }


        [Test, ExpectedException(typeof(NotSupportedException))]
        public void InsertIsNotSupported()
        {
            ReadOnlyTestee.Insert(0, TheTestItem1);
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void IndexerSetIsNotSupported()
        {
            ReadOnlyTestee[0] = TheTestItem1;
        }

        [Test]
        public void IndexOf()
        {
            for (int i = 0; i < _size; i++)
            {
                Assert.AreEqual(i, ReadOnlyTestee.IndexOf(MakeTestItem(i)));
            }

            Assert.AreEqual(-1, ReadOnlyTestee.IndexOf(TheTestItem1));
        }

        [Test]
        public void IndexerGet()
        {
            TestHelper.AssertException<ArgumentOutOfRangeException>(
                delegate { T o = ReadOnlyTestee[-1]; });
            TestHelper.AssertException<ArgumentOutOfRangeException>(
                delegate { T o = ReadOnlyTestee[_size]; });

            for (int i = 0; i < _size; i++)
            {
                Assert.AreEqual(i, ReadOnlyTestee[i]);
            }

        }

        #endregion

    }
}
