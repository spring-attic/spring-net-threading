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
    public class ArrayBlockingNoFairQueueAsGenericTest<T> : BlockingQueueTestFixture<T>
    {
        public ArrayBlockingNoFairQueueAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize);
        }
        protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
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
    public class ArrayBlockingFairQueueAsGenericTest<T> : BlockingQueueTestFixture<T>
    {
        public ArrayBlockingFairQueueAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
            _isFair = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true);
        }
        protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true, TestData<T>.MakeTestArray(_sampleSize));
        }

    }
}
