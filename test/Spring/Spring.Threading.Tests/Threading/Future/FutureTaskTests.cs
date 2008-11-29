//using System;
//using System.Threading;
//using NUnit.Framework;
//using Spring.Threading.Execution;

//namespace Spring.Threading.Future
//{
//    [TestFixture]
//    public class FutureTaskTest : BaseThreadingTestCase
//    {
//        private class NoOpCallable : ICallable
//        {
//            #region ICallable Members

//            public object Call()
//            {
//                return true;
//            }

//            #endregion
//        }

//        private class AnonymousClassCallable : ICallable
//        {
//            public AnonymousClassCallable()
//            {
//            }

//            public Object Call()
//            {
//                try
//                {
//                    Thread.Sleep(MEDIUM_DELAY_MS);
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//                return true;
//            }
//        }

//        private class AnonymousClassCallable1 : ICallable
//        {
//            public AnonymousClassCallable1()
//            {
//            }

//            public Object Call()
//            {
//                Thread.Sleep(SMALL_DELAY_MS);
//                return true;
//            }
//        }

//        private class AnonymousClassRunnable : IRunnable
//        {
//            public AnonymousClassRunnable(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                ft.GetResult();
//            }
//        }

//        private class AnonymousClassRunnable1 : IRunnable
//        {
//            public AnonymousClassRunnable1(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                try
//                {
//                    ft.GetResult(SHORT_DELAY_MS);
//                }
//                catch (TimeoutException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassCallable4 : ICallable
//        {
//            public AnonymousClassCallable4()
//            {
//            }

//            public Object Call()
//            {
//                try
//                {
//                    Thread.Sleep(SMALL_DELAY_MS);
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//                return true;
//            }
//        }

//        private class AnonymousClassRunnable2 : IRunnable
//        {
//            public AnonymousClassRunnable2(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                try
//                {
//                    ft.GetResult(MEDIUM_DELAY_MS);
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (CancellationException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable3 : IRunnable
//        {
//            public AnonymousClassRunnable3(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                try
//                {
//                    ft.GetResult();
//                    Assert.Fail("Should throw an exception.");
//                }
//                catch (CancellationException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassCallable6 : ICallable
//        {
//            public AnonymousClassCallable6()
//            {
//            }

//            public Object Call()
//            {
//                int zero = 0;
//                int i = 5/zero;
//                return true;
//            }
//        }

//        private class AnonymousClassCallable7 : ICallable
//        {
//            public AnonymousClassCallable7()
//            {
//            }

//            public Object Call()
//            {
//                int zero = 0;
//                int i = 5/zero;
//                return true;
//            }
//        }

//        private class AnonymousClassRunnable4 : IRunnable
//        {
//            public AnonymousClassRunnable4(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                try
//                {
//                    ft.GetResult();
//                    Assert.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable5 : IRunnable
//        {
//            public AnonymousClassRunnable5(FutureTask ft)
//            {
//                this.ft = ft;
//            }

//            private FutureTask ft;

//            public void Run()
//            {
//                try
//                {
//                    ft.GetResult(MEDIUM_DELAY_MS);
//                    Assert.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }


//        internal class PublicFutureTask : FutureTask
//        {
//            public PublicFutureTask(ICallable r) : base(r)
//            {
//            }

//            public bool CallRunAndReset()
//            {
//                return base.runAndReset();
//            }

//            [Test]
//            public void SetupResult(object result)
//            {
//                base.setResult(result);
//            }

//            [Test]
//            public void SetupException(Exception t)
//            {
//                base.setException(t);
//            }
//        }


//        [Test]
//        [ExpectedException(typeof (ArgumentNullException))]
//        public void Constructor()
//        {
//            new FutureTask(null);
//            Assert.Fail("Should throw an exception.");
//        }


//        [Test]
//        [ExpectedException(typeof (ArgumentNullException))]
//        public void Constructor2()
//        {
//            new FutureTask(null, true);
//            Assert.Fail("Should throw an exception.");
//        }


//        [Test]
//        public void IsDone()
//        {
//            FutureTask task = new FutureTask(new NoOpCallable());
//            task.Run();
//            Assert.IsTrue(task.IsDone);
//            Assert.IsFalse(task.IsCancelled);
//        }


//        [Test]
//        public void RunAndReset()
//        {
//            PublicFutureTask task = new PublicFutureTask(new NoOpCallable());
//            Assert.IsTrue(task.CallRunAndReset());
//            Assert.IsFalse(task.IsDone);
//        }


//        [Test]
//        public void ResetAfterCancel()
//        {
//            PublicFutureTask task = new PublicFutureTask(new NoOpCallable());
//            Assert.IsTrue(task.Cancel(false));
//            Assert.IsFalse(task.CallRunAndReset());
//            Assert.IsTrue(task.IsDone);
//            Assert.IsTrue(task.IsCancelled);
//        }


//        [Test]
//        public void Set()
//        {
//            PublicFutureTask task = new PublicFutureTask(new NoOpCallable());
//            task.SetupResult(one);
//            Assert.AreEqual(task.GetResult(), one);
//        }


//        [Test]
//        public void SetException()
//        {
//            Exception nse = new ArgumentOutOfRangeException();
//            PublicFutureTask task = new PublicFutureTask(new NoOpCallable());
//            task.SetupException(nse);
//            try
//            {
//                task.GetResult();
//                Assert.Fail("Should throw an exception.");
//            }
//            catch (ExecutionException ee)
//            {
//                Exception cause = ee.InnerException;
//                Assert.AreEqual(cause, nse);
//            }
//        }


//        [Test]
//        public void CancelBeforeRun()
//        {
//            FutureTask task = new FutureTask(new NoOpCallable());
//            Assert.IsTrue(task.Cancel(false));
//            task.Run();
//            Assert.IsTrue(task.IsDone);
//            Assert.IsTrue(task.IsCancelled);
//        }


//        [Test]
//        public void CancelBeforeRun2()
//        {
//            FutureTask task = new FutureTask(new NoOpCallable());
//            Assert.IsTrue(task.Cancel(true));
//            task.Run();
//            Assert.IsTrue(task.IsDone);
//            Assert.IsTrue(task.IsCancelled);
//        }


//        [Test]
//        public void CancelAfterRun()
//        {
//            FutureTask task = new FutureTask(new NoOpCallable());
//            task.Run();
//            Assert.IsFalse(task.Cancel(false));
//            Assert.IsTrue(task.IsDone);
//            Assert.IsFalse(task.IsCancelled);
//        }


//        [Test]
//        public void CancelInterrupt()
//        {
//            FutureTask task = new FutureTask(new AnonymousClassCallable());
//            Thread t = new Thread(new ThreadStart(task.Run));
//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(task.Cancel(true));
//            t.Join();
//            Assert.IsTrue(task.IsDone);
//            Assert.IsTrue(task.IsCancelled);
//        }


//        [Test]
//        public void CancelNoInterrupt()
//        {
//            FutureTask task = new FutureTask(new AnonymousClassCallable1());
//            Thread t = new Thread(new ThreadStart(task.Run));
//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(task.Cancel());
//            t.Join();
//            Assert.IsTrue(task.IsDone);
//            Assert.IsTrue(task.IsCancelled);
//        }


//        [Test]
//        public void Get1()
//        {
//            FutureTask ft = new FutureTask(new AnonymousClassCallable1());
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ft).Run));
//            Assert.IsFalse(ft.IsDone);
//            Assert.IsFalse(ft.IsCancelled);
//            t.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            ft.Run();
//            t.Join();
//            Assert.IsTrue((bool) ft.GetResult());
//            Assert.IsTrue(ft.IsDone);
//            Assert.IsFalse(ft.IsCancelled);
//        }


//        [Test]
//        public void TimedGet1()
//        {
//            FutureTask ft = new FutureTask(new AnonymousClassCallable1());
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(ft).Run));
//            Assert.IsFalse(ft.IsDone);
//            Assert.IsFalse(ft.IsCancelled);
//            t.Start();
//            ft.Run();
//            t.Join();
//            Assert.IsTrue(ft.IsDone);
//            Assert.IsFalse(ft.IsCancelled);
//        }


//        [Test]
//        public void TimedGet_Cancellation()
//        {
//            FutureTask ft = new FutureTask(new AnonymousClassCallable4());
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable2(ft).Run));
//            Thread t2 = new Thread(new ThreadStart(ft.Run));
//            t1.Start();
//            t2.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            ft.Cancel(true);
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        public void Get_Cancellation()
//        {
//            // 1. Sleep for SHORT, then return true
//            FutureTask ft = new FutureTask(new AnonymousClassCallable1());
//            // 2. call GetResult, should throw exception
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable3(ft).Run));
//            Thread t2 = new Thread(new ThreadStart(ft.Run));
//            t1.Start();
//            t2.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            ft.Cancel(true);
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        public void Get_ExecutionException()
//        {
//            FutureTask ft = new FutureTask(new AnonymousClassCallable6());
//            try
//            {
//                ft.Run();
//                ft.GetResult();
//                Assert.Fail("Should throw an exception");
//            }
//            catch (ExecutionException)
//            {
//            }
//        }


//        [Test]
//        public void TimedGet_ExecutionException2()
//        {
//            FutureTask ft = new FutureTask(new AnonymousClassCallable7());
//            try
//            {
//                ft.Run();
//                ft.GetResult(SHORT_DELAY_MS);
//                Assert.Fail("Should throw an exception.");
//            }
//            catch (ExecutionException)
//            {
//            }
//            catch (TimeoutException)
//            {
//            }
//        }


//        [Test]
//        public void Get_InterruptedException()
//        {
//            FutureTask ft = new FutureTask(new NoOpCallable());
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(ft).Run));
//            t.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            t.Interrupt();
//            t.Join();
//        }


//        [Test]
//        public void TimedGet_InterruptedException2()
//        {
//            FutureTask ft = new FutureTask(new NoOpCallable());
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(ft).Run));
//            t.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            t.Interrupt();
//            t.Join();
//        }


//        [Test]
//        public void Get_TimeoutException()
//        {
//            try
//            {
//                FutureTask ft = new FutureTask(new NoOpCallable());
//                ft.GetResult(new TimeSpan(0, 0, 0, 0, 1));
//                Assert.Fail("Should throw an exception.");
//            }
//            catch (TimeoutException)
//            {
//            }
//        }
//    }
//}