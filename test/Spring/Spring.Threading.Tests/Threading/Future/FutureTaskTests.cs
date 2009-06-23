using System;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.Execution;

namespace Spring.Threading.Future
{
    [TestFixture]
    public class FutureTaskTest : BaseThreadingTestCase
    {
        private class NoOpCallable : ICallable<object>
        {
            #region ICallable Members

            public object Call()
            {
                return true;
            }

            #endregion
        }


        internal class PublicFutureTask<T> : FutureTask<T>
        {
            public PublicFutureTask(ICallable<T> r) : base(r)
            {
            }

            public bool CallRunAndReset()
            {
                return base.RunAndReset();
            }

            [Test]
            public void SetupResult(T result)
            {
                base.SetResult(result);
            }

            [Test]
            public void SetupException(Exception t)
            {
                base.SetException(t);
            }
        }


        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Constructor()
        {
            new FutureTask<bool>((Call<bool>)null);
            Assert.Fail("Should throw an exception.");
        }


        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Constructor2()
        {
            new FutureTask<bool>((Task)null, true);
            Assert.Fail("Should throw an exception.");
        }


        [Test]
        public void IsDone()
        {
            FutureTask<object> task = new FutureTask<object>(new NoOpCallable());
            task.Run();
            Assert.IsTrue(task.IsDone);
            Assert.IsFalse(task.IsCancelled);
        }


        [Test]
        public void RunAndReset()
        {
            PublicFutureTask<object> task = new PublicFutureTask<object>(new NoOpCallable());
            Assert.IsTrue(task.CallRunAndReset());
            Assert.IsFalse(task.IsDone);
        }


        [Test]
        public void ResetAfterCancel()
        {
            PublicFutureTask<object> task = new PublicFutureTask<object>(new NoOpCallable());
            Assert.IsTrue(task.Cancel(false));
            Assert.IsFalse(task.CallRunAndReset());
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void Set()
        {
            PublicFutureTask<object> task = new PublicFutureTask<object>(new NoOpCallable());
            task.SetupResult(one);
            Assert.AreEqual(task.GetResult(), one);
        }


        [Test]
        public void SetException()
        {
            Exception nse = new ArgumentOutOfRangeException();
            PublicFutureTask<object> task = new PublicFutureTask<object>(new NoOpCallable());
            task.SetupException(nse);
            try
            {
                task.GetResult();
                Assert.Fail("Should throw an exception.");
            }
            catch (ExecutionException ee)
            {
                Exception cause = ee.InnerException;
                Assert.AreEqual(cause, nse);
            }
        }


        [Test]
        public void CancelBeforeRun()
        {
            FutureTask<object> task = new FutureTask<object>(new NoOpCallable());
            Assert.IsTrue(task.Cancel(false));
            task.Run();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void CancelBeforeRun2()
        {
            FutureTask<object> task = new FutureTask<object>(new NoOpCallable());
            Assert.IsTrue(task.Cancel(true));
            task.Run();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void CancelAfterRun()
        {
            FutureTask<object> task = new FutureTask<object>(new NoOpCallable());
            task.Run();
            Assert.IsFalse(task.Cancel(false));
            Assert.IsTrue(task.IsDone);
            Assert.IsFalse(task.IsCancelled);
        }


        [Test]
        public void CancelInterrupt()
        {
            FutureTask<bool> task = new FutureTask<bool>(delegate
            {
                try
                {
                    Thread.Sleep(MEDIUM_DELAY);
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
                return true;
            });
            Thread t = new Thread(task.Run);
            t.Start();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(task.Cancel(true));
            t.Join();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void CancelNoInterrupt()
        {
            FutureTask<bool> task = new FutureTask<bool>(delegate
            {
                Thread.Sleep(SMALL_DELAY);
                return true;
            });
            Thread t = new Thread(new ThreadStart(task.Run));
            t.Start();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(task.Cancel());
            t.Join();
            Assert.IsTrue(task.IsDone);
            Assert.IsTrue(task.IsCancelled);
        }


        [Test]
        public void Get1()
        {
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                Thread.Sleep(SMALL_DELAY);
                return true;
            });
            Thread t = new Thread(delegate()
            {
                ft.GetResult();
            });
            Assert.IsFalse(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
            t.Start();
            Thread.Sleep(SHORT_DELAY);
            ft.Run();
            t.Join();
            Assert.IsTrue((bool) ft.GetResult());
            Assert.IsTrue(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
        }


        [Test]
        public void TimedGet1()
        {
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                Thread.Sleep(SMALL_DELAY);
                return true;
            });
            Thread t = new Thread(delegate()
            {
                try
                {
                    ft.GetResult(SHORT_DELAY);
                }
                catch (TimeoutException)
                {
                }
            });
            Assert.IsFalse(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
            t.Start();
            ft.Run();
            t.Join();
            Assert.IsTrue(ft.IsDone);
            Assert.IsFalse(ft.IsCancelled);
        }


        [Test]
        public void TimedGet_Cancellation()
        {
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                try
                {
                    Thread.Sleep(SMALL_DELAY);
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
                return true;
            });
            Thread t1 = new Thread(delegate()
            {
                try
                {
                    ft.GetResult(MEDIUM_DELAY);
                    Assert.Fail("Should throw an exception");
                }
                catch (CancellationException)
                {
                }
            });
            Thread t2 = new Thread(ft.Run);
            t1.Start();
            t2.Start();
            Thread.Sleep(SHORT_DELAY);
            ft.Cancel(true);
            t1.Join();
            t2.Join();
        }


        [Test]
        public void Get_Cancellation()
        {
            // 1. Sleep for SHORT, then return true
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                Thread.Sleep(SMALL_DELAY);
                return true;
            });
            // 2. call GetResult, should throw exception
            Thread t1 = new Thread(delegate()
            {
                try
                {
                    ft.GetResult();
                    Assert.Fail("Should throw an exception.");
                }
                catch (CancellationException)
                {
                }
            });
            Thread t2 = new Thread(new ThreadStart(ft.Run));
            t1.Start();
            t2.Start();
            Thread.Sleep(SHORT_DELAY);
            ft.Cancel(true);
            t1.Join();
            t2.Join();
        }


        [Test]
        public void Get_ExecutionException()
        {
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                int zero = 0;
                int i = 5 / zero;
                return true;
            });
            try
            {
                ft.Run();
                ft.GetResult();
                Assert.Fail("Should throw an exception");
            }
            catch (ExecutionException)
            {
            }
        }


        [Test]
        public void TimedGet_ExecutionException2()
        {
            FutureTask<bool> ft = new FutureTask<bool>(delegate
            {
                int zero = 0;
                int i = 5 / zero;
                return true;
            });
            try
            {
                ft.Run();
                ft.GetResult(SHORT_DELAY);
                Assert.Fail("Should throw an exception.");
            }
            catch (ExecutionException)
            {
            }
            catch (TimeoutException)
            {
            }
        }


        [Test]
        public void Get_InterruptedException()
        {
            FutureTask<object> ft = new FutureTask<object>(new NoOpCallable());
            Thread t = new Thread(delegate()
            {
                try
                {
                    ft.GetResult();
                    Assert.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            });
            t.Start();
            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void TimedGet_InterruptedException2()
        {
            FutureTask<object> ft = new FutureTask<object>(new NoOpCallable());
            Thread t = new Thread(delegate()
            {
                try
                {
                    ft.GetResult(MEDIUM_DELAY);
                    Assert.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            });
            t.Start();
            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void Get_TimeoutException()
        {
            try
            {
                FutureTask<object> ft = new FutureTask<object>(new NoOpCallable());
                ft.GetResult(new TimeSpan(0, 0, 0, 0, 1));
                Assert.Fail("Should throw an exception.");
            }
            catch (TimeoutException)
            {
            }
        }
    }
}