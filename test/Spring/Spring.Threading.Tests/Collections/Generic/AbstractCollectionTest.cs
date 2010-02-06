using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.Generic;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test case for <see cref="AbstractCollection{T}"/> class
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))] // reference type
    [TestFixture(typeof(int))] // value type
    public class AbstractCollectionTest<T>
    {
        private AbstractCollection<T> _sut;

        [SetUp] public void SetUp()
        {
            _sut = Mockery.GeneratePartialMock<AbstractCollection<T>>();
        }

        [Test] public void IsSynchronizedAlwaysReturnFalse()
        {
            Assert.IsFalse(((ICollection)_sut).IsSynchronized);
        }

        [Test] public void SyncRootAlwaysReturnNull()
        {
            Assert.That(((ICollection)_sut).SyncRoot, Is.Null);
        }

        [Test] public void AddRangeAddAllElementsInTraversalOrder()
        {
            T[] testData = TestData<T>.MakeTestArray(5);
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything));
            Assert.That(_sut.AddRange(testData), Is.True);
            Activities last = null;
            foreach (T data in testData)
            {
                T element = data;
                var call = _sut.ActivityOf(x => x.Add(element));
                if (last != null) Mockery.Assert(last < call);
                last = call;
            }
        }

        [Test] public void AddRangeWelcomesNullElements()
        {
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything));
            _sut.AddRange(new T[5]);
        }

        [Test] public void AddRangeReturnsFalseWhenParameterIsEmptyCollection()
        {
            T[] testData = new T[0];
            Assert.That(_sut.AddRange(testData), Is.False);
        }

        [Test] public void AddRangeChokesOnNullParameter()
        {
            var e = Assert.Throws<ArgumentNullException>(() => _sut.AddRange(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeChokesWhenAddItself()
        {
            var e = Assert.Throws<ArgumentException>(() => _sut.AddRange(_sut));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeChokesWhenNotEnoughRoom()
        {
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything)).Repeat.Once();
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything)).Throw(new InvalidOperationException()).Repeat.Once();

            Assert.Throws<InvalidOperationException>(
                () => _sut.AddRange(TestData<T>.MakeTestArray(2)));
        }

        [TestFixture(typeof(string))] // reference type
        [TestFixture(typeof(int))] // value type
        public class AsGenericReadOnly : CollectionContract<T>
        {
            public AsGenericReadOnly() : base(
                CollectionContractOptions.ReadOnly | 
                CollectionContractOptions.ToStringPrintItems)
            {
            }

            protected override ICollection<T> NewCollectionFilledWithSample()
            {
                return new ReadOnlyMockCollection
                {
                    TrueCollection = TestData<T>.MakeTestArray(SampleSize)
                };
            }

            protected override ICollection<T> NewCollection()
            {
                return new ReadOnlyMockCollection
                {
                    TrueCollection = new T[0]
                };
            }
        }

        [TestFixture(typeof(string))] // reference type
        [TestFixture(typeof(int))] // value type
        public class AsGeneric : CollectionContract<T>
        {
            public AsGeneric() : base( CollectionContractOptions.ToStringPrintItems)
            {
            }

            protected override ICollection<T> NewCollection()
            {
                return new MockCollection();
            }
        }

        [TestFixture(typeof(string))] // reference type
        [TestFixture(typeof(int))] // value type
        public class AsNonGenericReadOnly : CollectionContract
        {
            public AsNonGenericReadOnly() : base(
                CollectionContractOptions.ReadOnly | 
                CollectionContractOptions.ToStringPrintItems)
            {
            }

            protected override ICollection NewCollection()
            {
                return new ReadOnlyMockCollection
                {
                    TrueCollection = TestData<T>.MakeTestArray(9)
                };
            }
        }

        [TestFixture(typeof(string))] // reference type
        [TestFixture(typeof(int))] // value type
        public class AsNonGeneric : CollectionContract
        {
            public AsNonGeneric() : base(CollectionContractOptions.ToStringPrintItems)
            {
            }

            protected override ICollection NewCollection()
            {
                return new MockCollection();
            }
        }

        private class ReadOnlyMockCollection : AbstractCollection<T>
        {
            public ICollection<T> TrueCollection;

            public override IEnumerator<T> GetEnumerator()
            {
                return TrueCollection.GetEnumerator();
            }
        }

        private class MockCollection : ReadOnlyMockCollection
        {
            public MockCollection()
            {
                TrueCollection = new List<T>();
            }
            public override void Add(T item)
            {
                TrueCollection.Add(item);
            }

            public override void Clear()
            {
                TrueCollection.Clear();
            }

            public override bool Remove(T item)
            {
                return TrueCollection.Remove(item);
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
        }
    }
}