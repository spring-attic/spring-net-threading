using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.Execution.ExecutionPolicy
{
    [TestFixture]
    public class CallerRunsPolicyTests : BaseMockTestCase
    {
        [Test]
        public void RunsRunnableWithNonShutdownExecutorService()
        {
            IExecutorService executorServiceMock =  _repository.StrictMock<IExecutorService>();
            Expect.On(executorServiceMock).Call(executorServiceMock.IsShutdown).Return(false);

            IRunnable runnableMock = _repository.StrictMock<IRunnable>();
            runnableMock.Run();

            _repository.ReplayAll();

            CallerRunsPolicy callerRunsPolicy = new CallerRunsPolicy();
            callerRunsPolicy.RejectedExecution(runnableMock, executorServiceMock);
        }
        [Test]
        public void DoesNotRunRunnableWithShutdownExecutorService()
        {
            IExecutorService executorServiceMock =  _repository.StrictMock<IExecutorService>();
            Expect.On(executorServiceMock).Call(executorServiceMock.IsShutdown).Return(true);

            IRunnable runnableMock =  _repository.StrictMock<IRunnable>();
            _repository.ReplayAll();

            CallerRunsPolicy callerRunsPolicy = new CallerRunsPolicy();
            callerRunsPolicy.RejectedExecution(runnableMock, executorServiceMock );
        }
    }
}