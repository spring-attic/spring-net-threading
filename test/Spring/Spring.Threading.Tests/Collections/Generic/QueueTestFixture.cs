using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Threading.Collections.Generic;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IQueue{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class QueueTestFixture<T> : CollectionTestFixture<T>
    {
        protected bool _isCapacityRestricted;

        protected bool _isFifoQueue;

        // Queue in .Net should allow null in gneneral.
        protected bool _allowNull = true;

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
            foreach (T sample in _samples)
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

        public QueueTestFixture()
        {
            _allowDuplicate = true;
        }

        [Test] public void AddChokesWhenQueueIsFull()
        {
            SkipIfUnboundedQueue();
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.Throws<InvalidOperationException>(delegate { queue.Add(_samples[0]); });
        }

        [Test] public void AddHandlesNullAsExpexcted()
        {
            var q = NewQueue();
            if(!typeof(T).IsValueType && !_allowNull)
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

        [Test] public void AddAllSamplesSuccessfully()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in _samples) queue.Add(sample);
            Assert.That(queue.Count, Is.EqualTo(_samples.Length));
            CollectionAssert.AreEquivalent(_samples, queue);
        }

        [Test] public void OfferReturnsFalseWhenQueueIsFull()
        {
            SkipIfUnboundedQueue();
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Offer(_samples[0]));
            Assert.That(queue.Count, Is.EqualTo(_sampleSize));
        }

        [Test] public void OfferHandlesNullAsExpexcted()
        {
            var q = NewQueue();
            if(!typeof(T).IsValueType && !_allowNull)
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

        [Test] public void OfferAllSamplesSuccessfully()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in _samples)
            {
                Assert.IsTrue(queue.Offer(sample));
            }
            Assert.That(queue.Count, Is.EqualTo(_samples.Length));
            CollectionAssert.AreEquivalent(_samples, queue);
        }

        [Test] public void RemoveChokesWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            Assert.Throws<NoElementsException>(() => queue.Remove());
        }

        [Test] public void RemoveSucceedsWhenQueueIsNotEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = 0; i < _sampleSize; ++i)
            {
                AssertRetrievedResult(queue.Remove(), i);
            }
        }

        [Test] public void RemoveByElementFollowedByAddSucceeds()
        {
            var q = NewQueueFilledWithSample();
            Assert.IsTrue(q.Remove(TestData<T>.MakeData(1)));
            Assert.IsTrue(q.Remove(TestData<T>.MakeData(2)));
            q.Add(TestData<T>.MakeData(3));
            T dummy; Assert.That(q.Poll(out dummy), Is.True);
        }

        [Test] public void PollReturnsFalseWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Poll(out result));
        }

        [Test] public void PollSucceedsUntilEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            T result;
            for (int i = 0; i < _sampleSize; i++)
            {
                Assert.IsTrue(queue.Poll(out result));
                AssertRetrievedResult(result, i);
            }
            Assert.IsFalse(queue.Poll(out result));
        }

        protected void AssertRetrievedResult(T result, int i)
        {
            if(_isFifoQueue)
                Assert.That(result, Is.EqualTo(_samples[i]));
            else
                CollectionAssert.Contains(_samples, result);
        }

        [Test] public void ElementChokesWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            Assert.Throws<NoElementsException>(() => queue.Element());
        }

        [Test] public void ElementSucceedsWhenQueueIsNotEmpty()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
			for (int i = 0; i < _sampleSize; ++i)
            {
                AssertRetrievedResult(queue.Element(), i);
                queue.Remove();
            }
        }

        [Test] public void PeekReturnsFalseWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Peek(out result));
            Assert.That(result, Is.EqualTo(default(T)));
        }

        [Test] public void PeekSucceedsWhenQueueIsNotEmpty()
        {
            var queue = NewQueueFilledWithSample();
            for (int i = 0; i < _sampleSize; ++i)
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

        [Test] public void AddRemoveInMultipeLoops()
        {
            IQueue<T> queue = NewQueue();
            AddRemoveOneLoop(queue, _sampleSize / 2);
            AddRemoveOneLoop(queue, _sampleSize );
            AddRemoveOneLoop(queue, _sampleSize *2 / 3);
            AddRemoveOneLoop(queue, _sampleSize);
        }

        [Test] public void RemainingCapacityDecreasesOnAddIncreasesOnRemove()
        {
            IQueue<T> queue = NewQueue();
            AssertRemainingCapacity(queue, _sampleSize);
            for (int i = _sampleSize - 1; i >= 0; i--)
            {
                queue.Add(_samples[i]);
                AssertRemainingCapacity(queue, i);
            }
            queue = NewQueueFilledWithSample();
            AssertRemainingCapacity(queue, 0);
            for (int i = 1; i <= _sampleSize; i++)
            {
                queue.Remove();
                AssertRemainingCapacity(queue, i);
            }
        }

        [Test] public void ClearRemovesAllElements()
        {
            var q = NewQueueFilledWithSample();
            q.Clear();
            Assert.AreEqual(0, q.Count);
            AssertRemainingCapacity(q, _sampleSize);
            q.Add(_samples[1]);
            Assert.AreEqual(1, q.Count);
            Assert.IsTrue(q.Contains(_samples[1]));
            q.Clear();
            Assert.AreEqual(0, q.Count);
        }

        [Test] public void TransitionsFromEmptyToFullWhenElementsAdded()
        {
            SkipIfUnboundedQueue();
            var q = NewQueue();
            Assert.That(q.Count, Is.EqualTo(0));
            AssertRemainingCapacity(q, _sampleSize, "should have room for " + _sampleSize);
            foreach (var sample in _samples)
            {
                q.Add(sample);
                Assert.That(q.Count, Is.GreaterThan(0));
            }
            AssertRemainingCapacity(q, 0);
            Assert.IsFalse(q.Offer(TestData<T>.One));
        }

        [Test] public override void EnumerateThroughAllElements()
        {
            if(_isFifoQueue)
                CollectionAssert.AreEqual(_samples, NewCollectionFilledWithSample());
            else
                base.EnumerateThroughAllElements();
        }

        [Test] public void DeserializedQueueIsSameAsOriginal()
        {
            var q = NewQueueFilledWithSample();
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, q);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            LinkedBlockingQueue<T> r = (LinkedBlockingQueue<T>)formatter2.Deserialize(bin);

            Assert.AreEqual(q.Count, r.Count);
            while (q.Count>0)
                Assert.AreEqual(q.Remove(), r.Remove());
        }

        protected void SkipIfUnboundedQueue()
        {
            if (!_isCapacityRestricted) Assert.Pass("Skipped as queue is unbounded.");
        }

        protected void AssertRemainingCapacity(IQueue<T> queue, int size)
        {
            AssertRemainingCapacity(queue, size, null);
        }

        protected void AssertRemainingCapacity(IQueue<T> queue, int size, string message)
        {
            if (_isCapacityRestricted)
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(size), message);
            }
            else
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(int.MaxValue), message);
            }
        }

        private void AddRemoveOneLoop(IQueue<T> queue, int size)
        {
            for (int i = 0; i < size; i++)
            {
                queue.Add(_samples[i]);
            }
            for (int i = 0; i < size; i++)
            {
                T removed = queue.Remove();
                if(_isFifoQueue)
                {
                    Assert.That(removed, Is.EqualTo(_samples[i]));
                }
                else
                {
                    CollectionAssert.Contains(_samples, removed);
                }
            }
        }
    }
}
