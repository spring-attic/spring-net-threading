using System;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture] public class DiscardPolicyTest
    {
        private IBlockingQueue<IRunnable> _queue;
        private IRunnable _runnable;
        private DiscardPolicy _discardPolicy;
        private ThreadPoolExecutor _threadPoolExecutor;

        [SetUp]
        public void SetUp()
        {
            _queue = MockRepository.GenerateStub<IBlockingQueue<IRunnable>>();
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _discardPolicy = new DiscardPolicy();
            _threadPoolExecutor = Mockery.GenerateStrickMock<ThreadPoolExecutor>(1, 1, TimeSpan.FromSeconds(1), _queue);
        }

        [Test] public void AlwaysDiscardRunnable()
        {
            _discardPolicy.RejectedExecution(_runnable, _threadPoolExecutor);
            _runnable.AssertWasNotCalled(r=>r.Run());
        }
    }
}
