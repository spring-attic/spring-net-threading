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
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    public abstract class ExecutorServiceTestFixtureBase : ThreadingTestFixture
    {
        private IExecutorService _executorService;

        protected IExecutorService ExecutorService
        {
            get
            {
                if (_executorService == null) 
                    _executorService = NewExecutorService();
                return _executorService;
            }
            set
            {
                _executorService = value;
            }
        }

        protected IRunnable _runnable;
        protected Action _action;
        private bool? _joinPoolWithInterrupt;
        protected Action _mediumInterruptableAction;
        protected const int _size = 5;

        [SetUp] public void SetUpExecutorAndActions()
        {
            _joinPoolWithInterrupt = null;
            _executorService = null;
            _mediumInterruptableAction = NewInterruptableAction(MEDIUM_DELAY);
            _runnable = MockRepository.GenerateMock<IRunnable>();
            _action = MockRepository.GenerateMock<Action>();
        }

        [TearDown] public void ShutdownExecutor()
        {
            var es = ExecutorService;
            ExecutorService = null;
            if (es != null && !es.IsTerminated)
            {
                if (_joinPoolWithInterrupt == null)
                {
                    InterruptAndJoinPool(es);
                }
                else
                {
                    if (_joinPoolWithInterrupt == false) es.ShutdownNow();
                    Assert.Fail("Thread pool was not terminated.");
                }
            }
        }

        protected override void OnJoinPool(IExecutorService exec, bool isInterrupted)
        {
            if (exec == ExecutorService) _joinPoolWithInterrupt = isInterrupted;
        }

        protected Action NewInterruptableAction(TimeSpan timeout)
        {
            return ThreadManager.NewVerifiableTask(
                () =>
                {
                    try { Thread.Sleep(timeout); }
                    catch (ThreadInterruptedException) { }
                });
        }

        protected abstract IExecutorService NewExecutorService();
    }

    public abstract class ExecutorServiceTestFixture : ExecutorServiceTestFixtureBase
    {
        [Test]
        public void ExecuteActionChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => ExecutorService.Execute((Action)null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test]
        public void ExecuteRunnableChokesOnNullArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => ExecutorService.Execute((IRunnable)null));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test]
        public void SubmitExecutesTheRunnable()
        {
            var future = ExecutorService.Submit(_runnable);
            future.GetResult();
            Assert.IsTrue(future.IsDone);
            _runnable.AssertWasCalled(x => x.Run());
            JoinPool(ExecutorService);
        }

        [Test]
        public void SubmitExecutesTheAction()
        {
            var future = ExecutorService.Submit(_action);
            future.GetResult();
            Assert.IsTrue(future.IsDone);
            _action.AssertWasCalled(t => t());
            JoinPool(ExecutorService);
        }

        [Test, Description("IsShutdown returns false before shutdown, true after")] 
        public void IsShutdownReturnsFalseBeforeShutdownTrueAfter() 
        {
            //ExecutorService = NewThreadPoolExecutor(1, 1);
            Assert.IsFalse(ExecutorService.IsShutdown);

            ExecutorService.Execute(_mediumInterruptableAction);
            ExecutorService.Shutdown();

            Assert.IsTrue(ExecutorService.IsShutdown);
            InterruptAndJoinPool(ExecutorService);
        }

        [Test, Description("ShutdownNow returns a list containing tasks that were not run")]
        public void ShutdownNowReturnsTasksThatWereNotRun()
        {
            var nRun = new AtomicInteger(0);
            var action = ThreadManager.NewVerifiableTask(
                () =>
                {
                    nRun.IncrementValueAndReturn();
                    try { Thread.Sleep(MEDIUM_DELAY); }
                    catch (ThreadInterruptedException) { }
                });
            var es = ExecutorService;
            int submitCount = 0;
            
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    es.Execute(action);
                    submitCount++;
                }
            }
            catch (RejectedExecutionException) { }

            IList<IRunnable> l = es.ShutdownNow();
            Assert.IsTrue(es.IsShutdown);
            Assert.IsNotNull(l);
            Assert.That(l.Count, Is.LessThanOrEqualTo(submitCount-nRun.Value));
            ThreadManager.JoinAndVerify(LONG_DELAY);
        }

        [Test, Description("Executor rejects task when saturated")]
        public void ExecuteChokesWhenSaturated()
        {
            Assert.Throws<RejectedExecutionException>(
                () =>
                    {
                        for (int i = 0; i < 1000; ++i)
                            ExecutorService.Execute(_mediumInterruptableAction);
                    });
            InterruptAndJoinPool((ExecutorService));
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("Execute rejects task on shutdown")]
        public void ExecuteRejectsTaskOnShutdown()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            ExecutorService.Shutdown();

            Assert.Throws<RejectedExecutionException>(() => ExecutorService.Execute(runnable));

            JoinPool(ExecutorService);
            runnable.AssertWasNotCalled(r => r.Run());
        }

        [Test, Description("Completed Submit of runnable returns sucessfully")]
        public void SubmitReturnsFutureThatReturnsResultOfCompletedRunnable()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            IFuture<object> future = ExecutorService.Submit(runnable);
            future.GetResult();
            runnable.AssertWasCalled(c => c.Run());
            JoinPool(ExecutorService);
        }

        [Test, Description("Completed Submit of action returns sucessfully")]
        public void SubmitReturnsFutureThatReturnsResultOfCompletedAction()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            IFuture<object> future = ExecutorService.Submit(runnable);
            future.GetResult();
            runnable.AssertWasCalled(c => c.Run());
            JoinPool(ExecutorService);
        }

        [Test]
        public void ExecuteRunsActionAndRunnable()
        {
            bool actionCalled = false;
            bool runnableCalled = false;
            var es = ExecutorService;
            var action = ThreadManager.NewVerifiableTask(() => actionCalled = true);
            IRunnable runnable = new Runnable(ThreadManager.NewVerifiableTask(() => runnableCalled = true));

            es.Execute(action);
            es.Execute(runnable);

            JoinPool(es);
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(runnableCalled);
        }
    }

    /// <summary>
    /// Reusable test cases for implementaitons of <see cref="IExecutorService"/>.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    public abstract class ExecutorServiceTestFixture<T> : ExecutorServiceTestFixtureBase
    {
        const int _halfSize = _size / 2;
        protected Func<T> _call;
        protected ICallable<T> _callable;
        protected ICallable<T>[] _callables = new ICallable<T>[_size];
        protected Func<T>[] _calls = new Func<T>[3];

        [SetUp]
        public void SetUp()
        {
            _calls = new Func<T>[_size];
            _callables = new ICallable<T>[_size];
            for (int i = _size - 1; i >= 0; i--)
            {
                _callables[i] = MockRepository.GenerateMock<ICallable<T>>();
                _calls[i] = MockRepository.GenerateMock<Func<T>>();
            }
            _call = _calls[0];
            _callable = _callables[0];
        }

        [Test]
        public void SubmitChokesOnNullArgument()
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                () => es.Submit((IRunnable)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
            e = Assert.Throws<ArgumentNullException>(
                () => es.Submit((Action)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("action"));
            e = Assert.Throws<ArgumentNullException>(
                () => es.Submit((ICallable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
            e = Assert.Throws<ArgumentNullException>(
                () => es.Submit((Func<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }

        [Test, Description("Submit(runnable, result£©runs the runnable and sets the future result")]
        public void SubmitExecutesTheRunnableAndSetFutureResult()
        {
            var future = ExecutorService.Submit(_runnable, TestData<T>.One);
            Assert.AreEqual(TestData<T>.One, future.GetResult());
            Assert.IsTrue(future.IsDone);
            _runnable.AssertWasCalled(x => x.Run());
            JoinPool(ExecutorService);
        }

        [Test, Description("Submit(action, result£©runs the action and sets the future result")]
        public void SubmitExecutesTheActionAndSetFutureResult()
        {
            var future = ExecutorService.Submit(_action, TestData<T>.One);

            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.One));
            Assert.IsTrue(future.IsDone);
            _action.AssertWasCalled(t => t());
            JoinPool(ExecutorService);
        }

        [Test, Description("Submit(call) runs the call and set future result to its return value")]
        public void SubmitExecutesTheCallAndSetFutureResult()
        {
            _call.Stub(c => c()).Return(TestData<T>.Two);

            var future = ExecutorService.Submit(_call);

            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));
            Assert.IsTrue(future.IsDone);
            _call.AssertWasCalled(c => c());
            JoinPool(ExecutorService);
        }

        [Test, Description("Submit(callable) runs the callable and set future result to its return value")]
        public void SubmitExecutesTheCallableAndSetFutureResult()
        {
            _callable.Stub(c => c.Call()).Return(TestData<T>.Two);

            var future = ExecutorService.Submit(_callable);

            Assert.That(future.GetResult(), Is.EqualTo(TestData<T>.Two));
            Assert.IsTrue(future.IsDone);
            _callable.AssertWasCalled(c => c.Call());
            JoinPool(ExecutorService);
        }

        [Test]
        public void SubmitReturnsFutureCanBeInterruptedOnGetResult()
        {
            _callable.Stub(x => x.Call()).Do(new Delegates.Function<T>(
                delegate
                {
                    Assert.Throws<ThreadInterruptedException>(() => Thread.Sleep(TestData.SmallDelay));
                    return TestData<T>.One;
                }));

            var t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    var future = ExecutorService.Submit(_callable);
                    Assert.Throws<ThreadInterruptedException>(() => future.GetResult());
                });
            Thread.Sleep(TestData.ShortDelay);
            t.Interrupt();
            InterruptAndJoinPool(ExecutorService);
            ThreadManager.JoinAndVerify();
            _callable.AssertWasCalled(c => c.Call());
        }

        [Test]
        public void SubmitReturnsFutureChokesOnGetResultWhenTaskChokes()
        {
            var exception = new Exception();
            _callable.Stub(c => c.Call()).Do(new Delegates.Function<T>(
                delegate
                {
                    Thread.Sleep(TestData.ShortDelay);
                    throw exception;
                }));

            var future = ExecutorService.Submit(_callable);
            var e = Assert.Throws<ExecutionException>(() => future.GetResult());
            Assert.That(e.InnerException, Is.SameAs(exception));
            ThreadManager.JoinAndVerify();
            _callable.AssertWasCalled(c => c.Call());
        }

        [Test]
        public void InvokeAnyChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const Func<T>[] calls = null;
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test, Description("InvokeAny(empty collection) throws ArgumentException")]
        public void InvokeAnyChokesOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[0];
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[0];
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test]
        public void InvokeAnyChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            _call.Stub(c => c()).WhenCalled(m => Thread.Sleep(SHORT_DELAY));
            _callable.Stub(c => c.Call()).WhenCalled(m => Thread.Sleep(SHORT_DELAY));
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[] {_call, null };
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] {_callable, null };
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test, Description("InvokeAll(null) throws ArgumentNullException")]
        public void InvokeAllChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const Func<T>[] calls = null;
                    if (!isTimed) es.InvokeAll(calls);
                    else es.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
                    if (!isTimed) es.InvokeAll(calls);
                    else es.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test]
        public void InvokeAllOrFailChokesOnNullArgument([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const Func<T>[] calls = null;
                    if (!isTimed) es.InvokeAll(calls);
                    else es.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    const ICallable<T>[] calls = null;
                    if (!isTimed) es.InvokeAllOrFail(calls);
                    else es.InvokeAllOrFail(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("tasks"));
        }

        [Test, Description("InvokeAll(empty collection) returns empty collection")]
        public void InvokeAllNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            Func<T>[] calls = new Func<T>[0];
            var es = ExecutorService;
            var futures = isTimed ?
                es.InvokeAll(TestData.ShortDelay, calls) :
                es.InvokeAll(calls);
            CollectionAssert.IsEmpty(futures);

            ICallable<T>[] callables = new ICallable<T>[0];
            futures = isTimed ?
                es.InvokeAll(TestData.ShortDelay, callables) :
                es.InvokeAll(callables);
            CollectionAssert.IsEmpty(futures);
        }

        [Test]
        public void InvokeAllOrFailNopOnEmptyTaskCollection([Values(true, false)] bool isTimed)
        {
            Func<T>[] calls = new Func<T>[0];
            var es = ExecutorService;
            var futures = isTimed ?
                es.InvokeAll(TestData.ShortDelay, calls) :
                es.InvokeAll(calls);
            CollectionAssert.IsEmpty(futures);

            ICallable<T>[] callables = new ICallable<T>[0];
            futures = isTimed ?
                es.InvokeAllOrFail(TestData.ShortDelay, callables) :
                es.InvokeAllOrFail(callables);
            CollectionAssert.IsEmpty(futures);
        }

        [Test, Description("InvokeAll(c) throws ArgumentNullException if c has null elements")]
        public void InvokeAllChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[] { _call, null };
                    if (!isTimed) es.InvokeAll(calls);
                    else es.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] { _callable, null };
                    if (!isTimed) es.InvokeAll(calls);
                    else es.InvokeAll(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test]
        public void InvokeAllOrFailChokesOnNullTaskInCollection([Values(true, false)] bool isTimed)
        {
            var es = ExecutorService;
            var e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[] { _call, null };
                    if (!isTimed) es.InvokeAllOrFail(calls);
                    else es.InvokeAllOrFail(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("call"));

            e = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ICallable<T>[] calls = new ICallable<T>[] { _callable, null };
                    if (!isTimed) es.InvokeAllOrFail(calls);
                    else es.InvokeAllOrFail(TestData.ShortDelay, calls);
                });
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

        [Test, Description("InvokeAny(c) throws ExecutionException if no task completes")]
        public void InvokeAnyChokesWhenNoTaskCompletes([Values(true, false)] bool isTimed)
        {
            var expected = new Exception();
            Func<T> call = delegate { throw expected; };
            var es = ExecutorService;
            var e1 = Assert.Throws<ExecutionException>(
                delegate
                {
                    Func<T>[] calls = new Func<T>[] { call };
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e1.InnerException, Is.SameAs(expected));

            var e2 = Assert.Throws<ExecutionException>(
                delegate
                {
                    ICallable<T>[] calls = new Callable<T>[] { call };
                    if (!isTimed) es.InvokeAny(calls);
                    else es.InvokeAny(TestData.ShortDelay, calls);
                });
            Assert.That(e2.InnerException, Is.SameAs(expected));
        }

        [Test, Description("InvokeAny returns result of any successfully completed calls")]
        public void InvokeAnyReturnsWhenOneCallCompletes([Values(true, false)] bool isTimed)
        {
            var call1 = _calls[0];
            var call2 = _calls[1];
            call1.Stub(c => c()).Throw(new Exception());
            call2.Stub(c => c()).Return(TestData<T>.Four).WhenCalled(i => Thread.Sleep(TestData.ShortDelay));

            Func<T>[] calls = new Func<T>[] { call1, call2, call1, call2 };
            var result = isTimed ?
                ExecutorService.InvokeAny(TestData.SmallDelay, calls) :
                ExecutorService.InvokeAny(calls);
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("InvokeAny returns result of any successfully completed callables")]
        public void InvokeAnyReturnsWhenOneCallableCompletes([Values(true, false)] bool isTimed)
        {
            var call1 = _callables[0];
            var call2 = _callables[1];
            var call3 = _callables[2];
            call1.Stub(c => c.Call()).Throw(new Exception());
            call2.Stub(c => c.Call()).Return(TestData<T>.Four).WhenCalled(i => Thread.Sleep(SHORT_DELAY));
            call3.Stub(c => c.Call()).Return(TestData<T>.Four).WhenCalled(i => _mediumInterruptableAction());

            ICallable<T>[] calls = new ICallable<T>[] { call1, call2, call3 };
            var result = isTimed ?
                ExecutorService.InvokeAny(TestData.SmallDelay, calls) :
                ExecutorService.InvokeAny(calls);
            call1.AssertWasCalled(c => c.Call());
            Assert.That(result, Is.EqualTo(TestData<T>.Four));
        }

        [Test, Description("InvokeAll returns results of all completed callables")]
        public void InvokeAllReturnsWhenAllCallablesComplete([Values(true, false)] bool isTimed)
        {
            for (int i = _callables.Length - 1; i > 0; i--)
            {
                _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
            }
            var e = new Exception();
            _callables[0].Stub(c => c.Call()).Throw(e);

            var futures = isTimed ?
                ExecutorService.InvokeAll(TestData.ShortDelay, _callables) :
                ExecutorService.InvokeAll(_callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i > 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
            Assert.IsTrue(futures[0].IsDone);
            var ee = Assert.Throws<ExecutionException>(()=>futures[0].GetResult());
            Assert.That(ee.InnerException, Is.SameAs(e));
        }

        [Test, Description("InvokeAll returns results of all completed calls")]
        public void InvokeAllReturnsWhenAllCallsComplete([Values(true, false)] bool isTimed)
        {
            for (int i = _calls.Length - 1; i > 0; i--)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
            }
            var e = new Exception();
            _calls[0].Stub(c => c()).Throw(e);

            var futures = isTimed ?
                ExecutorService.InvokeAll(TestData.ShortDelay, _calls) :
                ExecutorService.InvokeAll(_calls);
            Assert.That(futures.Count, Is.EqualTo(_calls.Length));
            for (int i = futures.Count - 1; i > 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
            Assert.IsTrue(futures[0].IsDone);
            var ee = Assert.Throws<ExecutionException>(() => futures[0].GetResult());
            Assert.That(ee.InnerException, Is.SameAs(e));
        }

        [Test]
        public void InvokeAllOrFailReturnsWhenAllCallableComplete([Values(true, false)] bool isTimed)
        {
            for (int i = _callables.Length - 1; i >= 0; i--)
            {
                _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ?
                ExecutorService.InvokeAllOrFail(TestData.ShortDelay, _callables) :
                ExecutorService.InvokeAllOrFail(_callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test]
        public void InvokeAllOrFailReturnsWhenAllCallComplete([Values(true, false)] bool isTimed)
        {
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
            }

            var futures = isTimed ?
                ExecutorService.InvokeAllOrFail(TestData.ShortDelay, _calls) :
                ExecutorService.InvokeAllOrFail(_calls);
            Assert.That(futures.Count, Is.EqualTo(_calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(futures[i].IsDone);
                Assert.That(futures[i].GetResult(), Is.EqualTo(TestData<T>.MakeData(i)));
            }
        }

        [Test]
        public void InvokeAllOrFailChokesWhenOneCallFailedAndCancelsTheRest([Values(true, false)] bool isTimed)
        {
            var expectedException = new Exception();
            _calls[0].Stub(c => c()).Do(
                new Func<T>(
                    delegate
                    {
                        Thread.Sleep(TestData.ShortDelay);
                        throw expectedException;
                    }));
            bool call1Called = false;
            bool call1Interrupted = false;
            _calls[1].Stub(c => c()).Do(
                new Func<T>(
                    delegate
                    {
                        call1Called = true;
                        Assert.Throws<ThreadInterruptedException>(
                            () => Thread.Sleep(TestData.LongDelay));
                        call1Interrupted = true;
                        return default(T);
                    }));
            for (int i = 2; i < _size; i++)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
            }

            var e = Assert.Throws<ExecutionException>(
                delegate
                {
                    if (isTimed)
                        ExecutorService.InvokeAllOrFail(TestData.SmallDelay, _calls);
                    else
                        ExecutorService.InvokeAllOrFail(_calls);
                });
            // this ensures memeory barrier so we can assert below.
            JoinPool(ExecutorService); 

            Assert.That(e.InnerException, Is.SameAs(expectedException));
            Assert.IsTrue(call1Interrupted == call1Called);
        }

        [Test]
        public void TimedInvokeAnyChokesWhenTimeoutOnCalls()
        {
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                _calls[i].Stub(c => c()).Return(TestData<T>.Four)
                    .WhenCalled(x => Thread.Sleep(TestData.SmallDelay));
            }

            Assert.Throws<TimeoutException>(() => ExecutorService.InvokeAny(TestData.ShortDelay, _calls));
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("InvokeAll(timeout, c) Cancels callables not completed by timeout")]
        public void TimedInvokeAllCancelsUncompletedCallablesWhenTimeout()
        {
            for (int i = _callables.Length - 1; i >= 0; i--)
            {
                var x = _callables[i].Stub(c => c.Call()).Return(TestData<T>.MakeData(i));
                if (i > _halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.MediumDelayMillis*2));
            }

            var futures = ExecutorService.InvokeAll(TestData.ShortDelay, _callables);
            Assert.That(futures.Count, Is.EqualTo(_callables.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i > _halfSize)
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

        [Test, Description("InvokeAll(timeout, c) Cancels calls not completed by timeout")]
        public void TimedInvokeAllCancelsUncompletedCallsWhenTimeout()
        {
            for (int i = _calls.Length - 1; i >= 0; i--)
            {
                var x = _calls[i].Stub(c => c()).Return(TestData<T>.MakeData(i));
                if (i > _halfSize)
                    x.WhenCalled(invoke => Thread.Sleep(TestData.SmallDelay));
            }

            var futures = ExecutorService.InvokeAll(TestData.ShortDelay, _calls);
            Assert.That(futures.Count, Is.EqualTo(_calls.Length));
            for (int i = futures.Count - 1; i >= 0; i--)
            {
                if (i > _halfSize)
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

        [Test]
        public void InvokeAllOrFailChokeWhenTimeoutOnCalls()
        {
            for (int i = 0; i < _size; i++)
            {
                _calls[i].Stub(c => c()).Do(
                new Func<T>(
                    delegate
                    {
                        Assert.Throws<ThreadInterruptedException>(() => Thread.Sleep(TestData.MediumDelay));
                        return default(T);
                    }));
            }

            Assert.Throws<TimeoutException>(
                delegate
                {
                    ExecutorService.InvokeAllOrFail(TestData.ShortDelay, _calls);
                });
            JoinPool(ExecutorService);
        }

    }
}