using System;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture]
    public class AbortPolicyTests : BaseThreadingTestCase
    {
        [Test]
        [ExpectedException(typeof (RejectedExecutionException))]
        public void AbortPolicyThrowsExceptionUponHandling()
        {
            IBlockingQueue<IRunnable> queue = MockRepository.GenerateStub<IBlockingQueue<IRunnable>>();
            ThreadPoolExecutor executor = new ThreadPoolExecutor(1, 1, TimeSpan.FromSeconds(1),queue);
            AbortPolicy abortPolicy = new AbortPolicy();
            abortPolicy.RejectedExecution(new NullRunnable(), executor);
        }
    }
}