using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="ExecutorCompletionService{T}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ExecutorCompletionServiceTest<T> : ThreadingTestFixture
    {
        private AbstractExecutorService _executor;
        private IBlockingQueue<IFuture<T>> _blockingQueue;
        private ExecutorCompletionService<T> _sut;

        [SetUp] public void SetUp()
        {
            _executor = Mockery.GeneratePartialMock<AbstractExecutorService>();
            _sut = new ExecutorCompletionService<T>(_executor);
            _blockingQueue = MockRepository.GenerateMock<IBlockingQueue<IFuture<T>>>();
        }

        [Test] public void ConstructorChokesOnNullExecutorArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ExecutorCompletionService<T>(null));
            Assert.That(e.ParamName, Is.EqualTo("executor"));
            e = Assert.Throws<ArgumentNullException>(
                () => new ExecutorCompletionService<T>(null, _blockingQueue));
            Assert.That(e.ParamName, Is.EqualTo("executor"));
        }

        [Test] public void ConstructorChokesOnNullCompletionQueueArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ExecutorCompletionService<T>(_executor, null));
            Assert.That(e.ParamName, Is.EqualTo("completionQueue"));
        }

        [Test] public void SubmitChokesOnNullCallableArguement()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((ICallable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test] public void SubmitChokesOnNullCallArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Func<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }

        [Test] public void SubmitChokesOnNullRunnableArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((IRunnable)null));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((IRunnable)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test] public void SubmitChokesOnNullTaskArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Action)null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Action)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void TakeReturnsOnlyWhenTaskIsCompleted()
        {
            var callable = MockRepository.GenerateMock<ICallable<T>>();
            ExpectExecuteCallAndRunTheRunnableInCurrentThread();

            _sut.Submit(callable);
            var f = _sut.Take();
            Assert.IsTrue(f.IsDone);
            callable.AssertWasCalled(c=>c.Call());
        }

        [Test] public void TakeReturnsSameFutureReturnedBySubmit()
        {
            var callable = MockRepository.GenerateMock<ICallable<T>>();
            ExpectExecuteCallAndRunTheRunnableInCurrentThread();

            var f1 = _sut.Submit(callable);
            var f2 = _sut.Take();
            Assert.That(f2, Is.SameAs(f1));
        }

        [Test] public void PollNeverReturnsNonCompletedTask()
        {
            var callable = MockRepository.GenerateMock<ICallable<T>>();
            ExpectExecuteCallAndRunTheRunnableInNewThread();

            Assert.IsNull(_sut.Poll());
            _sut.Submit(callable);
            IFuture<T> f = null;
            for (int i = 300 - 1; i >= 0 && f==null; i--)
            {
                f = _sut.Poll();
                Thread.Sleep(1);
            }
            Assert.IsNotNull(f);
            Assert.IsTrue(f.IsDone);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TimedPollNeverReturnsNonCompletedTask()
        {
            Func<T> call = delegate
                               {
                                   Thread.Sleep(TestData.SmallDelay);
                                   return TestData<T>.One;
                               };
            ExpectExecuteCallAndRunTheRunnableInNewThread();

            Assert.IsNull(_sut.Poll());
            _sut.Submit(call);
            Assert.IsNull(_sut.Poll(TestData.ShortDelay));

            IFuture<T> f = _sut.Poll(TestData.MediumDelay);
            Assert.IsNotNull(f);
            Assert.IsTrue(f.IsDone);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void NewTaskForCallGetsNewTaskFromAbstractExecutorService()
        {
            Func<T> call = delegate
                               {
                                   return TestData<T>.One;
                               };

            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(call);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(call)).Return(futureTask);

            var f1 = _sut.Submit(call);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        [Test] public void NewTaskForCallableGetsNewTaskFromAbstractExecutorService()
        {
            ICallable<T> callable = new Callable<T>(delegate
                               {
                                   return TestData<T>.One;
                               });

            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(callable);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(callable)).Return(futureTask);

            var f1 = _sut.Submit(callable);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        [Test] public void NewTaskForTaskWithResultGetsNewTaskFromAbstractExecutorService()
        {
            Action action = delegate {};
            var result = TestData<T>.Two;
            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(action, result);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(action, result)).Return(futureTask);

            var f1 = _sut.Submit(action, result);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        [Test] public void NewTaskForRunnableWitResultGetsNewTaskFromAbstractExecutorService()
        {
            IRunnable runnable = new Runnable(delegate {});
            var result = TestData<T>.Two;
            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(runnable, result);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(runnable, result)).Return(futureTask);

            var f1 = _sut.Submit(runnable, result);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        [Test] public void NewTaskForTaskGetsNewTaskFromAbstractExecutorService()
        {
            Action action = delegate { };
            var result = default(T);
            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(action, result);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(action, result)).Return(futureTask);

            var f1 = _sut.Submit(action);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        [Test] public void NewTaskForRunnableGetsNewTaskFromAbstractExecutorService()
        {
            IRunnable runnable = new Runnable(delegate { });
            var result = default(T);
            var futureTask = Mockery.GeneratePartialMock<FutureTask<T>>(runnable, result);

            ExpectExecuteCallAndRunTheRunnableInCurrentThread();
            _executor.Expect(e => e.NewTaskFor(runnable, result)).Return(futureTask);

            var f1 = _sut.Submit(runnable);
            Assert.That(f1, Is.SameAs(futureTask));
            var f2 = _sut.Poll();
            Assert.That(f2, Is.SameAs(futureTask), "submit and take must return same objects");
            futureTask.AssertWasCalled(ft => ft.Done());
            _executor.VerifyAllExpectations();
        }

        private void ExpectExecuteCallAndRunTheRunnableInNewThread()
        {
            _executor.Stub(e => e.Execute(Arg<IRunnable>.Is.NotNull))
                .WhenCalled(m => ThreadManager.StartAndAssertRegistered(
                                     "T1",
                                     ((IRunnable)m.Arguments[0]).Run));
        }

        private void ExpectExecuteCallAndRunTheRunnableInCurrentThread()
        {
            _executor.Expect(e => e.Execute(Arg<IRunnable>.Is.NotNull))
                .WhenCalled(r => ((IRunnable)r.Arguments[0]).Run());
        }
    }
}
