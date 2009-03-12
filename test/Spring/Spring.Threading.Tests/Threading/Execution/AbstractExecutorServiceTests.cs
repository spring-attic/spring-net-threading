using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    [TestFixture]
    public class AbstractExecutorServiceTests : BaseThreadingTestCase
    {
        private sealed class AnonymousClassRunnable : IRunnable
        {
            private readonly ThreadPoolExecutor p;

            public AnonymousClassRunnable(ThreadPoolExecutor p)
            {
                this.p = p;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    p.Submit(new AnonymousClassCallable()).GetResult();
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion

            #region Nested type: AnonymousClassCallable

            private sealed class AnonymousClassCallable : ICallable
            {
                #region ICallable Members

                public Object Call()
                {
                    try
                    {
                        Thread.Sleep(MEDIUM_DELAY_MS);
                        Assert.Fail("Should throw an exception");
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    return null;
                }

                #endregion
            }

            #endregion
        }

        private sealed class AnonymousClassCallable : ICallable
        {
            private readonly ThreadPoolExecutor p;

            public AnonymousClassCallable(ThreadPoolExecutor p)
            {
                this.p = p;
            }

            #region ICallable Members

            public Object Call()
            {
                try
                {
                    p.Submit(new SmallCallable()).GetResult();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (RejectedExecutionException)
                {
                }
                catch (ExecutionException)
                {
                }
                return true;
            }

            #endregion
        }

        private sealed class AnonymousClassRunnable1 : IRunnable
        {
            private readonly ICallable c;

            public AnonymousClassRunnable1(ICallable c)
            {
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    c.Call();
                }
                catch (Exception)
                {
                }
            }

            #endregion
        }

        private sealed class AnonymousClassCallable1 : ICallable
        {
            #region ICallable Members

            public Object Call()
            {
                int zero = 0;
                int i = 5/zero;
                return true;
            }

            #endregion
        }

        internal class DirectExecutorService : AbstractExecutorService
        {
            private volatile bool _shutdown;

            public override bool IsShutdown
            {
                get { return _shutdown; }
            }

            public override bool IsTerminated
            {
                get { return IsShutdown; }
            }

            public override void Execute(IRunnable r)
            {
                r.Run();
            }

            public override void Shutdown()
            {
                _shutdown = true;
            }

            public override IList<IRunnable> ShutdownNow()
            {
                _shutdown = true;
                return (IList<IRunnable>) ArrayList.ReadOnly(new List<IRunnable>());
            }

            public override bool AwaitTermination(TimeSpan duration)
            {
                return IsShutdown;
            }
        }


        [Test]
        public void Execute1()
        {
            var p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 1, 0), new ArrayBlockingQueue<IRunnable>(1));
            try
            {
                for (int i = 0; i < 5; ++i)
                {
                    p.Submit(new MediumRunnable());
                }
                Assert.Fail("Should throw an exception.");
            }
            catch (RejectedExecutionException)
            {
            }
            finally
            {
                JoinPool(p);
            }
        }


        [Test]
        public void Execute2()
        {
            var p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 1, 0), new ArrayBlockingQueue<IRunnable>(1));
            try
            {
                for (int i = 0; i < 5; ++i)
                {
                    p.Submit(new SmallCallable());
                }
                Assert.Fail("Should throw an exception.");
            }
            catch (RejectedExecutionException)
            {
            }
            JoinPool(p);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ExecuteNullRunnable()
        {
            IExecutorService e = new DirectExecutorService();
            TrackedShortRunnable task = null;
            e.Submit(task);
        }

        [Test]
        public void ExecuteRunnable()
        {
            IExecutorService e = new DirectExecutorService();
            var task = new TrackedShortRunnable();
            Assert.IsFalse(task.IsDone);
            IFuture future = e.Submit(task);
            future.GetResult();
            Assert.IsTrue(task.IsDone);
        }


        [Test]
        public void InterruptedSubmit()
        {
			var p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 1, 0),  new ArrayBlockingQueue<IRunnable>(10));
			var t = new Thread(new AnonymousClassRunnable(p).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            JoinPool(p);
        }


        [Test]
        public void InvokeAll1()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAll((ICollection) null);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAll2()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                IList r = e.InvokeAll(new ArrayList());
                Assert.IsTrue((r.Count == 0));
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAll3()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), null};
                e.InvokeAll(l);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAll4()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new NPETask()};
                IList result = e.InvokeAll(l);
                Assert.AreEqual(1, result.Count);
                foreach (IFuture future in result)
                {
                    future.GetResult();
                }
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAll5()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), new StringTask()};
                IList result = e.InvokeAll(l);
                Assert.AreEqual(2, result.Count);
                foreach (IFuture future in result)
                {
                    Assert.AreSame(TEST_STRING, future.GetResult());
                }
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void InvokeAny1()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAny((ICollection)null);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAny2()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAny(new ArrayList());
            }
            catch (ArgumentException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAny3()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), null};
                e.InvokeAny(l);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAny4()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new NPETask()};
                e.InvokeAny(l);
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void InvokeAny5()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), new StringTask()};
                var result = (String) e.InvokeAny(l);
                Assert.AreSame(TEST_STRING, result);
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void SubmitCallable()
        {
            IExecutorService e = new DirectExecutorService();
            IFuture future = e.Submit(new StringTask());
            var result = (String) future.GetResult();
            Assert.AreSame(TEST_STRING, result);
        }

        [Test]
        public void SubmitEE()
        {
			var p = new ThreadPoolExecutor(1, 1, new TimeSpan(0,0,0,60), new ArrayBlockingQueue<IRunnable>(10));
		
			try
			{
				ICallable c = new AnonymousClassCallable1();
			
				for (var i = 0; i < 5; i++)
				{
					p.Submit(c).GetResult();
				}
				Assert.Fail("Should throw an exception.");
			}
			catch (ExecutionException)
			{
			}
			JoinPool(p);
        }

        [Test]
        public void SubmitIE()
        {
            var p = new ThreadPoolExecutor(1, 1, new TimeSpan(0, 60, 0), new ArrayBlockingQueue<IRunnable>(10));

            ICallable c = new AnonymousClassCallable(p);

            var t = new Thread(new AnonymousClassRunnable1(c).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();

            JoinPool(p);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SubmitNullCallable()
        {
            IExecutorService e = new DirectExecutorService();
            StringTask t = null;
            e.Submit(t);
        }

        [Test]
        public void SubmitRunnable()
        {
            IExecutorService e = new DirectExecutorService();
            IFuture future = e.Submit(new NullRunnable());
            future.GetResult();
            Assert.IsTrue(future.IsDone);
        }


        [Test]
        public void SubmitRunnable2()
        {
            IExecutorService e = new DirectExecutorService();
            IFuture future = e.Submit(new NullRunnable(), TEST_STRING);
            var result = (String) future.GetResult();
            Assert.AreSame(TEST_STRING, result);
        }


        [Test]
        public void TimedInvokeAll1()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAll((ICollection) null, MEDIUM_DELAY_MS);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAll2()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                IList r = e.InvokeAll(new ArrayList(), MEDIUM_DELAY_MS);
                Assert.IsTrue((r.Count == 0));
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAll3()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), null};
                e.InvokeAll(l, MEDIUM_DELAY_MS);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAll4()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new NPETask()};
                IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
                Assert.AreEqual(1, result.Count);
                foreach (IFuture future in result)
                {
                    future.GetResult();
                }
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAll5()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), new StringTask()};
                IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
                Assert.AreEqual(2, result.Count);
                foreach (IFuture future in result)
                {
                    Assert.AreSame(TEST_STRING, future.GetResult());
                }
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAll6()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var list = new ArrayList {new StringTask(), Executors.CreateCallable(new MediumPossiblyInterruptedRunnable(), TEST_STRING), new StringTask()};
                var result = e.InvokeAll(list, SMALL_DELAY_MS);
                Assert.AreEqual(3, result.Count);
                var it = result.GetEnumerator();
                IFuture f1 = null; 
                IFuture f2 = null; 
                IFuture f3 = null;

                if (it.MoveNext()) f1 = (IFuture)it.Current;
                if (it.MoveNext()) f2 = (IFuture)it.Current;
                if (it.MoveNext()) f3 = (IFuture)it.Current;

                if ( f1 == null || f2 == null || f3 == null )
                {
                    Assert.Fail("Missing some futures");
                }

                Assert.IsTrue(f1.IsDone);
                Assert.IsFalse(f1.IsCancelled);
                Assert.IsTrue(f2.IsDone);
                Assert.IsTrue(f3.IsDone);
                Assert.IsTrue(f3.IsCancelled);
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void TimedInvokeAllNullTimeUnit()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAll((ICollection) null, MEDIUM_DELAY_MS);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void TimedInvokeAny1()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAny((ICollection) null, MEDIUM_DELAY_MS);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void TimedInvokeAny2()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAny(new ArrayList(), MEDIUM_DELAY_MS);
            }
            catch (ArgumentException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAny3()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), null};
                e.InvokeAny(l, MEDIUM_DELAY_MS);
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAny4()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new NPETask()};
                e.InvokeAny(l, MEDIUM_DELAY_MS);
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }


        [Test]
        public void TimedInvokeAny5()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                var l = new ArrayList {new StringTask(), new StringTask()};
                var result = (String) e.InvokeAny(l, MEDIUM_DELAY_MS);
                Assert.AreSame(TEST_STRING, result);
            }
            catch (ExecutionException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }

        [Test]
        public void TimedInvokeAnyNull()
        {
            IExecutorService e = new DirectExecutorService();
            try
            {
                e.InvokeAny((ICollection) null, new TimeSpan(0));
            }
            catch (ArgumentNullException)
            {
            }
            finally
            {
                JoinPool(e);
            }
        }
    }
}