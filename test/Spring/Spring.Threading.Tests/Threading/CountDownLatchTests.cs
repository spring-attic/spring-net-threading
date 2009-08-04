using System;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="CountDownLatch"/>.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu (Interlocked)</author>
    [TestFixture]
    public class CountDownLatchTests : ThreadingTestFixture
    {
        [Test]
        public void ConstructorChokesOnNegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                ()=>new CountDownLatch(- 1));
        }

        [Test]
        public void CountReturnsInitialCountAndDecreasesAftercountDown()
        {
            CountDownLatch l = new CountDownLatch(2);
            Assert.AreEqual(2, l.Count);
            l.CountDown();
            Assert.AreEqual(1, l.Count);
        }


        [Test]
        public void CountDownDecrementsCountWhenPositiveAndHasNoEffectWhenZero()
        {
            CountDownLatch l = new CountDownLatch(1);
            Assert.AreEqual(1, l.Count);
            l.CountDown();
            Assert.AreEqual(0, l.Count);
            l.CountDown();
            Assert.AreEqual(0, l.Count);
        }


        [Test]
        public void AwaitReturnsAfterCountDownToZeroButNotBefore()
        {
            CountDownLatch l = new CountDownLatch(2);

            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate 
                    {
                        Assert.IsTrue(l.Count > 0);
                        l.Await();
                        Assert.IsTrue(l.Count == 0);

                    });
            Assert.AreEqual(l.Count, 2);

            Thread.Sleep(SHORT_DELAY);
            l.CountDown();
            Assert.AreEqual(l.Count, 1);
            l.CountDown();
            Assert.AreEqual(l.Count, 0);
            ThreadManager.JoinAndVerify();
        }

        [Test]
        public void AwaitReturnsImmediatelyIfCountIsZero()
        {
            CountDownLatch l = new CountDownLatch(1);
            l.CountDown();

            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    Assert.IsTrue(l.Count == 0);
                    l.Await();

                });
            ThreadManager.JoinAndVerify();
        }

        [Test]
        public void TimedAwaitReturnsAfterCountDownToZero()
        {
            CountDownLatch l = new CountDownLatch(2);

            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.IsTrue(l.Count > 0);
                        Assert.IsTrue(l.Await(SMALL_DELAY));
                    });
            Assert.AreEqual(l.Count, 2);

            Thread.Sleep(SHORT_DELAY);
            l.CountDown();
            Assert.AreEqual(l.Count, 1);
            l.CountDown();
            Assert.AreEqual(l.Count, 0);
            ThreadManager.JoinAndVerify();
        }

        [Test]
        public void TimedAwaitReturnsImmediatelyIfCountIsZero()
        {
            CountDownLatch l = new CountDownLatch(1);
            l.CountDown();

            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    Assert.IsTrue(l.Count == 0);
                    Assert.IsTrue(l.Await(MEDIUM_DELAY));
                    Assert.IsTrue(l.Await(TimeSpan.Zero));
                });
            ThreadManager.JoinAndVerify(SHORT_DELAY);
        }

        [Test]
        public void TimedAwaitReturnsFalseOnNonPositiveWaitTime()
        {
            CountDownLatch l = new CountDownLatch(1);

            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    Assert.IsTrue(l.Count == 1);
                    Assert.IsFalse(l.Await(TimeSpan.Zero));
                });
            ThreadManager.JoinAndVerify(SHORT_DELAY);
        }

        [Test]
        public void AwaitChokesIfInterruptedBeforeCountedDown()
        {
            CountDownLatch l = new CountDownLatch(1);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.IsTrue(l.Count > 0);
                        Assert.Throws<ThreadInterruptedException>(l.Await);
                    });
            Assert.AreEqual(l.Count, 1);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }


        [Test]
        public void TimedAwaitChokesIfInterruptedBeforeCountedDown()
        {
            CountDownLatch l = new CountDownLatch(1);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.IsTrue(l.Count > 0);
                        Assert.Throws<ThreadInterruptedException>(()=>l.Await(MEDIUM_DELAY));
                    });
            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(l.Count, 1);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }


        [Test]
        public void TimedAwaitTimesOutIfNotCountedDownBeforeTimeout()
        {
            CountDownLatch l = new CountDownLatch(1);
            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        Assert.IsTrue(l.Count > 0);
                        Assert.IsFalse(l.Await(SHORT_DELAY));
                        Assert.IsTrue(l.Count > 0);

                    });
            Assert.AreEqual(l.Count, 1);
            ThreadManager.JoinAndVerify();
            Assert.AreEqual(l.Count, 1);
        }


        [Test]
        public void ToStringIndicatesCurrentCount()
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