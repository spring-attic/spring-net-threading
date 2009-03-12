using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.Execution;
using Spring.Threading.Future;

namespace Spring.Threading
{
    public class BaseThreadingTestCase
    {
        public const string TEST_STRING = "a test string";
        public static int DEFAULT_COLLECTION_SIZE = 20;
        protected static Int32 eight = Int32.Parse("8");
        protected static Int32 five = Int32.Parse("5");
        protected static Int32 four = Int32.Parse("4");
        public static TimeSpan LONG_DELAY_MS;
        protected static Int32 m1 = Int32.Parse("-1");
        protected static Int32 m10 = Int32.Parse("-10");
        protected static Int32 m2 = Int32.Parse("-2");
        protected static Int32 m3 = Int32.Parse("-3");
        protected static Int32 m4 = Int32.Parse("-4");
        protected static Int32 m5 = Int32.Parse("-5");
        public static TimeSpan MEDIUM_DELAY_MS;
        protected static Int32 nine = Int32.Parse("9");
        protected static Int32 one = Int32.Parse("1");
        protected static Int32 seven = Int32.Parse("7");

        public static TimeSpan SHORT_DELAY_MS;
        protected static Int32 six = Int32.Parse("6");
        public static TimeSpan SMALL_DELAY_MS;
        protected static Int32 three = Int32.Parse("3");
        protected static Int32 two = Int32.Parse("2");
        protected static Int32 zero = Int32.Parse("0");

        private volatile bool threadFailed;

        protected BaseThreadingTestCase()
        {
            SHORT_DELAY_MS = new TimeSpan(0, 0, 0, 0, 300);
            SMALL_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds*5);
            MEDIUM_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds*10);
            LONG_DELAY_MS = new TimeSpan(0, 0, 0, 0, SHORT_DELAY_MS.Milliseconds*50);
        }

        public void UnexpectedException()
        {
            Assert.Fail("Unexpected exception");
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

        public void ThreadUnexpectedException()
        {
            threadFailed = true;
            Assert.Fail("Unexpected exception");
        }
    }

    internal class SmallRunnable : IRunnable
    {
        #region IRunnable Members

        public void Run()
        {
            Thread.Sleep(BaseThreadingTestCase.SMALL_DELAY_MS);
        }

        #endregion
    }

    internal class ShortDelayClassRunnable : IRunnable
    {
        #region IRunnable Members

        public virtual void Run()
        {
            Thread.Sleep(BaseThreadingTestCase.SHORT_DELAY_MS);
        }

        #endregion
    }

    internal class ShortRunnable : IRunnable
    {
        internal volatile bool done;

        public bool IsDone
        {
            get { return done; }
        }

        #region IRunnable Members

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

        #endregion
    }

    internal class SmallCallable : ICallable
    {
        #region ICallable Members

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

        #endregion
    }

    internal class TrackedLongRunnable : IRunnable
    {
        private volatile bool done;

        public bool IsDone
        {
            get { return done; }
        }

        #region IRunnable Members

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

        #endregion
    }

    internal class TrackedNoOpRunnable : IRunnable
    {
        private volatile bool done;

        public bool IsDone
        {
            get { return done; }
        }

        #region IRunnable Members

        public void Run()
        {
            done = true;
        }

        #endregion
    }

    internal class NoOpRunnable : IRunnable
    {
        #region IRunnable Members

        public void Run()
        {
        }

        #endregion
    }

    internal class TrackedShortRunnable : IRunnable
    {
        internal volatile bool done;

        public bool IsDone
        {
            get { return done; }
        }

        #region IRunnable Members

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

        #endregion
    }

    internal class StringTask : ICallable
    {
        #region ICallable Members

        public object Call()
        {
            return BaseThreadingTestCase.TEST_STRING;
        }

        #endregion
    }

    internal class MediumRunnable : IRunnable
    {
        #region IRunnable Members

        public virtual void Run()
        {
            Thread.Sleep(BaseThreadingTestCase.MEDIUM_DELAY_MS);
        }

        #endregion
    }

    internal class NPETask : ICallable
    {
        #region ICallable Members

        public virtual Object Call()
        {
            throw new NullReferenceException();
        }

        #endregion
    }

    internal class MediumPossiblyInterruptedRunnable : IRunnable
    {
        #region IRunnable Members

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

        #endregion
    }

    internal class TrackedCallable : ICallable
    {
        public volatile bool done;

        #region ICallable Members

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

        #endregion
    }

    internal class NoOpREHandler : IRejectedExecutionHandler
    {
        #region IRejectedExecutionHandler Members

        public void RejectedExecution(IRunnable r, IExecutorService executor)
        {
        }

        #endregion
    }

    internal class SimpleThreadFactory : IThreadFactory
    {
        #region IThreadFactory Members

        public Thread NewThread(IRunnable runnable)
        {
            return new Thread(runnable.Run);
        }

        #endregion
    }

    internal class NoOpExecutorService : IExecutorService
    {
        #region IExecutorService Members

        public object InvokeAny(ICollection tasks, TimeSpan durationToWait)
        {
            // TODO:  Add NoOpExecutorService.InvokeAny implementation
            return null;
        }

        object IExecutorService.InvokeAny(ICollection tasks)
        {
            // TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.InvokeAny implementation
            return null;
        }

        public IList InvokeAll(ICollection tasks, TimeSpan durationToWait)
        {
            // TODO:  Add NoOpExecutorService.InvokeAll implementation
            return null;
        }

        public IFuture Submit(Task task)
        {
            throw new NotImplementedException();
        }

        IList IExecutorService.InvokeAll(ICollection tasks)
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

        public IFuture Submit(IRunnable task)
        {
            // TODO:  Add NoOpExecutorService.Submit implementation
            return null;
        }

        public IFuture Submit(Task task, object result)
        {
            throw new NotImplementedException();
        }

        IFuture IExecutorService.Submit(IRunnable task, object result)
        {
            // TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.Submit implementation
            return null;
        }

        IFuture IExecutorService.Submit(ICallable task)
        {
            // TODO:  Add NoOpExecutorService.Spring.Threading.Execution.IExecutorService.Submit implementation
            return null;
        }

        public bool AwaitTermination(TimeSpan timeSpan)
        {
            // TODO:  Add NoOpExecutorService.AwaitTermination implementation
            return false;
        }

        public void Execute(IRunnable command)
        {
            // TODO:  Add NoOpExecutorService.Execute implementation
        }

        public void Execute(Task task)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}