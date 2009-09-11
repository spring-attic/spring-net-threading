using NUnit.Framework;
using Spring.Collections;
using Spring.Collections.Generic;
using CollectionOptions = Spring.Collections.CollectionOptions;

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
    public class ArrayBlockingQueueTest<T>
    {
        [TestFixture(typeof(int), CollectionOptions.Fair)]
        [TestFixture(typeof(int), CollectionOptions.NoFair)]
        [TestFixture(typeof(string), CollectionOptions.Fair)]
        [TestFixture(typeof(string), CollectionOptions.NoFair)]
        public class AsGeneric : BlockingQueueTestFixture<T>
        {
            public AsGeneric(CollectionOptions options)
                : base(options | CollectionOptions.Bounded | CollectionOptions.Fifo) { }

            protected override IBlockingQueue<T> NewBlockingQueue()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair);
            }
            protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair, TestData<T>.MakeTestArray(_sampleSize));
            }
        }

        [TestFixture(typeof(int), CollectionOptions.Fair)]
        [TestFixture(typeof(int), CollectionOptions.NoFair)]
        [TestFixture(typeof(string), CollectionOptions.Fair)]
        [TestFixture(typeof(string), CollectionOptions.NoFair)]
        public class AsNonGeneric : TypedQueueTestFixture<T>
        {
            private readonly bool _isFair;
            public AsNonGeneric(CollectionOptions options)
                : base(options | CollectionOptions.Bounded | CollectionOptions.Fifo)
            {
                _isFair = (options & CollectionOptions.Fair) > 0;
            }

            protected override IQueue NewQueue()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair);
            }

            protected override IQueue NewQueueFilledWithSample()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair, TestData<T>.MakeTestArray(_sampleSize));
            }
        }
    }
}
