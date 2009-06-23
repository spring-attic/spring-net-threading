using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading
{
    [TestFixture]
    public class CyclicBarrierTests : BaseThreadingTestCase
    {
        private class MyAction : IRunnable
        {
            private volatile int countAction;
            public void Run() { ++countAction; }
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor1()
        {
            new CyclicBarrier(-1, null);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor2()
        {
            new CyclicBarrier(-1);
        }

        [Test]
        public void GetParties()
        {
            CyclicBarrier b = new CyclicBarrier(2);
            Assert.AreEqual(2, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
        }


        [Test]
        public void SingleParty()
        {
            CyclicBarrier b = new CyclicBarrier(1);
            Assert.AreEqual(1, b.Parties);
            Assert.AreEqual(0, b.NumberOfWaitingParties);
            b.Await();
            b.Await();
            Assert.AreEqual(0, b.NumberOfWaitingParties);

        }


        //        [Test]
        //        public void BarrierAction()
        //        {
        //            countAction = 0;
        //            CyclicBarrier b = new CyclicBarrier(1, new MyAction());
        //            Assert.AreEqual(1, b.Parties);
        //            Assert.AreEqual(0, b.NumberOfWaitingParties);
        //            b.Await();
        //            b.Await();
        //            Assert.AreEqual(0, b.NumberOfWaitingParties);
        //            Assert.AreEqual(countAction, 2);
        //
        //        }

        [Test]
        public void TwoParties()
        {
            CyclicBarrier b = new CyclicBarrier(2);
            Thread t = new Thread(delegate()
                                      {
                                          b.Await();
                                          b.Await();
                                          b.Await();
                                          b.Await();
                                      });

            t.Start();
            b.Await();
            b.Await();
            b.Await();
            b.Await();
            t.Join();
        }



        [Test]
        [Ignore("Failing")]
        public void Await1_Interrupted_BrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await();
                                               Debug.Fail("Should throw exception");
                                           }
                                           catch (ThreadInterruptedException)
                                           {
                                           }
                                       });
            Thread t2 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await();
                                               Debug.Fail("Should throw exception");
                                           }
                                           catch (BrokenBarrierException)
                                           {
                                           }
                                       });
            t1.Start();
            t2.Start();
            Thread.Sleep(SHORT_DELAY);
            t1.Interrupt();
            t1.Join();
            t2.Join();

        }

        [Test]
        [Ignore("Failing")]
        public void Await2_Interrupted_BrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await(LONG_DELAY);
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (ThreadInterruptedException success)
                                           {
                                           }
                                       });
            Thread t2 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await(LONG_DELAY);
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (BrokenBarrierException success)
                                           {
                                           }
                                       });
            t1.Start();
            t2.Start();
            Thread.Sleep(SHORT_DELAY);
            t1.Interrupt();
            t1.Join();
            t2.Join();

        }

        [Test]
        [Ignore("Failing")]
        public void Await3_TimeOutException()
        {
            CyclicBarrier c = new CyclicBarrier(2);
            Thread t = new Thread(delegate()
                                      {
                                          try
                                          {
                                              c.Await(SHORT_DELAY);
                                              Debug.Fail("should throw exception");
                                          }
                                          catch (TimeoutException success)
                                          {
                                          }
                                      });
            t.Start();
            t.Join();
        }


        [Test]
        [Ignore("Failing")]
        public void Await4_Timeout_BrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await(SHORT_DELAY);
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (TimeoutException success)
                                           {
                                           }
                                       });
            Thread t2 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await(MEDIUM_DELAY);
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (BrokenBarrierException success)
                                           {
                                           }
                                       });
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

        }


        [Test]
        [Ignore("Failing")]
        public void Await5_Timeout_BrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
            {
                try
                {
                    c.Await(SHORT_DELAY);
                    Debug.Fail("should throw exception");
                }
                catch (TimeoutException success)
                {
                }
            });
            Thread t2 = new Thread(delegate()
            {
                try
                {
                    c.Await();
                    Debug.Fail("should throw exception");
                }
                catch (BrokenBarrierException success)
                {
                }
            });
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }

        [Test]
        public void Reset_BrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await();
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (BrokenBarrierException success)
                                           {
                                           }
                                       });
            Thread t2 = new Thread(delegate()
                                       {
                                           try
                                           {
                                               c.Await();
                                               Debug.Fail("should throw exception");
                                           }
                                           catch (BrokenBarrierException success)
                                           {
                                           }
                                       });
            t1.Start();
            t2.Start();
            Thread.Sleep(SHORT_DELAY);
            c.Reset();
            t1.Join();
            t2.Join();
        }

        [Test]
        public void Reset_NoBrokenBarrier()
        {
            CyclicBarrier c = new CyclicBarrier(3);
            Thread t1 = new Thread(delegate()
            {
                c.Await();
            });
            Thread t2 = new Thread(delegate()
            {
                c.Await();
            });
            c.Reset();
            t1.Start();
            t2.Start();
            c.Await();
            t1.Join();
            t2.Join();
        }
        [Test]
        [Ignore("Failing")]
        public void Reset_Leakage()
        {
            CyclicBarrier c = new CyclicBarrier(2);
            AtomicBoolean done = new AtomicBoolean();
            Thread t = new Thread(delegate()
                                      {
                                          while (!done.Value)
                                          {
                                              try
                                              {
                                                  while (c.IsBroken)
                                                      c.Reset();
                                                  c.Await();
                                                  Debug.Fail("Await should not return");
                                              }
                                              catch (BrokenBarrierException e)
                                              {
                                              }
                                              catch (ThreadInterruptedException ie)
                                              {
                                              }
                                          }
                                      });

            t.Start();
            for (int i = 0; i < 4; i++)
            {
                Thread.Sleep(SHORT_DELAY);
                t.Interrupt();
            }
            done.Value = true;
            t.Interrupt();
        }



        [Test]
        public void ResetWithoutBreakage()
        {
            CyclicBarrier Start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            for (int i = 0; i < 3; i++)
            {
                Thread t1 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                Thread t2 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });


                t1.Start();
                t2.Start();
                try { Start.Await(); }
                catch (Exception ie) { Debug.Fail("Start barrier"); }
                barrier.Await();
                t1.Join();
                t2.Join();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                if (i == 1) barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }

        }


        [Test]
        [Ignore("Failing")]
        public void ResetAfterInterrupt()
        {
            CyclicBarrier Start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            for (int i = 0; i < 2; i++)
            {
                Thread t1 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (ThreadInterruptedException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                Thread t2 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (BrokenBarrierException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                t1.Start();
                t2.Start();
                try { Start.Await(); }
                catch (Exception ie) { Debug.Fail("Start barrier"); }
                t1.Interrupt();
                t1.Join();
                t2.Join();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }

        }


        [Test]
        [Ignore("Failing")]
        public void ResetAfterTimeout()
        {
            CyclicBarrier Start = new CyclicBarrier(3);
            CyclicBarrier barrier = new CyclicBarrier(3);
            for (int i = 0; i < 2; i++)
            {
                Thread t1 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(MEDIUM_DELAY); }
                    catch (TimeoutException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                Thread t2 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (BrokenBarrierException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                t1.Start();
                t2.Start();
                try { Start.Await(); }
                catch (Exception ie) { Debug.Fail("Start barrier"); }
                t1.Join();
                t2.Join();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }

        }


        private class NullRefRunnable : IRunnable
        {
            public void Run()
            {
                throw new NullReferenceException();
            }
        }
        [Test]
        [Ignore("Timing Out")]
        public void ResetAfterCommandException()
        {
            CyclicBarrier Start = new CyclicBarrier(3);
            CyclicBarrier barrier =
                new CyclicBarrier(3, new NullRefRunnable());
            for (int i = 0; i < 2; i++)
            {
                Thread t1 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (BrokenBarrierException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                Thread t2 = new Thread(delegate()
                {
                    try { Start.Await(); }
                    catch (Exception ie)
                    {
                        Debug.Fail("Start barrier");
                    }
                    try { barrier.Await(); }
                    catch (BrokenBarrierException ok) { }
                    catch (Exception thrown)
                    {
                        Debug.Fail("unexpected exception");
                    }
                });

                t1.Start();
                t2.Start();
                try { Start.Await(); }
                catch (Exception ie) { Debug.Fail("Start barrier"); }
                while (barrier.NumberOfWaitingParties < 2) { Thread.CurrentThread.Join(); }
                try { barrier.Await(); }
                catch (Exception ok) { }
                t1.Join();
                t2.Join();
                Assert.IsTrue(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
                barrier.Reset();
                Assert.IsFalse(barrier.IsBroken);
                Assert.AreEqual(0, barrier.NumberOfWaitingParties);
            }
        }
    }
}
