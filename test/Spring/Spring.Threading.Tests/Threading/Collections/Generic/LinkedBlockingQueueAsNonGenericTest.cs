using NUnit.Framework;
using Spring.Collections;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for bound <see cref="LinkedBlockingQueue{T}"/> as a non generic
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueBoundAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public LinkedBlockingQueueBoundAsNonGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }

        protected override IQueue NewQueue()
        {
            return new LinkedBlockingQueue<T>(_sampleSize);
        }

        protected override IQueue NewQueueFilledWithSample()
        {
            var sut = new LinkedBlockingQueue<T>(_sampleSize);
            sut.AddRange(TestData<T>.MakeTestArray(_sampleSize));
            return sut;
        }
    }

    /// <summary>
    /// Functional test case for unbound <see cref="LinkedBlockingQueue{T}"/> as a non generic
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueUnboundAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public LinkedBlockingQueueUnboundAsNonGenericTest()
        {
            _isCapacityRestricted = false;
            _isFifoQueue = true;
        }

        protected override IQueue NewQueue()
        {
            return new LinkedBlockingQueue<T>();
        }

        protected override IQueue NewQueueFilledWithSample()
        {
            return new LinkedBlockingQueue<T>(TestData<T>.MakeTestArray(_sampleSize));
        }
    }
}
