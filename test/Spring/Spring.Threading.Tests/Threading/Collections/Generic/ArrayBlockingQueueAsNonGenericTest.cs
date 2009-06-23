using NUnit.Framework;
using Spring.Collections;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for no fair <see cref="ArrayBlockingQueue{T}"/> as a non generic
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayBlockingQueueNoFairAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public ArrayBlockingQueueNoFairAsNonGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }

        protected override IQueue NewQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize);
        }

        protected override IQueue NewQueueFilledWithSample()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, false, TestData<T>.MakeTestArray(_sampleSize));
        }
    }

    /// <summary>
    /// Functional test case for no fair <see cref="ArrayBlockingQueue{T}"/> as a non generic
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayBlockingQueueFairAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public ArrayBlockingQueueFairAsNonGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }

        protected override IQueue NewQueue()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true);
        }

        protected override IQueue NewQueueFilledWithSample()
        {
            return new ArrayBlockingQueue<T>(_sampleSize, true, TestData<T>.MakeTestArray(_sampleSize));
        }
    }

}
