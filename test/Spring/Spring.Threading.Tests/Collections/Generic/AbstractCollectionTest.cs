using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test case for <see cref="AbstractCollection{T}"/> class
    /// </summary>
    /// <author>Kenneth Xu</author>
    //TODO:Ken Fix this test case and move the AddRange tests from AbstractQueueTest to here
    [TestFixture(typeof(string))] // reference type
    [TestFixture(typeof(int))] // value type
    public class AbstractCollectionTest<T> : BaseAbstractCollectionTest<T>
    {
        private MockCollection mock;
        private ICollection<T> _backCollection;
        private int _size = 5;
        private T _theTestItem1 = TestData<T>.MakeData(999);

        protected override ICollection<T> BackCollection
        {
            get { return _backCollection; }
        }

        protected override AbstractCollection<T> Testee
        {
            get { return mock; }
        }

        protected override T TheTestItem1
        {
            get { return _theTestItem1; }
        }

        [SetUp]
        public void SetUp()
        {
            _backCollection = CollectionTestUtils.MakeTestCollection<T>(_size);
            mock = new MockCollection();
            mock.TrueCollection = _backCollection;
        }

        [Test] public void CheckToString()
        {
            Spring.TestFixture.Collections.Generic.CollectionTestFixture<T>.ToStringContainsToStringOfElements(mock);
            //Assert.AreEqual("MockCollection`1(0, 1, 2, 3, 4)", mock.ToString());
        }

        private class MockCollection : AbstractCollection<T>
        {
            public ICollection<T> TrueCollection;

            public override IEnumerator<T> GetEnumerator()
            {
                return TrueCollection.GetEnumerator();
            }
        }
    }
}