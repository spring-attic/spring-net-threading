//using System;
//using NUnit.Framework;
//using Spring.Threading.Execution;

//namespace Spring.Threading.Execution
//{
//    [TestFixture]
//    public class ExecutionExceptionTests
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