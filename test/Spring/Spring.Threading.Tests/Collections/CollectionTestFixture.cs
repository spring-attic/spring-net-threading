using System;
using System.Collections;
using NUnit.Framework;

namespace Spring.Collections
{

    /*

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ListAsCollectionTest<T> : CollectionTestFixture
    {
        protected override ICollection NewCollection()
        {
            return new System.Collections.Generic.List<T>(TestData<T>.MakeTestArray(55));
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayListAsCollectionTest<T> : CollectionTestFixture
    {
        protected override ICollection NewCollection()
        {
            return new ArrayList(TestData<T>.MakeTestArray(55));
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayAsCollectionTest<T> : CollectionTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            AntiHangingLimit = 600;
        }
        protected override ICollection NewCollection()
        {
            return TestData<T>.MakeTestArray(555);
        }
    }

     */

    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="ICollection"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class CollectionTestFixture : EnumerableTestFixture
    {
        protected override sealed IEnumerable NewEnumerable()
        {
            return NewCollection();
        }

        protected abstract ICollection NewCollection();

        [Test] public void CopyToChokesWithNullArray()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                NewCollection().CopyTo(null, 0);
            });
        }

        [Test] public void CopyToChokesOnMultiDimensionArray()
        {
            ICollection c = NewCollection();
            Assert.Throws<ArgumentException>(delegate
            {
                c.CopyTo(new object[c.Count, 2], 0);
            });
        }


        [Test] public void CopyToChokesWithNegativeIndex()
        {
            ICollection c = NewCollection();
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                c.CopyTo(new object[0], -1);
            });

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                c.CopyTo(NewArray<object>(1, c.Count), 0);
            });
        }

        [Test] public void CopyToChokesWhenArrayIsTooSmallToHold()
        {
            ICollection c = NewCollection();
            Assert.Throws<ArgumentException>(delegate
            {
                c.CopyTo(new object[c.Count -1], 0);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                c.CopyTo(NewArray<object>(1, c.Count-1), 1);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                c.CopyTo(NewArray<object>(1, c.Count), 2);
            });
        }

        [Test] public void CopyToZeroLowerBoundArray()
        {
            ICollection c = NewCollection();
            object[] target = new object[c.Count];
            c.CopyTo(target, 0);
            CollectionAssert.AreEqual(target, c);
        }

        [Test] public void CopyToArbitraryLowerBoundArray()
        {
            ICollection c = NewCollection();
            Array target = NewArray<object>(1, c.Count);
            c.CopyTo(target, 1);
            CollectionAssert.AreEqual(target, c);
        }

        private static Array NewArray<T>(int from, int to)
        {
            return Array.CreateInstance(
                typeof(T), // Array type
                new int[] { to - from + 1 }, // Size
                new int[] { from }); // lower bound
        }

    }

}
