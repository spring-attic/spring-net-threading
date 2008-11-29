//using System;
//using NUnit.Framework;

//namespace Spring.Threading.Execution
//{
//    [TestFixture]
//    public class RejectedExecutionExceptionTests
//    {
//        [Test]
//        public void RootCuaseConstructor()
//        {
//            TimeoutException timeoutException = new TimeoutException();
//            ExecutionException exception = new ExecutionException(timeoutException);
//            Assert.AreEqual(timeoutException, exception.InnerException);
//            Assert.AreEqual(String.Empty, exception.Message);
//        }
//    }
//}
