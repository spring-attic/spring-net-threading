using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture]
    public class CallerRunsPolicyTests
    {
        private MockRepository _mockery;
        private IBlockingQueue<IRunnable> _queue;
        private IRunnable _runnable;
        private CallerRunsPolicy _callerRunsPolicy;
        private ThreadPoolExecutor _threadPoolExecutor;

        [SetUp] public void SetUp()
        {
            _mockery = new MockRepository();
            _queue = _mockery.Stub<IBlockingQueue<IRunnable>>();
            _runnable = _mockery.StrictMock<IRunnable>();
            _callerRunsPolicy = new CallerRunsPolicy();
            _threadPoolExecutor = new ThreadPoolExecutor(1, 1, TimeSpan.FromSeconds(1), _queue);
        }

        [Test]
        public void RunsRunnableWithNonShutdownExecutorService()
        {
            Thread callerThread = Thread.CurrentThread;

            _runnable.Run();
            LastCall.Callback(delegate
                {
                    return ReferenceEquals(Thread.CurrentThread, callerThread);
                });

            _mockery.ReplayAll();

            _callerRunsPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            _mockery.VerifyAll();
        }

        [Test]
        public void DoesNotRunRunnableWithShutdownExecutorService()
        {
            _runnable.Run();
            LastCall.Repeat.Never();

            _mockery.ReplayAll();

            _threadPoolExecutor.ShutdownNow();
            _callerRunsPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            _mockery.VerifyAll();
        }
    }
}