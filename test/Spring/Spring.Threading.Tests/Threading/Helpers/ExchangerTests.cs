using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Helpers
{
    [TestFixture]
    public class ExchangerTests : BaseThreadingTestCase
    {
        private class AnonymousClassRunnable : IRunnable
        {
            public AnonymousClassRunnable(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Object v = e.Exchange(one);
                Assert.AreEqual(v, two);
                Object w = e.Exchange(v);
                Assert.AreEqual(w, one);
            }
        }

        private class AnonymousClassRunnable1 : IRunnable
        {
            public AnonymousClassRunnable1(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Object v = e.Exchange(two);
                Assert.AreEqual(v, one);
                Object w = e.Exchange(v);
                Assert.AreEqual(w, two);
            }
        }

        private class AnonymousClassRunnable2 : IRunnable
        {
            public AnonymousClassRunnable2(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Object v = e.Exchange(one, SHORT_DELAY);
                Assert.AreEqual(v, two);
                Object w = e.Exchange(v, SHORT_DELAY);
                Assert.AreEqual(w, one);
            }
        }

        private class AnonymousClassRunnable3 : IRunnable
        {
            public AnonymousClassRunnable3(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Object v = e.Exchange(two, SHORT_DELAY);
                Assert.AreEqual(v, one);
                Object w = e.Exchange(v, SHORT_DELAY);
                Assert.AreEqual(w, two);
            }
        }

        private class AnonymousClassRunnable4 : IRunnable
        {
            public AnonymousClassRunnable4(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                try
                {
                    e.Exchange(one);
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable5 : IRunnable
        {
            public AnonymousClassRunnable5(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                try
                {
                    e.Exchange(null, MEDIUM_DELAY);
                    Debug.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable6 : IRunnable
        {
            public AnonymousClassRunnable6(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                try
                {
                    e.Exchange(null, SHORT_DELAY);
                    Debug.Fail("Should throw an exception.");
                }
                catch (TimeoutException)
                {
                }

            }
        }

        private class AnonymousClassRunnable7 : IRunnable
        {
            public AnonymousClassRunnable7(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                try
                {
                    Object v = e.Exchange(one);
                    Assert.AreEqual(v, two);
                    e.Exchange(v);
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable8 : IRunnable
        {
            public AnonymousClassRunnable8(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Object v = e.Exchange(two);
                Assert.AreEqual(v, one);
                Thread.Sleep(SMALL_DELAY);
                Object w = e.Exchange(v);
                Assert.AreEqual(w, three);
            }
        }

        private class AnonymousClassRunnable9 : IRunnable
        {
            public AnonymousClassRunnable9(Exchanger e)
            {
                this.e = e;
            }

            private readonly Exchanger e;

            public void Run()
            {
                Thread.Sleep(SMALL_DELAY);
                Object w = e.Exchange(three);
                Assert.AreEqual(w, one);
            }
        }


        [Test]
        public void Exchange()
        {
            Exchanger e = new Exchanger();
            Thread t1 = new Thread(new AnonymousClassRunnable(e).Run);
            Thread t2 = new Thread(new AnonymousClassRunnable1(e).Run);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }


        [Test]
        public void TimedExchange()
        {
            Exchanger e = new Exchanger();
            Thread t1 = new Thread(new AnonymousClassRunnable2(e).Run);
            t1.Name = "thread1";
            Thread t2 = new Thread(new AnonymousClassRunnable3(e).Run);
            t2.Name = "thread2";
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }


        [Test]
        public void Exchange_InterruptedException()
        {
            Exchanger e = new Exchanger();
            Thread t = new Thread(new AnonymousClassRunnable4(e).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void TimedExchange_InterruptedException()
        {
            Exchanger e = new Exchanger();
            Thread t = new Thread(new AnonymousClassRunnable5(e).Run);
            t.Start();
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void Exchange_TimeOutException()
        {
            Exchanger e = new Exchanger();
            Thread t = new Thread(new AnonymousClassRunnable6(e).Run);
            t.Start();
            t.Join();
        }


        [Test]
        public void ReplacementAfterExchange()
        {
            Exchanger e = new Exchanger();
            Thread t1 = new Thread(new AnonymousClassRunnable7(e).Run);
            Thread t2 = new Thread(new AnonymousClassRunnable8(e).Run);
            Thread t3 = new Thread(new AnonymousClassRunnable9(e).Run);

            t1.Start();
            t2.Start();
            t3.Start();
            Thread.Sleep(SHORT_DELAY);
            t1.Interrupt();
            t1.Join();
            t2.Join();
            t3.Join();
        }
    }
}