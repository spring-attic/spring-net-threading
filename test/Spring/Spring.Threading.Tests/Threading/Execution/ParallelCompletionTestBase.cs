using System;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Base class for <see cref="ParallelCompletionNoLocalTest{T}"/> and
    /// <see cref="ParallelCompletionLocalTest{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class ParallelCompletionTestBase : ThreadingTestFixture
    {
        protected const int Parallelism = 5;
        protected int _sampleSize;
        protected ThreadManagerExecutor _executor;

        protected class ManagedThreadLimiter
        {
            protected readonly TestThreadManager _manager;
            public int Threshold = int.MaxValue;
            public readonly AtomicInteger ThreadCount = new AtomicInteger();

            protected ManagedThreadLimiter(TestThreadManager manager) 
            {
                _manager = manager;
            }

            protected int NextThreadId(bool noException)
            {
                if (noException || ThreadCount.Value < Threshold)
                {
                    var next = ThreadCount.IncrementValueAndReturn();
                    if (next <= Threshold) return next;
                    if(noException) return -next;
                }
                throw new RejectedExecutionException();
            }
        }

        protected class ManagedThreadFactory : ManagedThreadLimiter, IThreadFactory
        {
            public ManagedThreadFactory(TestThreadManager manager) : base(manager)
            {
            }

            public Thread NewThread(IRunnable runnable)
            {
                return _manager.NewVerifiableThread(runnable.Run, "T" + NextThreadId(false));
            }
        }

        protected class ThreadManagerExecutor : ManagedThreadLimiter, IExecutor
        {
            public TimeSpan Delay { get; set; }

            public ThreadManagerExecutor(TestThreadManager manager)
                : base(manager)
            {
            }

            public void Execute(IRunnable command)
            {
                Execute(command.Run);
            }

            public virtual void Execute(Action action)
            {
                var useDelay = Delay > TimeSpan.Zero;
                var tid = NextThreadId(useDelay);
                ThreadStart ts;
                if (useDelay && tid < 0)
                    ts = delegate
                    {
                        Thread.Sleep(Delay);
                        action();
                    };
                else ts = () => action();
                _manager.StartAndAssertRegistered("T" + tid, ts);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _sampleSize = 20;
            _executor = new ThreadManagerExecutor(ThreadManager);
        }

        protected static bool BreakAt(ILoopState s, long breakAt)
        {
            if (s.CurrentIndex == breakAt)
            {
                Thread.Sleep(SHORT_DELAY);
                s.Break();
                return true;
            }
            if (s.CurrentIndex == breakAt + 1)
            {
                Thread.Sleep(SHORT_DELAY);
                s.Break();
                return true;
            }
            if (s.CurrentIndex == breakAt + 2)
            {
                Thread.Sleep(10);
                s.Break();
                return true;
            }
            Thread.Sleep(10);
            return false;
        }
    }
}
