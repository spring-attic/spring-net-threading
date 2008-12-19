using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
	[TestFixture]
	public class AbstractExecutorServiceTests : BaseThreadingTestCase
	{
		private class AnonymousClassRunnable : IRunnable
		{
			public AnonymousClassRunnable(ThreadPoolExecutor p)
			{
				this.p = p;
			}

			private class AnonymousClassCallable : ICallable
			{
				public AnonymousClassCallable()
				{
				}

				public virtual Object Call()
				{
					try
					{
						Thread.Sleep(MEDIUM_DELAY_MS);
						Assert.Fail("Should throw an exception");
					}
					catch (ThreadInterruptedException)
					{
					}
					return null;
				}
			}

			private ThreadPoolExecutor p;

			public virtual void Run()
			{
				try
				{
					p.Submit(new AnonymousClassCallable()).GetResult();
				}
				catch (ThreadInterruptedException)
				{
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}

		private class AnonymousClassCallable : ICallable
		{
			public AnonymousClassCallable(ThreadPoolExecutor p)
			{
				this.p = p;
			}

			private ThreadPoolExecutor p;

			public virtual Object Call()
			{
				try
				{
					p.Submit(new SmallCallable()).GetResult();
					Assert.Fail("Should throw an exception");
				}
				catch (ThreadInterruptedException)
				{
				}
				catch (RejectedExecutionException)
				{
				}
				catch (ExecutionException)
				{
				}
				return true;
			}
		}

		private class AnonymousClassRunnable1 : IRunnable
		{
			public AnonymousClassRunnable1(ICallable c)
			{
				this.c = c;
			}

			private ICallable c;

			public virtual void Run()
			{
				try
				{
					c.Call();
				}
				catch (Exception)
				{
				}
			}
		}

		private class AnonymousClassCallable1 : ICallable
		{
			public AnonymousClassCallable1()
			{
			}

			public virtual Object Call()
			{
				int zero = 0;
				int i = 5/zero;
				return true;
			}
		}

		internal class DirectExecutorService : AbstractExecutorService
		{
			public override bool IsShutdown
			{
				get { return _shutdown; }

			}

			public override bool IsTerminated
			{
				get { return IsShutdown; }

			}

			public override void Execute(IRunnable r)
			{
				r.Run();
			}

			public override void Shutdown()
			{
				_shutdown = true;
			}

			public override IList<IRunnable> ShutdownNow()
			{
				_shutdown = true;
			    return (IList<IRunnable>) ArrayList.ReadOnly(((IList) new List<IRunnable>()));
			}

			public override bool AwaitTermination(TimeSpan duration)
			{
				return IsShutdown;
			}

			private volatile bool _shutdown = false;
		}


		[Test]
		public void ExecuteRunnable()
		{
			IExecutorService e = new DirectExecutorService();
			TrackedShortRunnable task = new TrackedShortRunnable();
			Assert.IsFalse(task.IsDone);
			IFuture future = e.Submit(task);
			future.GetResult();
			Assert.IsTrue(task.IsDone);
		}


		[Test]
		public void SubmitCallable()
		{
			IExecutorService e = new DirectExecutorService();
			IFuture future = e.Submit(new StringTask());
			String result = (String) future.GetResult();
			Assert.AreSame(TEST_STRING, result);
		}


		[Test]
		public void SubmitRunnable()
		{
			IExecutorService e = new DirectExecutorService();
			IFuture future = e.Submit(new NullRunnable());
			future.GetResult();
			Assert.IsTrue(future.IsDone);
		}


		[Test]
		public void SubmitRunnable2()
		{
			IExecutorService e = new DirectExecutorService();
			IFuture future = e.Submit(new NullRunnable(), TEST_STRING);
			String result = (String) future.GetResult();
			Assert.AreSame(TEST_STRING, result);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ExecuteNullRunnable()
		{
			IExecutorService e = new DirectExecutorService();
			TrackedShortRunnable task = null;
			e.Submit(task);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SubmitNullCallable()
		{
			IExecutorService e = new DirectExecutorService();
			StringTask t = null;
			e.Submit(t);
		}


		[Test]
		[Ignore("Run test when ThreadPoolExecutor & ArrayBlockingQueue are implemented.")]
		public void Execute1()
		{
			ThreadPoolExecutor p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 0, 1), new ArrayBlockingQueue<IRunnable>(1));
			try
			{
				for (int i = 0; i < 5; ++i)
				{
					p.Submit(new MediumRunnable());
				}
				Assert.Fail("Should throw an exception.");
			}
			catch (RejectedExecutionException)
			{
			}
			finally
			{
				JoinPool(p);
			}
		}


		[Test]
		[Ignore("Run test when ThreadPoolExecutor & ArrayBlockingQueue are implemented.")]
		public void Execute2()
		{
			ThreadPoolExecutor p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 0, 1), new ArrayBlockingQueue<IRunnable>(1));
			try
			{
				for (int i = 0; i < 5; ++i)
				{
					p.Submit(new SmallCallable());
				}
				Assert.Fail("Should throw an exception.");
			}
			catch (RejectedExecutionException)
			{
			}
			JoinPool(p);
		}


		[Test]
		[Ignore("Run test when ThreadPoolExecutor & ArrayBlockingQueue are implemented.")]
		public void InterruptedSubmit()
		{
//			ThreadPoolExecutor p = new ThreadPoolExecutor(1, 1, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(10));
//			SupportClass.ThreadClass t = new SupportClass.ThreadClass(new System.Threading.ThreadStart(new AnonymousClassRunnable(pthis).Run));
//			try
//			{
//				t.Start();
//				//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangThreadsleep_long_3"'
//				System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * SHORT_DELAY_MS));
//				t.Interrupt();
//			}
//			catch (System.Exception e)
//			{
//				throw;
//			}
//			JoinPool(p);
		}


		[Test]
		[Ignore("Run test when ThreadPoolExecutor & ArrayBlockingQueue are implemented.")]
		public void SubmitIE()
		{
//			//UPGRADE_NOTE: Final was removed from the declaration of 'p '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003_3"'
//			ThreadPoolExecutor p = new ThreadPoolExecutor(1, 1, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(10));
//		
//			//UPGRADE_NOTE: Final was removed from the declaration of 'c '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003_3"'
//			Callable c = new AnonymousClassCallable(pthis);
//		
//		
//		
//			SupportClass.ThreadClass t = new SupportClass.ThreadClass(new System.Threading.ThreadStart(new AnonymousClassRunnable1(c, this).Run));
//			try
//			{
//				t.Start();
//				//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangThreadsleep_long_3"'
//				System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * SHORT_DELAY_MS));
//				t.Interrupt();
//				t.Join();
//			}
//			catch (System.Threading.ThreadInterruptedException e)
//			{
//				throw;
//			}
//		
//			JoinPool(p);
		}


		[Test]
		[Ignore("Run test when ThreadPoolExecutor & ArrayBlockingQueue are implemented.")]
		public void SubmitEE()
		{
//			ThreadPoolExecutor p = new ThreadPoolExecutor(1, 1, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(10));
//		
//			try
//			{
//				Callable c = new AnonymousClassCallable1(this);
//			
//				for (int i = 0; i < 5; i++)
//				{
//					p.Submit(c).GetResult();
//				}
//			
//				Assert.Fail("Should throw an exception.");
//			}
//			catch (ExecutionException)
//			{
//			}
//			catch (System.Exception e)
//			{
//				throw;
//			}
//			JoinPool(p);
		}


		[Test]
		public void InvokeAny1()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAny((ICollection)null);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAny2()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAny(new ArrayList());
			}
			catch (ArgumentException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAny3()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(null);
				e.InvokeAny(l);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAny4()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new NPETask());
				e.InvokeAny(l);
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAny5()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(new StringTask());
				String result = (String) e.InvokeAny(l);
				Assert.AreSame(TEST_STRING, result);
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAll1()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAll((ICollection)null);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAll2()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				IList r = e.InvokeAll(new ArrayList());
				Assert.IsTrue((r.Count == 0));
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAll3()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(null);
				e.InvokeAll(l);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAll4()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new NPETask());
				IList result = e.InvokeAll(l);
				Assert.AreEqual(1, result.Count);
				foreach (IFuture future in result)
				{
					future.GetResult();
				}
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void InvokeAll5()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(new StringTask());
				IList result = e.InvokeAll(l);
				Assert.AreEqual(2, result.Count);
				foreach (IFuture future in result)
				{
					Assert.AreSame(TEST_STRING, future.GetResult());
				}
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAny1()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAny((ICollection)null, MEDIUM_DELAY_MS);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAnyNull()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAny((ICollection)null, new TimeSpan(0));
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAny2()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAny(new ArrayList(), MEDIUM_DELAY_MS);
			}
			catch (ArgumentException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAny3()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(null);
				e.InvokeAny(l, MEDIUM_DELAY_MS);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAny4()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new NPETask());
				e.InvokeAny(l, MEDIUM_DELAY_MS);
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAny5()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(new StringTask());
				String result = (String) e.InvokeAny(l, MEDIUM_DELAY_MS);
				Assert.AreSame(TEST_STRING, result);
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAll1()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAll((ICollection)null, MEDIUM_DELAY_MS);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAllNullTimeUnit()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				e.InvokeAll((ICollection)null, MEDIUM_DELAY_MS);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAll2()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				IList r = e.InvokeAll(new ArrayList(), MEDIUM_DELAY_MS);
				Assert.IsTrue((r.Count == 0));
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAll3()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(null);
				e.InvokeAll(l, MEDIUM_DELAY_MS);
			}
			catch (ArgumentNullException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAll4()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new NPETask());
				IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
				Assert.AreEqual(1, result.Count);
				foreach (IFuture future in result)
				{
					future.GetResult();
				}
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		public void TimedInvokeAll5()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(new StringTask());
				IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
				Assert.AreEqual(2, result.Count);
				foreach (IFuture future in result)
				{
					Assert.AreSame(TEST_STRING, future.GetResult());
				}
			}
			catch (ExecutionException)
			{
			}
			finally
			{
				JoinPool(e);
			}
		}


		[Test]
		[Ignore("Run when Executors is ported.")]
		public void TimedInvokeAll6()
		{
			IExecutorService e = new DirectExecutorService();
			try
			{
				ArrayList l = new ArrayList();
				l.Add(new StringTask());
				l.Add(Executors.CreateCallable(new MediumPossiblyInterruptedRunnable(), TEST_STRING));
				l.Add(new StringTask());
				IList result = e.InvokeAll(l, SMALL_DELAY_MS);
				Assert.AreEqual(3, result.Count);
				IEnumerator it = result.GetEnumerator();
				IFuture f1 = (IFuture) it.Current;
				IFuture f2 = (IFuture) it.Current;
				IFuture f3 = (IFuture) it.Current;
				Assert.IsTrue(f1.IsDone);
				Assert.IsFalse(f1.IsCancelled);
				Assert.IsTrue(f2.IsDone);
				Assert.IsTrue(f3.IsDone);
				Assert.IsTrue(f3.IsCancelled);
			}
			finally
			{
				JoinPool(e);
			}
		}
	}
}