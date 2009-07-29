using NUnit.Framework;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for bounded <see cref="LinkedBlockingQueue{T}"/> as a generic
    /// <see cref="IBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueBoundAsGenericTest<T> : BlockingQueueTestFixture<T>
    {
        public LinkedBlockingQueueBoundAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new LinkedBlockingQueue<T>(_sampleSize);
        }
        protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            var sut = new LinkedBlockingQueue<T>(_sampleSize);
            sut.AddRange(TestData<T>.MakeTestArray(_sampleSize));
            return sut;
        }

    }

    /// <summary>
    /// Functional test case for unbounded <see cref="LinkedBlockingQueue{T}"/> as a generic
    /// <see cref="IBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueUnboundAsGenericTest<T> : BlockingQueueTestFixture<T>
    {
        public LinkedBlockingQueueUnboundAsGenericTest()
        {
            _isCapacityRestricted = false;
            _isFifoQueue = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new LinkedBlockingQueue<T>();
        }
        protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return new LinkedBlockingQueue<T>(TestData<T>.MakeTestArray(_sampleSize));
        }

    }
}
