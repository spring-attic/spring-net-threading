using System;
using System.Collections;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.Framework;
using Spring.Collections;

namespace Spring.TestFixtures.Collections.NonGeneric
{
#if PHASED
    using IQueue = ICollection;
#endif

    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IQueue"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class QueueContract : CollectionContract
    {
        protected bool IsFifo
        {
            get { return Options.Has(CollectionContractOptions.Fifo); }
            set { Options = Options.Set(CollectionContractOptions.Fifo, value); }
        }
        protected bool IsBounded
        {
            get { return Options.Has(CollectionContractOptions.Bounded); }
        }

        /// <summary>
        /// Only evaluates option <see cref="CollectionContractOptions.Unique"/>,
        /// <see cref="CollectionContractOptions.Fifo"/>.
        /// </summary>
        /// <param name="options"></param>
        protected QueueContract(CollectionContractOptions options) : base(options)
        {
        }

        protected override sealed ICollection NewCollection()
        {
            return NewQueueFilledWithSample();
        }

        protected virtual IQueue NewQueueFilledWithSample()
        {
            IQueue queue = NewQueue();
#if !PHASED
            foreach (object o in Samples)
            {
                queue.Offer(o);
            }
#endif
            return queue;
        }

        /// <summary>
        /// Return a new empty queue.
        /// </summary>
        /// <returns></returns>
        protected abstract IQueue NewQueue();

        protected virtual object MakeData(int i)
        {
            return new object();
        }

        protected virtual object[] MakeTestArray(int count)
        {
            var result = new object[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = MakeData(i);
            }
            return result;
        }

        protected override object[] NewSamples()
        {
            return MakeTestArray(SampleSize);
        }

#if !PHASED
        [Test] public void AddChokesWhenQueueIsFull()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            IQueue queue = NewQueueFilledWithSample();
            Assert.Throws<InvalidOperationException>(() => queue.Add(MakeData(0)));
        }

        [Test] public void AddAllSamplesuccessfully()
        {
            IQueue queue = NewQueue();
            foreach (object o in Samples) queue.Add(o);
            Assert.That(queue.Count, Is.EqualTo(Samples.Length));
            CollectionAssert.AreEquivalent(Samples, queue);
        }

        [Test] public void AddReturnsFalseWhenQueueIsFull()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            IQueue queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Offer(MakeData(0)));
            Assert.That(queue.Count, Is.EqualTo(SampleSize));
        }

        [Test] public void OfferAllSamplesuccessfully()
        {
            IQueue queue = NewQueue();
            foreach (object o in Samples)
            {
                Assert.IsTrue(queue.Offer(o));
            }
            Assert.That(queue.Count, Is.EqualTo(Samples.Length));
            CollectionAssert.AreEquivalent(Samples, queue);
        }

        [Test] public void RemoveChokesWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Remove());
        }

        [Test] public void RemoveAllSamplesucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();

            for (int i = queue.Count - 1; i >= 0; i--)
            {
                object o = queue.Remove();
                Assert.IsNotNull(o);
                CollectionAssert.Contains(Samples, o);
            }
        }

        [Test] public void PollReturnsNullWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.IsNull(queue.Poll());
        }

        [Test] public void PollAllSamplesucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                object o = queue.Poll();
                Assert.IsNotNull(o);
                CollectionAssert.Contains(Samples, o);
            }
        }

        [Test] public void ElementChokesWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Element());
        }

        [Test] public void ElementGetsAllSamplesucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                CollectionAssert.Contains(Samples, queue.Element());
                queue.Remove();
            }
        }

        [Test] public void PeekReturnsNullWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.IsNull(queue.Peek());
        }

        [Test] public void PeekAllSamplesucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                CollectionAssert.Contains(Samples, queue.Peek());
                queue.Remove();
            }
        }

        [Test] public void IsEmptyReturnsTrueWhenQueueIsEmptyElseFalse()
        {
            IQueue queue = NewQueue();
            Assert.IsTrue(queue.IsEmpty);
            if (SampleSize ==0) Assert.Pass();
            queue.Add(Samples[0]);
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test] public void AddRemoveInMultipeLoops()
        {
            IQueue queue = NewQueue();
            AddRemoveOneLoop(queue, SampleSize / 2);
            AddRemoveOneLoop(queue, SampleSize );
            AddRemoveOneLoop(queue, SampleSize *2 / 3);
            AddRemoveOneLoop(queue, SampleSize);
        }

        private void AddRemoveOneLoop(IQueue queue, int size)
        {
            for (int i = 0; i < size; i++)
            {
                queue.Add(Samples[i]);
            }
            for (int i = 0; i < size; i++)
            {
                object o = queue.Remove();
                if(IsFifo)
                {
                    Assert.That(o, Is.EqualTo(Samples[i]));
                }
                else
                {
                    CollectionAssert.Contains(Samples, o);
                }
            }
        }
#endif
    }
}