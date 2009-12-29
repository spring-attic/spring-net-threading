using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Threading;
using NUnit.Framework;
using Spring.Threading.Execution;
using Spring.Threading.Future;

namespace Spring.Threading
{
    public class ExecutorTestBase
    {
        public void JoinPool(IExecutorService exec)
        {
            JoinPool(exec, Delays.Long);
        }

        public void JoinPool(IExecutorService exec, TimeSpan waitTime)
        {
            exec.Shutdown();
            Assert.IsTrue(exec.AwaitTermination(waitTime));
        }
    }

    public class BaseThreadingTestCase : ExecutorTestBase
    {
        [Serializable]
        public class Integer
        {
            internal readonly int Value;

            public Integer(int value)
            {
                this.Value = value;
            }

            public static implicit operator int(Integer integer)
            {
                return integer.Value;
            }

            public static implicit operator Integer(int value)
            {
                return new Integer(value);
            }

            public override string ToString()
            {
                return string.Format("Integer({0})", Value);
            }
        }

        public const string TEST_STRING = "a test string";
        public static int DEFAULT_COLLECTION_SIZE = 20;
        protected static Integer eight = Int32.Parse("8");
        protected static Integer five = Int32.Parse("5");
        protected static Integer four = Int32.Parse("4");
        protected static Integer m1 = Int32.Parse("-1");
        protected static Integer m10 = Int32.Parse("-10");
        protected static Integer m2 = Int32.Parse("-2");
        protected static Integer m3 = Int32.Parse("-3");
        protected static Integer m4 = Int32.Parse("-4");
        protected static Integer m5 = Int32.Parse("-5");
        protected static Integer nine = Int32.Parse("9");
        protected static Integer one = Int32.Parse("1");
        protected static Integer seven = Int32.Parse("7");

        protected static Integer six = Int32.Parse("6");
        protected static Integer three = Int32.Parse("3");
        protected static Integer two = Int32.Parse("2");
        protected static Integer zero = Int32.Parse("0");

        private volatile bool threadFailed;
        private TestThreadManager _threadManager;

        protected BaseThreadingTestCase()
        {
        }

        protected internal TestThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        public void UnexpectedException()
        {
            Assert.Fail("Unexpected exception");
        }

        public void ThreadUnexpectedException()
        {
            threadFailed = true;
            Assert.Fail("Unexpected exception");
        }

        [SetUp] public void SetUp()
        {
            _threadManager = new TestThreadManager();
        }

        [TearDown] public void TearDown()
        {
            _threadManager.TearDown(true);
        }
    }

    internal class SmallRunnable : IRunnable
    {
        #region IRunnable Members

        public void Run()
        {
            Thread.Sleep(Delays.Small);
        }

        #endregion
    }

    internal class ShortDelayClassRunnable : IRunnable
    {
        #region IRunnable Members

        public virtual void Run()
        {
            Thread.Sleep(Delays.Short);
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
                Thread.Sleep(Delays.Short);
                done = true;
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }

    internal class SmallCallable : ICallable<bool>
    {
        #region ICallable Members

        public bool Call()
        {
            try
            {
                Thread.Sleep(Delays.Small);
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
                Thread.Sleep(Delays.Long);
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
                Thread.Sleep(Delays.Small);
                done = true;
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }

    internal class StringTask : ICallable<string>
    {
        #region ICallable Members

        public string Call()
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
            Thread.Sleep(Delays.Medium);
        }

        #endregion
    }

    internal class NPETask<T> : ICallable<T>
    {
        #region ICallable Members

        public virtual T Call()
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
                Thread.Sleep(Delays.Medium);
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        #endregion
    }

    internal class TrackedCallable : ICallable<bool>
    {
        public volatile bool done;

        #region ICallable Members

        public bool Call()
        {
            try
            {
                Thread.Sleep(Delays.Small);
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
        public void RejectedExecution(IRunnable runnable, ThreadPoolExecutor executor)
        {
            throw new NotImplementedException();
        }
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
#if !PHASED
    internal class NoOpExecutorService : IExecutorService
    {
        #region IExecutorService Members

        public object InvokeAny(ICollection tasks, TimeSpan durationToWait)
        {
            // TODO:  Add NoOpExecutorService.InvokeAny implementation
            return null;
        }

        public IList InvokeAll(ICollection tasks, TimeSpan durationToWait)
        {
            // TODO:  Add NoOpExecutorService.InvokeAll implementation
            return null;
        }

        public IFuture<Void> Submit(Action action)
        {
            throw new NotImplementedException();
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

        public IFuture<Void> Submit(IRunnable task)
        {
            // TODO:  Add NoOpExecutorService.Submit implementation
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

        public void Execute(Action action)
        {
            throw new NotImplementedException();
        }

        public IFuture<T> Submit<T>(IRunnable runnable, T result)
        {
            throw new NotImplementedException();
        }

        public IFuture<T> Submit<T>(Action action, T result)
        {
            throw new NotImplementedException();
        }

        public IFuture<T> Submit<T>(ICallable<T> callable)
        {
            throw new NotImplementedException();
        }

        public IFuture<T> Submit<T>(Func<T> call)
        {
            throw new NotImplementedException();
        }

        public IList<IFuture<T>> InvokeAll<T>(IEnumerable<ICallable<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public IList<IFuture<T>> InvokeAll<T>(IEnumerable<Func<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params Func<T>[] tasks)
        {
            throw new NotImplementedException();
        }

        public T InvokeAny<T>(IEnumerable<ICallable<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public T InvokeAny<T>(IEnumerable<Func<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
        {
            throw new NotImplementedException();
        }

        public T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
        {
            throw new NotImplementedException();
        }

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

        public void ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            throw new NotImplementedException();
        }

        public void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
#endif
}