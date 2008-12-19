using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.Execution;
using Spring.Threading.Future;

namespace Spring.Threading
{
	public class BaseThreadingTestCase
	{
		protected static Int32 zero = Int32.Parse("0");
		protected static Int32 one = Int32.Parse("1");
		protected static Int32 two = Int32.Parse("2");
		protected static Int32 three = Int32.Parse("3");
		protected static Int32 four = Int32.Parse("4");
		protected static Int32 five = Int32.Parse("5");
		protected static Int32 six = Int32.Parse("6");
		protected static Int32 seven = Int32.Parse("7");
		protected static Int32 eight = Int32.Parse("8");
		protected static Int32 nine = Int32.Parse("9");
		protected static Int32 m1 = Int32.Parse("-1");
		protected static Int32 m2 = Int32.Parse("-2");
		protected static Int32 m3 = Int32.Parse("-3");
		protected static Int32 m4 = Int32.Parse("-4");
		protected static Int32 m5 = Int32.Parse("-5");
		protected static Int32 m10 = Int32.Parse("-10");

		public static int DEFAULT_COLLECTION_SIZE = 20;

		public static TimeSpan SHORT_DELAY_MS;
		public static TimeSpan SMALL_DELAY_MS;
		public static TimeSpan MEDIUM_DELAY_MS;
		public static TimeSpan LONG_DELAY_MS;

		public const string TEST_STRING = "a test string";

		protected BaseThreadingTestCase()
		{
			SHORT_DELAY_MS = new TimeSpan(0, 0, 0, 0, 300);
			SMALL_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds*5);
			MEDIUM_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds*10);
			LONG_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds * 20);
		}

		public void JoinPool(IExecutorService exec)
		{
			try
			{
				exec.Shutdown();
				Assert.IsTrue(exec.AwaitTermination(LONG_DELAY_MS));
			}
			catch (ThreadInterruptedException)
			{
				Assert.Fail("Unexpected exception");
			}
		}

	}

	internal class SmallRunnable : IRunnable
	{
		public void Run()
		{
			Thread.Sleep(BaseThreadingTestCase.SMALL_DELAY_MS);
		}
	}

	internal class ShortDelayClassRunnable : IRunnable
	{
		public ShortDelayClassRunnable()
		{
		}

		public virtual void Run()
		{
			Thread.Sleep(BaseThreadingTestCase.SHORT_DELAY_MS);
		}
	}

	internal class ShortRunnable : IRunnable
	{
		internal volatile bool done = false;

		public virtual void Run()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.SHORT_DELAY_MS);
				done = true;
			}
			catch (Exception)
			{
			}
		}

		public bool IsDone
		{
			get { return done; }
		}

	}

	internal class SmallCallable : ICallable
	{
		public Object Call()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.SMALL_DELAY_MS);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			return true;
		}
	}

	internal class TrackedLongRunnable : IRunnable
	{
		private volatile bool done = false;

		public void Run()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.LONG_DELAY_MS);
				done = true;
			}
			catch (Exception)
			{
			}
		}

		public bool IsDone
		{
			get { return done; }
		}
	}

	internal class TrackedNoOpRunnable : IRunnable
	{
		private volatile bool done = false;

		public void Run()
		{
			done = true;
		}

		public bool IsDone
		{
			get { return done; }
		}
	}

	internal class NoOpRunnable : IRunnable
	{
		public void Run()
		{
		}
	}

	internal class TrackedShortRunnable : IRunnable
	{
		internal volatile bool done = false;

		public virtual void Run()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.SMALL_DELAY_MS);
				done = true;
			}
			catch (Exception)
			{
			}
		}

		public bool IsDone
		{
			get { return done; }
		}
	}

	internal class StringTask : ICallable
	{
		public object Call()
		{
			return BaseThreadingTestCase.TEST_STRING;
		}
	}

	internal class MediumRunnable : IRunnable
	{
		public MediumRunnable()
		{
		}

		public virtual void Run()
		{
			Thread.Sleep(BaseThreadingTestCase.MEDIUM_DELAY_MS);
		}
	}

	internal class NPETask : ICallable
	{
		public virtual Object Call()
		{
			throw new NullReferenceException();
		}
	}

	internal class MediumPossiblyInterruptedRunnable : IRunnable
	{
		public MediumPossiblyInterruptedRunnable()
		{
		}

		public virtual void Run()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.MEDIUM_DELAY_MS);
			}
			catch (ThreadInterruptedException)
			{
			}
		}
	}

	internal class TrackedCallable : ICallable
	{
		public volatile bool done = false;

		public Object Call()
		{
			try
			{
				Thread.Sleep(BaseThreadingTestCase.SMALL_DELAY_MS);
				done = true;
			}
			catch (Exception)
			{
			}
			return true;
		}
	}

	internal class NoOpREHandler : IRejectedExecutionHandler
	{
		public void RejectedExecution(IRunnable r, IExecutorService executor)
		{
		}
	}

	internal class SimpleThreadFactory : IThreadFactory
	{
		public SimpleThreadFactory()
		{
		}

		#region IThreadFactory Members

		public Thread NewThread(IRunnable runnable)
		{
			return new Thread(new ThreadStart(runnable.Run));
		}

		#endregion
	}

	internal class NoOpExecutorService : IExecutorService
	{
		#region IExecutorService Members

		public object InvokeAny(System.Collections.ICollection tasks, TimeSpan durationToWait)
		{
			// TODO:  Add NoOpExecutorService.InvokeAny implementation
			return null;
		}

		object Spring.Threading.Execution.IExecutorService.InvokeAny(System.Collections.ICollection tasks)
		{
			// TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.InvokeAny implementation
			return null;
		}

		public System.Collections.IList InvokeAll(System.Collections.ICollection tasks, TimeSpan durationToWait)
		{
			// TODO:  Add NoOpExecutorService.InvokeAll implementation
			return null;
		}

	    public IFuture Submit(Task task)
	    {
	        throw new System.NotImplementedException();
	    }

	    System.Collections.IList Spring.Threading.Execution.IExecutorService.InvokeAll(System.Collections.ICollection tasks)
		{
			// TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.InvokeAll implementation
			return null;
		}

		public IList<IRunnable> ShutdownNow()
		{
			// TODO:  Add NoOpExecutorService.ShutdownNow implementation
			return null;
		}

		public bool IsTerminated
		{
			get
			{
				// TODO:  Add NoOpExecutorService.IsTerminated getter implementation
				return false;
			}
		}

		public void Shutdown()
		{
			// TODO:  Add NoOpExecutorService.Shutdown implementation
		}

		public bool IsShutdown
		{
			get
			{
				// TODO:  Add NoOpExecutorService.IsShutdown getter implementation
				return false;
			}
		}

		public Spring.Threading.Future.IFuture Submit(IRunnable task)
		{
			// TODO:  Add NoOpExecutorService.Submit implementation
			return null;
		}

	    public IFuture Submit(Task task, object result)
	    {
	        throw new System.NotImplementedException();
	    }

	    Spring.Threading.Future.IFuture Spring.Threading.Execution.IExecutorService.Submit(IRunnable task, object result)
		{
			// TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.Submit implementation
			return null;
		}

		Spring.Threading.Future.IFuture Spring.Threading.Execution.IExecutorService.Submit(ICallable task)
		{
			// TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.Submit implementation
			return null;
		}

		public bool AwaitTermination(TimeSpan timeSpan)
		{
			// TODO:  Add NoOpExecutorService.AwaitTermination implementation
			return false;
		}

		#endregion

		#region IExecutor Members

		public void Execute(IRunnable command)
		{
			// TODO:  Add NoOpExecutorService.Execute implementation
		}

	    public void Execute(Task task)
	    {
	        throw new System.NotImplementedException();
	    }

	    #endregion

	}
}