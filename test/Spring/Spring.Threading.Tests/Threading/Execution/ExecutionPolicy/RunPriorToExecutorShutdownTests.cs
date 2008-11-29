//using DotNetMock.Dynamic;
//using NUnit.Framework;

//namespace Spring.Threading.Execution.ExecutionPolicy
//{
//    [TestFixture]
//    public class RunPriorToExecutorShutdownTests
//    {
//        [Test]
//            public void RunsRunnableWithNonShutdownExecutorService()
//        {
//            DynamicMock executorServiceMock = new DynamicMock(typeof(IExecutorService));
//            executorServiceMock.ExpectAndReturn("IsShutdown", false);
//            DynamicMock runnableMock = new DynamicMock(typeof(IRunnable));
//            runnableMock.Expect("Run");

//            RunPriorToExecutorShutdown runPriorToExecutorShutdown = new RunPriorToExecutorShutdown();
//            runPriorToExecutorShutdown.RejectedExecution((IRunnable)runnableMock.Object, (IExecutorService) executorServiceMock.Object );

//            executorServiceMock.Verify();
//            runnableMock.Verify();
//        }
//        [Test]
//            [Ignore("Fix DotNetMock ExpectNoCall bug")]
//        public void DoesNotRunRunnableWithShutdownExecutorService()
//        {
//            DynamicMock executorServiceMock = new DynamicMock(typeof(IExecutorService));
//            executorServiceMock.ExpectAndReturn("IsShutdown", true);
//            DynamicMock runnableMock = new DynamicMock(typeof(IRunnable));
//            runnableMock.ExpectNoCall("Run");

//            RunPriorToExecutorShutdown runPriorToExecutorShutdown = new RunPriorToExecutorShutdown();
//            runPriorToExecutorShutdown.RejectedExecution((IRunnable)runnableMock.Object, (IExecutorService) executorServiceMock.Object );

//            executorServiceMock.Verify();
//            runnableMock.Verify();
//        }
//    }
//}