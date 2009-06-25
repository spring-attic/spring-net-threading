using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Threading;
using NUnit.Framework;
using Spring.Collections;
using Spring.Threading.Execution;

namespace Spring.Threading.Collections.Generic
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueTest<T>
    {
        private int _sampleSize = 9;
        private T[] _samples;

        TestThreadManager ThreadManager { get; set; }

        [SetUp] public void SetUp()
        {
            _samples = TestData<T>.MakeTestArray(_sampleSize);
            ThreadManager = new TestThreadManager();
        }

        [TearDown] public void TearDown()
        {
            ThreadManager.TearDown();
        }

        /// <summary>
        /// Create a queue of given size containing objects of type 
        /// <typeparamref name="T"/> converted from consecutive integer 
        /// 0 ... n-1.
        /// </summary>
        private LinkedBlockingQueue<T> PopulatedQueue(int n)
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(n);
            Assert.IsTrue(q.IsEmpty);
            for (int i = 0; i < n; i++)
                Assert.IsTrue(q.Offer(TestData<T>.MakeData(i)));
            Assert.IsFalse(q.IsEmpty);
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.AreEqual(n, q.Count);
            return q;
        }

        [Test] public void ConstructorCreatesQueueWithGivenCapacityOrMaxInteger()
        {
            Assert.AreEqual(_sampleSize, new LinkedBlockingQueue<T>(_sampleSize).RemainingCapacity);
            Assert.AreEqual(int.MaxValue, new LinkedBlockingQueue<T>().RemainingCapacity);
        }

        [Test] public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => { new LinkedBlockingQueue<T>(capacity); });
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
            Assert.That(e.ActualValue, Is.EqualTo(capacity));
        }

        [Test] public void ConstructorChokesOnNullCollectionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName,Is.EqualTo("collection"));
        }

        [Test] public void ConstructorWelcomesNullElememtInCollectionArgument()
        {
            T[] arrayWithDefaulValue = new T[_sampleSize];
            var q = new LinkedBlockingQueue<T>(arrayWithDefaulValue);
            foreach (T sample in arrayWithDefaulValue)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test] public void ConstructorCreatesQueueConstainsAllElementsInCollection()
        {
            var q = new LinkedBlockingQueue<T>(_samples);
            foreach (T sample in _samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test] public void TransitionsFromEmptyToFullWhenElementsAdded()
        {
            var q = new LinkedBlockingQueue<T>(2);
            Assert.IsTrue(q.IsEmpty);
            Assert.AreEqual(2, q.RemainingCapacity, "should have room for 2");
            q.Add(_samples[0]);
            Assert.IsFalse(q.IsEmpty);
            q.Add(_samples[1]);
            Assert.IsFalse(q.IsEmpty);
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.IsFalse(q.Offer(_samples[2]));
        }

        [Test] public void RemainingCapacityDecreasesOnAddIncreasesOnRemove()
        {
            var q = PopulatedQueue(_sampleSize);
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.AreEqual(i, q.RemainingCapacity);
                Assert.AreEqual(_sampleSize - i, q.Count);
                q.Remove();
            }
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.AreEqual(_sampleSize - i, q.RemainingCapacity);
                Assert.AreEqual(i, q.Count);
                q.Add(_samples[i]);
            }
        }

        [Test] public void OfferWelcomesNull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            q.Offer(default(T));
        }

        [Test] public void AddWelcomesNull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            q.Add(default(T));
        }

        [Test] public void OfferSucceedsIfNotFullFailsIfFull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            Assert.IsTrue(q.Offer(_samples[0]));
            Assert.IsFalse(q.Offer(_samples[1]));
        }

        [Test] public void AddSucceedsIfNotFullChokesIfFull()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(_sampleSize);
            for (int i = 0; i < _sampleSize; ++i)
            {
                q.Add(_samples[i]);
            }
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.Throws<InvalidOperationException>(
                () => q.Add(TestData<T>.MakeData(_sampleSize)));
        }

        [Test] public void AddRangeChokesOnNullArgument() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(1);
            var e = Assert.Throws<ArgumentNullException>(()=>q.AddRange(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeChokesWhenAddItself()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            var e = Assert.Throws<ArgumentException>(()=>q.AddRange(q));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeWelcomesNullElements() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            T[] arrayWithDefaulValue = new T[_sampleSize];
            q.AddRange(arrayWithDefaulValue);
            foreach (T sample in arrayWithDefaulValue)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

    	[Test] public void AddRangeChokesWhenNotEnoughRoom() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(1);
            Assert.Throws<InvalidOperationException>(()=>q.AddRange(_samples));
    	}

    	[Test] public void AddRangeAddAllElementsInTraversalOrder() {
            T[] empty = new T[0];
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            Assert.IsFalse(q.AddRange(empty));
            Assert.IsTrue(q.AddRange(_samples));
            foreach (T sample in _samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

     	[Test] public void PutWelcomesNull() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            q.Put(default(T));
     	    Assert.That(q.Remove(), Is.EqualTo(default(T)));
     	}

    	[Test] public void PutAddsElementsToQueue() {
             LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
             for (int i = 0; i < _sampleSize; ++i) {
                 q.Put(_samples[i]);
                 Assert.IsTrue(q.Contains(_samples[i]));
             }
             Assert.AreEqual(0, q.RemainingCapacity);
	    }

        [Test] public void PutBlocksInterruptiblyWhenFull()
        {
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", 
                delegate
                    {
                        int added = 0;
                        LinkedBlockingQueue<T> q =
                            new LinkedBlockingQueue<T>(_sampleSize);
                        for (int i = 0; i < _sampleSize; ++i)
                        {
                            q.Put(_samples[i]);
                            ++added;
                        }
                        Assert.Throws<ThreadInterruptedException>(
                            () => q.Put(TestData<T>.MakeData(_sampleSize)));
                        Assert.AreEqual(added, _sampleSize);
                    });
            Thread.Sleep(TestData.SmallDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test] public void PutBlocksWaitingForTakeWhenFull()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(2);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        int added = 0;
                        q.Put(_samples[0]);
                        ++added;
                        q.Put(_samples[1]);
                        ++added;
                        q.Put(_samples[2]);
                        ++added;
                        Assert.Throws<ThreadInterruptedException>(
                            delegate
                                {
                                    q.Put(_samples[3]);
                                    ++added;
                                });
                        Assert.IsTrue(added >= 2);
                    });
            Thread.Sleep(TestData.ShortDelay);
            q.Take();
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TimedOfferTimesOutIfFullAndElementsNotTaken()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(2);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        q.Put(_samples[0]);
                        q.Put(_samples[1]);
                        Assert.IsFalse(q.Offer(_samples[2],
                                               TestData.ShortDelay));
                        Assert.Throws<ThreadInterruptedException>(
                            ()=>q.Offer(_samples[3], TestData.LongDelay));
                    });

            Thread.Sleep(TestData.SmallDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void TakeRetrievesElementsInFifoOrder()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.AreEqual(_samples[i], q.Take());
            }
        }

        [Test] public void TakeBlocksInterruptiblyWhenEmpty()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(2);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.Throws<ThreadInterruptedException>(() => q.Take());
                    });
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void TakeRemovesExistingElementsUntilEmptyThenBlocksInterruptibly()
        {
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
                        for (int i = 0; i < _sampleSize; ++i)
                        {
                            Assert.AreEqual(_samples[i], q.Take());
                        }
                        Assert.Throws<ThreadInterruptedException>(() => q.Take());
                    });
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void PollSucceedsUnlessEmpty()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            T value;
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.IsTrue(q.Poll(out value)); 
                Assert.AreEqual(_samples[i], value);
            }
            Assert.IsFalse(q.Poll(out value));
        }

        [Test] public void TimedPoolWithZeroTimeoutSucceedsWhenNonEmptyElseTimesOut()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            T value;
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.IsTrue(q.Poll(TimeSpan.Zero, out value)); 
                Assert.AreEqual(_samples[i], value);
            }
            Assert.IsFalse(q.Poll(TimeSpan.Zero, out value));
        }

		[Test] public void TimedPoolWithNonZeroTimeoutSucceedsWhenNonEmptyElseTimesOut() {
            LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T value;
            for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(q.Poll(TestData.ShortDelay, out value));
				Assert.AreEqual(_samples[i], value);
            }
            Assert.IsFalse(q.Poll(TestData.ShortDelay, out value));
		}

		[Test] public void TimedPollIsInterruptable() {
			Thread t = ThreadManager.StartAndAssertRegistered(
				"T1",
                delegate {
					LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
					T value;
					for (int i = 0; i < _sampleSize; ++i) {
						Assert.IsTrue(q.Poll(TestData.ShortDelay, out value));
						Assert.AreEqual(_samples[i], value);
					}
					Assert.Throws<ThreadInterruptedException>(()=>
                        q.Poll(TestData.ShortDelay, out value));
                });
           Thread.Sleep(TestData.ShortDelay);
           t.Interrupt();
           ThreadManager.JoinAndVerify(t);
		}

        [Test] public void TimedPollFailsBeforeDelayedOfferSucceedsAfterOfferChokesOnInterruption() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(2);
            T value;
            Thread t = ThreadManager.StartAndAssertRegistered(
			    "T1",
                    delegate {
                        Assert.IsFalse(q.Poll(TestData.ShortDelay, out value));
                        Assert.IsTrue(q.Poll(TestData.LongDelay, out value));
                        Assert.Throws<ThreadInterruptedException>(()=>q.Poll(TestData.LongDelay, out value));
                });
            Thread.Sleep(TestData.SmallDelay);
            Assert.IsTrue(q.Offer(_samples[0], TestData.ShortDelay));
            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

		[Test] public void PeekReturnsNextElementOrNullIfEmpty() {
        LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T value;
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(q.Peek(out value));
				Assert.AreEqual(_samples[i], value);
				q.Poll(out value);
			    q.Peek(out value);
                Assert.That(value, Is.Not.EqualTo(_samples[i]));
			}
			Assert.IsFalse(q.Peek(out value));
            Assert.That(value, Is.EqualTo(default(T)));
		}

		[Test] public void ElementReturnsNextElementOrChokesIfEmpty() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.AreEqual(_samples[i], q.Element());
				T v; q.Poll(out v);
			}
            Assert.Throws<NoElementsException>(() => q.Element());
		}

		[Test] public void RemoveRemovesNextElementOrChokesIfEmpty() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.AreEqual(_samples[i], q.Remove());
			}
			Assert.Throws<NoElementsException>(()=>q.Remove());
		}

		[Test] public void RemoveByElementRemovesElementAndReturnsTrueIfPresent() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			for (int i = 1; i < _sampleSize; i+=2) {
				Assert.IsTrue(q.Remove(TestData<T>.MakeData(i)));
			}
			for (int i = 0; i < _sampleSize; i+=2) {
				Assert.IsTrue(q.Remove(TestData<T>.MakeData(i)));
				Assert.IsFalse(q.Remove(TestData<T>.MakeData(i+1)));
			}
			Assert.IsTrue(q.IsEmpty);
		}

		[Test] public void RemoveByElementFollowedByAddSucceeds() {
				LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(2);
				q.Add(TestData<T>.MakeData(1));
				q.Add(TestData<T>.MakeData(2));
				Assert.IsTrue(q.Remove(TestData<T>.MakeData(1)));
				Assert.IsTrue(q.Remove(TestData<T>.MakeData(2)));
				q.Add(TestData<T>.MakeData(3));
				Assert.That(q.Take(), Is.Not.EqualTo(default(T)));
		}

		[Test] public void ContainsReportsTrueWhenElementsAddedButNotYetRemoved() {
			LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
			
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(q.Contains(TestData<T>.MakeData(i)));
				T v; q.Poll(out v);
				Assert.IsFalse(q.Contains(TestData<T>.MakeData(i)));
			}
		}

		[Test] public void ClearRemovesAllElements() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			q.Clear();
			Assert.IsTrue(q.IsEmpty);
			Assert.AreEqual(0, q.Count);
			Assert.AreEqual(_sampleSize, q.RemainingCapacity);
			q.Add(_samples[1]);
			Assert.IsFalse(q.IsEmpty);
			Assert.IsTrue(q.Contains(_samples[1]));
			q.Clear();
			Assert.IsTrue(q.IsEmpty);
		}

		[Test] public void ToArrayWritesAllElementsToNewArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T[] o = q.ToArray();
			for(int i = 0; i < o.Length; i++)
				Assert.AreEqual(o[i], q.Take());
		}

		[Test] public void ToArrayWritesAllElementsToExistingArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T[] ints = new T[_sampleSize];
			ints = q.ToArray(ints);
				for(int i = 0; i < ints.Length; i++)
					Assert.AreEqual(ints[i], q.Take());
		}

		[Test] public void ToArrayChokesOnNullArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			var e = Assert.Throws<ArgumentNullException>(()=>q.ToArray(null));
            Assert.That(e.ParamName, Is.EqualTo("targetArray"));
		}

        [Test] public void ToArrayChokesOnIncompatibleArray()
        {
            LinkedBlockingQueue<object> q = new LinkedBlockingQueue<object>(2);
            q.Offer(new object());
            Assert.Throws<ArrayTypeMismatchException>(() => q.ToArray(new string[10]));
        }

		[Test] public void EnumerateThroughAllElements() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
            TestHelper.AssertEnumeratorEquals(_samples.GetEnumerator(), q.GetEnumerator());
		}

		[Test] public void EnumeratorOrderingIsFifo() {
			LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(3);
			q.Add(TestData<T>.One);
			q.Add(TestData<T>.Two);
			q.Add(TestData<T>.Three);
			Assert.AreEqual(0, q.RemainingCapacity);
			int k = 0;
			for (var it = q.GetEnumerator(); it.MoveNext();) {
				Assert.AreEqual(TestData<T>.MakeData(++k), it.Current);
			}
			Assert.AreEqual(3, k);
		}

		[Test] public void EnumeratorFailsWhenQueueIsModified ()
		{
            EnumeratorFailsWhen(q => q.Remove());
            EnumeratorFailsWhen(q => q.Add(TestData<T>.Three));
		}

        private void EnumeratorFailsWhen(Action<LinkedBlockingQueue<T>> action)
        {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(4);
            q.Add(TestData<T>.One);
            q.Add(TestData<T>.Two);
            Assert.Throws<InvalidOperationException>(
                delegate
                    {
                        var it = q.GetEnumerator();
                        while( it.MoveNext())
                        {
                            action(q);
                        }
                    });
            var e = q.GetEnumerator();
            action(q);
            Assert.Throws<InvalidOperationException>(e.Reset);
        }

		[Test] public void DrainToChokesOnNullArgument() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			var e = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentNullException>(()=>q.DrainTo(null, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public void DrainToChokesWhenDrainToSelf() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			var e = Assert.Throws<ArgumentException>(()=>q.DrainTo(q));
			Assert.That(e.ParamName, Is.EqualTo("collection"));
			var e2 = Assert.Throws<ArgumentException>(()=>q.DrainTo(q, 0));
			Assert.That(e2.ParamName, Is.EqualTo("collection"));
		}

		[Test] public void DrainToEmptiesQueueIntoAnotherCollection() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			List<T> l = new List<T>();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, _sampleSize);
			for (int i = 0; i < _sampleSize; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
			q.Add(_samples[0]);
			q.Add(_samples[1]);
			Assert.IsFalse(q.IsEmpty);
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
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
		    Thread t = ThreadManager.StartAndAssertRegistered(
		        "T1",
		        delegate
		            {
		                q.Put(TestData<T>.MakeData(_sampleSize + 1));

		            });
			List<T> l = new List<T>();
			q.DrainTo(l);
			Assert.IsTrue(l.Count >= _sampleSize);
			for (int i = 0; i < _sampleSize; ++i)
				Assert.AreEqual(l[i], TestData<T>.MakeData(i));
			ThreadManager.JoinAndVerify(t);
			Assert.IsTrue(q.Count + l.Count >= _sampleSize);
		}

		[Test] public void LimitedDrainToEmptiesFirstNElementsIntoCollection() {
			LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>();
			for (int i = 0; i < _sampleSize + 2; ++i) {
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

		[Test] public void ToStringContainsToStringOfElements() {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			String s = q.ToString();
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(s.IndexOf(_samples[i].ToString()) >= 0);
			}
		}

        [Test] public void DeserializedQueueIsSameAsOriginal()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, q);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            LinkedBlockingQueue<T> r = (LinkedBlockingQueue<T>)formatter2.Deserialize(bin);

            Assert.AreEqual(q.Count, r.Count);
            while (!q.IsEmpty)
                Assert.AreEqual(q.Remove(), r.Remove());
        }

		[Test] public void OfferTransfersElementsAcrossExecutorTasks()
		{
			LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(2);
			q.Add(_samples[1]);
			q.Add(TestData<T>.Two);
			IExecutorService executor = Executors.NewFixedThreadPool(2);
		    executor.Execute(
		        delegate
		            {
		                Assert.IsFalse(q.Offer(TestData<T>.Three));
		                Assert.IsTrue(q.Offer(TestData<T>.Three, TestData.MediumDelay));
		                Assert.AreEqual(0, q.RemainingCapacity);
		            });

		    executor.Execute(
		        delegate
		            {
		                Thread.Sleep(TestData.SmallDelay);
		                Assert.AreEqual(_samples[1], q.Take());
		            }); 

			JoinPool(executor);
		}

		[Test] public void PollRetrievesElementsAcrossExecutorThreads()
		{
			LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(2);
			IExecutorService executor = Executors.NewFixedThreadPool(2);
		    executor.Execute(
		        delegate
		            {
		                T value;
		                Assert.IsFalse(q.Poll(out value));
		                Assert.IsTrue(q.Poll(TestData.MediumDelay, out value));
		                Assert.IsTrue(q.IsEmpty);
		            });

		    executor.Execute(
		        delegate
		            {
		                Thread.Sleep(TestData.SmallDelay);
		                q.Put(_samples[1]);
		            });

			JoinPool(executor);
		}

        public void JoinPool(IExecutorService exec)
        {
            try
            {
                exec.Shutdown();
                Assert.IsTrue(exec.AwaitTermination(TestData.LongDelay));
            }
            catch (SecurityException)
            {
                // Allowed in case test doesn't have privs
            }
        }

    }
}