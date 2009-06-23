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
    public class LinkedBlockingQueueBoundAsGenericTest<T> : QueueTestFixture<T>
    {
        public LinkedBlockingQueueBoundAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IQueue<T> NewQueue()
        {
            return new LinkedBlockingQueue<T>(_sampleSize);
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            var sut = new LinkedBlockingQueue<T>(_sampleSize);
            sut.AddRange(TestData<T>.MakeTestArray(_sampleSize));
            return sut;
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
    public class LinkedBlockingQueueUnboundAsGenericTest<T> : QueueTestFixture<T>
    {
        public LinkedBlockingQueueUnboundAsGenericTest()
        {
            _isCapacityRestricted = false;
            _isFifoQueue = true;
        }
        protected override IQueue<T> NewQueue()
        {
            return new LinkedBlockingQueue<T>();
        }
        protected override IQueue<T> NewQueueFilledWithSample()
        {
            return new LinkedBlockingQueue<T>(TestData<T>.MakeTestArray(_sampleSize));
        }

    }
}
