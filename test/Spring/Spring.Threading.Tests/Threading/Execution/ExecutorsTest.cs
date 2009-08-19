using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="Executors"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class ExecutorsTest : ThreadingTestFixture
    {
        private static void CanExecuteRunnable(IExecutor e)
        {
            Action task = delegate {};
            e.Execute(task);
            e.Execute(task);
            e.Execute(task);
        }

        [Test] public void NewCachedThreadPoolCanExecuteRunnables()
        {
            var e = Executors.NewCachedThreadPool();
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewCachedThreadPoolByFactoryCanExecuteRunnables()
        {
            var e = Executors.NewCachedThreadPool(new SimpleThreadFactory());
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewCachedThreadPoolChokesOnNullFactory()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.NewCachedThreadPool(null));
            Assert.That(e.ParamName, Is.EqualTo("threadFactory"));
        }
    
        [Test] public void NewSingleThreadExecutorCanExecuteRunnables()
        {
            var e = Executors.NewSingleThreadExecutor();
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewSingleThreadExecutorWithFactoryCanExecuteRunnables()
        {
            var e = Executors.NewSingleThreadExecutor(new SimpleThreadFactory());
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewSingleThreadPoolChokesOnNullFactory()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.NewSingleThreadExecutor(null));
            Assert.That(e.ParamName, Is.EqualTo("threadFactory"));
        }

        [Test] public void NewFixedThreadPoolCanExecuteRunnables()
        {
            var e = Executors.NewFixedThreadPool(2);
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewFixedThreadPoolByFactoryCanExecuteRunnables()
        {
            var e = Executors.NewFixedThreadPool(2, new SimpleThreadFactory());
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void NewFixedThreadPoolChokesOnNullFactory()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.NewFixedThreadPool(2, null));
            Assert.That(e.ParamName, Is.EqualTo("threadFactory"));
        }

        [Test] public void NewFixedThreadPollChokesOnNonPositiveSize()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => Executors.NewFixedThreadPool(0));
            Assert.That(e.ParamName, Is.EqualTo("threadPoolSize"));
            var e2 = Assert.Throws<ArgumentOutOfRangeException>(
                () => Executors.NewFixedThreadPool(0, new SimpleThreadFactory()));
            Assert.That(e2.ParamName, Is.EqualTo("threadPoolSize"));
        }

        [Test] public void UnconfigurableExecutorServiceCanExecuteRunnables()
        {
            var e = Executors.UnconfigurableExecutorService(Executors.NewFixedThreadPool(2));
            CanExecuteRunnable(e);
            JoinPool(e);
        }

        [Test] public void UnconfigurableExecutorServiceChokesOnNullArgument()
        {
            var e2 = Assert.Throws<ArgumentNullException>(
                () => Executors.UnconfigurableExecutorService(null));
            Assert.That(e2.ParamName, Is.EqualTo("executor"));
        }

        [Test] public void UnconfigurableExecutorCannotBeCastedToConcreteImplementation()
        {
            var original = new ThreadPoolExecutor(1, 1, TimeSpan.Zero, new LinkedBlockingQueue<IRunnable>());
            IExecutorService e = Executors.UnconfigurableExecutorService(original);

            Assert.That(e, Is.Not.InstanceOf<ThreadPoolExecutor>());
            JoinPool(e);
        }

        [Test] public void TimeoutsFromExecuteWillTimeOutIfTheyComputeTooLong()
        {
            const int n = 10000;
            IExecutorService executor = Executors.NewSingleThreadExecutor();
            List<ICallable<int>> tasks = new List<ICallable<int>>(n);
            try
            {
                DateTime startTime = DateTime.UtcNow;

                while (tasks.Count < n)
                {
                    tasks.Add(new TimedCallable<int>(executor,
                        delegate { Thread.SpinWait(300); return 1; }, TimeSpan.FromMilliseconds(1)));
                }

                int iters = 0;
                foreach (var task in tasks)
                {
                    try
                    {
                        ++iters;
                        task.Call();
                    }
                    catch (TimeoutException)
                    {
                        Assert.That(iters, Is.GreaterThan(0));
                        return;
                    }
                }
                // if by chance we didn't ever time out, total time must be small
                TimeSpan elapsed = DateTime.UtcNow - startTime;
                Assert.That(elapsed.TotalMilliseconds, Is.LessThan(n));
            }
            finally
            {
                JoinPool(executor);
            }
        }

        [Test] public void NewDefaultThreadFactoryHasSpecifiedPriorityBackgroundStatusAndName()
        {
            Action r = ThreadManager.NewVerifiableTask(
                delegate
                    {
                        Thread current = Thread.CurrentThread;
                        Assert.IsTrue(!current.IsBackground);
                        Assert.IsTrue(current.Priority <= ThreadPriority.Normal);
                        String name = current.Name;
                        Assert.IsTrue(name.EndsWith("thread-1"));
                    });
            IExecutorService e = Executors.NewSingleThreadExecutor(Executors.NewDefaultThreadFactory());

            e.Execute(r);
            e.Shutdown();
            Thread.Sleep(TestData.ShortDelay);
            JoinPool(e);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void CreateRunnableChokesOnNullAction()
        {
            var e = Assert.Throws<ArgumentNullException>(()=>Executors.CreateRunnable(null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void CreateRunnableRunsActionWhenCalled()
        {
            var action = MockRepository.GenerateMock<Action>();
            var r = Executors.CreateRunnable(action);
            r.Run();
            action.AssertWasCalled(a => a());
        }

        [Test] public void CreateCallableFromRunnableReturnsNullWhenCalled()
        {
            var runnable = MockRepository.GenerateMock<IRunnable>();
            var c = Executors.CreateCallable(runnable);

            Assert.That(c.Call(), Is.Null);
            runnable.AssertWasCalled(r => r.Run());
        }

        [Test] public void CreateCallableFromActionReturnsNullWhenCalled()
        {
            var action = MockRepository.GenerateMock<Action>();
            var c = Executors.CreateCallable(action);

            Assert.That(c.Call(), Is.Null);
            action.AssertWasCalled(a => a());
        }

        [Test] public void CreateCallableChokesOnNullRunnableArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                ()=>Executors.CreateCallable((IRunnable)null));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test] public void CreateCallableChokesOnNullActionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                ()=>Executors.CreateCallable((Action)null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        private class TimedCallable<T> : ICallable<T>
        {
            private readonly IExecutorService _exec;
            private readonly Func<T> _func;
            private readonly TimeSpan _duration;

            public TimedCallable(IExecutorService exec, Func<T> func, TimeSpan duration)
            {
                _exec = exec;
                _func = func;
                _duration = duration;
            }

            public T Call()
            {
                IFuture<T> ftask = _exec.Submit(_func);
                try
                {
                    return ftask.GetResult(_duration);
                }
                finally
                {
                    ftask.Cancel(true);
                }
            }
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ExecutorsTest<T>
    {
        [Test] public void CreateCallableFromRunnableReturnsSetRestulWhenCalled()
        {
            var runnable = MockRepository.GenerateMock<IRunnable>();
            T result = TestData<T>.Five;
            var c = Executors.CreateCallable(runnable, result);

            Assert.That(c.Call(), Is.EqualTo(result));
            runnable.AssertWasCalled(r => r.Run());
        }

        [Test] public void CreateCallableChokesOnNullRunnableArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCallable((IRunnable) null, default(T)));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test] public void CreateCallableFromActionReturnsSetRestulWhenCalled()
        {
            var action = MockRepository.GenerateMock<Action>();
            T result = TestData<T>.Five;
            var c = Executors.CreateCallable(action, result);

            Assert.That(c.Call(), Is.EqualTo(result));
            action.AssertWasCalled(a => a());
        }

        [Test] public void CreateCallableChokesOnNullActionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCallable((Action) null, default(T)));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void CreateCallableFromCallReturnsSetRestulWhenCalled()
        {
            var action = MockRepository.GenerateMock<Func<T>>();
            T result = TestData<T>.Five;
            action.Stub(a => a()).Return(result);
            var c = Executors.CreateCallable(action);

            Assert.That(c.Call(), Is.EqualTo(result));
            action.AssertWasCalled(a => a());
        }

        [Test] public void CreateCallableChokesOnNullCallArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCallable((Func<T>) null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }

        [Test] public void CreateCallFromRunnableReturnsSetRestulWhenCalled()
        {
            var runnable = MockRepository.GenerateMock<IRunnable>();
            T result = TestData<T>.Five;
            var call = Executors.CreateCall(runnable, result);

            Assert.That(call(), Is.EqualTo(result));
            runnable.AssertWasCalled(r => r.Run());
        }

        [Test] public void CreateCallChokesOnNullRunnableArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCall((IRunnable) null, default(T)));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }

        [Test] public void CreateCallFromActionReturnsSetRestulWhenCalled()
        {
            var action = MockRepository.GenerateMock<Action>();
            T result = TestData<T>.Five;
            var call = Executors.CreateCall(action, result);

            Assert.That(call(), Is.EqualTo(result));
            action.AssertWasCalled(a => a());
        }

        [Test] public void CreateCallChokesOnNullActionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCall((Action) null, default(T)));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void CreateCallFromCallableReturnsSetRestulWhenCalled()
        {
            var callable = MockRepository.GenerateMock<ICallable<T>>();
            T result = TestData<T>.Five;
            callable.Stub(c => c.Call()).Return(result);
            var call = Executors.CreateCall(callable);
            Assert.That(call(), Is.EqualTo(result));
            callable.AssertWasCalled(c => c.Call());
        }

        [Test] public void CreateCallChokesOnNullCallArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => Executors.CreateCall((ICallable<T>) null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }

    }
}
