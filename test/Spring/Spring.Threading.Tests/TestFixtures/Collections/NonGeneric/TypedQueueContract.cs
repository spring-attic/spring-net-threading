using System;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.Collections;
using Spring.Collections.Generic;

namespace Spring.TestFixtures.Collections.NonGeneric
{
#if PHASED
    using IQueue = System.Collections.ICollection;
#endif
    /// <summary>
    /// Basic functionality test for implementations of <see cref="IQueue"/> 
    /// that only accepts object of type <typeparamref name="T"/>. Typically
    /// for queues that implements both <see cref="IQueue"/> and <see cref="IQueue{T}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class TypedQueueContract<T> : QueueContract
    {
        /// <summary>
        /// Only evaluates option <see cref="CollectionContractOptions.Unique"/> and
        /// <see cref="CollectionContractOptions.Fifo"/>.
        /// </summary>
        /// <param name="options"></param>
        protected TypedQueueContract(CollectionContractOptions options)
            : base(typeof(T).IsValueType ? CollectionContractOptions.NoNull | options : options)
        {
        }

        protected override object MakeData(int i)
        {
            return TestData<T>.MakeData(i);
        }

#if !PHASED
        [Test] public void AddChokesOnIncompatibleDataType()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidCastException>(() => queue.Add(new object()));
        }

        [Test] public void OfferChokesOnIncompatibleDataType()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidCastException>(() => queue.Add(new object()));
        }
#endif
    }
}