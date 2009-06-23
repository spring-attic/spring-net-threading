using NUnit.Framework;
using Spring.Collections.Generic;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for no fair <see cref="ArrayBlockingQueue{T}"/> as a generic
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayBlockingNoFairQueueAsGenericTest<T> : QueueTestFixture<T>
    {
        public ArrayBlockingNoFairQueueAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IQueue<T> NewQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize);
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, false, TestData<T>.MakeTestArray(_sampleSize));
        }

    }

    /// <summary>
    /// Functional test case for fair <see cref="ArrayBlockingQueue{T}"/> as a generic
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayBlockingFairQueueAsGenericTest<T> : QueueTestFixture<T>
    {
        public ArrayBlockingFairQueueAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IQueue<T> NewQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true);
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true, TestData<T>.MakeTestArray(_sampleSize));
        }

    }
}
