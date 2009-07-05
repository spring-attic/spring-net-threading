using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.Execution
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AbstractExecutorServiceTests<T>
    {
        private AbstractExecutorService _sut;
        private IRunnable _runnable;
        private Task _task;
        private Call<T> _call;
        private ICallable<T> _callable;
        private Action<IRunnable> _actionOnExecute;
        TestThreadManager ThreadManager { get; set; }

        private void Execute(IRunnable runnable)
        {
            _actionOnExecute(runnable);
        }

        [SetUp] public void SetUp()
        {
            _sut = Mockery.GeneratePartialMock<AbstractExecutorService>();
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _task = MockRepository.GenerateMock<Task>();
            _call = MockRepository.GenerateMock<Call<T>>();
            _callable = MockRepository.GenerateMock<ICallable<T>>();

            _sut.Stub(x => x.Execute(Arg<IRunnable>.Is.NotNull))
                .Do(new Action<IRunnable>(Execute));

            _actionOnExecute = r => r.Run(); // Default run it.

            ThreadManager = new TestThreadManager();
        }

        [Test] public void ExecuteTaskChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Execute((Task) null));
            Assert.That(e.ParamName, Is.EqualTo("task"));
        }

        [Test] public void ExecuteTaskCallsExecuteCallable()
        {
            _sut.Execute(_task);

            _task.AssertWasCalled(x => x());
            _sut.AssertWasCalled(x => x.Execute(Arg<IRunnable>.Is.NotNull));
        }

        [Test] public void SubmitChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((IRunnable)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Task)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("task"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((ICallable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Call<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }

        [Test] public void SubmitExecutesTheRunnable()
        {
            var future = _sut.Submit(_runnable);
            Assert.IsTrue(future.IsDone);
            _runnable.AssertWasCalled(x => x.Run());
        }

        [Test] public void SubmitExecutesTheRunnableAndSetFutureResult()
        {
            var future = _sut.Submit(_runnable, TestData<T>.One);
            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));

            _runnable.AssertWasCalled(x => x.Run());
        }

        [Test] public void SubmitExecutesTheTask()
        {
            var future = _sut.Submit(_task);
            Assert.IsTrue(future.IsDone);
            _task.AssertWasCalled(t => t());
        }

        [Test] public void SubmitExecutesTheTaskAndSetFutureResult()
        {
            var future = _sut.Submit(_task, TestData<T>.One);

            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));
            _task.AssertWasCalled(t => t());
        }

        [Test] public void SubmitExecutesTheCallAndSetFutureResult()
        {
            _call.Stub(c=>c()).Return(TestData<T>.Two);

            var future = _sut.Submit(_call);

            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));

            _call.AssertWasCalled(c => c());
        }

        [Test] public void SubmitExecutesTheCallableAndSetFutureResult()
        {
            _callable.Stub(c=>c.Call()).Return(TestData<T>.Two);

            var future = _sut.Submit(_callable);

            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));
            _sut.AssertWasCalled(x => x.Execute(Arg<IRunnable>.Is.NotNull))
                .Before(_callable.AssertWasCalled(c => c.Call()));
        }

        [Test] public void SubmitReturnsFutureCanBeInterruptedOnGetResult()
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T2", r.Run);
            _callable.Stub(x=>x.Call()).Do(new Delegates.Function<T>(
                delegate
                {
                    Thread.Sleep(TestData.SmallDelay);
                    return TestData<T>.One;
                }));

            var t = ThreadManager.StartAndAssertRegistered(
                "T1", 
                delegate
                    {
                        var future = _sut.Submit(_callable);
                        Assert.Throws<ThreadInterruptedException>(()=>future.GetResult());
                    });
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
            _callable.AssertWasCalled(c => c.Call());
        }

        [Test] public void SubmitReturnsFutureChokesOnGetResultWhenTaskChokes()
        {
            var exception = new Exception();
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T2", r.Run);
            _callable.Stub(c=>c.Call()).Do(new Delegates.Function<T>(
                delegate
                {
                    Thread.Sleep(TestData.ShortDelay);
                    throw exception;
                }));

            var future = _sut.Submit(_callable);
            var e = Assert.Throws<ExecutionException>(() => future.GetResult());
            Assert.That(e.InnerException, Is.SameAs(exception));
            ThreadManager.JoinAndVerify();
            _callable.AssertWasCalled(c => c.Call());
        }

        [Test] public void InvokeAnyChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        const IEnumerable<Call<T>> calls = null;
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const IEnumerable<ICallable<T>> calls = null;
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test] public void InvokeAnyChokesOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentException>(
                delegate
                    {
                        IEnumerable<Call<T>> calls = new Call<T>[0];
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentException>(
                delegate
                {
                    IEnumerable<ICallable<T>> calls = new ICallable<T>[0];
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test] public void InvokeAnyChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        IEnumerable<Call<T>> calls = new Call<T>[] { null };
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    IEnumerable<ICallable<T>> calls = new ICallable<T>[] { null };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test] public void InvokeAllChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        const IEnumerable<Call<T>> calls = null;
                        if (!isTimed) _sut.InvokeAll(calls);
                        else _sut.InvokeAll(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const IEnumerable<ICallable<T>> calls = null;
                    if (!isTimed) _sut.InvokeAll(calls);
                    else _sut.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test] public void InvokeAllNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            IEnumerable<Call<T>> calls = new Call<T>[0];
            var futures = isTimed ? 
                _sut.InvokeAll(TestData.ShortDelay, calls) :
                _sut.InvokeAll(calls);
            CollectionAssert.IsEmpty(futures);

            IEnumerable<ICallable<T>> callables = new ICallable<T>[0];
            futures = isTimed ?
                _sut.InvokeAll(TestData.ShortDelay, callables) :
                _sut.InvokeAll(callables);
            CollectionAssert.IsEmpty(futures);
        }

        [Test] public void InvokeAllChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        IEnumerable<Call<T>> calls = new Call<T>[] { null };
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    IEnumerable<ICallable<T>> calls = new ICallable<T>[] { null };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test] public void InvokeAnyChokesWhenNoTaskCompletes([Values(true, false)] bool isTimed)
        {
            var expected = new Exception();
            Call<T> call = delegate { throw expected; };
            var e1 = Assert.Throws<ExecutionException>(
                delegate
                {
                    IEnumerable<Call<T>> calls = new Call<T>[] { call };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e1.InnerException, Is.SameAs(expected));

            var e2 = Assert.Throws<ExecutionException>(
                delegate
                {
                    IEnumerable<ICallable<T>> calls = new Callable<T>[] { call };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e2.InnerException, Is.SameAs(expected));
        }

        [Test] public void InvokeAnyReturnsWhenOneCallCompletes([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            var call1 = MockRepository.GenerateMock<Call<T>>();
            var call2 = MockRepository.GenerateMock<Call<T>>();
            call1.Stub(c => c()).Throw(new Exception());
            call2.Stub(c => c()).Return(TestData<T>.Four).WhenCalled(i => Thread.Sleep(TestData.ShortDelay));

            IEnumerable<Call<T>> calls = new Call<T>[] { call1, call2, call1, call2 };
            var result = isTimed ? 
                _sut.InvokeAny(TestData.SmallDelay, calls) :
                _sut.InvokeAny(calls);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void InvokeAnyReturnsWhenOneCallableCompletes([Values(true, false)] bool isTimed)
        {
            var call1 = MockRepository.GenerateMock<ICallable<T>>();
            var call2 = MockRepository.GenerateMock<ICallable<T>>();
            var call3 = MockRepository.GenerateMock<ICallable<T>>();
            call1.Stub(c => c.Call()).Throw(new Exception());
            call2.Stub(c => c.Call()).Return(TestData<T>.Four);

            IEnumerable<ICallable<T>> calls = new ICallable<T>[] { call1, call2, call3};
            var result = isTimed ? 
                _sut.InvokeAny(TestData.ShortDelay, calls) :
                _sut.InvokeAny(calls);
            call1.AssertWasCalled(c => c.Call());
            call3.AssertWasNotCalled(c=>c.Call());
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
        }

        [Test] public void InvokeAllReturnsWhenAllCallableComplete([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            ICallable<T>[] calls = new ICallable<T>[5];
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                calls[i] = MockRepository.GenerateMock<ICallable<T>>();
                calls[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ? 
                _sut.InvokeAll(TestData.ShortDelay, calls) :
                _sut.InvokeAll(calls);
            Assert.That(futures.Count, Is.EqualTo(calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test] public void InvokeAllReturnsFutureChokesWhenCallFailed([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            Call<T>[] calls = new Call<T>[3];
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                calls[i] = MockRepository.GenerateMock<Call<T>>();
            }
            var expectedException = new Exception();
            calls[0].Stub(c => c()).Throw(expectedException);
            calls[1].Stub(c => c()).Return(TestData<T>.One).WhenCalled(i => Thread.Sleep(TestData.ShortDelay));
            calls[2].Stub(c => c()).Return(TestData<T>.Two);

            var futures = isTimed ? 
                _sut.InvokeAll(TestData.SmallDelay, calls) :
                _sut.InvokeAll(calls);
            foreach (var future in futures)
            {
                Assert.IsTrue(future.IsDone);
            }
            var e = Assert.Throws<ExecutionException>(() => futures[0].GetResult());
            Assert.That(e.InnerException, Is.SameAs(expectedException));
            Assert.That(futures[1].GetResult(), Is.EqualTo(TestData<T>.One));
            Assert.That(futures[2].GetResult(), Is.EqualTo(TestData<T>.Two));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TimedInvokeAnyChokesWhenTimeoutOnCalls()
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            Call<T>[] calls = new Call<T>[3];
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                calls[i] = MockRepository.GenerateMock<Call<T>>();
                calls[i].Stub(c => c()).Return(TestData<T>.Four)
                    .WhenCalled(x => Thread.Sleep(TestData.SmallDelay));
            }

            Assert.Throws<TimeoutException>(() => _sut.InvokeAny(TestData.ShortDelay, calls));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TimedInvokeAllCancelsUncompletedCallablesWhenTimeout()
        {
            const int size = 5;
            const int halfSize = size / 2;
            ICallable<T>[] calls = new ICallable<T>[size];
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                calls[i] = MockRepository.GenerateMock<ICallable<T>>();
                var x = calls[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
                if (i > halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.SmallDelay));
            }

            var futures = _sut.InvokeAll(TestData.ShortDelay, calls);
            Assert.That(futures.Count, Is.EqualTo(calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i > halfSize+1)
                {
                    Assert.IsTrue(futures[i].IsCancelled);
                }
                else
                {
                    Assert.IsTrue(futures[i].IsDone);
                    Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
                }
            }
        }

        [Test] public void TimedInvokeAllCancelsUncompletedCallsWhenTimeout()
        {
            const int size = 5;
            const int halfSize = size/2;
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            Call<T>[] calls = new Call<T>[size];
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                calls[i] = MockRepository.GenerateMock<Call<T>>();
                var x = calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
                if(i>halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.SmallDelay));
            }

            var futures = _sut.InvokeAll(TestData.ShortDelay, calls);
            Assert.That(futures.Count, Is.EqualTo(calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i>halfSize)
                {
                    Assert.IsTrue(futures[i].IsCancelled);
                }
                else
                {
                    Assert.IsTrue(futures[i].IsDone);
                    Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
                }
            }
            ThreadManager.JoinAndVerify();
        }

    }
}