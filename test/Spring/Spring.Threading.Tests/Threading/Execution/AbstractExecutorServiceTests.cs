#region License
/*
* Copyright (C) 2002-2009 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="AbstractExecutorService"/>.
    /// </summary>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    public class AbstractExecutorServiceTests<T>
    {
        const int _size = 5;
        const int _halfSize = _size / 2;

        private AbstractExecutorService _sut;
        private IRunnable _runnable;
        private Action _action;
        private Func<T> _call;
        private ICallable<T> _callable;
        ICallable<T>[] _callables = new ICallable<T>[_size];
        Func<T>[] _calls = new Func<T>[3];
        private Action<IRunnable> _actionOnExecute;
        TestThreadManager ThreadManager { get; set; }

        private void Execute(IRunnable runnable)
        {
            _actionOnExecute(runnable);
        }

        private IContextCarrier SetupContextCarrier()
        {
            var ccf = MockRepository.GenerateStub<IContextCarrierFactory>();
            var cc = MockRepository.GenerateStub<IContextCarrier>();
            ccf.Stub(x => x.CreateContextCarrier()).Return(cc);

            _sut.ContextCarrierFactory = ccf;
            Assert.That(_sut.ContextCarrierFactory, Is.SameAs(ccf));
            return cc;
        }

        [SetUp] public void SetUp()
        {
            _sut = Mockery.GeneratePartialMock<AbstractExecutorService>();
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _action = MockRepository.GenerateMock<Action>();

            _calls = new Func<T>[_size];
            _callables = new ICallable<T>[_size];
            for (int i = _size - 1; i >= 0; i--)
            {
                _callables[i] = MockRepository.GenerateMock<ICallable<T>>();
                _calls[i] = MockRepository.GenerateMock<Func<T>>();
            }
            _call = _calls[0];
            _callable = _callables[0];

            _sut.Stub(x => x.Protected<Action<IRunnable>>("DoExecute")(Arg<IRunnable>.Is.NotNull))
                .Do(new Action<IRunnable>(Execute));

            _actionOnExecute = r => r.Run(); // Default run it.

            ThreadManager = new TestThreadManager();
        }

        [Test] public void ExecuteActionChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Execute((Action) null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void ExecuteRunnableChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Execute((IRunnable) null));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test] public void ExecuteActionCallsDoExecute()
        {
            _sut.Execute(_action);

            _action.AssertWasCalled(x => x());
            _sut.AssertWasCalled(x => x.Protected<Action<IRunnable>>("DoExecute")(Arg<IRunnable>.Is.NotNull));
        }

        [Test] public void ExecuteCopiesContextBeforeExecutesAction()
        {
            var cc = SetupContextCarrier();
            _sut.Execute(_action);

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _action.ActivityOf(x => x()));
            _sut.AssertWasCalled(x => x.Protected<Action<IRunnable>>("DoExecute")(Arg<IRunnable>.Is.NotNull));
        }

        [Test] public void ExecuteRunnableCallsDoExecute()
        {
            _sut.Execute(_runnable);

            _runnable.AssertWasCalled(x => x.Run());
            _sut.AssertWasCalled(x => x.Protected<Action<IRunnable>>("DoExecute")(Arg<IRunnable>.Is.NotNull));
        }

        [Test] public void ExecuteCopiesContextBeforeExecutesRunnable()
        {
            var cc = SetupContextCarrier();
            _sut.Execute(_runnable);

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _runnable.ActivityOf(x => x.Run()));
            _sut.AssertWasCalled(x => x.Protected<Action<IRunnable>>("DoExecute")(Arg<IRunnable>.Is.NotNull));
        }

        [Test] public void SubmitChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((IRunnable)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Action)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("action"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((ICallable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.Submit((Func<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }

        [Test] public void SubmitExecutesTheRunnable()
        {
            var future = _sut.Submit(_runnable);
            Assert.IsTrue(future.IsDone);
            _runnable.AssertWasCalled(x => x.Run());
        }

        [Test] public void SubmitCopiesContextBeforeExecutesRunnable()
        {
            IContextCarrier cc = SetupContextCarrier();
            var future = _sut.Submit(_runnable, TestData<T>.One);
            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _runnable.ActivityOf(x=>x.Run()));
        }

        [Test] public void SubmitExecutesTheRunnableAndSetFutureResult()
        {
            var future = _sut.Submit(_runnable);
            Assert.IsTrue(future.IsDone);
            _runnable.AssertWasCalled(x => x.Run());
        }

        [Test] public void SubmitExecutesTheTask()
        {
            var future = _sut.Submit(_action);
            Assert.IsTrue(future.IsDone);
            _action.AssertWasCalled(t => t());
        }

        [Test] public void SubmitCopiesContextBeforeExecutesAction()
        {
            IContextCarrier cc = SetupContextCarrier();
            var future = _sut.Submit(_action, TestData<T>.One);
            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _action.ActivityOf(x=>x()));
        }

        [Test] public void SubmitExecutesTheTaskAndSetFutureResult()
        {
            var future = _sut.Submit(_action, TestData<T>.One);

            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));
            _action.AssertWasCalled(t => t());
        }

        [Test] public void SubmitExecutesTheCallAndSetFutureResult()
        {
            _call.Stub(c=>c()).Return(TestData<T>.Two);

            var future = _sut.Submit(_call);

            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));

            _call.AssertWasCalled(c => c());
        }

        [Test] public void SubmitCopiesContextBeforeExecutesCall()
        {
            IContextCarrier cc = SetupContextCarrier();
            _call.Stub(x => x()).Return(TestData<T>.Two);
            var future = _sut.Submit(_call);
            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _call.ActivityOf(x=>x()));
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

        [Test] public void SubmitCopiesContextBeforeExecutesCallable()
        {
            IContextCarrier cc = SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Two);
            var future = _sut.Submit(_callable);
            Assert.IsTrue(future.IsDone);
            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));

            Mockery.Assert(cc.ActivityOf(x=>x.Restore()) < _callable.ActivityOf(x=>x.Call()));
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
                        const Func<T>[] calls = null;
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
                        Func<T>[] calls = new Func<T>[0];
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
                        Func<T>[] calls = new Func<T>[] { null };
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
                        const Func<T>[] calls = null;
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

        [Test] public void InvokeAllOrFailChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        const Func<T>[] calls = null;
                        if (!isTimed) _sut.InvokeAll(calls);
                        else _sut.InvokeAll(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
                    if (!isTimed) _sut.InvokeAllOrFail(calls);
                    else _sut.InvokeAllOrFail(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test] public void InvokeAllNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            Func<T>[] calls = new Func<T>[0];
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

        [Test] public void InvokeAllOrFailNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            Func<T>[] calls = new Func<T>[0];
            var futures = isTimed ? 
                _sut.InvokeAll(TestData.ShortDelay, calls) :
                _sut.InvokeAll(calls);
            CollectionAssert.IsEmpty(futures);

            ICallable<T>[] callables = new ICallable<T>[0];
            futures = isTimed ?
                _sut.InvokeAllOrFail(TestData.ShortDelay, callables) :
                _sut.InvokeAllOrFail(callables);
            CollectionAssert.IsEmpty(futures);
        }

        [Test] public void InvokeAllChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        Func<T>[] calls = new Func<T>[] {_call, null };
                        if (!isTimed) _sut.InvokeAll(calls);
                        else _sut.InvokeAll(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] {_callable, null };
                    if (!isTimed) _sut.InvokeAll(calls);
                    else _sut.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test] public void InvokeAllOrFailChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                    {
                        Func<T>[] calls = new Func<T>[] {_call, null };
                        if (!isTimed) _sut.InvokeAll(calls);
                        else _sut.InvokeAll(TestData.ShortDelay, calls);
                    });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] {_callable, null };
                    if (!isTimed) _sut.InvokeAllOrFail(calls);
                    else _sut.InvokeAllOrFail(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test] public void InvokeAnyChokesWhenNoTaskCompletes([Values(true, false)] bool isTimed)
        {
            var expected = new Exception();
            Func<T> call = delegate { throw expected; };
            var e1 = Assert.Throws<ExecutionException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[] { call };
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

            Func<T>[] calls = new Func<T>[] { call1, call2, call1, call2 };
            var result = isTimed ? 
                _sut.InvokeAny(TestData.SmallDelay, calls) :
                _sut.InvokeAny(calls);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void InvokeAnyCopiesContextBeforeExecuteCall([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            _call.Stub(x => x()).Return(TestData<T>.Four);
            var result = isTimed ?
                _sut.InvokeAny(TestData.SmallDelay, _call) :
                _sut.InvokeAny(_call);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            Mockery.Assert(cc.ActivityOf(x => x.Restore()) < _call.ActivityOf(x => x()));
        }

        [Test] public void InvokeAnyCreatesOnlyOneContextCopierForAllCalls([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            SetupContextCarrier();
            _call.Stub(x => x()).Return(TestData<T>.Four)
                //important to make all of then submitted are called.
                .WhenCalled(i => Thread.Sleep(TestData.ShortDelay)); 
            var result = isTimed ?
                _sut.InvokeAny(TestData.SmallDelay, _call, _call) :
                _sut.InvokeAny(_call, _call);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
            _sut.AssertWasCalled(x => x.NewContextCarrier(), m=>m.Repeat.Once());
        }

        [Test] public void InvokeAnyCopiesContextWhenNewTaskForCallWasOverriden([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            var runnableFuture = MockRepository.GenerateMock<IRunnableFuture<T>>();
            _call.Stub(x => x()).Return(TestData<T>.Four);
            _sut.Stub(x => x.NewTaskFor(Arg<Func<T>>.Is.NotNull)).Return(runnableFuture);
            runnableFuture.Stub(x => x.Run()).Do(new Action(()=>_call()));

            if (isTimed)
                _sut.InvokeAny(TestData.SmallDelay, _call, _call);
            else
                _sut.InvokeAny(_call, _call);
            runnableFuture.AssertWasCalled(x => x.Run());
            Mockery.Assert(cc.ActivityOf(x=>x.Restore()).First < _call.ActivityOf(x=>x()).First);
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

        [Test] public void InvokeAnyCopiesContextBeforeExecuteCallable([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            var result = isTimed ?
                _sut.InvokeAny(TestData.SmallDelay, _callable) :
                _sut.InvokeAny(_callable);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            Mockery.Assert(cc.ActivityOf(x => x.Restore()) < _callable.ActivityOf(x => x.Call()));
        }

        [Test] public void InvokeAnyCreatesOnlyOneContextCopierForAllCallables([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four)
                //important to make all of then submitted are called.
                .WhenCalled(i => Thread.Sleep(TestData.ShortDelay));
            var result = isTimed ?
                _sut.InvokeAny(TestData.SmallDelay, _callable, _callable) :
                _sut.InvokeAny(_callable, _callable);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
            _sut.AssertWasCalled(x => x.NewContextCarrier(), m => m.Repeat.Once());
        }

        [Test] public void InvokeAnyCopiesContextWhenNewTaskForCallableWasOverriden([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            var runnableFuture = MockRepository.GenerateMock<IRunnableFuture<T>>();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            _sut.Stub(x => x.NewTaskFor(Arg<ICallable<T>>.Is.NotNull)).Return(runnableFuture);
            runnableFuture.Stub(x => x.Run()).Do(new Action(() => _callable.Call()));

            if (isTimed)
                _sut.InvokeAny(TestData.SmallDelay, _callable, _callable);
            else
                _sut.InvokeAny(_callable, _callable);
            runnableFuture.AssertWasCalled(x => x.Run());
            Mockery.Assert(cc.ActivityOf(x => x.Restore()).First < _callable.ActivityOf(x => x.Call()).First);
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

        [Test] public void InvokeAllCopiesContextBeforeExecuteCallable([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            var futures = isTimed ?
                _sut.InvokeAll(TestData.SmallDelay, _callable) :
                _sut.InvokeAll(_callable);
            foreach (IFuture<T> future in futures)
            {
                Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Four));
            }
            Mockery.Assert(cc.ActivityOf(x => x.Restore()) < _callable.ActivityOf(x => x.Call()));
        }

        [Test] public void InvokeAllCreatesOnlyOneContextCopierForAllCallables([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four)
                //important to make all of then submitted are called.
                .WhenCalled(i => Thread.Sleep(TestData.ShortDelay));
            var futures = isTimed ?
                _sut.InvokeAll(TestData.SmallDelay, _callable, _callable) :
                _sut.InvokeAll(_callable, _callable);
            foreach (IFuture<T> future in futures)
            {
                Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Four));
            }
            ThreadManager.JoinAndVerify();
            _sut.AssertWasCalled(x => x.NewContextCarrier(), m => m.Repeat.Once());
        }

        [Test] public void InvokeAllCopiesContextWhenNewTaskForCallableWasOverriden([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            var runnableFuture = MockRepository.GenerateMock<IRunnableFuture<T>>();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            _sut.Stub(x => x.NewTaskFor(Arg<ICallable<T>>.Is.NotNull)).Return(runnableFuture);
            runnableFuture.Stub(x => x.Run()).Do(new Action(() => _callable.Call()));

            if (isTimed)
                _sut.InvokeAll(TestData.SmallDelay, _callable, _callable);
            else
                _sut.InvokeAll(_callable, _callable);
            runnableFuture.AssertWasCalled(x => x.Run());
            Mockery.Assert(cc.ActivityOf(x => x.Restore()).First < _callable.ActivityOf(x => x.Call()).First);
        }

        [Test] public void InvokeAllOrFailReturnsWhenAllCallableComplete([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            for (int i = _callables.Length - 1; i >= 0; i--)
            {
                _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ? 
                _sut.InvokeAllOrFail(TestData.ShortDelay, _callables) :
                _sut.InvokeAllOrFail(_callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test] public void InvokeAllOrFailCopiesContextBeforeExecuteCallable([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            var futures = isTimed ?
                _sut.InvokeAllOrFail(TestData.SmallDelay, _callable) :
                _sut.InvokeAllOrFail(_callable);
            foreach (IFuture<T> future in futures)
            {
                Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Four));
            }
            Mockery.Assert(cc.ActivityOf(x => x.Restore()) < _callable.ActivityOf(x => x.Call()));
        }

        [Test] public void InvokeAllOrFailCreatesOnlyOneContextCopierForAllCallables([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            SetupContextCarrier();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four)
                //important to make all of then submitted are called.
                .WhenCalled(i => Thread.Sleep(TestData.ShortDelay));
            var futures = isTimed ?
                _sut.InvokeAllOrFail(TestData.SmallDelay, _callable, _callable) :
                _sut.InvokeAllOrFail(_callable, _callable);
            foreach (IFuture<T> future in futures)
            {
                Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Four));
            }
            ThreadManager.JoinAndVerify();
            _sut.AssertWasCalled(x => x.NewContextCarrier(), m => m.Repeat.Once());
        }

        [Test] public void InvokeAllOrFailCopiesContextWhenNewTaskForCallableWasOverriden([Values(true, false)] bool isTimed)
        {
            var cc = SetupContextCarrier();
            var runnableFuture = MockRepository.GenerateMock<IRunnableFuture<T>>();
            _callable.Stub(x => x.Call()).Return(TestData<T>.Four);
            _sut.Stub(x => x.NewTaskFor(Arg<ICallable<T>>.Is.NotNull)).Return(runnableFuture);
            runnableFuture.Stub(x => x.Run()).Do(new Action(() => _callable.Call()));

            if (isTimed)
                _sut.InvokeAllOrFail(TestData.SmallDelay, _callable, _callable);
            else
                _sut.InvokeAllOrFail(_callable, _callable);
            runnableFuture.AssertWasCalled(x => x.Run());
            Mockery.Assert(cc.ActivityOf(x => x.Restore()).First < _callable.ActivityOf(x => x.Call()).First);
        }


        [Test] public void InvokeAllOrFailReturnsWhenAllCallComplete([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ? 
                _sut.InvokeAllOrFail(TestData.ShortDelay, _calls) :
                _sut.InvokeAllOrFail(_calls);
            Assert.That(futures.Count, Is.EqualTo(_calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test] public void InvokeAllOrFailChokesWhenCallFailed([Values(true, false)] bool isTimed)
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);
            var expectedException = new Exception();
            _calls[0].Stub(c => c()).Do(
                new Func<T>(
                    delegate{
                        Thread.Sleep(TestData.ShortDelay);
                        throw expectedException;
                    }));
            ThreadInterruptedException call1Interrupted = null;
            _calls[1].Stub(c => c()).Do(
                new Func<T>(
                    delegate
                        {
                            call1Interrupted = Assert.Throws<ThreadInterruptedException>(
                                () => Thread.Sleep(TestData.SmallDelay));
                            return default(T);
                        }));
            for (int i = 2; i < _size; i++)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
            }

            var e  = Assert.Throws<ExecutionException>(
                delegate 
                    {
                        if (isTimed)
                            _sut.InvokeAllOrFail(TestData.SmallDelay, _calls);
                        else
                            _sut.InvokeAllOrFail(_calls);
                    });
            ThreadManager.JoinAndVerify(); // this ensures memeory barrier so we can assert below.
            Assert.That(e.InnerException, Is.SameAs(expectedException));
            Assert.That(call1Interrupted, Is.Not.Null);
            _calls[2].AssertWasCalled(c => c());
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
                if (i > _halfSize+1) //TODO: why +1?
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

        [Test] public void InvokeAllOrFailChokeWhenTimeoutOnCalls()
        {
            _actionOnExecute = r => ThreadManager.StartAndAssertRegistered("T", r.Run);

            int count = 0;
            for (int i = 0; i < _size; i++)
            {
                _calls[i].Stub(c => c()).Do(
                new Func<T>(
                    delegate
                    {
                        Assert.Throws<ThreadInterruptedException>(() => Thread.Sleep(TestData.MediumDelay));
                        Interlocked.Increment(ref count);
                        return default(T);
                    }));
            }

            Assert.Throws<TimeoutException>(
                delegate
                    {
                        _sut.InvokeAllOrFail(TestData.ShortDelay, _calls);
                    });
            ThreadManager.JoinAndVerify(); // this ensures memeory barrier so we can assert below.
            Assert.That(count, Is.EqualTo(_size));
        }

    }
}