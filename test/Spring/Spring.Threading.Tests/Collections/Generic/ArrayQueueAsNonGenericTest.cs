using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Functional test case for <see cref="ArrayQueue{T}"/> as a non generic
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayQueueAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public ArrayQueueAsNonGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
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
