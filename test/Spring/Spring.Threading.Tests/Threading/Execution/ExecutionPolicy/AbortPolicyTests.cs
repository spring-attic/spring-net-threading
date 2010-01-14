using System;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture]
    public class AbortPolicyTests
    {
        [Test] public void AbortPolicyThrowsExceptionUponHandling()
        {
            IBlockingQueue<IRunnable> queue = MockRepository.GenerateStub<IBlockingQueue<IRunnable>>();

            var executor = Mockery.GeneratePartialMock<ThreadPoolExecutor>(1, 1, TimeSpan.FromSeconds(1), queue);
            var runnable = MockRepository.GenerateStub<IRunnable>();
            ThreadPoolExecutor.AbortPolicy abortPolicy = new ThreadPoolExecutor.AbortPolicy();
            Assert.Throws<RejectedExecutionException>(
                ()=>abortPolicy.RejectedExecution(runnable, executor));
            executor.AssertWasNotCalled(e=>e.Execute(Arg<IRunnable>.Is.Anything));
        }
    }
}