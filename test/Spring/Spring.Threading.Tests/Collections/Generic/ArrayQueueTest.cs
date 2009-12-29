using System;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.TestFixtures.Collections;
using Spring.TestFixtures.Collections.Generic;
using Spring.TestFixtures.Collections.NonGeneric;

namespace Spring.Collections.Generic
{

    /// <summary>
    /// Functional test case for <see cref="ArrayQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayQueueTest<T> : QueueTestFixture<T>
    {
        public ArrayQueueTest()
            : base(CollectionContractOptions.Fifo)
        {
        }
        protected override IQueue<T> NewQueue()
        {
            return new ArrayQueue<T>(SampleSize);
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            return new ArrayQueue<T>(SampleSize, TestData<T>.MakeTestArray(SampleSize));
        }

        [Test] public void ConstructorChokesOnNonPositiveCapacity([Values(-1, 0)] int badCapacity)
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new ArrayQueue<T>(badCapacity); });
            Assert.That(e.ActualValue, Is.EqualTo(badCapacity));
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
        }

        [Test] public void ConstructorChokesOnNullCollection()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(
                delegate { new ArrayQueue<T>(5, null); });
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [TestCase(1, 2), TestCase(5,10), TestCase(7,8)] 
        public void ConstructorChokesWhenCollectionBiggerThenCapacity(int badCapacity, int collectionSize)
        {
            var colleciton = new T[collectionSize];
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new ArrayQueue<T>(badCapacity, colleciton); });
            Assert.That(e.ActualValue, Is.EqualTo(colleciton));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void CapacityReturnsRightValue([Random(1, 10, 3)] int capacity)
        {
            Assert.That(new ArrayQueue<T>(capacity).Capacity, Is.EqualTo(capacity));
        }

        [Test] public void RemoveByElementRemovesElementAndReturnTrueWhenExists()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            int index = Samples.Length/2;
            Assert.IsTrue(queue.Contains(Samples[index]));
            Assert.IsTrue(queue.Remove(Samples[index]));
            Assert.IsFalse(queue.Contains(Samples[index]));
        }

        [Test] public void RemoveByElementReturnsFalseWhenElementDoesNotExist()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Remove(TestData<T>.MakeData(Samples.Length)));
            queue = NewQueue();
            Assert.IsFalse(queue.Remove(TestData<T>.MakeData(0)));
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsNonGeneric : TypedQueueTestFixture<T>
        {
            public AsNonGeneric()
                : base(CollectionContractOptions.Fifo)
            {
            }

            protected override IQueue NewQueue()
            {
                return new ArrayQueue<T>(_sampleSize);
            }

            protected override IQueue NewQueueFilledWithSample()
            {
                return new ArrayQueue<T>(_sampleSize, TestData<T>.MakeTestArray(_sampleSize));
            }
        }
    }
}
