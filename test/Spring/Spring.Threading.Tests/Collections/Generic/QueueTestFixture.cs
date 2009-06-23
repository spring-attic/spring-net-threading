using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IQueue"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class QueueTestFixture<T> : CollectionTestFixture<T>
    {
        protected bool _isCapacityRestricted;

        protected bool _isFifoQueue;

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
            if(!_isCapacityRestricted) Assert.Pass("Skipped as queue is unbounded.");
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.Throws<InvalidOperationException>(delegate { queue.Add(_samples[0]); });
        }

        [Test] public void AddAllSamplesSuccessfully()
        {
            IQueue<T> queue = NewQueue();
            foreach (T sample in _samples) queue.Add(sample);
            Assert.That(queue.Count, Is.EqualTo(_samples.Length));
            CollectionAssert.AreEquivalent(_samples, queue);
        }

        [Test] public void AddReturnsFalseWhenQueueIsFull()
        {
            if(!_isCapacityRestricted) Assert.Pass("Skipped as queue is unbounded.");
            IQueue<T> queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Offer(_samples[0]));
            Assert.That(queue.Count, Is.EqualTo(_sampleSize));
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
            Assert.Throws<NoElementsException>(delegate { queue.Remove(); });
        }

        [Test] public void RemoveAllSamplesSucessfully()
        {
            IQueue<T> queue = NewQueueFilledWithSample();

            for (int i = queue.Count - 1; i >= 0; i--)
            {
                object o = queue.Remove();
                Assert.IsNotNull(o);
                CollectionAssert.Contains(_samples, o);
            }
        }

        [Test] public void PollReturnsNullWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Poll(out result));
        }

        [Test] public void PollAllSamplesSucessfully()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                T result;
                Assert.IsTrue(queue.Poll(out result));
                CollectionAssert.Contains(_samples, result);
            }
        }

        [Test] public void ElementChokesWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            Assert.Throws<NoElementsException>(delegate { queue.Element(); });
        }

        [Test] public void ElementGetsAllSamplesSucessfully()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                CollectionAssert.Contains(_samples, queue.Element());
                queue.Remove();
            }
        }

        [Test] public void PeekReturnsNullWhenQuqueIsEmpty()
        {
            IQueue<T> queue = NewQueue();
            T result;
            Assert.IsFalse(queue.Peek(out result));
        }

        [Test] public void PeekAllSamplesSucessfully()
        {
            IQueue<T> queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                T result;
                Assert.IsTrue(queue.Peek(out result));
                CollectionAssert.Contains(_samples, result);
                queue.Remove();
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

        [Test] public void RemainingCapacitySunnyDay()
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

        private void AssertRemainingCapacity(IQueue<T> queue, int size)
        {
            if (_isCapacityRestricted)
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(size));
            }
            else
            {
                Assert.That(queue.RemainingCapacity, Is.EqualTo(int.MaxValue));
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
