using System;
using NUnit.Framework;

namespace Spring.Threading.Execution
{
    [TestFixture]
    public class ExecutionExceptionTests
    {
        [Test]
        public void RootCuaseConstructor()
        {
            TimeoutException timeoutException = new TimeoutException();
            ExecutionException exception = new ExecutionException(timeoutException);
            Assert.AreEqual(timeoutException, exception.InnerException);
            Assert.AreEqual(timeoutException.Message, exception.Message);
        }
    }
}