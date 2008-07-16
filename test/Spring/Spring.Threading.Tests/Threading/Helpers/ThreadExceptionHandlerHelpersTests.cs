using System;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Helpers
{
	[TestFixture]
	public class ThreadExceptionHandlerHelpersTests
	{
		private static bool _handlerExecuted = false;

		private class FailingRunnable : IRunnable
		{
			public void Run()
			{
				throw new NotImplementedException();
			}
		}
		[SetUp]
		public void Init()
		{
			_handlerExecuted = false;
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void NullRunnableException()
		{
			ThreadExceptionHandlerHelpers.AssignExceptionHandler(null, new UncaughtExceptionHandlerDelegate(MyExceptionHandler));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void NullDelegateException()
		{
			ThreadExceptionHandlerHelpers.AssignExceptionHandler(new FailingRunnable(), null);
		}
		[Test]
		public void ThreadHelperRunnableRunsExceptionHandler()
		{
			IRunnable runner = ThreadExceptionHandlerHelpers.AssignExceptionHandler(new FailingRunnable(), new UncaughtExceptionHandlerDelegate(MyExceptionHandler));
			runner.Run();
			Assert.IsTrue(_handlerExecuted);
		}
		[Test]
		public void ThreadHelperRunnableNoExceptionHandler()
		{
			IRunnable runner = ThreadExceptionHandlerHelpers.AssignExceptionHandler(new NullRunnable(), new UncaughtExceptionHandlerDelegate(MyExceptionHandler));
			runner.Run();
			Assert.IsFalse(_handlerExecuted);
		}

		private void MyExceptionHandler(Thread thread, Exception exception)
		{
			_handlerExecuted = true;
		}
	}
}