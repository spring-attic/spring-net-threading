using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test case for <see cref="AbstractCollection{T}"/> class
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class AbstractCollectionTest : BaseAbstractCollectionTest<int>
    {
        private MockCollection<int> mock;
        private ICollection<int> _backCollection;
        private int _size = 5;
        private int _theTestItem1 = 999;

        protected override ICollection<int> BackCollection
        {
            get { return _backCollection; }
        }

        protected override AbstractCollection<int> Testee
        {
            get { return mock; }
        }

        protected override int TheTestItem1
        {
            get { return _theTestItem1; }
        }

        [SetUp]
        public void SetUp()
        {
            _backCollection = CollectionTestUtils.MakeTestCollection<int>(_size);
            mock = new MockCollection<int>();
            mock.TrueCollection = _backCollection;
        }

        [Test] public void CheckToString()
        {
            Assert.AreEqual("MockCollection`1(0, 1, 2, 3, 4)", mock.ToString());
        }

        private class MockCollection<T> : AbstractCollection<T>
        {
            public ICollection<T> TrueCollection;

            public override IEnumerator<T> GetEnumerator()
            {
                return TrueCollection.GetEnumerator();
            }
        }
    }
}