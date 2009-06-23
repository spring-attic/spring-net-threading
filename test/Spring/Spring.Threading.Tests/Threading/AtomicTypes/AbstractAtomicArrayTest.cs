using System;
using System.Collections;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.AtomicTypes
{
    /// <summary>
    /// Test cases for <see cref="AbstractAtomicArray{T}"/>
    /// </summary>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AbstractAtomicArrayTest<T>
    {
        private MockRepository _mockery;
        private AbstractAtomicArray<T> _aaa;
        private IList _asList;

        [SetUp] public void SetUp()
        {
            _mockery = new MockRepository();
            _aaa = _mockery.PartialMock<AbstractAtomicArray<T>>();
            _asList = _aaa;
        }

        [Test] public void IsReadOnlyReturnFalse()
        {
            _mockery.ReplayAll();
            Assert.IsFalse(_aaa.IsReadOnly);
        }

        [Test] public void IsSynchronizedReturnTrue()
        {
            _mockery.ReplayAll();
            Assert.IsTrue(_asList.IsSynchronized);
        }

        [Test] public void IsFixedSizeReturnTrue()
        {
            _mockery.ReplayAll();
            Assert.IsTrue(_asList.IsFixedSize);
        }

        [Test] public void GetEnumeratorBasedOnCountAndIndexer()
        {
            const int count = 5;
            T[] expected = new T[5];
            SetupResult.For(_aaa.Count).Return(5);
            for (int i = 0; i < count; i++)
            {
                expected[i] = (T) Convert.ChangeType(i, typeof (T));
                SetupResult.For(_aaa[i]).Return(expected[i]);
            }
            _mockery.ReplayAll();
            TestHelper.AssertEnumeratorEquals(
                expected.GetEnumerator(), _aaa.GetEnumerator());
        }
    }
}
