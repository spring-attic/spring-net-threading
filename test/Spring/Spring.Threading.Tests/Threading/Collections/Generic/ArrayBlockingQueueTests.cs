using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using Spring.Collections;
using Spring.Threading.Execution;
using Spring.Threading.Future;
using Spring.Util;

namespace Spring.Threading.Collections.Generic
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

			public IList<IRunnable> ShutdownNow()
			{
				throw new NotImplementedException();
			}

			public bool AwaitTermination(TimeSpan timeSpan)
			{
				return true;
			}

			public IFuture<object> Submit(IRunnable task)
			{
				throw new NotImplementedException();
			}

            public IFuture<object> Submit(Action action)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IFuture<T> Submit<T>(IRunnable runnable, T result)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IFuture<T> Submit<T>(Action action, T result)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IFuture<T> Submit<T>(ICallable<T> callable)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IFuture<T> Submit<T>(Func<T> call)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IList<IFuture<T>> InvokeAll<T>(IEnumerable<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IList<IFuture<T>> InvokeAll<T>(IEnumerable<Func<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params Func<T>[] tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public T InvokeAny<T>(IEnumerable<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(IEnumerable<Func<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IExecutor Members


            public void Execute(Action action)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Execute(IRunnable command)
			{
				new Thread(new ThreadStart(command.Run));
			}
            #endregion



            #region IExecutorService Members


            public IList<IFuture<T>> InvokeAll<T>(params ICallable<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public IList<IFuture<T>> InvokeAll<T>(params Func<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params Func<T>[] tasks)
            {
                throw new NotImplementedException();
            }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<ICallable<T>> tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(params ICallable<T>[] tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<Func<T>> tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(params Func<T>[] tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
		    {
		        throw new NotImplementedException();
		    }

		    public T InvokeAny<T>(params ICallable<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public T InvokeAny<T>(params Func<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public T InvokeAny<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
            {
                throw new NotImplementedException();
            }

            public T InvokeAny<T>(TimeSpan durationToWait, params Func<T>[] tasks)
            {
                throw new NotImplementedException();
            }

		    public void ForEach<T>(IEnumerable<T> source, Action<T> body)
		    {
		        throw new NotImplementedException();
		    }

		    public void ForEach<T>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> body)
		    {
		        throw new NotImplementedException();
		    }

		    #endregion
        }

		[Test]
		public void Serialization()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);

			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, q);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();

			ArrayBlockingQueue<int> r = (ArrayBlockingQueue<int>) formatter2.Deserialize(bin);
			Assert.AreEqual(q.Count, r.Count);
			while (!(q.Count == 0))
				Assert.AreEqual(q.Remove(), r.Remove());
		}

		private class AnonymousClassRunnable : IRunnable
		{
			public AnonymousClassRunnable(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;

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
			public AnonymousClassRunnable1(ArrayBlockingQueue<object> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<object> q;

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
			public AnonymousClassRunnable2(ArrayBlockingQueue<object> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<object> q;

			public virtual void Run()
			{
				try
				{
					q.Put(new Object());
					q.Put(new Object());
					Assert.IsFalse(q.Offer(new Object(), SHORT_DELAY));
					q.Offer(new Object(), LONG_DELAY);
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable3 : IRunnable
		{
			public AnonymousClassRunnable3(ArrayBlockingQueue<object> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<object> q;


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
			private ArrayBlockingQueue<int> _q;

			public AnonymousClassRunnable4(ArrayBlockingQueue<int> q)
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
			private ArrayBlockingQueue<int> _q;

			public AnonymousClassRunnable5(ArrayBlockingQueue<int> q)
			{
				_q = q;
			}

			public virtual void Run()
			{
				try
				{
				    int j;
					for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
					{
                        if ( _q.Poll(SHORT_DELAY, out j))
                        {
						    Assert.AreEqual(i, j);
                        }
					}
					Assert.IsFalse(_q.Poll(SHORT_DELAY, out j));
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable6 : IRunnable
		{
			public AnonymousClassRunnable6(ArrayBlockingQueue<object> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<object> q;

			public virtual void Run()
			{
				try
				{
				    object obj;
					Assert.IsFalse(q.Poll(SHORT_DELAY, out obj));
					Assert.IsNull(obj);
					q.Poll(LONG_DELAY, out obj);
					q.Poll(LONG_DELAY, out obj);
					Assert.Fail("should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		private class AnonymousClassRunnable7 : IRunnable
		{
			public AnonymousClassRunnable7(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;


			public virtual void Run()
			{
				Assert.IsFalse(q.Offer(three));
				try
				{
					Assert.IsTrue(q.Offer(three, MEDIUM_DELAY));
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
			public AnonymousClassRunnable8(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;

			public virtual void Run()
			{
				try
				{
					Thread.Sleep(SMALL_DELAY);
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
			public AnonymousClassRunnable9(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;

			public virtual void Run()
			{
			    int i;
				Assert.IsFalse(q.Poll(out i));
				try
				{
					Assert.IsTrue(q.Poll(MEDIUM_DELAY, out i));
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
			public AnonymousClassRunnable10(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;


			public virtual void Run()
			{
				try
				{
					Thread.Sleep(SMALL_DELAY);
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
			public AnonymousClassRunnable11(ArrayBlockingQueue<int> q)
			{
				this.q = q;
			}

			private ArrayBlockingQueue<int> q;


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

		private ArrayBlockingQueue<int> populatedQueue(int n)
		{
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(n);
			Assert.IsTrue((q.Count == 0));
			for (int i = 0; i < n; i++)
				Assert.IsTrue(q.Offer(i));
			Assert.IsFalse((q.Count == 0));
			Assert.AreEqual(0, q.RemainingCapacity);
			Assert.AreEqual(n, q.Count);
			return q;
		}



		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddAll1()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(1);
			q.AddRange(null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void AddAllSelf()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.AddRange(q);
		}

        // In Java, null is used to indicate empty queue for the Poll related methods.
        // In .NET, out parameter is used in the Poll method and a bool return value
        // is used to indicate a empty queue. So null can well be a valid element in the
        // queue.
		[Test]
		public void AddCollectionOfNullElements()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(DEFAULT_COLLECTION_SIZE);
			object[] objects = new object[DEFAULT_COLLECTION_SIZE];

			q.AddRange(new List<object>(objects));
		}


		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void AddAll4()
		{
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(1);
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;

			q.AddRange(new List<int>(ints));
		}

		[Test]
		public void AddAll5()
		{
			Int32[] empty = new Int32[0];
			Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ints[i] = i;
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(DEFAULT_COLLECTION_SIZE);

		    q.AddRange(new List<int>(empty));
			Assert.IsTrue(q.RemainingCapacity == DEFAULT_COLLECTION_SIZE);

		    q.AddRange(new List<int>(ints));
			Assert.IsFalse(q.RemainingCapacity == DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
			    q.Poll(out j);
				Assert.AreEqual(ints[i], j);
			}
		}


        // In Java, null is used to indicate empty queue for the Poll related methods.
        // In .NET, out parameter is used in the Poll method and a bool return value
        // is used to indicate a empty queue. So null can well be a valid element in the
        // queue.
        [Test]
		public void PutNull()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(DEFAULT_COLLECTION_SIZE);
			q.Put(null);
		}


		[Test]
		public void Put()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(DEFAULT_COLLECTION_SIZE);
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
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(DEFAULT_COLLECTION_SIZE);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void PutWithTake()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY);
			q.Take();
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TimedOffer()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(q).Run));

			t.Start();

			Thread.Sleep(SHORT_DELAY);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Take()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Take()));
			}
		}


		[Test]
		public void TakeFromEmpty()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(2);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(q).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TakeRemovesElementsUntilEmptyThenBlocks()
		{
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(populatedQueue(DEFAULT_COLLECTION_SIZE)).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Poll()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
			    q.Poll(out j);
				Assert.AreEqual(i, j);
			}
			Assert.IsFalse(q.Poll(out j));
		}


		[Test]
		public void TimedPoll0()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
			    q.Poll(new TimeSpan(0), out j);
				Assert.AreEqual(i, j);
			}
			Assert.IsFalse(q.Poll(new TimeSpan(0), out j));
		}


		[Test]
		public void TimedPoll()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
			    q.Poll(SHORT_DELAY, out j);
				Assert.AreEqual(i, j);
			}
			Assert.IsFalse(q.Poll(SHORT_DELAY, out j));
		}


		[Test]
		public void InterruptedTimedPoll()
		{
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(populatedQueue(DEFAULT_COLLECTION_SIZE)).Run));
			t.Start();

			Thread.Sleep(SHORT_DELAY);
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void TimedPollWithOffer()
		{
			ArrayBlockingQueue<object> q = new ArrayBlockingQueue<object>(2);
			Thread t = new Thread(new AnonymousClassRunnable6(q).Run);
			t.Start();

			Thread.Sleep(SMALL_DELAY);
			Assert.IsTrue(q.Offer(zero, SHORT_DELAY));
			t.Interrupt();
			t.Join();
		}


		[Test]
		public void Peek()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
			    q.Peek(out j);
				Assert.AreEqual(i, j);
			    q.Poll(out j);
				Assert.IsTrue(!q.Peek(out j) || i != j);
			}
			Assert.IsFalse(q.Peek(out j));
		}


		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void ElementThrowsNoElementsExceptionWhenEmpty()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Element()));
				q.Poll(out j);
			}
			q.Element();
		}


		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void RemoveThrowsNoElementsExceptionWhenEmpty()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(i, ((Int32) q.Remove()));
			}
			q.Remove();
		}


		[Test]
		public void RemoveElement()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
		    int j;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsTrue(q.Contains(i));
				q.Poll(out j);
				Assert.IsFalse(q.Contains(i));
			}
		}


		[Test]
		public void Clear()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			ArrayBlockingQueue<int> p = new ArrayBlockingQueue<int>(DEFAULT_COLLECTION_SIZE);
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
			ArrayBlockingQueue<int> list = new ArrayBlockingQueue<int>(3);
			list.Add(one);
			list.Add(two);
			list.Add(three);

			int[] targetArray = new int[3];

			list.CopyTo(targetArray, 0);

			Assert.AreEqual(3, targetArray.Length);
			Assert.AreEqual(one, targetArray[0]);
			Assert.AreEqual(two, targetArray[1]);
			Assert.AreEqual(three, targetArray[2]);
		}
		[Test]
		public void CopyToPartialArray()
		 {
			 ArrayBlockingQueue<int> list = new ArrayBlockingQueue<int>(3);
			 list.Add(one);
			 list.Add(two);
			 list.Add(three);

			 int[] targetArray = new int[4];

			 list.CopyTo(targetArray, 1);

			 Assert.AreEqual(4, targetArray.Length);
			 Assert.AreEqual(zero, targetArray[0]);
			 Assert.AreEqual(one, targetArray[1]);
			 Assert.AreEqual(two, targetArray[2]);
			 Assert.AreEqual(three, targetArray[3]);
		 }

		[Test]
		public void Iterator()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			IEnumerator it = q.GetEnumerator();
			while (it.MoveNext())
			{
				Assert.AreEqual(it.Current, q.Take());
			}
		}

		[Test]
		public void IteratorOrdering()
		{
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(3);
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
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(3);
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo((ICollection<int>)null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DrainToSelf()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(q);
		}


		[Test]
		public void DrainTo()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			IList<int> l = new List<int>();
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable11(q).Run));
			t.Start();
			IList<int> l = new List<int>();
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
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo((ICollection<int>)null, 0);
		}


		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DrainToSelfN()
		{
			ArrayBlockingQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
			q.DrainTo(q, 0);
		}


		[Test]
		public void DrainToN()
		{
			ArrayBlockingQueue<int> q = new ArrayBlockingQueue<int>(DEFAULT_COLLECTION_SIZE*2);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE + 2; ++i)
			{
				for (int j = 0; j < DEFAULT_COLLECTION_SIZE; j++)
					Assert.IsTrue(q.Offer(j));
				IList<int> l = new List<int>();
				q.DrainTo(l, i);
				int k = (i < DEFAULT_COLLECTION_SIZE) ? i : DEFAULT_COLLECTION_SIZE;
				Assert.AreEqual(l.Count, k);
				Assert.AreEqual(q.Count, DEFAULT_COLLECTION_SIZE - k);
				for (int j = 0; j < k; ++j)
					Assert.AreEqual(l[j], j);
			    int x;
				while (q.Poll(out x))
				{
				}
			}
		}
	}
}