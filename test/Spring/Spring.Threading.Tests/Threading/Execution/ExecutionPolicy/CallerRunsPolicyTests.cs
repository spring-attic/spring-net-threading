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
        private IBlockingQueue<IRunnable> _queue;
        private IRunnable _runnable;
        private ThreadPoolExecutor.CallerRunsPolicy _callerRunsPolicy;
        private ThreadPoolExecutor _threadPoolExecutor;

        [SetUp] public void SetUp()
        {
            _queue = MockRepository.GenerateStub<IBlockingQueue<IRunnable>>();
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _callerRunsPolicy = new ThreadPoolExecutor.CallerRunsPolicy();
            _threadPoolExecutor = new ThreadPoolExecutor(1, 1, TimeSpan.FromSeconds(1), _queue);
        }

        [Test]
        public void RunsRunnableWithNonShutdownExecutorService()
        {
            Thread callerThread = Thread.CurrentThread;
            _runnable.Expect(r => r.Run()).Callback(delegate
                {
                    return ReferenceEquals(Thread.CurrentThread, callerThread);
                });

            _callerRunsPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            _runnable.VerifyAllExpectations();
        }

        [Test]
        public void DoesNotRunRunnableWithShutdownExecutorService()
        {
            _threadPoolExecutor.ShutdownNow();
            _callerRunsPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            _runnable.AssertWasNotCalled(r=>r.Run());
        }
    }
}