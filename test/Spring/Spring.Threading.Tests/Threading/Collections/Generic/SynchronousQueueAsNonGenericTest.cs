using NUnit.Framework;
using Spring.Collections;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for no fair <see cref="SynchronousQueue{T}"/> as 
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class SynchronousQueueNoFairAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public SynchronousQueueNoFairAsNonGenericTest()
        {
            _sampleSize = 0;
        }
        protected override IQueue NewQueue()
        {
            return new SynchronousQueue<T>();
        }
    }

    /// <summary>
    /// Test cases for fair <see cref="SynchronousQueue{T}"/> as 
    /// <see cref="IQueue"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class SynchronousQueueFairAsNonGenericTest<T> : TypedQueueTestFixture<T>
    {
        public SynchronousQueueFairAsNonGenericTest()
        {
            _sampleSize = 0;
        }
        protected override IQueue NewQueue()
        {
            return new SynchronousQueue<T>(true);
        }
    }
}