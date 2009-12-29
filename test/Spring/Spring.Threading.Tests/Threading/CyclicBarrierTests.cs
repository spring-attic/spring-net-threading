using System;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="CyclicBarrier"/>
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class CyclicBarrierTests : ThreadingTestFixture
    {
        [Test, Description("Creating with non positive parties throws ArgumentOutOfRangeException")]
        public void ConstructorChokesOnNonPositiveParties([Values(0, -1)] int parties)
        {
            const string expected = "parties";
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                ()=>new CyclicBarrier(parties));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new CyclicBarrier(parties, (Action)null));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new CyclicBarrier(parties, (IRunnable)null));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Parties returns the number of parties given in constructor")]
        public void PartiesReturnsTheValueGiveinInConstructor([Values(1,5)] int parties)
        {
            CyclicBarrier b = new CyclicBarrier(parties);
            Assert.AreEqual(parties, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
        }


        [Test, Description("A 1-party barrier triggers after single await")]
        public void SinglePartyBarrierTriggersAfterSingleAwait([Values(true, false)] bool isTimed)
        {
            CyclicBarrier b = new CyclicBarrier(1);
            Assert.AreEqual(1, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            if (isTimed)
            {
                b.Await(Delays.Small);
                b.Await(Delays.Small);
            }
            else
            {
                b.Await();
                b.Await();
            }
            Assert.AreEqual(0, b.NumberOfWaitingParties);

        }


        [Test, Description("The supplied barrier runnable is run at barrier")]
        public void SuppliedRunnableIsRunAtBarries()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            CyclicBarrier b = new CyclicBarrier(1, runnable);
            Assert.AreEqual(1, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            b.Await();
            b.Await();
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            runnable.AssertWasCalled(r => r.Run(), m => m.Repeat.Twice());
        }

        [Test, Description("The supplied barrier action is run at barrier")]
        public void SuppliedActionIsRunAtBarries()
        {
            var action = MockRepository.GenerateStub<Action>();
            CyclicBarrier b = new CyclicBarrier(1, action);
            Assert.AreEqual(1, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            b.Await();
            b.Await(Delays.Small);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            action.AssertWasCalled(a => a(), m => m.Repeat.Twice());
        }

        [Test, Description("A 2-party/thread barrier triggers after both threads invoke await")]
        public void TwoPartieBarrierTriggersAfterBothInvokeAwait()
        {
            CyclicBarrier b = new CyclicBarrier(2);
            ThreadStart action = () =>
                                     {
                                         b.Await();
                                         b.Await(Delays.Small);
                                         b.Await();
                                         b.Await(Delays.Small);
                                     };
            ThreadManager.StartAndAssertRegistered("T", action, action);
            ThreadManager.JoinAndVerify();
        }



        [Test, Description("An interruption in one party causes others waiting to throw BrokenBarrierException")]
        public void InterruptedAwaitBreaksBarrier([Values(true, false)] bool isTimed)
        {
            CyclicBarrier c = new CyclicBarrier(3);

            Thread t1 = ThreadManager.StartAndAssertRegistered(
                "T1", () => Assert.Throws<ThreadInterruptedException>(() => AwaitOrTimedAwait(isTimed, c)));
            ThreadManager.StartAndAssertRegistered(
                "T2", () => Assert.Throws<BrokenBarrierException>(() => AwaitOrTimedAwait(isTimed, c)));
            Thread.Sleep(Delays.Short);
            t1.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("A timeout in timed await throws TimeoutException")]
        public void AwaitChokesWhenTimeOut()
        {
            CyclicBarrier c = new CyclicBarrier(2);
            ThreadManager.StartAndAssertRegistered(
                "T", () => Assert.Throws<TimeoutException>(() => c.Await(Delays.Short)));

            ThreadManager.JoinAndVerify();
        }


        [Test, Description("A timeout in one party causes others waiting to throw BrokenBarrierException")]
        public void TimeoutAwaitBreaksBarrier([Values(true, false)] bool isTimed)
        {
            CyclicBarrier c = new CyclicBarrier(3);
            ThreadManager.StartAndAssertRegistered(
                "T1", () => Assert.Throws<TimeoutException>(()=>c.Await(Delays.Short)));
            ThreadManager.StartAndAssertRegistered(
                "T2", () => Assert.Throws<BrokenBarrierException>(() => AwaitOrTimedAwait(isTimed, c)));
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("Await on already broken barrier throw BrokenBarrierException")]
        public void AwaitChokesOnBrokenBarrier([Values(true, false)] bool isTimed)
        {
            CyclicBarrier c = new CyclicBarrier(2);
            ThreadManager.StartAndAssertRegistered(
                "T1", 
                delegate
                    {
                        Assert.Throws<TimeoutException>(() => c.Await(Delays.Short));
                        Assert.Throws<BrokenBarrierException>(() => AwaitOrTimedAwait(isTimed, c));
                    });
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("A reset of an active barrier causes waiting threads to throw BrokenBarrierException")]
        public void ResetBreaksBarrierWhenTheadWaiting([Values(true, false)] bool isTimed)
        {
            CyclicBarrier c = new CyclicBarrier(3);
            ThreadStart action = () => Assert.Throws<BrokenBarrierException>(() => AwaitOrTimedAwait(isTimed, c));
            ThreadManager.StartAndAssertRegistered("T", action, action);
            Thread.Sleep(Delays.Short);
            c.Reset();
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("A reset before threads enter barrier does not throw BrokenBarrierException")]
        public void ResetDoesNotBreakBarrierWhenNoThreadWaiting([Values(true, false)] bool isTimed)
        {
            CyclicBarrier c = new CyclicBarrier(3);

            ThreadStart action = () => AwaitOrTimedAwait(isTimed, c);

            c.Reset();

            ThreadManager.StartAndAssertRegistered("T", action, action, action);
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("All threads block while a barrier is broken")]
        public void ResetLeakage() //TODO: didn't understand test name and description; copied from Java -K.X.
        {
            CyclicBarrier c = new CyclicBarrier(2);
            AtomicBoolean done = new AtomicBoolean();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    while (!done.Value)
                    {
                        while (c.IsBroken) c.Reset();
                        var e = Assert.Catch(() => c.Await());
                        Assert.That(e,
                                    Is.InstanceOf<BrokenBarrierException>()
                                    .Or.InstanceOf<ThreadInterruptedException>());
                    }
                });


            for (int i = 0; i < 4; i++)
            {
                Thread.Sleep(Delays.Short);
                t.Interrupt();
            }
            done.Value = true;
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }



        [Test, Description("Reset of a non-broken barrier does not break barrier")]
        public void ResetSucceedsOnNonBrokenBarrier()
        {
            CyclicBarrier start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            ThreadStart action = () => { start.Await(); barrier.Await(); };
            for (int i = 0; i < 3; i++)
            {
                ThreadManager.StartAndAssertRegistered("T" + i + "-", action, action);

                start.Await();
                barrier.Await();
                ThreadManager.JoinAndVerify();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                if (i == 1) barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }
        }


        [Test, Description("Reset of a barrier after interruption reinitializes it")]
        public void ResetReinitializeBarrierAfterInterrupt()
        {
            CyclicBarrier start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            for (int i = 0; i < 2; i++)
            {
                var threads = ThreadManager.StartAndAssertRegistered(
                    "T" + i + "-",
                    delegate
                    {
                        start.Await();
                        Assert.Catch<ThreadInterruptedException>(() => barrier.Await());
                    },
                    delegate
                    {
                        start.Await();
                        Assert.Catch<BrokenBarrierException>(() => barrier.Await());
                    });

                start.Await();
                threads[0].Interrupt();
                ThreadManager.JoinAndVerify();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }
        }


        [Test, Description("Reset of a barrier after timeout reinitializes it")]
        public void ResetReinitializeBarrierAfterTimeout()
        {
            CyclicBarrier start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            for (int i = 0; i < 2; i++)
            {
                ThreadManager.StartAndAssertRegistered(
                    "T" + i + "-",
                    delegate
                    {
                        start.Await();
                        Assert.Catch<TimeoutException>(() => barrier.Await(Delays.Short));
                    },
                    delegate
                    {
                        start.Await();
                        Assert.Catch<BrokenBarrierException>(() => barrier.Await());
                    });

                start.Await();
                ThreadManager.JoinAndVerify();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }
        }

        [Test, Description("Reset of a barrier after a failed command reinitializes it")]
        public void ResetReinitializeBarrierAfterExceptionInBarrierAction()
        {
            var e = new NullReferenceException();
            CyclicBarrier start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3, () => { throw e; });
            ThreadStart action = delegate
            {
                start.Await();
                Assert.Catch<BrokenBarrierException>(() => barrier.Await());
            };
            for (int i = 0; i < 2; i++)
            {
                ThreadManager.StartAndAssertRegistered("T" + i + "-", action, action);

                start.Await();
                while (barrier.NumberOfWaitingParties < 2) { Thread.Sleep(1); }
                Assert.That(Assert.Catch(()=>barrier.Await()), Is.SameAs(e));
                ThreadManager.JoinAndVerify();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }
        }

        private static void AwaitOrTimedAwait(bool isTimed, CyclicBarrier c)
        {
            if (isTimed) c.Await(Delays.Long);
            else c.Await();
        }
    }
}
