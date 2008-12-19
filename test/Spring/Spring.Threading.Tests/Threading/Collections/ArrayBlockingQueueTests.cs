using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using Spring.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;
using Spring.Threading.Future;
using Spring.Util;

namespace Spring.Threading.Collections
{
	[TestFixture]
	public class ArrayBlockingQueueTests : BaseThreadingTestCase
	{
        public class SimpleExecutorService : IExecutorService
		{
			public SimpleExecutorService(int size)
			{
			}

			
            #region IExecutorService Members

            public bool IsShutdown
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsTerminated
			{
				get { throw new NotImplementedException(); }
			}

			public void Shutdown()
			{
			}

			public IList ShutdownNow()
			{
				throw new NotImplementedException();
			}

			public bool AwaitTermination(TimeSpan timeSpan)
			{
				return true;
			}

			public IFuture Submit(ICallable task)
			{
				throw new NotImplementedException();
			}

			public IFuture Submit(IRunnable task, object result)
			{
				throw new NotImplementedException();
			}

			public IFuture Submit(IRunnable task)
			{
				throw new NotImplementedException();
			}

			public IList InvokeAll(ICollection tasks)
			{
				throw new NotImplementedException();
			}

			public IList InvokeAll(ICollection tasks, TimeSpan durationToWait)
			{
				throw new NotImplementedException();
			}

			public object InvokeAny(ICollection tasks)
			{
				throw new NotImplementedException();
			}

			public object InvokeAny(ICollection tasks, TimeSpan durationToWait)
			{
				throw new NotImplementedException();
			}

            public IFuture Submit(Task task, object result)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IFuture Submit(Task task)
            {
                throw new Exception("The method or operation is not implemented.");
            }

//            public IFuture<T> Submit<T>(IRunnable task, T result)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public IFuture<T> Submit<T>(Task task, T result)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public IFuture<T> Submit<T>(ICallable<T> callable)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public IFuture<T> Submit<T>(Call<T> call)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }

            public System.Collections.Generic.IList<IFuture> InvokeAll(System.Collections.Generic.ICollection<ICallable> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.ICollection<ICallable<T>> tasks)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.ICollection<Call<T>> tasks)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.IEnumerable<ICallable<T>> tasks)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.IEnumerable<Call<T>> tasks)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture> InvokeAll(System.Collections.Generic.ICollection<ICallable> tasks, TimeSpan durationToWait)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.ICollection<Call<T>> tasks, TimeSpan durationToWait)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.IEnumerable<ICallable<T>> tasks, TimeSpan durationToWait)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//
//            public System.Collections.Generic.IList<IFuture<T>> InvokeAll<T>(System.Collections.Generic.IEnumerable<Call<T>> tasks, TimeSpan durationToWait)
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }

            public object InvokeAny(System.Collections.Generic.ICollection<ICallable> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.ICollection<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.ICollection<Call<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.IEnumerable<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.IEnumerable<Call<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public object InvokeAny(System.Collections.Generic.ICollection<ICallable> tasks, TimeSpan durationToWait)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.ICollection<Call<T>> tasks, TimeSpan durationToWait)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.IEnumerable<ICallable<T>> tasks, TimeSpan durationToWait)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(System.Collections.Generic.IEnumerable<Call<T>> tasks, TimeSpan durationToWait)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IExecutor Members


            public void Execute(Task task)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Execute(IRunnable command)
			{
				new Thread(new ThreadStart(command.Run));
			}
            #endregion

			
		}
		[Test]
		public void OfferInExecutor()
		{
			ArrayBlockingQueue<> q = new ArrayBlockingQueue(2);
			q.Add(one);
			q.Add(two);
			IExecutorService executor = new SimpleExecutorService(2);
			executor.Execute(new AnonymousClassRunnable7(q));
			executor.Execute(new AnonymousClassRunnable8(q));

			JoinPool(executor);
		}


		[Test]
		public void PollInExecutor()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			IExecutorService executor = new SimpleExecutorService(2);
			executor.Execute(new AnonymousClassRunnable9(q));

			executor.Execute(new AnonymousClassRunnable10(q));

			JoinPool(executor);
		}


		[Test]
		public void Serialization()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);

			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, q);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();

			ArrayBlockingQueue r = (ArrayBlockingQueue) formatter2.Deserialize(bin);
			Assert.AreEqual(q.Count, r.Count);
			while (!(q.Count == 0))
				Assert.AreEqual(q.Remove(), r.Remove());
		}

		private class AnonymousClassRunnable : IRunnable
		{
			public AnonymousClassRunnable(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				int Added = 0;
				try
				{
					for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
					{
						q.Put(i);
						++Added;
					}
					q.Put(DEFAULT_COLLECTION_SIZE);
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
					Assert.AreEqual(Added, DEFAULT_COLLECTION_SIZE);
				}
			}
		}

		private class AnonymousClassRunnable1 : IRunnable
		{
			public AnonymousClassRunnable1(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				int Added = 0;
				try
				{
					q.Put(new Object());
					++Added;
					q.Put(new Object());
					++Added;
					q.Put(new Object());
					++Added;
					q.Put(new Object());
					++Added;
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
					Assert.IsTrue(Added >= 2);
				}
			}
		}

		private class AnonymousClassRunnable2 : IRunnable
		{
			public AnonymousClassRunnable2(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				try
				{
					q.Put(new Object());
					q.Put(new Object());
					Assert.IsFalse(q.Offer(new Object(), SHORT_DELAY_MS));
					q.Offer(new Object(), LONG_DELAY_MS);
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable3 : IRunnable
		{
			public AnonymousClassRunnable3(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;


			public virtual void Run()
			{
				try
				{
					q.Take();
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable4 : IRunnable
		{
			private ArrayBlockingQueue _q;

			public AnonymousClassRunnable4(ArrayBlockingQueue q)
			{
				_q = q;
			}


			public virtual void Run()
			{
				try
				{
					for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
					{
						Assert.AreEqual(i, ((Int32) _q.Take()));
					}
					_q.Take();
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
					Assert.AreEqual(0, _q.Count);
				}
			}
		}

		private class AnonymousClassRunnable5 : IRunnable
		{
			private ArrayBlockingQueue _q;

			public AnonymousClassRunnable5(ArrayBlockingQueue q)
			{
				_q = q;
			}

			public virtual void Run()
			{
				try
				{
					for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
					{
						Assert.AreEqual(i, ((Int32) _q.Poll(SHORT_DELAY_MS)));
					}
					Assert.IsNull(_q.Poll(SHORT_DELAY_MS));
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable6 : IRunnable
		{
			public AnonymousClassRunnable6(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				try
				{
					Assert.IsNull(q.Poll(SHORT_DELAY_MS));
					q.Poll(LONG_DELAY_MS);
					q.Poll(LONG_DELAY_MS);
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable7 : IRunnable
		{
			public AnonymousClassRunnable7(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;


			public virtual void Run()
			{
				Assert.IsFalse(q.Offer(three));
				try
				{
					Assert.IsTrue(q.Offer(three, MEDIUM_DELAY_MS));
					Assert.AreEqual(0, q.RemainingCapacity);
				}
				catch (ThreadInterruptedException)
				{
					throw;
				}
			}
		}

		private class AnonymousClassRunnable8 : IRunnable
		{
			public AnonymousClassRunnable8(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				try
				{
					Thread.Sleep(SMALL_DELAY_MS);
					Assert.AreEqual(one, q.Take());
				}
				catch (ThreadInterruptedException)
				{
					throw;
				}
			}
		}

		private class AnonymousClassRunnable9 : IRunnable
		{
			public AnonymousClassRunnable9(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;

			public virtual void Run()
			{
				Assert.IsNull(q.Poll());
				try
				{
					Assert.IsTrue(null != q.Poll(MEDIUM_DELAY_MS));
					Assert.IsTrue((q.Count == 0));
				}
				catch (ThreadInterruptedException)
				{
					throw;
				}
			}
		}

		private class AnonymousClassRunnable10 : IRunnable
		{
			public AnonymousClassRunnable10(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;


			public virtual void Run()
			{
				try
				{
					Thread.Sleep(SMALL_DELAY_MS);
					q.Put(one);
				}
				catch (ThreadInterruptedException)
				{
					throw;
				}
			}
		}

		private class AnonymousClassRunnable11 : IRunnable
		{
			public AnonymousClassRunnable11(ArrayBlockingQueue q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue q;


			public virtual void Run()
			{
				try
				{
					q.Put((DEFAULT_COLLECTION_SIZE + 1));
				}
				catch (ThreadInterruptedException)
				{
					throw;
				}
			}
		}

		private ArrayBlockingQueue populatedQueue(int n)
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(n);
			Assert.IsTrue((q.Count == 0));
			for (int i = 0; i < n; i++)
				Assert.IsTrue(q.Offer(i));
			Assert.IsFalse((q.Count == 0));
			Assert.AreEqual(0, q.RemainingCapacity);
			Assert.AreEqual(n, q.Count);
			return q;
		}


		[Test]
		public void Constructor1()
		{
			Assert.AreEqual(DEFAULT_COLLECTION_SIZE, new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE).RemainingCapacity);
		}


		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Constructor2()
		{
			new ArrayBlockingQueue(0);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Constructor3()
		{
			new ArrayBlockingQueue(1, true, null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InitializeQueueFromCollectionOfNullElements()
		{
			object[] objects = new object[DEFAULT_COLLECTION_SIZE];

			new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE, false, new ArrayList(objects));
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InitializeQueueFromCollectionWithSomeNullElementsStillThrowsException()
		{
			object[] objects = new object[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i)
				objects[i] = i;

			new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE, false, new ArrayList(objects));
		}


		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Constructor6()
		{
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;

			new ArrayBlockingQueue(1, false, new ArrayList(ints));
		}


		[Test]
		public void Constructor7()
		{
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;

			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE, true, new ArrayList(ints));
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				Assert.AreEqual(ints[i], q.Poll());
		}


		[Test]
		public void EmptyFull()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			Assert.IsTrue( q.IsEmpty );
			Assert.AreEqual(2, q.RemainingCapacity);
			q.Add(one);
			Assert.IsFalse( q.IsEmpty );
			q.Add(two);
			Assert.IsFalse( q.IsEmpty );
			Assert.AreEqual(0, q.RemainingCapacity);
			Assert.IsFalse(q.Offer(three));
		}


		[Test]
		public void RemainingCapacity()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, q.RemainingCapacity);
				Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.Count);
				q.Remove();
			}
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.RemainingCapacity);
				Assert.AreEqual(i, q.Count);
				q.Add(i);
			}
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void OfferNull()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(1);
			q.Offer(null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddNull()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(1);
			q.Add(null);
		}


		[Test]
		public void Offer()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(1);
			Assert.IsTrue(q.Offer(zero));
			Assert.IsFalse(q.Offer(one));
		}


		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void Add()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsTrue(q.Add(i));
			}
			Assert.AreEqual(0, q.RemainingCapacity);
			q.Add(DEFAULT_COLLECTION_SIZE);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddAll1()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(1);
			q.AddAll(null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void AddAllSelf()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.AddAll(q);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddCollectionOfNullElements()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			object[] objects = new object[DEFAULT_COLLECTION_SIZE];

			q.AddAll(new ArrayList(objects));
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddCollectionOfSomeNullElements()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			object[] objects = new object[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i)
				objects[i] = i;

			q.AddAll(new ArrayList(objects));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void AddAll4()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(1);
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;

			q.AddAll(new ArrayList(ints));
		}

		[Test]
		public void AddAll5()
		{
			Int32[] empty = new Int32[0];
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);

			Assert.IsFalse(q.AddAll(new ArrayList(empty)));

			Assert.IsTrue(q.AddAll(new ArrayList(ints)));
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				Assert.AreEqual(ints[i], q.Poll());
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void PutNull()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			q.Put(null);
		}


		[Test]
		public void Put()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Int32 I = i;
				q.Put(I);
				Assert.IsTrue(q.Contains(I));
			}
			Assert.AreEqual(0, q.RemainingCapacity);
		}


		[Test]
		public void PutBlocksIfFull()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void PutWithTake()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			q.Take();
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TimedOffer()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(q).Run));

			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Take()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Take()));
			}
		}


		[Test]
		public void TakeFromEmpty()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TakeRemovesElementsUntilEmptyThenBlocks()
		{
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(populatedQueue(DEFAULT_COLLECTION_SIZE)).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Poll()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Poll()));
			}
			Assert.IsNull(q.Poll());
		}


		[Test]
		public void TimedPoll0()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Poll(new TimeSpan(0))));
			}
			Assert.IsNull(q.Poll(new TimeSpan(0)));
		}


		[Test]
		public void TimedPoll()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Poll(SHORT_DELAY_MS)));
			}
			Assert.IsNull(q.Poll(SHORT_DELAY_MS));
		}


		[Test]
		public void InterruptedTimedPoll()
		{
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(populatedQueue(DEFAULT_COLLECTION_SIZE)).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY_MS);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TimedPollWithOffer()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable6(q).Run));
			t.Start();

			Thread.Sleep(SMALL_DELAY_MS);
			Assert.IsTrue(q.Offer(zero, SHORT_DELAY_MS));
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Peek()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Peek()));
				q.Poll();
				Assert.IsTrue(q.Peek() == null || i != ((Int32) q.Peek()));
			}
			Assert.IsNull(q.Peek());
		}


		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void ElementThrowsNoElementsExceptionWhenEmpty()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Element()));
				q.Poll();
			}
			q.Element();
		}


		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void RemoveThrowsNoElementsExceptionWhenEmpty()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Remove()));
			}
			q.Remove();
		}


		[Test]
		public void RemoveElement()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 1; i < DEFAULT_COLLECTION_SIZE; i += 2)
			{
				Assert.IsTrue(q.Remove(i));
			}
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; i += 2)
			{
				Assert.IsTrue(q.Remove(i));
				Assert.IsFalse(q.Remove((i + 1)));
			}
			Assert.IsTrue((q.Count == 0));
		}


		[Test]
		public void Contains()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsTrue(q.Contains(i));
				q.Poll();
				Assert.IsFalse(q.Contains(i));
			}
		}


		[Test]
		public void Clear()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.Clear();
			Assert.IsTrue((q.Count == 0));
			Assert.AreEqual(0, q.Count);
			Assert.AreEqual(DEFAULT_COLLECTION_SIZE, q.RemainingCapacity);
			q.Add(one);
			Assert.IsFalse((q.Count == 0));
			Assert.IsTrue(q.Contains(one));
			q.Clear();
			Assert.IsTrue((q.Count == 0));
		}


		[Test]
		public void ContainsAll()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			ArrayBlockingQueue p = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsFalse(CollectionUtils.ContainsAll(p, q));
				p.Add(i);
			}
			Assert.IsTrue(CollectionUtils.ContainsAll(p, q));
		}

		[Test]
			public void CopyToCompleteArray()
		{
			ArrayBlockingQueue list = new ArrayBlockingQueue(3);
			list.Add(one);
			list.Add(two);
			list.Add(three);

			object[] targetArray = new object[3];

			list.CopyTo(targetArray, 0);

			Assert.AreEqual(3, targetArray.Length);
			Assert.AreEqual(one, targetArray[0]);
			Assert.AreEqual(two, targetArray[1]);
			Assert.AreEqual(three, targetArray[2]);
		}
		[Test]
		public void CopyToPartialArray()
		 {
			 ArrayBlockingQueue list = new ArrayBlockingQueue(3);
			 list.Add(one);
			 list.Add(two);
			 list.Add(three);

			 object[] targetArray = new object[4];

			 list.CopyTo(targetArray, 1);

			 Assert.AreEqual(4, targetArray.Length);
			 Assert.IsNull(targetArray[0]);
			 Assert.AreEqual(one, targetArray[1]);
			 Assert.AreEqual(two, targetArray[2]);
			 Assert.AreEqual(three, targetArray[3]);
		 }
		[Test]
			[ExpectedException(typeof(InvalidCastException))]
			public void CopyToThrowsMisMatchException()
		{
			ArrayBlockingQueue list = new ArrayBlockingQueue(3);
			list.Add(one);
			list.Add(two);
			list.Add(three);

			string[] targetArray = new string[3];

			list.CopyTo(targetArray, 0);
		}
		[Test]
		public void RemoveAll()
		{
			for (int i = 1; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
				ArrayBlockingQueue p = populatedQueue(i);
				CollectionUtils.RemoveAll(q, p);
				Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.Count);
				for (int j = 0; j < i; ++j)
				{
					Int32 I = (Int32) (p.Remove());
					Assert.IsFalse(q.Contains(I));
				}
			}
		}


		[Test]
		public void ToArray()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			Object[] o = q.ToArray();
			for (int i = 0; i < o.Length; i++)
				Assert.AreEqual(o[i], q.Take());
		}


		[Test]
		public void ToArray2()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			object[] ints = new object[DEFAULT_COLLECTION_SIZE];

			ints = q.ToArray(ints);
			for (int i = 0; i < ints.Length; i++)
				Assert.AreEqual(ints[i], q.Take());
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ToArray_BadArg()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.ToArray(null);
		}


		[Test]
		[ExpectedException(typeof (ArrayTypeMismatchException))]
		public void ToArray1_BadArg()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.ToArray(new String[10]);
		}


		[Test]
		public void Iterator()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			IEnumerator it = q.GetEnumerator();
			while (it.MoveNext())
			{
				Assert.AreEqual(it.Current, q.Take());
			}
		}

		[Test]
		public void IteratorOrdering()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(3);
			q.Add(one);
			q.Add(two);
			q.Add(three);

			Assert.AreEqual(0, q.RemainingCapacity, "queue should be full");

			int k = 0;

			for (IEnumerator it = q.GetEnumerator(); it.MoveNext(); )
			{
				int i = ((Int32) (it.Current));
				Assert.AreEqual(++k, i);
			}
			Assert.AreEqual(3, k);
		}


		[Test]
		public void WeaklyConsistentIteration()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(3);
			q.Add(one);
			q.Add(two);
			q.Add(three);
			for (IEnumerator it = q.GetEnumerator(); it.MoveNext(); )
			{
				q.Remove();

				object o = it.Current;
			}
			Assert.AreEqual(0, q.Count);
		}


		[Test]
		public void ArrayBlockingQueueToString()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			String s = q.ToString();
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsTrue(s.IndexOf(Convert.ToString(i)) >= 0);
			}
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void DrainToNull()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(null);
		}


		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void DrainToSelf()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(q);
		}


		[Test]
		public void DrainTo()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			ArrayList l = new ArrayList();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				Assert.AreEqual(l[i], i);
			q.Add(zero);
			q.Add(one);
			Assert.IsFalse((q.Count == 0));
			Assert.IsTrue(q.Contains(zero));
			Assert.IsTrue(q.Contains(one));
			l.Clear();
			q.DrainTo(l);
			Assert.AreEqual(q.Count, 0);
			Assert.AreEqual(l.Count, 2);
			for (int i = 0; i < 2; ++i)
				Assert.AreEqual(l[i], i);
		}


		[Test]
		public void DrainToWithActivePut()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable11(q).Run));
			t.Start();
			ArrayList l = new ArrayList();
			q.DrainTo(l);
			Assert.IsTrue(l.Count >= DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				Assert.AreEqual(l[i], i);
			t.Join();
			Assert.IsTrue(q.Count + l.Count >= DEFAULT_COLLECTION_SIZE);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void DrainToNullN()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(null, 0);
		}


		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void DrainToSelfN()
		{
			ArrayBlockingQueue q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(q, 0);
		}


		[Test]
		public void DrainToN()
		{
			ArrayBlockingQueue q = new ArrayBlockingQueue(DEFAULT_COLLECTION_SIZE*2);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE + 2; ++i)
			{
				for (int j = 0; j < DEFAULT_COLLECTION_SIZE; j++)
					Assert.IsTrue(q.Offer(j));
				ArrayList l = new ArrayList();
				q.DrainTo(l, i);
				int k = (i < DEFAULT_COLLECTION_SIZE) ? i : DEFAULT_COLLECTION_SIZE;
				Assert.AreEqual(l.Count, k);
				Assert.AreEqual(q.Count, DEFAULT_COLLECTION_SIZE - k);
				for (int j = 0; j < k; ++j)
					Assert.AreEqual(l[j], j);
				while (q.Poll() != null)
					;
			}
		}
	}
}