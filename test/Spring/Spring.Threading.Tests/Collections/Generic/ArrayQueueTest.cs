using System;
using NUnit.Framework;

namespace Spring.Collections.Generic
{

    /// <summary>
    /// Functional test case for <see cref="ArrayQueue{T}"/> as a generic
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayQueueTest<T> : QueueTestFixture<T>
    {
        public ArrayQueueTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IQueue<T> NewQueue()
        {
            return new ArrayQueue<T>(_sampleSize);
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            return new ArrayQueue<T>(_sampleSize, TestData<T>.MakeTestArray(_sampleSize));
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
            int index = _samples.Length/2;
            Assert.IsTrue(queue.Contains(_samples[index]));
            Assert.IsTrue(queue.Remove(_samples[index]));
            Assert.IsFalse(queue.Contains(_samples[index]));
        }

        [Test] public void RemoveByElementReturnsFalseWhenElementDoesNotExist()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Remove(TestData<T>.MakeData(_samples.Length)));
            queue = NewQueue();
            Assert.IsFalse(queue.Remove(TestData<T>.MakeData(0)));
        }
    }

}
