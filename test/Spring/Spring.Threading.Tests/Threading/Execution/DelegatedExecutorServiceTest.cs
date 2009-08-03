using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    [TestFixture] public class DelegatedExecutorServiceTest
    {
        private IExecutorService _mockService;
        private Executors.DelegatedExecutorService _sut;

        [SetUp] public void SetUp()
        {
            _mockService = MockRepository.GenerateStub<IExecutorService>();
            _sut = new Executors.DelegatedExecutorService(_mockService);
        }

        [Test] public void ExecuteRunnable()
        {
            var command = MockRepository.GenerateStub<IRunnable>();
            _sut.Execute(command);
            _mockService.AssertWasCalled(s => s.Execute(command));
        }

        [Test] public void ExecuteAction()
        {
            var action = MockRepository.GenerateStub<Action>();
            _sut.Execute(action);
            _mockService.AssertWasCalled(s => s.Execute(action));
        }

        [Test] public void IsShutdown([Values(true, false)] bool result)
        {
            _mockService.Stub(s => s.IsShutdown).Return(result);
            Assert.That(_sut.IsShutdown, Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.IsShutdown);
        }

        [Test] public void IsTerminated([Values(true, false)] bool result)
        {
            _mockService.Stub(s => s.IsTerminated).Return(result);
            Assert.That(_sut.IsTerminated, Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.IsTerminated);
        }

        [Test] public void Shutdown()
        {
            _sut.Shutdown();
            _mockService.AssertWasCalled(s => s.Shutdown());
        }

        [Test] public void ShutdownNow()
        {
            var result = MockRepository.GenerateStub<IList<IRunnable>>();
            _mockService.Stub(s => s.ShutdownNow()).Return(result);
            Assert.That(_sut.ShutdownNow(), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.ShutdownNow());
        }

        [Test] public void AwaitTermination([Values(0, 100)] long waitTicks, [Values(true, false)] bool result)
        {
            var timeSpan = TimeSpan.FromTicks(waitTicks);
            _mockService.Stub(s => s.AwaitTermination(timeSpan)).Return(result);
            Assert.That(_sut.AwaitTermination(timeSpan), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.AwaitTermination(timeSpan));
        }

        [Test] public void SubmitRunnable()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            var future = MockRepository.GenerateStub<IFuture<object>>();
            _mockService.Stub(s => s.Submit(runnable)).Return(future);
            Assert.That(_sut.Submit(runnable), Is.SameAs(future));
            _mockService.AssertWasCalled(s => s.Submit(runnable));
        }

        [Test] public void SubmitAction()
        {
            var action = MockRepository.GenerateStub<Action>();
            var future = MockRepository.GenerateStub<IFuture<object>>();
            _mockService.Stub(s => s.Submit(action)).Return(future);
            Assert.That(_sut.Submit(action), Is.SameAs(future));
            _mockService.AssertWasCalled(s => s.Submit(action));
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class DelegatedExecutorServiceTest<T>
    {
        private IExecutorService _mockService;
        private Executors.DelegatedExecutorService _sut;

        [SetUp]
        public void SetUp()
        {
            _mockService = MockRepository.GenerateStub<IExecutorService>();
            _sut = new Executors.DelegatedExecutorService(_mockService);
        }


        [Test] public void SubmitRunnableResult()
        {
            var future = MockRepository.GenerateStub<IFuture<T>>();
            var runnable = MockRepository.GenerateStub<IRunnable>();
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.Submit(runnable, result)).Return(future);
            Assert.That(_sut.Submit(runnable, result), Is.EqualTo(future));
            _mockService.AssertWasCalled(s => s.Submit(runnable, result));
        }

        [Test] public void SubmitActionResult()
        {
            var future = MockRepository.GenerateStub<IFuture<T>>();
            var action = MockRepository.GenerateStub<Action>();
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.Submit(action, result)).Return(future);
            Assert.That(_sut.Submit(action, result), Is.EqualTo(future));
            _mockService.AssertWasCalled(s => s.Submit(action, result));
        }

        [Test] public void SubmitCallable()
        {
            var future = MockRepository.GenerateStub<IFuture<T>>();
            var callable = MockRepository.GenerateStub<ICallable<T>>();
            _mockService.Stub(s => s.Submit(callable)).Return(future);
            Assert.That(_sut.Submit(callable), Is.EqualTo(future));
            _mockService.AssertWasCalled(s => s.Submit(callable));
        }

        [Test] public void SubmitFunc()
        {
            var future = MockRepository.GenerateStub<IFuture<T>>();
            var func = MockRepository.GenerateStub<Func<T>>();
            _mockService.Stub(s => s.Submit(func)).Return(future);
            Assert.That(_sut.Submit(func), Is.EqualTo(future));
            _mockService.AssertWasCalled(s => s.Submit(func));
        }

        [Test] public void InvokeAllEnumerableOfCallable()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<ICallable<T>>>();
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAll(tasks)).Return(result);
            Assert.That(_sut.InvokeAll(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAll(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAll(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(waitTime, tasks));
        }

        [Test] public void InvokeAllArrayOfCallable()
        {
            var tasks = new ICallable<T>[0];
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAll(tasks)).Return(result);
            Assert.That(_sut.InvokeAll(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAll(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAll(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(waitTime, tasks));
        }

        [Test] public void InvokeAllEnumerableOfFunc()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<Func<T>>>();
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAll(tasks)).Return(result);
            Assert.That(_sut.InvokeAll(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAll(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAll(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(waitTime, tasks));
        }

        [Test] public void InvokeAllArrayOfFunc()
        {
            var tasks = new Func<T>[0];
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAll(tasks)).Return(result);
            Assert.That(_sut.InvokeAll(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAll(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAll(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAll(waitTime, tasks));
        }

        [Test]
        public void InvokeAllOrFailEnumerableOfCallable()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<ICallable<T>>>();
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAllOrFail(tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAllOrFail(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(waitTime, tasks));
        }

        [Test]
        public void InvokeAllOrFailArrayOfCallable()
        {
            var tasks = new ICallable<T>[0];
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAllOrFail(tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAllOrFail(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(waitTime, tasks));
        }

        [Test]
        public void InvokeAllOrFailEnumerableOfFunc()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<Func<T>>>();
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAllOrFail(tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAllOrFail(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(waitTime, tasks));
        }

        [Test]
        public void InvokeAllOrFailArrayOfFunc()
        {
            var tasks = new Func<T>[0];
            var result = MockRepository.GenerateStub<IList<IFuture<T>>>();
            _mockService.Stub(s => s.InvokeAllOrFail(tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAllOrFail(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAllOrFail(waitTime, tasks), Is.SameAs(result));
            _mockService.AssertWasCalled(s => s.InvokeAllOrFail(waitTime, tasks));
        }

        [Test] public void InvokeAnyEnumerableOfCallable()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<ICallable<T>>>();
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.InvokeAny(tasks)).Return(result);
            Assert.That(_sut.InvokeAny(tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAny(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAny(waitTime, tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(waitTime, tasks));
        }

        [Test] public void InvokeAnyArrayOfCallable()
        {
            var tasks = new ICallable<T>[0];
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.InvokeAny(tasks)).Return(result);
            Assert.That(_sut.InvokeAny(tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAny(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAny(waitTime, tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(waitTime, tasks));
        }

        [Test] public void InvokeAnyEnumerableOfFunc()
        {
            var tasks = MockRepository.GenerateStub<IEnumerable<Func<T>>>();
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.InvokeAny(tasks)).Return(result);
            Assert.That(_sut.InvokeAny(tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAny(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAny(waitTime, tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(waitTime, tasks));
        }

        [Test] public void InvokeAnyArrayOfFunc()
        {
            var tasks = new Func<T>[0];
            var result = TestData<T>.Zero;
            _mockService.Stub(s => s.InvokeAny(tasks)).Return(result);
            Assert.That(_sut.InvokeAny(tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(tasks));

            var waitTime = TestData.ShortDelay;
            _mockService.Stub(s => s.InvokeAny(waitTime, tasks)).Return(result);
            Assert.That(_sut.InvokeAny(waitTime, tasks), Is.EqualTo(result));
            _mockService.AssertWasCalled(s => s.InvokeAny(waitTime, tasks));
        }

    }
}
