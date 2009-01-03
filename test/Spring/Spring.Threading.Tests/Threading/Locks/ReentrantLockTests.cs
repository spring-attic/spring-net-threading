using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Threading.Locks
{
    [TestFixture]
    public class ReentrantLockTests : BaseThreadingTestCase
    {
        #region Helper Classes

        private class AnonymousClassRunnable : IRunnable
        {
            private ReentrantLock lockRenamed;

            public AnonymousClassRunnable(ReentrantLock lockRenamed)
            {
                this.lockRenamed = lockRenamed;
            }

            public void Run()
            {
                try
                {
                    lockRenamed.TryLock(MEDIUM_DELAY_MS);
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable1 : IRunnable
        {
            private ReentrantLock lockRenamed;

            public AnonymousClassRunnable1(ReentrantLock lockRenamed)
            {
                this.lockRenamed = lockRenamed;
            }

            public void Run()
            {
                Debug.Assert(!lockRenamed.TryLock());
            }
        }

        private class AnonymousClassRunnable2 : IRunnable
        {
            public AnonymousClassRunnable2(ReentrantLock lockRenamed)
            {
                this.lockRenamed = lockRenamed;
            }

            private ReentrantLock lockRenamed;

            public void Run()
            {
                try
                {
                    Debug.Assert(!lockRenamed.TryLock(new TimeSpan(0, 0, 1/1000)));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable3 : IRunnable
        {
            private ReentrantLock lockRenamed;

            public AnonymousClassRunnable3(ReentrantLock lockRenamed)
            {
                this.lockRenamed = lockRenamed;
            }

            public void Run()
            {
                lockRenamed.Lock();
                try
                {
                    Thread.Sleep(new TimeSpan(10000*SMALL_DELAY_MS.Milliseconds));
                }
                catch (Exception)
                {
                    throw;
                }
                lockRenamed.Unlock();
            }
        }

        private class AnonymousClassRunnable4 : IRunnable
        {
            private ReentrantLock lockRenamed;
            private ICondition condition;

            public AnonymousClassRunnable4(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable5 : IRunnable
        {
            private ReentrantLock lockRenamed;
            private ICondition condition;

            public AnonymousClassRunnable5(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    Debug.Assert(!lockRenamed.HasWaiters(condition));
                    Debug.Assert(0 == lockRenamed.GetWaitQueueLength(condition));
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable6 : IRunnable
        {
            public AnonymousClassRunnable6(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition condition;

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    Debug.Assert(!lockRenamed.HasWaiters(condition));
                    Debug.Assert(0 == lockRenamed.GetWaitQueueLength(condition));
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable7 : IRunnable
        {
            public AnonymousClassRunnable7(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition condition;

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    Debug.Assert(lockRenamed.HasWaiters(condition));
                    Debug.Assert(1 == lockRenamed.GetWaitQueueLength(condition));
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable8 : IRunnable
        {
            public AnonymousClassRunnable8(PublicReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private PublicReentrantLock lockRenamed;
            private ICondition condition;


            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    Debug.Assert(lockRenamed.GetWaitingThreads(condition).Count == 0);
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable9 : IRunnable
        {
            public AnonymousClassRunnable9(PublicReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private PublicReentrantLock lockRenamed;
            private ICondition condition;


            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    Debug.Assert(lockRenamed.GetWaitingThreads(condition).Count != 0);
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable10 : IRunnable
        {
            public AnonymousClassRunnable10(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition condition;

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    condition.Await();
                    lockRenamed.Unlock();
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable11 : IRunnable
        {
            public AnonymousClassRunnable11(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition condition;

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    condition.Await(new TimeSpan(0,0,0,1));
                    lockRenamed.Unlock();
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable12 : IRunnable
        {
            public AnonymousClassRunnable12(ReentrantLock lockRenamed, ICondition c)
            {
                _lockRenamed = lockRenamed;
                _condition = c;
            }

            private ReentrantLock _lockRenamed;
            private ICondition _condition;

            public void Run()
            {
                try
                {
                    _lockRenamed.Lock();
                    DateTime d = DateTime.Now;
                    DateTime tempAux = new DateTime(d.Millisecond + 10000);
                    _condition.AwaitUntil(tempAux);
                    _lockRenamed.Unlock();
                    Debug.Fail("Should throw exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private class AnonymousClassRunnable13 : IRunnable
        {
            public AnonymousClassRunnable13(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.condition = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition condition;

            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    condition.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        private class AnonymousClassRunnable14 : IRunnable
        {
            public AnonymousClassRunnable14(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.c = c;
            }

            private ReentrantLock lockRenamed;
            private ICondition c;


            public void Run()
            {
                try
                {
                    lockRenamed.Lock();
                    c.Await();
                    lockRenamed.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
            }
        }

        internal class InterruptibleLockRunnable : IRunnable
        {
            internal ReentrantLock lockRenamed;

            internal InterruptibleLockRunnable(ReentrantLock l)
            {
                lockRenamed = l;
            }

            public void Run()
            {
                try
                {
                    lockRenamed.LockInterruptibly();
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        internal class InterruptedLockRunnable : IRunnable
        {
            internal ReentrantLock lockRenamed;

            internal InterruptedLockRunnable(ReentrantLock l)
            {
                lockRenamed = l;
            }

            public void Run()
            {
                try
                {
                    lockRenamed.LockInterruptibly();
                    Debug.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        internal class UninterruptableThread : IRunnable
        {
            private ReentrantLock lockRenamed;
            private ICondition c;
            private Thread _internalThread;

            public volatile bool canAwake = false;
            public volatile bool interrupted = false;
            public volatile bool lockStarted = false;
			
            public UninterruptableThread(ReentrantLock lockRenamed, ICondition c)
            {
                this.lockRenamed = lockRenamed;
                this.c = c;
                _internalThread = new Thread(new ThreadStart(this.Run));
            }

            public void Run()
            {
                lockRenamed.Lock();
                lockStarted = true;

                try
                {
                    while (!canAwake)
                    {
                        c.AwaitUninterruptibly();
                    }
                }
                catch (ThreadInterruptedException)
                {
                    interrupted = true;
                }

                lockRenamed.Unlock();
            }
            public void Start()
            {
                _internalThread.Start();
            }
            public void Interrupt()
            {
                _internalThread.Interrupt();
            }
            public bool IsAlive
            {
                get { return _internalThread.IsAlive;}
            }
            public void Join()
            {
                _internalThread.Join();
            }
        }

        [Serializable]
        internal class PublicReentrantLock : ReentrantLock
        {
            internal PublicReentrantLock() 
            {
            }

            internal PublicReentrantLock(bool fair) : base(fair)
            {
            }
        }

        #endregion

        [Test]
        public void Constructor()
        {
            ReentrantLock rl = new ReentrantLock();
            Assert.IsFalse(rl.IsFair);
            ReentrantLock r2 = new ReentrantLock(true);
            Assert.IsTrue(r2.IsFair);
        }


        [Test]
        public void Lock()
        {
            ReentrantLock rl = new ReentrantLock();
            rl.Lock();
            Assert.IsTrue(rl.IsLocked);
            rl.Unlock();
        }


        [Test]
        public void IsFairLock()
        {
            ReentrantLock rl = new ReentrantLock(true);
            rl.Lock();
            Assert.IsTrue(rl.IsLocked);
            rl.Unlock();
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void Unlock_InvalidOperationException()
        {
            ReentrantLock rl = new ReentrantLock();
            rl.Unlock();
            Assert.Fail("Should throw an exception");
        }


        [Test]
        public void TryLock()
        {
            ReentrantLock rl = new ReentrantLock();
            Assert.IsTrue(rl.TryLock());
            Assert.IsTrue(rl.IsLocked);
            rl.Unlock();
        }


        [Test]
        public void HasQueuedThreads()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lockRenamed).Run));

            Assert.IsFalse(lockRenamed.HasQueuedThreads);
            lockRenamed.Lock();
            t1.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(lockRenamed.HasQueuedThreads);
            t2.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(lockRenamed.HasQueuedThreads);
            t1.Interrupt();
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(lockRenamed.HasQueuedThreads);
            lockRenamed.Unlock();
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(lockRenamed.HasQueuedThreads);
            t1.Join();
            t2.Join();
        }


        [Test]
        public void GetQueueLength()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lockRenamed).Run));

            Assert.AreEqual(0, lockRenamed.QueueLength);
            lockRenamed.Lock();
            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(1, lockRenamed.QueueLength);
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(2, lockRenamed.QueueLength);
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(1, lockRenamed.QueueLength);
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(0, lockRenamed.QueueLength);
            t1.Join();
            t2.Join();
        }


        [Test]
        public void GetQueueLength_fair()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lockRenamed).Run));
            Assert.AreEqual(0, lockRenamed.QueueLength);
            lockRenamed.Lock();
            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(1, lockRenamed.QueueLength);
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(2, lockRenamed.QueueLength);
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(1, lockRenamed.QueueLength);
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(0, lockRenamed.QueueLength);
            t1.Join();
            t2.Join();
        }


        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void HasQueuedThreadNPE()
        {
            ReentrantLock sync = new ReentrantLock(true);
            sync.IsQueuedThread(null);
        }


        [Test]
        public void HasQueuedThread()
        {
            ReentrantLock sync = new ReentrantLock(true);
            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(sync).Run));
            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(sync).Run));
            Assert.IsFalse(sync.IsQueuedThread(t1));
            Assert.IsFalse(sync.IsQueuedThread(t2));
            sync.Lock();
            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(sync.IsQueuedThread(t1));
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(sync.IsQueuedThread(t1));
            Assert.IsTrue(sync.IsQueuedThread(t2));
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(sync.IsQueuedThread(t1));
            Assert.IsTrue(sync.IsQueuedThread(t2));
            sync.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(sync.IsQueuedThread(t1));

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(sync.IsQueuedThread(t2));
            t1.Join();
            t2.Join();
        }


        [Test]
        public void GetQueuedThreads()
        {
            PublicReentrantLock lockRenamed = new PublicReentrantLock(true);
            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lockRenamed).Run));

            Assert.IsTrue((lockRenamed.QueuedThreads.Count == 0));
            lockRenamed.Lock();
            Assert.IsTrue((lockRenamed.QueuedThreads.Count == 0));
            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.QueuedThreads, t1));
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.QueuedThreads, t1));
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.QueuedThreads, t2));
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(CollectionUtils.Contains(lockRenamed.QueuedThreads, t1));
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.QueuedThreads, t2));
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue((lockRenamed.QueuedThreads.Count == 0));
            t1.Join();
            t2.Join();
        }


        [Test]
            [Ignore("Failing")]
        public void InterruptedException2()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.Lock();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(lockRenamed).Run));
            t.Start();
            t.Interrupt();
        }


        [Test]
        public void TryLockWhenIsLocked()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.Lock();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(lockRenamed).Run));
            t.Start();
            t.Join();
            lockRenamed.Unlock();
        }


        [Test]
        public void TryLock_Timeout()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.Lock();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(lockRenamed).Run));
            t.Start();
            t.Join();
            lockRenamed.Unlock();
        }


        [Test]
        public void GetHoldCount()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            for (int i = 1; i <= DEFAULT_COLLECTION_SIZE; i++)
            {
                lockRenamed.Lock();
                Assert.AreEqual(i, lockRenamed.HoldCount);
            }
            for (int i = DEFAULT_COLLECTION_SIZE; i > 0; i--)
            {
                lockRenamed.Unlock();
                Assert.AreEqual(i - 1, lockRenamed.HoldCount);
            }
        }


        [Test]
        public void IsIsLocked()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.Lock();
            Assert.IsTrue(lockRenamed.IsLocked);
            lockRenamed.Unlock();
            Assert.IsFalse(lockRenamed.IsLocked);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(lockRenamed).Run));
            t.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(lockRenamed.IsLocked);
            t.Join();
            Assert.IsFalse(lockRenamed.IsLocked);
        }


        [Test]
            [Ignore("Failing")]
        public void LockInterruptibly1()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.Lock();
            Thread t = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            t.Start();
            t.Interrupt();
            lockRenamed.Unlock();
            t.Join();
        }


        [Test]
            [Ignore("Failing")]
        public void LockInterruptibly2()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            lockRenamed.LockInterruptibly();
            Thread t = new Thread(new ThreadStart(new InterruptedLockRunnable(lockRenamed).Run));
            t.Start();
            t.Interrupt();
            Assert.IsTrue(lockRenamed.IsLocked);
            Assert.IsTrue(lockRenamed.HeldByCurrentThread);
            t.Join();
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void Await_IllegalMonitor()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            c.Await();
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void Signal_IllegalMonitor()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            c.Signal();
        }


        [Test]
            [Ignore("Failing")]
        public void AwaitNanos_Timeout()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            lockRenamed.Lock();
			
            Assert.IsTrue(c.Await(new TimeSpan(1000 * 100)));
            lockRenamed.Unlock();
        }


        [Test]
        public void Await_Timeout()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            lockRenamed.Lock();
            c.Await(SHORT_DELAY_MS);
            lockRenamed.Unlock();
        }


        [Test]
            [Ignore("Failing")]
        public void AwaitUntil_Timeout()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            lockRenamed.Lock();
            DateTime tempAux = DateTime.Now.AddMilliseconds(10); 
            bool deadlinePassed = c.AwaitUntil(tempAux);
            Assert.IsTrue(deadlinePassed);
            lockRenamed.Unlock();
        }


        [Test]
        public void Await()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            ICondition c = lockRenamed.NewCondition();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(lockRenamed, c).Run));

            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            c.Signal();
            lockRenamed.Unlock();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }


        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void HasWaitersNRE()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            lockRenamed.HasWaiters(null);
        }


        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void GetWaitQueueLengthNRE()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            lockRenamed.GetWaitQueueLength(null);
        }


        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void GetWaitingThreadsNRE()
        {
            PublicReentrantLock lockRenamed = new PublicReentrantLock(true);
            lockRenamed.GetWaitingThreads(null);
        }


        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void HasWaitersIAE()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            ICondition c = lockRenamed.NewCondition();
            ReentrantLock lock2 = new ReentrantLock(true);
            lock2.HasWaiters(c);
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void HasWaitersIMSE()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            ICondition c = lockRenamed.NewCondition();
            lockRenamed.HasWaiters(c);
        }


        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void GetWaitQueueLengthArgumentException()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            ICondition c = (lockRenamed.NewCondition());
            ReentrantLock lock2 = new ReentrantLock(true);
            lock2.GetWaitQueueLength(c);
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void GetWaitQueueLengthSLE()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            ICondition c = (lockRenamed.NewCondition());
            lockRenamed.GetWaitQueueLength(c);
        }


        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void GetWaitingThreadsIAE()
        {
            PublicReentrantLock lockRenamed = new PublicReentrantLock(true);
            ICondition c = (lockRenamed.NewCondition());
            PublicReentrantLock lock2 = new PublicReentrantLock(true);
            lock2.GetWaitingThreads(c);
        }


        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void GetWaitingThreadsIMSE()
        {
            PublicReentrantLock lockRenamed = new PublicReentrantLock(true);
            ICondition c = (lockRenamed.NewCondition());
            lockRenamed.GetWaitingThreads(c);
        }


        [Test]
        public void HasWaiters()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);
            ICondition c = lockRenamed.NewCondition();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(lockRenamed, c).Run));

            t.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsTrue(lockRenamed.HasWaiters(c));
            Assert.AreEqual(1, lockRenamed.GetWaitQueueLength(c));
            c.Signal();
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsFalse(lockRenamed.HasWaiters(c));
            Assert.AreEqual(0, lockRenamed.GetWaitQueueLength(c));
            lockRenamed.Unlock();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }


        [Test]
        public void GetWaitQueueLength()
        {
            ReentrantLock lockRenamed = new ReentrantLock(true);

            ICondition c = lockRenamed.NewCondition();
            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable6(lockRenamed, c).Run));

            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable7(lockRenamed, c).Run));

            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsTrue(lockRenamed.HasWaiters(c));
            Assert.AreEqual(2, lockRenamed.GetWaitQueueLength(c));
            c.SignalAll();
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsFalse(lockRenamed.HasWaiters(c));
            Assert.AreEqual(0, lockRenamed.GetWaitQueueLength(c));
            lockRenamed.Unlock();
            t1.Join(SHORT_DELAY_MS);
            t2.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t1.IsAlive);
            Assert.IsFalse(t2.IsAlive);
        }


        [Test]
        public void GetWaitingThreads()
        {
            PublicReentrantLock lockRenamed = new PublicReentrantLock(true);

            ICondition c = lockRenamed.NewCondition();
            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable8(lockRenamed, c).Run));

            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable9(lockRenamed, c).Run));

            lockRenamed.Lock();
            Assert.IsTrue((lockRenamed.GetWaitingThreads(c).Count == 0));
            lockRenamed.Unlock();
            t1.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsTrue(lockRenamed.HasWaiters(c));
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.GetWaitingThreads(c), t1));
            Assert.IsTrue(CollectionUtils.Contains(lockRenamed.GetWaitingThreads(c), t2));
            c.SignalAll();
            lockRenamed.Unlock();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            Assert.IsFalse(lockRenamed.HasWaiters(c));
            Assert.IsTrue((lockRenamed.GetWaitingThreads(c).Count == 0));
            lockRenamed.Unlock();
            t1.Join(SHORT_DELAY_MS);
            t2.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t1.IsAlive);
            Assert.IsFalse(t2.IsAlive);
        }


        [Test]
            [Ignore("Failing")]
        public void AwaitUninterruptibly()
        {
            ReentrantLock lockRenamed = new ReentrantLock();

            ICondition c = lockRenamed.NewCondition();
            UninterruptableThread thread = new UninterruptableThread(lockRenamed, c);

            thread.Start();

            while (!thread.lockStarted)
            {
                Thread.Sleep(new TimeSpan((Int64) 10000*100));
            }

            lockRenamed.Lock();
            try
            {
                thread.Interrupt();
                thread.canAwake = true;
                c.Signal();
            }
            finally
            {
                lockRenamed.Unlock();
            }

            thread.Join();
            Assert.IsTrue(thread.interrupted);
            Assert.IsFalse(thread.IsAlive);
        }


        [Test]
        public void Await_Interrupt()
        {
            ReentrantLock lockRenamed = new ReentrantLock();

            ICondition c = lockRenamed.NewCondition();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable10(lockRenamed, c).Run));

            t.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }


        [Test]
            [Ignore("Failing")]
        public void AwaitNanos_Interrupt()
        {
            ReentrantLock lockRenamed = new ReentrantLock();

            ICondition c = lockRenamed.NewCondition();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable11(lockRenamed, c).Run));

            t.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }


        [Test]
        [Ignore("Failing")]
        public void AwaitUntil_Interrupt()
        {
            ReentrantLock lockRenamed = new ReentrantLock();

            ICondition c = lockRenamed.NewCondition();
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable12(lockRenamed, c).Run));

            t.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }


        [Test]
        public void SignalAll()
        {
            ReentrantLock lockRenamed = new ReentrantLock();

            ICondition c = lockRenamed.NewCondition();
            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable13(lockRenamed, c).Run));

            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable14(lockRenamed, c).Run));

            t1.Start();
            t2.Start();

            Thread.Sleep(SHORT_DELAY_MS);
            lockRenamed.Lock();
            c.SignalAll();
            lockRenamed.Unlock();
            t1.Join(SHORT_DELAY_MS);
            t2.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t1.IsAlive);
            Assert.IsFalse(t2.IsAlive);
        }


        [Test]
        public void Serialization()
        {
            ReentrantLock l = new ReentrantLock();
            l.Lock();
            l.Unlock();
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            ReentrantLock r = (ReentrantLock) formatter2.Deserialize(bin);
            r.Lock();
            r.Unlock();
        }


        [Test]
        public void OutputToString()
        {
            ReentrantLock lockRenamed = new ReentrantLock();
            String us = lockRenamed.ToString();
            Assert.IsTrue(us.IndexOf("Unlocked") >= 0);
            lockRenamed.Lock();
            String ls = lockRenamed.ToString();
            Assert.IsTrue(ls.IndexOf("Locked by thread") >= 0);
        }
    }
}