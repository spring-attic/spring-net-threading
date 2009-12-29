using System;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.Framework;
using Spring.Threading.Execution;

namespace Spring.Threading.Future
{
    /// <summary>
    /// Test cases for <see cref="FutureTask{T}"/>
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class FutureTaskTest<T> : ThreadingTestFixture<T>
    {
        private readonly Func<T> _noOpCall = () => default(T);


        private class PublicFutureTask : FutureTask<T>
        {
            public PublicFutureTask(Func<T> call) : base(call)
            {
            }

            public bool CallRunAndReset()
            {
                return base.RunAndReset();
            }

            public void SetupResult(T result)
            {
                base.SetResult(result);
            }

            public void SetupException(Exception t)
            {
                base.SetException(t);
            }
        }


        [Test]
        public void ConstructorChokesOnNullArgumentCall()
        {
            var e = Assert.Throws<ArgumentNullException>(()=>
                new FutureTask<T>((Func<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("call"));
        }


        [Test]
        public void ConstructorChokesOnNullArgumentTask()
        {
            var e = Assert.Throws<ArgumentNullException>(() => 
                new FutureTask<T>((Action)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test]
        public void ConstructorChokesOnNullArgumentCallable()
        {
            var e = Assert.Throws<ArgumentNullException>(()=>
                new FutureTask<T>((ICallable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("callable"));
        }


        [Test]
        public void ConstructorChokesOnNullArgumentRunnable()
        {
            var e = Assert.Throws<ArgumentNullException>(() => 
                new FutureTask<T>((IRunnable)null, TestData<T>.One));
            Assert.That(e.ParamName, Is.EqualTo("runnable"));
        }


        [Test]
        public void IsDoneIsTrueWhenTaskCompletes()
        {
            FutureTask<T> task = new FutureTask<T>(_noOpCall);
            task.Run();
            Assert.IsTrue(task.IsDone);
            Assert.IsFalse(task.IsCancelled);
        }


        [Test]
        public void RunAndResetSucceedsOnNonCancelledTask()
        {
            PublicFutureTask task = new PublicFutureTask(_noOpCall);
            Assert.IsTrue(task.CallRunAndReset());
            Assert.IsFalse(task.IsDone);
        }


        [Test]
        public void RunAndResetFaisAfterCancellation()
        {
            PublicFutureTask task = new PublicFutureTask(_noOpCall);
            Assert.IsTrue(task.Cancel(false));
            Assert.IsFalse(task.CallRunAndReset());
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void SetResultWillBeReturned()
        {
            PublicFutureTask task = new PublicFutureTask(_noOpCall);
            T result = TestData<T>.One;
            task.SetupResult(result);
            Assert.That(task.GetResult(), Is.EqualTo(result));
        }


        [Test]
        public void SetExceptionCauseGetResultToChoke()
        {
            Exception nse = new ArgumentOutOfRangeException();
            PublicFutureTask task = new PublicFutureTask(_noOpCall);
            task.SetupException(nse);
            var ee = Assert.Throws<ExecutionException>(()=>task.GetResult());
            Exception cause = ee.InnerException;
            Assert.That(cause, Is.SameAs(nse));
        }

        [Test]
        public void CancelSecceedsBeforeRun([Values(true, false)] bool mayInterruptIfRunning)
        {
            FutureTask<T> task = new FutureTask<T>(_noOpCall);
            Assert.IsTrue(task.Cancel(mayInterruptIfRunning));
            task.Run();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }

        [Test]
        public void CancelFailsOnCompletedTask([Values(true, false)] bool mayInterruptIfRunning)
        {
            FutureTask<T> task = new FutureTask<T>(_noOpCall);
            task.Run();
            Assert.IsFalse(task.Cancel(mayInterruptIfRunning));
            Assert.IsTrue(task.IsDone);
            Assert.IsFalse(task.IsCancelled);
        }


        [Test]
        public void CancelInterruptsRunningTask()
        {
            var t = ThreadManager.GetManagedAction(
                delegate
                    {
                        Assert.Throws<ThreadInterruptedException>(() => Thread.Sleep(Delays.Medium));
                    });
            FutureTask<T> task = new FutureTask<T>(t, default(T));
            new Thread(task.Run).Start();

            Thread.Sleep(Delays.Short);
            Assert.IsTrue(task.Cancel(true));
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void CancelDoesNotInterruptRunningTask()
        {
            var called = false;
            FutureTask<T> task = new FutureTask<T>(
                ThreadManager.GetManagedAction(
                    delegate
                        {
                            Thread.Sleep(Delays.Small);
                            called = true;
                        }),
                default(T));
            ThreadManager.StartAndAssertRegistered(new Thread(task.Run){Name="T1"});
            Thread.Sleep(Delays.Short);
            Assert.IsTrue(task.Cancel());
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
            Assert.IsTrue(called);
        }

        [Test]
        public void GetResultRetrievesValueSetFromAnotherThread()
        {
            T result = TestData<T>.One;
            FutureTask<T> ft = new FutureTask<T>(()=>result);
            Assert.IsFalse(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.That(ft.GetResult(), Is.EqualTo(result));
                    });
            Thread.Sleep(Delays.Short);
            ft.Run();
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
        }


        [Test]
        public void TimedGetResultRetrievesValueSetFromAnotherThread()
        {
            T result = TestData<T>.One;
            FutureTask<T> ft = new FutureTask<T>(()=>result);
            Assert.IsFalse(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.That(ft.GetResult(Delays.Medium), Is.EqualTo(result));
                    });
            ft.Run();
            ThreadManager.JoinAndVerify();
            Assert.IsTrue(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
        }

        [Test]
        public void GetChokesWhenTaskIsCancelled([Values(true, false)] bool isTimed)
        {
            FutureTask<T> ft = new FutureTask<T>(
                () => Thread.Sleep(Delays.Medium), default(T));

            ThreadStart getter = () => Assert.Throws<CancellationException>(
                delegate
                    {
                        if (isTimed) ft.GetResult(Delays.Medium);
                        else ft.GetResult();
                    });

            ThreadManager.StartAndAssertRegistered("T", getter, ft.Run);
            Thread.Sleep(Delays.Short);
            ft.Cancel(true);
            ThreadManager.JoinAndVerify();
        }


        [Test]
        public void GetChokesWhenTaskChokes([Values(true, false)] bool isTimed)
        {
            FutureTask<T> ft = new FutureTask<T>(
                delegate
                    {
                        int z = 0;
                        return TestData<T>.MakeData(5/z);
                    });
            ft.Run();
            Assert.Throws<ExecutionException>(
                delegate
                    {
                        if (isTimed) ft.GetResult(Delays.Medium);
                        else ft.GetResult();
                    });
        }

        [Test]
        public void GetChokesWhenInterrupted([Values(true, false)] bool isTimed)
        {
            FutureTask<T> ft = new FutureTask<T>(_noOpCall);
            var t = ThreadManager.StartAndAssertRegistered(
                "T1",
                () => Assert.Throws<ThreadInterruptedException>(
                delegate
                    {
                        if (isTimed) ft.GetResult(Delays.Medium);
                        else ft.GetResult();
                    }));
            Thread.Sleep(Delays.Short);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test]
        public void GetChokesWhenTimeout()
        {
            FutureTask<T> ft = new FutureTask<T>(_noOpCall);

            Assert.Throws<TimeoutException>(() => ft.GetResult(TimeSpan.FromMilliseconds(1)));
        }
    }
}