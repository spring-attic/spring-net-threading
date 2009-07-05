using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.Execution
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AbstractExecutorServiceTests<T>
    {
        const int _size = 5;
        const int _halfSize = _size / 2;

        private AbstractExecutorService _sut;
        private IRunnable _runnable;
        private Task _task;
        private Call<T> _call;
        private ICallable<T> _callable;
        ICallable<T>[] _callables = new ICallable<T>[_size];
        Call<T>[] _calls = new Call<T>[3];
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

            _calls = new Call<T>[_size];
            _callables = new ICallable<T>[_size];
            for (int i = _size - 1; i >= 0; i--)
            {
                _callables[i] = MockRepository.GenerateMock<ICallable<T>>();
                _calls[i] = MockRepository.GenerateMock<Call<T>>();
            }
            _call = _calls[0];
            _callable = _callables[0];

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
            Thread t2 = null;
            _actionOnExecute = r => { t2 = ThreadManager.StartAndAssertRegistered("T2", r.Run); };
            _callable.Stub(x=>x.Call()).Do(new Delegates.Function<T>(
                delegate
                {
                    Assert.Throws<ThreadInterruptedException>(()=>Thread.Sleep(TestData.SmallDelay));
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
            t2.Interrupt();
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
                        const Call<T>[] calls = null;
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
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
                        Call<T>[] calls = new Call<T>[0];
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[0];
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
                        Call<T>[] calls = new Call<T>[] { null };
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] { null };
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
                        const Call<T>[] calls = null;
                        if (!isTimed) _sut.InvokeAll(calls);
                        else _sut.InvokeAll(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
                    if (!isTimed) _sut.InvokeAll(calls);
                    else _sut.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test] public void InvokeAllNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            Call<T>[] calls = new Call<T>[0];
            var futures = isTimed ? 
                _sut.InvokeAll(TestData.ShortDelay, calls) :
                _sut.InvokeAll(calls);
            CollectionAssert.IsEmpty(futures);

            ICallable<T>[] callables = new ICallable<T>[0];
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
                        Call<T>[] calls = new Call<T>[] { null };
                        if (!isTimed) _sut.InvokeAny(calls);
                        else _sut.InvokeAny(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] { null };
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
                    Call<T>[] calls = new Call<T>[] { call };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e1.InnerException, Is.SameAs(expected));

            var e2 = Assert.Throws<ExecutionException>(
                delegate
                {
                    ICallable<T>[] calls = new Callable<T>[] { call };
                    if (!isTimed) _sut.InvokeAny(calls);
                    else _sut.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e2.InnerException, Is.SameAs(expected));
        }

        [Test] public void InvokeAnyReturnsWhenOneCallCompletes([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            var call1 = _calls[0];
            var call2 = _calls[1];
            call1.Stub(c => c()).Throw(new Exception());
            call2.Stub(c => c()).Return(TestData<T>.Four).WhenCalled(i => Thread.Sleep(TestData.ShortDelay));

            Call<T>[] calls = new Call<T>[] { call1, call2, call1, call2 };
            var result = isTimed ? 
                _sut.InvokeAny(TestData.SmallDelay, calls) :
                _sut.InvokeAny(calls);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void InvokeAnyReturnsWhenOneCallableCompletes([Values(true, false)] bool isTimed)
        {
            var call1 = _callables[0];
            var call2 = _callables[1];
            var call3 = _callables[2];
            call1.Stub(c => c.Call()).Throw(new Exception());
            call2.Stub(c => c.Call()).Return(TestData<T>.Four);

            ICallable<T>[] calls = new ICallable<T>[] { call1, call2, call3};
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
            for (int i = _callables.Length - 1; i >= 0; i--)
            {
                _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ? 
                _sut.InvokeAll(TestData.ShortDelay, _callables) :
                _sut.InvokeAll(_callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test] public void InvokeAllReturnsFutureChokesWhenCallFailed([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            var expectedException = new Exception();
            _calls[0].Stub(c => c()).Throw(expectedException);
            _calls[1].Stub(c => c()).Return(TestData<T>.One).WhenCalled(i => Thread.Sleep(TestData.ShortDelay));
            _calls[2].Stub(c => c()).Return(TestData<T>.Two);

            var futures = isTimed ? 
                _sut.InvokeAll(TestData.SmallDelay, _calls) :
                _sut.InvokeAll(_calls);
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
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.Four)
                    .WhenCalled(x => Thread.Sleep(TestData.SmallDelay));
            }

            Assert.Throws<TimeoutException>(() => _sut.InvokeAny(TestData.ShortDelay, _calls));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TimedInvokeAllCancelsUncompletedCallablesWhenTimeout()
        {
            for (int i = _callables.Length - 1; i >= 0; i--)
            {
                var x = _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
                if (i > _halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.ShortDelayMillis * 2));
            }

            var futures = _sut.InvokeAll(TestData.ShortDelay, _callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i > _halfSize+1)
                {
                    Assert.IsTrue(futures[i].IsCancelled, "future No. " + i + " should have been canceled");
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
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                var x = _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
                if(i>_halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.SmallDelay));
            }

            var futures = _sut.InvokeAll(TestData.ShortDelay, _calls);
            Assert.That(futures.Count, Is.EqualTo(_calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i>_halfSize)
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