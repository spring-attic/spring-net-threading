using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Collections;
using Spring.Collections.Generic;

namespace Spring.TestFixtures.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IQueue{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class QueueTestFixture<T> : CollectionTestFixture<T>
    {
        //protected bool _isCapacityRestricted;

        protected bool IsFifo
        {
            get { return Options.Has(CollectionOptions.Fifo); }
            set { Options = Options.Set(CollectionOptions.Fifo, value); }
        }
        protected bool IsUnbounded
        {
            get { return Options.Has(CollectionOptions.Unbounded); }
            set { Options = Options.Set(CollectionOptions.Unbounded, value); }
        }
        // Queue in .Net should allow null in gneneral.
        protected bool NoNull
        {
            get { return Options.Has(CollectionOptions.NoNull); }
            set { Options = Options.Set(CollectionOptions.NoNull, value); }
        }

        /// <summary>
        /// Only evaluates option <see cref="CollectionOptions.Unique"/>,
        /// <see cref="CollectionOptions.ReadOnly"/>,
        /// <see cref="CollectionOptions.Fifo"/> and
        /// <see cref="CollectionOptions.NoNull"/>.
        /// </summary>
        /// <param name="options"></param>
        protected QueueTestFixture(CollectionOptions options) : base(options)
        {
        }

        protected sealed override ICollection<T> NewCollection()
        {
            return NewQueue();
        }

        protected sealed override ICollection<T> NewCollectionFilledWithSample()
        {
            return NewQueueFilledWithSample();
        }

        protected virtual IQueue<T> NewQueueFilledWithSample()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in Samples)
            {
                queue.Add(sample);
            }
            return queue;
        }

        /// <summary>
        /// Return a new empty queue.
        /// </summary>
        /// <returns></returns>
        protected abstract IQueue<T> NewQueue();

        [Test] public virtual void AddChokesWhenQueueIsFull()
        {
            Options.SkipWhen(CollectionOptions.Unbounded);
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.Throws<InvalidOperationException>(() => queue.Add(TestData<T>.One));
        }

        [Test] public virtual void AddHandlesNullAsExpexcted()
        {
            var q = NewQueue();
            if(!typeof(T).IsValueType && NoNull)
            {
                var e = Assert.Throws<ArgumentNullException>(
                    () => q.Add(default(T)));
                Assert.That(e.ParamName, Is.EqualTo("element"));
            }
            else
            {
                q.Add(default(T));
            }
        }

        [Test] public virtual void AddAllSamplesSuccessfully()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in Samples) queue.Add(sample);
            Assert.That(queue.Count, Is.EqualTo(Samples.Length));
            CollectionAssert.AreEquivalent(Samples, queue);
        }

        [Test] public virtual void OfferReturnsFalseWhenQueueIsFull()
        {
            Options.SkipWhen(CollectionOptions.Unbounded);
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Offer(TestData<T>.One));
            Assert.That(queue.Count, Is.EqualTo(SampleSize));
        }

        [Test] public virtual void OfferHandlesNullAsExpexcted()
        {
            var q = NewQueue();
            if(!typeof(T).IsValueType && NoNull)
            {
                var e = Assert.Throws<ArgumentNullException>(
                    () => q.Offer(default(T)));
                Assert.That(e.ParamName, Is.EqualTo("element"));
            }
            else
            {
                q.Offer(default(T));
            }
        }

        [Test] public virtual void OfferAllSamplesSuccessfully()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in Samples)
            {
                Assert.IsTrue(queue.Offer(sample));
            }
            Assert.That(queue.Count, Is.EqualTo(Samples.Length));
            CollectionAssert.AreEquivalent(Samples, queue);
        }

        [Test] public virtual void RemoveChokesWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            Assert.Throws<NoElementsException>(() => queue.Remove());
        }

        [Test] public  void RemoveSucceedsWhenQueueIsNotEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = 0; i < SampleSize; ++i)
            {
                AssertRetrievedResult(queue.Remove(), i);
            }
        }

        [Test] public virtual void RemoveByElementReturnsFalseWhenEmpty()
        {
            var q = NewQueue();
            Assert.IsFalse(q.Remove(TestData<T>.Zero));
        }

        [Test] public virtual void RemoveByElementFollowedByAddSucceeds()
        {
            if (SampleSize == 0) Assert.Pass("Skip due to an empty queue.");
            var q = NewQueueFilledWithSample();
            Assert.IsTrue(q.Remove(TestData<T>.MakeData(SampleSize/2)));
            q.Add(TestData<T>.MakeData(3));
            T dummy; Assert.That(q.Poll(out dummy), Is.True);
        }

        [Test] public virtual void PollReturnsFalseWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Poll(out result));
        }

        [Test] public virtual void PollSucceedsUntilEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            T result;
            for (int i = 0; i < SampleSize; i++)
            {
                Assert.IsTrue(queue.Poll(out result));
                AssertRetrievedResult(result, i);
            }
            Assert.IsFalse(queue.Poll(out result));
        }

        protected void AssertRetrievedResult(T result, int i)
        {
            if(IsFifo)
                Assert.That(result, Is.EqualTo(Samples[i]));
            else
                CollectionAssert.Contains(Samples, result);
        }

        [Test] public virtual void ElementChokesWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            Assert.Throws<NoElementsException>(() => queue.Element());
        }

        [Test] public virtual void ElementSucceedsWhenQueueIsNotEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = 0; i < SampleSize; ++i)
            {
                AssertRetrievedResult(queue.Element(), i);
                queue.Remove();
            }
        }

        [Test] public virtual void PeekReturnsFalseWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Peek(out result));
            Assert.That(result, Is.EqualTo(default(T)));
        }

        [Test] public virtual void PeekSucceedsWhenQueueIsNotEmpty()
        {
            var queue = NewQueueFilledWithSample();
            for (int i = 0; i < SampleSize; ++i)
            {
                T value;
                Assert.IsTrue(queue.Peek(out value));
                AssertRetrievedResult(value, i);
                queue.Remove();
                T next;
                queue.Peek(out next);
                Assert.That(next, Is.Not.EqualTo(value));
            }
        }

        [Test] public virtual void AddRemoveInMultipeLoops()
        {
            IQueue<T> queue = NewQueue();
            AddRemoveOneLoop(queue, SampleSize / 2);
            AddRemoveOneLoop(queue, SampleSize );
            AddRemoveOneLoop(queue, SampleSize *2 / 3);
            AddRemoveOneLoop(queue, SampleSize);
        }

        [Test] public virtual void RemainingCapacityDecreasesOnAddIncreasesOnRemove()
        {
            IQueue<T> queue = NewQueue();
            AssertRemainingCapacity(queue, SampleSize);
            for (int i = SampleSize - 1; i >= 0; i--)
            {
                queue.Add(Samples[i]);
                AssertRemainingCapacity(queue, i);
            }
            queue = NewQueueFilledWithSample();
            AssertRemainingCapacity(queue, 0);
            for (int i = 1; i <= SampleSize; i++)
            {
                queue.Remove();
                AssertRemainingCapacity(queue, i);
            }
        }

        [Test] public virtual void ClearRemovesAllElements()
        {
            var q = NewQueueFilledWithSample();
            q.Clear();
            Assert.AreEqual(0, q.Count);
            if (q.RemainingCapacity == 0) Assert.Pass("Skip as this queue has no capacity.");
            AssertRemainingCapacity(q, SampleSize);
            q.Add(Samples[1]);
            Assert.AreEqual(1, q.Count);
            Assert.IsTrue(q.Contains(Samples[1]));
            q.Clear();
            Assert.AreEqual(0, q.Count);
        }

        [Test] public virtual void TransitionsFromEmptyToFullWhenElementsAdded()
        {
            Options.SkipWhen(CollectionOptions.Unbounded);
            var q = NewQueue();
            Assert.That(q.Count, Is.EqualTo(0));
            AssertRemainingCapacity(q, SampleSize, "should have room for " + SampleSize);
            foreach (var sample in Samples)
            {
                q.Add(sample);
                Assert.That(q.Count, Is.GreaterThan(0));
            }
            AssertRemainingCapacity(q, 0);
            Assert.IsFalse(q.Offer(TestData<T>.One));
        }

        [Test] public override void EnumerateThroughAllElements()
        {
            if(IsFifo)
                CollectionAssert.AreEqual(Samples, NewCollectionFilledWithSample());
            else
                base.EnumerateThroughAllElements();
        }

        [Test] public virtual void DeserializedQueueIsSameAsOriginal()
        {
            var q = NewQueueFilledWithSample();
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, q);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            var r = (IQueue<T>)formatter2.Deserialize(bin);

            Assert.AreEqual(q.Count, r.Count);
            while (q.Count>0)
                Assert.AreEqual(q.Remove(), r.Remove());
        }

        protected void AssertRemainingCapacity(IQueue<T> queue, int size)
        {
            AssertRemainingCapacity(queue, size, null);
        }

        protected void AssertRemainingCapacity(IQueue<T> queue, int size, string message)
        {
            if (IsUnbounded)
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(int.MaxValue), message);
            }
            else
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(size), message);
            }
        }

        private void AddRemoveOneLoop(IQueue<T> queue, int size)
        {
            for (int i = 0; i < size; i++)
            {
                queue.Add(Samples[i]);
            }
            for (int i = 0; i < size; i++)
            {
                T removed = queue.Remove();
                if(IsFifo)
                {
                    Assert.That(removed, Is.EqualTo(Samples[i]));
                }
                else
                {
                    CollectionAssert.Contains(Samples, removed);
                }
            }
        }
    }
}