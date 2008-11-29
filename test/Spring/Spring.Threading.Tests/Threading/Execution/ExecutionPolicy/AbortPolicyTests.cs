//using NUnit.Framework;

//namespace Spring.Threading.Execution.ExecutionPolicy
//{
//    [TestFixture]
//    public class AbortPolicyTests : BaseThreadingTestCase
//    {
//        [Test]
//        [ExpectedException(typeof (RejectedExecutionException))]
//        public void AbortPolicyThrowsExceptionUponHandling()
//        {
//            AbortPolicy abortPolicy = new AbortPolicy();
//            abortPolicy.RejectedExecution(new NullRunnable(), new NoOpExecutorService());
//        }

//    }
//}