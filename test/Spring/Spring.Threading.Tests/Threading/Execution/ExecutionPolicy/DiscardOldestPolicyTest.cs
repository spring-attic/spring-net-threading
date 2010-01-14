using System;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture] public class DiscardOldestPolicyTest
    {
        private IBlockingQueue<IRunnable> _queue;
        private IRunnable _runnable;
        private ThreadPoolExecutor.DiscardOldestPolicy _discardOldestPolicy;
        private ThreadPoolExecutor _threadPoolExecutor;

        [SetUp] public void SetUp()
        {
            _queue = MockRepository.GenerateStub<IBlockingQueue<IRunnable>>();
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _discardOldestPolicy = new ThreadPoolExecutor.DiscardOldestPolicy();
            _threadPoolExecutor = Mockery.GeneratePartialMock<ThreadPoolExecutor>(1, 1, TimeSpan.FromSeconds(1), _queue);
        }

        [Test] public void DiscardOldestAndRunRunnableWithNonShutdownExecutorService()
        {
            _discardOldestPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            IRunnable r;
            _queue.AssertWasCalled(q => q.Poll(out r));
            _threadPoolExecutor.AssertWasCalled(e => e.Execute(_runnable));
        }

        [Test] public void DoesNotRunRunnableWithShutdownExecutorService()
        {
            _threadPoolExecutor.ShutdownNow();
            _discardOldestPolicy.RejectedExecution(_runnable, _threadPoolExecutor);

            _runnable.AssertWasNotCalled(r => r.Run());
            _threadPoolExecutor.AssertWasNotCalled(e => e.Execute(Arg<Action>.Is.Anything));
            _threadPoolExecutor.AssertWasNotCalled(e => e.Execute(Arg<IRunnable>.Is.Anything));
        }
    }
}
