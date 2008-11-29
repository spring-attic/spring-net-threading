//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Threading;
//using NUnit.Framework;
//using Spring.Util;

//namespace Spring.Threading.Locks
//{
//    public class ReentrantLockTests : BaseThreadingTestCase
//    {
//        #region Helper Classes

//        private class AnonymousClassRunnable : IRunnable
//        {
//            private ReentrantLock lock_Renamed;

//            public AnonymousClassRunnable(ReentrantLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.TryLock(MEDIUM_DELAY_MS);
//                    Debug.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable1 : IRunnable
//        {
//            private ReentrantLock lock_Renamed;

//            public AnonymousClassRunnable1(ReentrantLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            public void Run()
//            {
//                Debug.Assert(!lock_Renamed.TryLock());
//            }
//        }

//        private class AnonymousClassRunnable2 : IRunnable
//        {
//            public AnonymousClassRunnable2(ReentrantLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    Debug.Assert(!lock_Renamed.TryLock(new TimeSpan(0, 0, 1/1000)));
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable3 : IRunnable
//        {
//            private ReentrantLock lock_Renamed;

//            public AnonymousClassRunnable3(ReentrantLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            public void Run()
//            {
//                lock_Renamed.Lock();
//                try
//                {
//                    Thread.Sleep(new TimeSpan(10000*SMALL_DELAY_MS.Milliseconds));
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//                lock_Renamed.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable4 : IRunnable
//        {
//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public AnonymousClassRunnable4(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable5 : IRunnable
//        {
//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public AnonymousClassRunnable5(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    Debug.Assert(!lock_Renamed.HasWaiters(condition));
//                    Debug.Assert(0 == lock_Renamed.GetWaitQueueLength(condition));
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable6 : IRunnable
//        {
//            public AnonymousClassRunnable6(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    Debug.Assert(!lock_Renamed.HasWaiters(condition));
//                    Debug.Assert(0 == lock_Renamed.GetWaitQueueLength(condition));
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable7 : IRunnable
//        {
//            public AnonymousClassRunnable7(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    Debug.Assert(lock_Renamed.HasWaiters(condition));
//                    Debug.Assert(1 == lock_Renamed.GetWaitQueueLength(condition));
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable8 : IRunnable
//        {
//            public AnonymousClassRunnable8(PublicReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private PublicReentrantLock lock_Renamed;
//            private ICondition condition;


//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    Debug.Assert(lock_Renamed.GetWaitingThreads(condition).Count == 0);
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable9 : IRunnable
//        {
//            public AnonymousClassRunnable9(PublicReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private PublicReentrantLock lock_Renamed;
//            private ICondition condition;


//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    Debug.Assert(lock_Renamed.GetWaitingThreads(condition).Count != 0);
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable10 : IRunnable
//        {
//            public AnonymousClassRunnable10(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                    Debug.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable11 : IRunnable
//        {
//            public AnonymousClassRunnable11(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    condition.Await(new TimeSpan(0,0,0,1));
//                    lock_Renamed.Unlock();
//                    Debug.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable12 : IRunnable
//        {
//            public AnonymousClassRunnable12(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    DateTime d = DateTime.Now;
//                    DateTime tempAux = new DateTime(d.Ticks + 10000);
//                    condition.AwaitUntil(tempAux);
//                    lock_Renamed.Unlock();
//                    Debug.Fail("Should throw exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable13 : IRunnable
//        {
//            public AnonymousClassRunnable13(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.condition = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition condition;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    condition.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        private class AnonymousClassRunnable14 : IRunnable
//        {
//            public AnonymousClassRunnable14(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantLock lock_Renamed;
//            private ICondition c;


//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.Lock();
//                    c.Await();
//                    lock_Renamed.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    throw;
//                }
//            }
//        }

//        internal class InterruptibleLockRunnable : IRunnable
//        {
//            internal ReentrantLock lock_Renamed;

//            internal InterruptibleLockRunnable(ReentrantLock l)
//            {
//                lock_Renamed = l;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.LockInterruptibly();
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }


//        internal class InterruptedLockRunnable : IRunnable
//        {
//            internal ReentrantLock lock_Renamed;

//            internal InterruptedLockRunnable(ReentrantLock l)
//            {
//                lock_Renamed = l;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.LockInterruptibly();
//                    Debug.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        internal class UninterruptableThread : IRunnable
//        {
//            private ReentrantLock lock_Renamed;
//            private ICondition c;
//            private Thread _internalThread;

//            public volatile bool canAwake = false;
//            public volatile bool interrupted = false;
//            public volatile bool lockStarted = false;
			
//            public UninterruptableThread(ReentrantLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//                _internalThread = new Thread(new ThreadStart(this.Run));
//            }

//            public void Run()
//            {
//                lock_Renamed.Lock();
//                lockStarted = true;

//                try
//                {
//                    while (!canAwake)
//                    {
//                        c.AwaitUninterruptibly();
//                    }
//                }
//                catch (ThreadInterruptedException)
//                {
//                    interrupted = true;
//                }

//                lock_Renamed.Unlock();
//            }
//            public void Start()
//            {
//                _internalThread.Start();
//            }
//            public void Interrupt()
//            {
//                _internalThread.Interrupt();
//            }
//            public bool IsAlive
//            {
//                get { return _internalThread.IsAlive;}
//            }
//            public void Join()
//            {
//                _internalThread.Join();
//            }
//        }

//        [Serializable]
//        internal class PublicReentrantLock : ReentrantLock
//        {
//            internal PublicReentrantLock() : base()
//            {
//            }

//            internal PublicReentrantLock(bool fair) : base(fair)
//            {
//            }

//            public override ICollection QueuedThreads
//            {
//                get { return base.QueuedThreads; }
//            }

//            public override ICollection GetWaitingThreads(ICondition c)
//            {
//                return base.GetWaitingThreads(c);
//            }
//        }

//        #endregion

//        [Test]
//        public void Constructor()
//        {
//            ReentrantLock rl = new ReentrantLock();
//            Assert.IsFalse(rl.IsFair);
//            ReentrantLock r2 = new ReentrantLock(true);
//            Assert.IsTrue(r2.IsFair);
//        }


//        [Test]
//        public void Lock()
//        {
//            ReentrantLock rl = new ReentrantLock();
//            rl.Lock();
//            Assert.IsTrue(rl.IsLocked);
//            rl.Unlock();
//        }


//        [Test]
//        public void IsFairLock()
//        {
//            ReentrantLock rl = new ReentrantLock(true);
//            rl.Lock();
//            Assert.IsTrue(rl.IsLocked);
//            rl.Unlock();
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void Unlock_InvalidOperationException()
//        {
//            ReentrantLock rl = new ReentrantLock();
//            rl.Unlock();
//            Assert.Fail("Should throw an exception");
//        }


//        [Test]
//        public void TryLock()
//        {
//            ReentrantLock rl = new ReentrantLock();
//            Assert.IsTrue(rl.TryLock());
//            Assert.IsTrue(rl.IsLocked);
//            rl.Unlock();
//        }


//        [Test]
//        public void HasQueuedThreads()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lock_Renamed).Run));

//            Assert.IsFalse(lock_Renamed.HasQueuedThreads);
//            lock_Renamed.Lock();
//            t1.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(lock_Renamed.HasQueuedThreads);
//            t2.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(lock_Renamed.HasQueuedThreads);
//            t1.Interrupt();
//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(lock_Renamed.HasQueuedThreads);
//            lock_Renamed.Unlock();
//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsFalse(lock_Renamed.HasQueuedThreads);
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        public void GetQueueLength()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lock_Renamed).Run));

//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            lock_Renamed.Lock();
//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(2, lock_Renamed.QueueLength);
//            t1.Interrupt();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        public void GetQueueLength_fair()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lock_Renamed).Run));
//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            lock_Renamed.Lock();
//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(2, lock_Renamed.QueueLength);
//            t1.Interrupt();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        [ExpectedException(typeof (NullReferenceException))]
//        public void HasQueuedThreadNPE()
//        {
//            ReentrantLock sync = new ReentrantLock(true);
//            sync.IsQueuedThread(null);
//        }


//        [Test]
//        public void HasQueuedThread()
//        {
//            ReentrantLock sync = new ReentrantLock(true);
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(sync).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(sync).Run));
//            Assert.IsFalse(sync.IsQueuedThread(t1));
//            Assert.IsFalse(sync.IsQueuedThread(t2));
//            sync.Lock();
//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(sync.IsQueuedThread(t1));
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(sync.IsQueuedThread(t1));
//            Assert.IsTrue(sync.IsQueuedThread(t2));
//            t1.Interrupt();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsFalse(sync.IsQueuedThread(t1));
//            Assert.IsTrue(sync.IsQueuedThread(t2));
//            sync.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsFalse(sync.IsQueuedThread(t1));

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsFalse(sync.IsQueuedThread(t2));
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//        public void GetQueuedThreads()
//        {
//            PublicReentrantLock lock_Renamed = new PublicReentrantLock(true);
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lock_Renamed).Run));

//            Assert.IsTrue((lock_Renamed.QueuedThreads.Count == 0));
//            lock_Renamed.Lock();
//            Assert.IsTrue((lock_Renamed.QueuedThreads.Count == 0));
//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.QueuedThreads, t1));
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.QueuedThreads, t1));
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.QueuedThreads, t2));
//            t1.Interrupt();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsFalse(CollectionUtils.Contains(lock_Renamed.QueuedThreads, t1));
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.QueuedThreads, t2));
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue((lock_Renamed.QueuedThreads.Count == 0));
//            t1.Join();
//            t2.Join();
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void InterruptedException2()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//        }


//        [Test]
//        public void TryLockWhenIsLocked()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.Unlock();
//        }


//        [Test]
//        public void TryLock_Timeout()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.Unlock();
//        }


//        [Test]
//        public void GetHoldCount()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            for (int i = 1; i <= DEFAULT_COLLECTION_SIZE; i++)
//            {
//                lock_Renamed.Lock();
//                Assert.AreEqual(i, lock_Renamed.HoldCount);
//            }
//            for (int i = DEFAULT_COLLECTION_SIZE; i > 0; i--)
//            {
//                lock_Renamed.Unlock();
//                Assert.AreEqual(i - 1, lock_Renamed.HoldCount);
//            }
//        }


//        [Test]
//        public void IsIsLocked()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.Lock();
//            Assert.IsTrue(lock_Renamed.IsLocked);
//            lock_Renamed.Unlock();
//            Assert.IsFalse(lock_Renamed.IsLocked);
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(lock_Renamed).Run));
//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            Assert.IsTrue(lock_Renamed.IsLocked);
//            t.Join();
//            Assert.IsFalse(lock_Renamed.IsLocked);
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void LockInterruptibly1()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.Lock();
//            Thread t = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            lock_Renamed.Unlock();
//            t.Join();
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void LockInterruptibly2()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            lock_Renamed.LockInterruptibly();
//            Thread t = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            Assert.IsTrue(lock_Renamed.IsLocked);
//            Assert.IsTrue(lock_Renamed.HeldByCurrentThread);
//            t.Join();
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void Await_IllegalMonitor()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            c.Await();
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void Signal_IllegalMonitor()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            c.Signal();
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void AwaitNanos_Timeout()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            lock_Renamed.Lock();
			
//            Assert.IsTrue(c.Await(new TimeSpan(1000 * 100)));
//            lock_Renamed.Unlock();
//        }


//        [Test]
//        public void Await_Timeout()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            lock_Renamed.Lock();
//            c.Await(SHORT_DELAY_MS);
//            lock_Renamed.Unlock();
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void AwaitUntil_Timeout()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            lock_Renamed.Lock();
//            DateTime tempAux = DateTime.Now.AddMilliseconds(10); 
//            bool deadlinePassed = c.AwaitUntil(tempAux);
//            Assert.IsTrue(deadlinePassed);
//            lock_Renamed.Unlock();
//        }


//        [Test]
//        public void Await()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            ICondition c = lock_Renamed.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(lock_Renamed, c).Run));

//            t.Start();
//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            c.Signal();
//            lock_Renamed.Unlock();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test]
//        [ExpectedException(typeof (NullReferenceException))]
//        public void HasWaitersNRE()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            lock_Renamed.HasWaiters(null);
//        }


//        [Test]
//        [ExpectedException(typeof (NullReferenceException))]
//        public void GetWaitQueueLengthNRE()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            lock_Renamed.GetWaitQueueLength(null);
//        }


//        [Test]
//        [ExpectedException(typeof (NullReferenceException))]
//        public void GetWaitingThreadsNRE()
//        {
//            PublicReentrantLock lock_Renamed = new PublicReentrantLock(true);
//            lock_Renamed.GetWaitingThreads(null);
//        }


//        [Test]
//        [ExpectedException(typeof (ArgumentException))]
//        public void HasWaitersIAE()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            ICondition c = lock_Renamed.NewCondition();
//            ReentrantLock lock2 = new ReentrantLock(true);
//            lock2.HasWaiters(c);
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void HasWaitersIMSE()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            ICondition c = lock_Renamed.NewCondition();
//            lock_Renamed.HasWaiters(c);
//        }


//        [Test]
//        [ExpectedException(typeof (ArgumentException))]
//        public void GetWaitQueueLengthArgumentException()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            ICondition c = (lock_Renamed.NewCondition());
//            ReentrantLock lock2 = new ReentrantLock(true);
//            lock2.GetWaitQueueLength(c);
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void GetWaitQueueLengthSLE()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            ICondition c = (lock_Renamed.NewCondition());
//            lock_Renamed.GetWaitQueueLength(c);
//        }


//        [Test]
//        [ExpectedException(typeof (ArgumentException))]
//        public void GetWaitingThreadsIAE()
//        {
//            PublicReentrantLock lock_Renamed = new PublicReentrantLock(true);
//            ICondition c = (lock_Renamed.NewCondition());
//            PublicReentrantLock lock2 = new PublicReentrantLock(true);
//            lock2.GetWaitingThreads(c);
//        }


//        [Test]
//        [ExpectedException(typeof (SynchronizationLockException))]
//        public void GetWaitingThreadsIMSE()
//        {
//            PublicReentrantLock lock_Renamed = new PublicReentrantLock(true);
//            ICondition c = (lock_Renamed.NewCondition());
//            lock_Renamed.GetWaitingThreads(c);
//        }


//        [Test]
//        public void HasWaiters()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);
//            ICondition c = lock_Renamed.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsTrue(lock_Renamed.HasWaiters(c));
//            Assert.AreEqual(1, lock_Renamed.GetWaitQueueLength(c));
//            c.Signal();
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsFalse(lock_Renamed.HasWaiters(c));
//            Assert.AreEqual(0, lock_Renamed.GetWaitQueueLength(c));
//            lock_Renamed.Unlock();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test]
//        public void GetWaitQueueLength()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock(true);

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable6(lock_Renamed, c).Run));

//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable7(lock_Renamed, c).Run));

//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsTrue(lock_Renamed.HasWaiters(c));
//            Assert.AreEqual(2, lock_Renamed.GetWaitQueueLength(c));
//            c.SignalAll();
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsFalse(lock_Renamed.HasWaiters(c));
//            Assert.AreEqual(0, lock_Renamed.GetWaitQueueLength(c));
//            lock_Renamed.Unlock();
//            t1.Join(SHORT_DELAY_MS);
//            t2.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t1.IsAlive);
//            Assert.IsFalse(t2.IsAlive);
//        }


//        [Test]
//        public void GetWaitingThreads()
//        {
//            PublicReentrantLock lock_Renamed = new PublicReentrantLock(true);

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable8(lock_Renamed, c).Run));

//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable9(lock_Renamed, c).Run));

//            lock_Renamed.Lock();
//            Assert.IsTrue((lock_Renamed.GetWaitingThreads(c).Count == 0));
//            lock_Renamed.Unlock();
//            t1.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsTrue(lock_Renamed.HasWaiters(c));
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.GetWaitingThreads(c), t1));
//            Assert.IsTrue(CollectionUtils.Contains(lock_Renamed.GetWaitingThreads(c), t2));
//            c.SignalAll();
//            lock_Renamed.Unlock();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            Assert.IsFalse(lock_Renamed.HasWaiters(c));
//            Assert.IsTrue((lock_Renamed.GetWaitingThreads(c).Count == 0));
//            lock_Renamed.Unlock();
//            t1.Join(SHORT_DELAY_MS);
//            t2.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t1.IsAlive);
//            Assert.IsFalse(t2.IsAlive);
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void AwaitUninterruptibly()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();

//            ICondition c = lock_Renamed.NewCondition();
//            UninterruptableThread thread = new UninterruptableThread(lock_Renamed, c);

//            thread.Start();

//            while (!thread.lockStarted)
//            {
//                Thread.Sleep(new TimeSpan((Int64) 10000*100));
//            }

//            lock_Renamed.Lock();
//            try
//            {
//                thread.Interrupt();
//                thread.canAwake = true;
//                c.Signal();
//            }
//            finally
//            {
//                lock_Renamed.Unlock();
//            }

//            thread.Join();
//            Assert.IsTrue(thread.interrupted);
//            Assert.IsFalse(thread.IsAlive);
//        }


//        [Test]
//        public void Await_Interrupt()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable10(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test]
//            [Ignore("Failing")]
//        public void AwaitNanos_Interrupt()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable11(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test]
//        public void AwaitUntil_Interrupt()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable12(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test]
//        public void SignalAll()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();

//            ICondition c = lock_Renamed.NewCondition();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable13(lock_Renamed, c).Run));

//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable14(lock_Renamed, c).Run));

//            t1.Start();
//            t2.Start();

//            Thread.Sleep(SHORT_DELAY_MS);
//            lock_Renamed.Lock();
//            c.SignalAll();
//            lock_Renamed.Unlock();
//            t1.Join(SHORT_DELAY_MS);
//            t2.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t1.IsAlive);
//            Assert.IsFalse(t2.IsAlive);
//        }


//        [Test]
//        public void Serialization()
//        {
//            ReentrantLock l = new ReentrantLock();
//            l.Lock();
//            l.Unlock();
//            MemoryStream bout = new MemoryStream(10000);

//            BinaryFormatter formatter = new BinaryFormatter();
//            formatter.Serialize(bout, l);

//            MemoryStream bin = new MemoryStream(bout.ToArray());
//            BinaryFormatter formatter2 = new BinaryFormatter();
//            ReentrantLock r = (ReentrantLock) formatter2.Deserialize(bin);
//            r.Lock();
//            r.Unlock();
//        }


//        [Test]
//        public void OutputToString()
//        {
//            ReentrantLock lock_Renamed = new ReentrantLock();
//            String us = lock_Renamed.ToString();
//            Assert.IsTrue(us.IndexOf("Unlocked") >= 0);
//            lock_Renamed.Lock();
//            String ls = lock_Renamed.ToString();
//            Assert.IsTrue(ls.IndexOf("Locked by thread") >= 0);
//        }
//    }
//}