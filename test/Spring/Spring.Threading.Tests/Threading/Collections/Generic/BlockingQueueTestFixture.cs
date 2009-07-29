using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IBlockingQueue{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class BlockingQueueTestFixture<T> : QueueTestFixture<T>
    {
        protected TestThreadManager ThreadManager { get; private set; }

        [SetUp] public void SetUpThreadManager()
        {
            ThreadManager = new TestThreadManager();
        }

        [TearDown] public void TearDownThreadManager()
        {
            ThreadManager.TearDown();
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

     	[Test] public void PutHandlesNullAsExpected() {
            if (_allowNull)
            {
                var q = NewBlockingQueue();
                q.Put(default(T));
                Assert.That(q.Remove(), Is.EqualTo(default(T)));
            }
     	}

    	[Test] public void PutAddsElementsToQueue() {
    	    var q = NewBlockingQueue();
            for (int i = 0; i < _sampleSize; ++i) {
                q.Put(_samples[i]);
                Assert.IsTrue(q.Contains(_samples[i]));
            }
            AssertRemainingCapacity(q, 0);
	    }

        [Test] public void PutBlocksInterruptiblyWhenFull()
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

        [Test] public void PutBlocksWaitingForTakeWhenFull()
        {
            SkipIfUnboundedQueue();
            var q = NewBlockingQueueFilledWithSample();
            AtomicBoolean added = new AtomicBoolean(false);
            ThreadManager.StartAndAssertRegistered(
                "T1", () => { q.Put(_samples[0]); added.Value = true; });
            Thread.Sleep(TestData.ShortDelay);
            Assert.IsFalse(added);
            q.Take();
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(added);
        }

        [Test] public void TimedOfferWaitsInterruptablyAndTimesOutIfFullAndElementsNotTaken()
        {
            SkipIfUnboundedQueue();
            var q = NewBlockingQueueFilledWithSample();
            var timedout = new AtomicBoolean(false);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.IsFalse(q.Offer(_samples[0], TestData.ShortDelay));
                        timedout.Value = true;
                        Assert.Throws<ThreadInterruptedException>(
                            ()=>q.Offer(_samples[1], TestData.LongDelay));
                    });

            for (int i = 5; i > 0 && !timedout; i--) Thread.Sleep(TestData.ShortDelay);
            Assert.That(timedout.Value, Is.True, "Offer should timeout by now.");
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void TakeRetrievesElementsInExpectedOrder()
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

        [Test] public void TakeBlocksInterruptiblyWhenEmpty()
        {
            var q = NewBlockingQueue();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", () => Assert.Throws<ThreadInterruptedException>(() => q.Take()));
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void TakeRemovesExistingElementsUntilEmptyThenBlocksInterruptibly()
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

        [Test] public void TimedPoolWithZeroTimeoutSucceedsWhenNonEmptyElseTimesOut()
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

		[Test] public void TimedPoolWithNonZeroTimeoutSucceedsWhenNonEmptyElseTimesOut() {
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

		[Test] public void TimedPollIsInterruptable() {
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

        [Test] public void TimedPollFailsBeforeDelayedOfferSucceedsAfterOfferChokesOnInterruption() {
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
            Assert.IsTrue(q.Offer(_samples[0], TestData.ShortDelay));
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

		[Test] public void DrainToChokesOnNullArgument() {
			var q = NewBlockingQueueFilledWithSample();
			var e = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public void DrainToChokesWhenDrainToSelf() {
            var q = NewBlockingQueueFilledWithSample();
            var e = Assert.Throws<ArgumentException>(() => q.DrainTo(q));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentException>(()=>q.DrainTo(q, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public void DrainToEmptiesQueueIntoAnotherCollection() {
            var q = NewBlockingQueueFilledWithSample();
            List<T> l = new List<T>();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, _sampleSize);
			for (int i = 0; i < _sampleSize; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
			q.Add(_samples[0]);
			q.Add(_samples[1]);
			Assert.IsFalse(q.Count == 0); // not empty
			Assert.IsTrue(q.Contains(_samples[0]));
			Assert.IsTrue(q.Contains(_samples[1]));
			l.Clear();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, 2);
			for (int i = 0; i < 2; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
		}

		[Test] public void DrainToEmptiesFullQueueAndUnblocksWaitingPut() {
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

		[Test] public void LimitedDrainToEmptiesFirstNElementsIntoCollection() {
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

        [Test] public void OfferTransfersElementsAcrossThreads()
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

		[Test] public void PollRetrievesElementsAcrossThreads()
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
