using System;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.Collections;
using Spring.Collections.Generic;

namespace Spring.TestFixtures.Collections.NonGeneric
{
    /// <summary>
    /// Basic functionality test for implementations of <see cref="IQueue"/> 
    /// that only accepts object of type <typeparamref name="T"/>. Typically
    /// for queues that implements both <see cref="IQueue"/> and <see cref="IQueue{T}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class TypedQueueTestFixture<T> : QueueTestFixture
    {
        /// <summary>
        /// Only evaluates option <see cref="CollectionContractOptions.Unique"/> and
        /// <see cref="CollectionContractOptions.Fifo"/>.
        /// </summary>
        /// <param name="options"></param>
        protected TypedQueueTestFixture(CollectionContractOptions options)
            : base(options)
        {
        }

        protected override object[] NewSamples()
        {
            object[] samples = new object[_sampleSize];
            for (int i = 0; i < _sampleSize; i++)
            {
                samples[i] = TestData<T>.MakeData(i);
            }
            return samples;
        }

        protected override object MakeData(int i)
        {
            return TestData<T>.MakeData(i);
        }

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
    }
}