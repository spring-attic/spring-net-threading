using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.Threading.AtomicTypes;
using CollectionOptions = Spring.Collections.CollectionOptions;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IBlockingQueue{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class BlockingQueueTestFixture<T> : QueueTestFixture<T>
    {
        protected bool _isFair;

        /// <summary>
        /// Only evaluates option <see cref="CollectionOptions.Unique"/>,
        /// <see cref="CollectionOptions.ReadOnly"/>,
        /// <see cref="CollectionOptions.Bounded"/>,
        /// <see cref="CollectionOptions.Fifo"/>,
        /// <see cref="CollectionOptions.NoNull"/> and
        /// <see cref="CollectionOptions.NoFair"/>.
        /// </summary>
        /// <param name="options"></param>
        protected BlockingQueueTestFixture(CollectionOptions options) : base(options)
        {
            if ((options & CollectionOptions.Fair) != 0) _isFair = true;
        }

        protected sealed override IQueue<T> NewQueue()
        {
            return NewBlockingQueue();
        }
        protected sealed override IQueue<T> NewQueueFilledWithSample()
        {
            return NewBlockingQueueFilledWithSample();
        }

        protected virtual IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return (IBlockingQueue<T>) base.NewQueueFilledWithSample();
        }

        protected abstract IBlockingQueue<T> NewBlockingQueue();

        public override void AddHandlesNullAsExpexcted()
        {
            var q = NewBlockingQueue();
            T value = default(T);
            if (!typeof(T).IsValueType && !_allowNull)
            {
                var e = Assert.Throws<ArgumentNullException>(
                    () => q.Add(value));
                Assert.That(e.ParamName, Is.EqualTo("element"));
            }
            else
            {
                // The thread is required to work with 0 capacity queue.
                ThreadManager.StartAndAssertRegistered("T1", () => PollOneFromQueue(q, value));
                q.Add(value);
                ThreadManager.JoinAndVerify();
            }
        }

     	[Test] public virtual void PutHandlesNullAsExpected() {
            T value = default(T);
            var q = NewBlockingQueue();
            if (!typeof(T).IsValueType && !_allowNull)
            {
                var e = Assert.Throws<ArgumentNullException>(
                    () => q.Add(value));
                Assert.That(e.ParamName, Is.EqualTo("element"));
            }
            else
            {
                // The thread is required to work with 0 capacity queue.
                ThreadManager.StartAndAssertRegistered("T1", () => PollOneFromQueue(q, value));
                q.Put(value);
                ThreadManager.JoinAndVerify();
            }
     	}

        private static void PollOneFromQueue(IBlockingQueue<T> q, T expectedValue)
        {
            T result;
            Assert.IsTrue(q.Poll(TestData.SmallDelay, out result));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

    	[Test] public virtual void PutAddsElementsToQueue() {
    	    var q = NewBlockingQueue();
            for (int i = 0; i < _sampleSize; ++i) {
                q.Put(_samples[i]);
                Assert.IsTrue(q.Contains(_samples[i]));
            }
            AssertRemainingCapacity(q, 0);
	    }

        [Test] public virtual void PutBlocksInterruptiblyWhenFull()
        {
            SkipIfUnboundedQueue();
            var q = NewBlockingQueueFilledWithSample();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", () => Assert.Throws<ThreadInterruptedException>(
                          () => q.Put(TestData<T>.MakeData(_sampleSize))));
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test] public virtual void PutBlocksWaitingForTakeWhenFull()
        {
            SkipIfUnboundedQueue();
            var q = NewBlockingQueueFilledWithSample();
            AtomicBoolean added = new AtomicBoolean(false);
            ThreadManager.StartAndAssertRegistered(
                "T1", () => { q.Put(TestData<T>.One); added.Value = true; });
            Thread.Sleep(TestData.ShortDelay);
            Assert.IsFalse(added);
            q.Take();
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(added);
        }

        [Test] public virtual void TimedOfferWaitsInterruptablyAndTimesOutIfFullAndSucceedAfterTaken()
        {
            SkipIfUnboundedQueue();
            var values = TestData<T>.MakeTestArray(_sampleSize + 3);
            var q = NewBlockingQueueFilledWithSample();
            var timedout = new AtomicBoolean(false);
            ThreadManager.StartAndAssertRegistered(
                "T2",
                () => Assert.IsTrue(q.Offer(values[_sampleSize], TestData.LongDelay)));
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    Assert.IsFalse(q.Offer(TestData<T>.M1, TestData.ShortDelay));
                    timedout.Value = true;
                    Assert.Throws<ThreadInterruptedException>(
                        () => q.Offer(TestData<T>.M2, TestData.LongDelay));
                });

            for (int i = 5; i > 0 && !timedout; i--) Thread.Sleep(TestData.ShortDelay);
            Assert.That(timedout.Value, Is.True, "Offer should timeout by now.");
            ThreadManager.StartAndAssertRegistered(
                "T3",
                () => Assert.IsTrue(q.Offer(values[_sampleSize + 1], TestData.LongDelay)));
            ThreadManager.StartAndAssertRegistered(
                "T4",
                () => Assert.IsTrue(q.Offer(values[_sampleSize + 2], TestData.LongDelay)));
            t.Interrupt();
            Thread.Sleep(TestData.ShortDelay);
            for (int i = 0; i < _sampleSize + 3; i++)
            {
                T result;
                Assert.IsTrue(q.Poll(TestData.ShortDelay, out result));
                if (_isFifoQueue && (i<_sampleSize || _isFair)) Assert.That(result, Is.EqualTo(values[i]));
                else CollectionAssert.Contains(values, result);
            }
            ThreadManager.JoinAndVerify();
        }

        [Test] public virtual void TakeRetrievesElementsInExpectedOrder()
        {
            var q = NewBlockingQueueFilledWithSample();
            var itemsTook = new List<T>();
            for (int i = 0; i < _sampleSize; ++i)
            {
                if(_isFifoQueue)
                    Assert.AreEqual(_samples[i], q.Take());
                else
                    itemsTook.Add(q.Take());
            }
            if(!_isFifoQueue) CollectionAssert.AreEquivalent(_samples, itemsTook);
        }

        [Test] public virtual void TakeBlocksInterruptiblyWhenEmpty()
        {
            var q = NewBlockingQueue();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", () => Assert.Throws<ThreadInterruptedException>(() => q.Take()));
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public virtual void TakeRemovesExistingElementsUntilEmptyThenBlocksInterruptibly()
        {
            var isEmpty = new AtomicBoolean(false);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        var q = NewBlockingQueueFilledWithSample();
                        for (int i = q.Count - 1; i >= 0; i--) q.Take();
                        isEmpty.Value = true;
                        Assert.Throws<ThreadInterruptedException>(() => q.Take());
                    });
            for (int i = 5 - 1; i >= 0 && !isEmpty; i--) Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public virtual void TimedPoolWithZeroTimeoutSucceedsWhenNonEmptyElseTimesOut()
        {
            var q = NewBlockingQueueFilledWithSample();
            // run it in a separate thread so test won't hang due to bad queue implementation.
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        T value;
                        for (int i = 0; i < _sampleSize; ++i)
                        {
                            Assert.IsTrue(q.Poll(TimeSpan.Zero, out value));
                            AssertRetrievedResult(value, i);
                        }
                        Assert.IsFalse(q.Poll(TimeSpan.Zero, out value));
                    });
            ThreadManager.JoinAndVerify(t);
        }

		[Test] public virtual void TimedPoolWithNonZeroTimeoutSucceedsWhenNonEmptyElseTimesOut() {
            var q = NewBlockingQueueFilledWithSample();
            // run it in a separate thread so test won't hang due to bad queue implementation.
            Thread t = ThreadManager.StartAndAssertRegistered(
		        "T1",
		        delegate
		            {
		                T value;
		                for (int i = 0; i < _sampleSize; ++i)
		                {
		                    Assert.IsTrue(q.Poll(TestData.ShortDelay, out value));
		                    AssertRetrievedResult(value, i);
		                }
		                Assert.IsFalse(q.Poll(TestData.ShortDelay, out value));
		            });
            ThreadManager.JoinAndVerify(t);
        }

		[Test] public virtual void TimedPollIsInterruptable() {
            var q = NewBlockingQueue();
		    var isEmpty = new AtomicBoolean(false);
		    Thread t = ThreadManager.StartAndAssertRegistered(
		        "T1",
		        delegate
		            {
		                T value;
		                while (q.Count > 0) q.Take();
		                isEmpty.Value = true;
		                Assert.Throws<ThreadInterruptedException>(
		                    () => q.Poll(TestData.MediumDelay, out value));
		            });
           for (int i = 5 - 1; i >= 0 && !isEmpty; i--) Thread.Sleep(TestData.ShortDelay);
           t.Interrupt();
           ThreadManager.JoinAndVerify(t);
		}

        [Test] public virtual void TimedPollFailsBeforeDelayedOfferSucceedsAfterOfferChokesOnInterruption() {
            var q = NewBlockingQueue();
            T value;
            var timedout = new AtomicBoolean(false);
            Thread t = ThreadManager.StartAndAssertRegistered(
			    "T1",
                    delegate {
                        Assert.IsFalse(q.Poll(TestData.ShortDelay, out value));
                        timedout.Value = true;
                        Assert.IsTrue(q.Poll(TestData.LongDelay, out value));
                        Assert.Throws<ThreadInterruptedException>(()=>q.Poll(TestData.LongDelay, out value));
                });
            for (int i = 5 - 1; i >= 0 && !timedout; i--) Thread.Sleep(TestData.ShortDelay);
            Assert.IsTrue(q.Offer(TestData<T>.One, TestData.ShortDelay));
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

		[Test] public virtual void DrainToChokesOnNullArgument() {
			var q = NewBlockingQueueFilledWithSample();
			var e = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public virtual void DrainToChokesWhenDrainToSelf() {
            var q = NewBlockingQueueFilledWithSample();
            var e = Assert.Throws<ArgumentException>(() => q.DrainTo(q));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentException>(()=>q.DrainTo(q, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public virtual void DrainToEmptiesQueueIntoAnotherCollection() {
            var q = NewBlockingQueueFilledWithSample();
            List<T> l = new List<T>();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, _sampleSize);
			for (int i = 0; i < _sampleSize; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
		    int count = Math.Min(2, _sampleSize);
		    for (int i = 0; i < count; i++) q.Add(_samples[i]);
		    Assert.AreEqual(count, q.Count);
            for (int i = 0; i < count; i++) Assert.IsTrue(q.Contains(_samples[i]));
			l.Clear();
			q.DrainTo(l);
			Assert.AreEqual(0, q.Count);
			Assert.AreEqual(count, l.Count);
			for (int i = 0; i < count; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
		}

		[Test] public virtual void DrainToEmptiesFullQueueAndUnblocksWaitingPut() {
            var q = NewBlockingQueueFilledWithSample();
            Thread t = ThreadManager.StartAndAssertRegistered(
		        "T1", () => q.Put(TestData<T>.MakeData(_sampleSize + 1)));
			List<T> l = new List<T>();
			q.DrainTo(l);
			Assert.IsTrue(l.Count >= _sampleSize);
			for (int i = 0; i < _sampleSize; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
			ThreadManager.JoinAndVerify(t);
			Assert.IsTrue(q.Count + l.Count >= _sampleSize);
		}

		[Test] public virtual void LimitedDrainToEmptiesFirstNElementsIntoCollection() {
            var q = NewBlockingQueue();
            for (int i = 0; i < _sampleSize + 2; ++i)
            {
				for(int j = 0; j < _sampleSize; j++)
					Assert.IsTrue(q.Offer(TestData<T>.MakeData(j)));
				List<T> l = new List<T>();
				q.DrainTo(l, i);
				int k = (i < _sampleSize)? i : _sampleSize;
				Assert.AreEqual(l.Count, k);
				Assert.AreEqual(q.Count, _sampleSize-k);
				for (int j = 0; j < k; ++j)
					Assert.AreEqual(l[j], TestData<T>.MakeData(j));
			    T v;
				while (q.Poll(out v)) {}
			}
		}

        [Test] public virtual void SelectiveDrainToMovesSelectedElementsIntoCollection()
        {
            var q = NewBlockingQueueFilledWithSample();
            List<T> l = new List<T>();
            q.DrainTo(l, e=>((int)Convert.ChangeType(e, typeof(int)))%2==0);
            Assert.That(l.Count, Is.LessThanOrEqualTo((_sampleSize + 1) / 2));
            Assert.That(q.Count, Is.LessThanOrEqualTo((_sampleSize + 1) / 2));
            Assert.AreEqual(_sampleSize, q.Count + l.Count);
            for (int i = 0; i < l.Count; i++)
                Assert.AreEqual(l[i], TestData<T>.MakeData(i*2));
        }

        [Test] public virtual void OfferTransfersElementsAcrossThreads()
        {
            SkipIfUnboundedQueue();
            var q = NewBlockingQueueFilledWithSample();
            ThreadManager.StartAndAssertRegistered(
                "T",
                delegate
                {
                    Assert.IsFalse(q.Offer(TestData<T>.Three));
                    Assert.IsTrue(q.Offer(TestData<T>.Three, TestData.LongDelay));
                    Assert.AreEqual(0, q.RemainingCapacity);
                },
                delegate
                {
                    Thread.Sleep(TestData.ShortDelay);
                    q.Take();
                });

            ThreadManager.JoinAndVerify(TestData.MediumDelay);
        }

		[Test] public virtual void PollRetrievesElementsAcrossThreads()
		{
		    var q = NewBlockingQueue();
            ThreadManager.StartAndAssertRegistered(
                "T",
                delegate
		            {
		                T value;
		                Assert.IsFalse(q.Poll(out value));
		                Assert.IsTrue(q.Poll(TestData.LongDelay, out value));
		                Assert.IsTrue(q.Count == 0); //empty
		            },
		        delegate
		            {
		                Thread.Sleep(TestData.ShortDelay);
		                q.Put(TestData<T>.One);
		            });

            ThreadManager.JoinAndVerify(TestData.MediumDelay);
        }

    }
}
