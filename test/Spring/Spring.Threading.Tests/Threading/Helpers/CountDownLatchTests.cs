using System;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Helpers
{
    [TestFixture]
    public class CountDownLatchTests : BaseThreadingTestCase
    {
        private class AnonymousClassRunnable : IRunnable
        {
            public AnonymousClassRunnable(CountDownLatch l)
            {
                this.l = l;
            }

            private CountDownLatch l;

            public virtual void Run()
            {
                Assert.IsTrue(l.Count > 0);
                l.Await();
                Assert.IsTrue(l.Count == 0);
            }
        }

        private class AnonymousClassRunnable1 : IRunnable
        {
            public AnonymousClassRunnable1(CountDownLatch l)
            {
                this.l = l;
            }

            private CountDownLatch l;

            public virtual void Run()
            {
                Assert.IsTrue(l.Count > 0);
                Assert.IsTrue(l.Await(SMALL_DELAY_MS));
            }
        }

        private class AnonymousClassRunnable2 : IRunnable
        {
            public AnonymousClassRunnable2(CountDownLatch l)
            {
                this.l = l;
            }

            private CountDownLatch l;

            public virtual void Run()
            {
                try
                {
                    Assert.IsTrue(l.Count > 0);
                    l.Await();
                    Assert.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable3 : IRunnable
        {
            public AnonymousClassRunnable3(CountDownLatch l)
            {
                this.l = l;
            }

            private CountDownLatch l;

            public virtual void Run()
            {
                try
                {
                    Assert.IsTrue(l.Count > 0);
                    l.Await(MEDIUM_DELAY_MS);
                    Assert.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable4 : IRunnable
        {
            public AnonymousClassRunnable4(CountDownLatch l)
            {
                this.l = l;
            }

            private CountDownLatch l;

            public virtual void Run()
            {
                Assert.IsTrue(l.Count > 0);
                Assert.IsFalse(l.Await(SHORT_DELAY_MS));
                Assert.IsTrue(l.Count > 0);
            }
        }


        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void Constructor()
        {
            new CountDownLatch(- 1);
        }


        [Test]
        public void GetCount()
        {
            CountDownLatch l = new CountDownLatch(2);
            Assert.AreEqual(2, l.Count);
            l.CountDown();
            Assert.AreEqual(1, l.Count);
        }


        [Test]
        public void CountDown()
        {
            CountDownLatch l = new CountDownLatch(1);
            Assert.AreEqual(1, l.Count);
            l.CountDown();
            Assert.AreEqual(0, l.Count);
            l.CountDown();
            Assert.AreEqual(0, l.Count);
        }


        [Test]
        public void Await()
        {
            CountDownLatch l = new CountDownLatch(2);

            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(l).Run));
            t.Start();
            Assert.AreEqual(l.Count, 2);

            Thread.Sleep(SHORT_DELAY_MS);
            l.CountDown();
            Assert.AreEqual(l.Count, 1);
            l.CountDown();
            Assert.AreEqual(l.Count, 0);
            t.Join();
        }


        [Test]
        public void TimedAwait()
        {
            CountDownLatch l = new CountDownLatch(2);

            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(l).Run));
            t.Start();
            Assert.AreEqual(l.Count, 2);

            Thread.Sleep(SHORT_DELAY_MS);
            l.CountDown();
            Assert.AreEqual(l.Count, 1);
            l.CountDown();
            Assert.AreEqual(l.Count, 0);
            t.Join();
        }


        [Test]
        public void Await_InterruptedException()
        {
            CountDownLatch l = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(l).Run));
            t.Start();
            Assert.AreEqual(l.Count, 1);
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void TimedAwait_InterruptedException()
        {
            CountDownLatch l = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(l).Run));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(l.Count, 1);
            t.Interrupt();
            t.Join();
        }


        [Test]
        public void AwaitTimeout()
        {
            CountDownLatch l = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(l).Run));
            t.Start();
            Assert.AreEqual(l.Count, 1);
            t.Join();
            Assert.AreEqual(l.Count, 1);
        }


        [Test]
        public void CountDownLatchToString()
        {
            CountDownLatch s = new CountDownLatch(2);
            String us = s.ToString();
            Assert.IsTrue(us.IndexOf("Count = 2") >= 0);
            s.CountDown();
            String s1 = s.ToString();
            Assert.IsTrue(s1.IndexOf("Count = 1") >= 0);
            s.CountDown();
            String s2 = s.ToString();
            Assert.IsTrue(s2.IndexOf("Count = 0") >= 0);
        }
    }
}