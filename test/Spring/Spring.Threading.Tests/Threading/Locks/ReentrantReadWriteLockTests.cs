//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Threading;
//using NUnit.Framework;

//namespace Spring.Threading.Locks
//{
//    public class ReentrantReadWriteLockTests : BaseThreadingTestCase
//    {
//        private class AnonymousClassRunnable : IRunnable
//        {
//            public AnonymousClassRunnable(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.LockInterruptibly();
//                    lock_Renamed.WriterLock.Unlock();
//                    lock_Renamed.WriterLock.LockInterruptibly();
//                    lock_Renamed.WriterLock.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable1 : IRunnable
//        {
//            public AnonymousClassRunnable1(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.TryLock(new TimeSpan(0, 0, 0, 0, 1000));
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable2 : IRunnable
//        {
//            public AnonymousClassRunnable2(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.ReaderLock.LockInterruptibly();
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable3 : IRunnable
//        {
//            public AnonymousClassRunnable3(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.ReaderLock.TryLock(new TimeSpan(0, 0, 0, 0, 1000));
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable4 : IRunnable
//        {
//            public AnonymousClassRunnable4(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                Debug.Assert(!lock_Renamed.WriterLock.TryLock());
//            }
//        }

//        private class AnonymousClassRunnable5 : IRunnable
//        {
//            public AnonymousClassRunnable5(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                Debug.Assert(!lock_Renamed.ReaderLock.TryLock());
//            }
//        }

//        private class AnonymousClassRunnable6 : IRunnable
//        {
//            public AnonymousClassRunnable6(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                Debug.Assert(lock_Renamed.ReaderLock.TryLock());
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable7 : IRunnable
//        {
//            public AnonymousClassRunnable7(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.ReaderLock.Lock();
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable8 : IRunnable
//        {
//            public AnonymousClassRunnable8(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.WriterLock.Lock();
//                lock_Renamed.WriterLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable9 : IRunnable
//        {
//            public AnonymousClassRunnable9(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.ReaderLock.Lock();
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable10 : IRunnable
//        {
//            public AnonymousClassRunnable10(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.ReaderLock.Lock();
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable11 : IRunnable
//        {
//            public AnonymousClassRunnable11(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.ReaderLock.Lock();
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable12 : IRunnable
//        {
//            public AnonymousClassRunnable12(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                lock_Renamed.ReaderLock.Lock();
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable13 : IRunnable
//        {
//            public AnonymousClassRunnable13(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                Debug.Assert(lock_Renamed.ReaderLock.TryLock());
//                lock_Renamed.ReaderLock.Unlock();
//            }
//        }

//        private class AnonymousClassRunnable14 : IRunnable
//        {
//            public AnonymousClassRunnable14(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                Debug.Assert(!lock_Renamed.WriterLock.TryLock());
//            }
//        }

//        private class AnonymousClassRunnable15 : IRunnable
//        {
//            public AnonymousClassRunnable15(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    Debug.Assert(!lock_Renamed.WriterLock.TryLock(new TimeSpan()));
//                }
//                catch (Exception)
//                {
//                    Assert.Fail("Should not throw an exception");
//                }
//            }
//        }

//        private class AnonymousClassRunnable16 : IRunnable
//        {
//            public AnonymousClassRunnable16(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    Debug.Assert(!lock_Renamed.ReaderLock.TryLock(new TimeSpan(0, 0, 0, 0, 1)));
//                }
//                catch (Exception)
//                {
//                    Assert.Fail("Should not throw an exception");
//                }
//            }
//        }

//        private class AnonymousClassRunnable17 : IRunnable
//        {
//            public AnonymousClassRunnable17(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.LockInterruptibly();
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable18 : IRunnable
//        {
//            public AnonymousClassRunnable18(ReentrantReadWriteLock lock_Renamed)
//            {
//                this.lock_Renamed = lock_Renamed;
//            }

//            private ReentrantReadWriteLock lock_Renamed;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.ReaderLock.LockInterruptibly();
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable19 : IRunnable
//        {
//            public AnonymousClassRunnable19(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    c.Await();
//                    lock_Renamed.WriterLock.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    Assert.Fail("Should not throw an exception.");
//                }
//            }
//        }

//        private class AnonymousClassRunnable20 : IRunnable
//        {
//            public AnonymousClassRunnable20(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    c.Await();
//                    lock_Renamed.WriterLock.Unlock();
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable21 : IRunnable
//        {
//            public AnonymousClassRunnable21(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    c.Await(MEDIUM_DELAY_MS);
//                    lock_Renamed.WriterLock.Unlock();
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable22 : IRunnable
//        {
//            public AnonymousClassRunnable22(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    DateTime now = DateTime.Now;
//                    DateTime deadline = new DateTime(now.Millisecond + 10000);
//                    c.AwaitUntil(deadline);
//                    lock_Renamed.WriterLock.Unlock();
//                    Assert.Fail("Should throw an exception");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        private class AnonymousClassRunnable23 : IRunnable
//        {
//            public AnonymousClassRunnable23(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    c.Await();
//                    lock_Renamed.WriterLock.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    Assert.Fail("Should not throw an exception.");
//                }
//            }
//        }

//        private class AnonymousClassRunnable24 : IRunnable
//        {
//            public AnonymousClassRunnable24(ReentrantReadWriteLock lock_Renamed, ICondition c)
//            {
//                this.lock_Renamed = lock_Renamed;
//                this.c = c;
//            }

//            private ReentrantReadWriteLock lock_Renamed;
//            private ICondition c;

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.Lock();
//                    c.Await();
//                    lock_Renamed.WriterLock.Unlock();
//                }
//                catch (ThreadInterruptedException)
//                {
//                    Assert.Fail("Should not throw an exception.");
//                }
//            }
//        }

//        internal class InterruptibleLockRunnable : IRunnable
//        {
//            internal ReentrantReadWriteLock lock_Renamed;

//            internal InterruptibleLockRunnable(ReentrantReadWriteLock l)
//            {
//                lock_Renamed = l;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.LockInterruptibly();
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }


//        internal class InterruptedLockRunnable : IRunnable
//        {
//            internal ReentrantReadWriteLock lock_Renamed;

//            internal InterruptedLockRunnable(ReentrantReadWriteLock l)
//            {
//                lock_Renamed = l;
//            }

//            public void Run()
//            {
//                try
//                {
//                    lock_Renamed.WriterLock.LockInterruptibly();
//                    Assert.Fail("Should throw an exception.");
//                }
//                catch (ThreadInterruptedException)
//                {
//                }
//            }
//        }

//        internal class UninterruptableThread : IRunnable
//        {
//            private ILock lock_Renamed;
//            private ICondition c;
//            private Thread _internalThread;

//            public volatile bool canAwake = false;
//            public volatile bool interrupted = false;
//            public volatile bool lockStarted = false;

//            public UninterruptableThread(ILock lock_Renamed, ICondition c)
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

//            [Test] public void Start()
//            {
//                _internalThread.Start();
//            }

//            [Test] public void Interrupt()
//            {
//                _internalThread.Interrupt();
//            }

//            public bool IsAlive
//            {
//                get { return _internalThread.IsAlive; }
//            }

//            [Test] public void Join()
//            {
//                _internalThread.Join();
//            }
//        }

//        [Serializable]
//        internal class PublicReentrantReadWriteLock2 : ReentrantReadWriteLock
//        {
//            internal PublicReentrantReadWriteLock2() : base()
//            {
//            }

//            protected PublicReentrantReadWriteLock2(SerializationInfo info, StreamingContext context) : base(info, context)
//            {
//            }

//            //			public override void GetObjectData(SerializationInfo info, StreamingContext context)
//            //			{
//            //				//base.GetObjectData(info, context);
//            //			}
//        }

//        [Test] public void Constructor()
//        {
//            ReentrantReadWriteLock rl = new ReentrantReadWriteLock();
//            Assert.IsFalse(rl.IsFair);
//            Assert.IsFalse(rl.IsWriteLockHeld);
//            Assert.AreEqual(0, rl.ReadLockCount);
//        }

//        [Test] public void Lock()
//        {
//            ReentrantReadWriteLock rl = new ReentrantReadWriteLock();
//            rl.WriterLock.Lock();
//            Assert.IsTrue(rl.IsWriteLockHeld);
//            Assert.IsTrue(rl.WriterLockedByCurrentThread);
//            Assert.AreEqual(0, rl.ReadLockCount);
//            rl.WriterLock.Unlock();
//            Assert.IsFalse(rl.IsWriteLockHeld);
//            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//            Assert.AreEqual(0, rl.ReadLockCount);
//            rl.ReaderLock.Lock();
//            Assert.IsFalse(rl.IsWriteLockHeld);
//            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//            Assert.AreEqual(1, rl.ReadLockCount);
//            rl.ReaderLock.Unlock();
//            Assert.IsFalse(rl.IsWriteLockHeld);
//            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//            Assert.AreEqual(0, rl.ReadLockCount);
//        }


//        [Test] public void GetHoldCount()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            for (int i = 1; i <= DEFAULT_COLLECTION_SIZE; i++)
//            {
//                lock_Renamed.WriterLock.Lock();
//                Assert.AreEqual(i, lock_Renamed.WriteHoldCount);
//            }
//            for (int i = DEFAULT_COLLECTION_SIZE; i > 0; i--)
//            {
//                lock_Renamed.WriterLock.Unlock();
//                Assert.AreEqual(i - 1, lock_Renamed.WriteHoldCount);
//            }
//        }


//        [Test] public void Unlock_IllegalMonitorStateException()
//        {
//            ReentrantReadWriteLock rl = new ReentrantReadWriteLock();
//            try
//            {
//                rl.WriterLock.Unlock();
//                Assert.Fail("Should throw an exception.");
//            }
//            catch (SynchronizationLockException)
//            {
//            }
//        }


//        [Test] public void WriteLockInterruptibly_Interrupted()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(lock_Renamed).Run));
//            lock_Renamed.WriterLock.Lock();
//            t.Start();
//            t.Interrupt();
//            lock_Renamed.WriterLock.Unlock();
//            t.Join();
//        }


//        [Test] public void WriteTryLock_Interrupted()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            lock_Renamed.WriterLock.Unlock();
//            t.Join();
//        }


//        [Test] public void ReadLockInterruptibly_Interrupted()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable2(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            lock_Renamed.WriterLock.Unlock();
//            t.Join();
//        }


//        [Test] [Ignore("Failing")] public void ReadTryLock_Interrupted()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable3(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            t.Join();
//        }


//        [Test] public void WriteTryLockWhenLocked()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable4(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void ReadTryLockWhenLocked()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable5(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void MultipleReadLocks()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.ReaderLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable6(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.ReaderLock.Unlock();
//        }


//        [Test] public void WriteAfterMultipleReadLocks()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.ReaderLock.Lock();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable7(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable8(lock_Renamed).Run));

//            t1.Start();
//            t2.Start();
//            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds*10000));
//            lock_Renamed.ReaderLock.Unlock();
//            t1.Join(MEDIUM_DELAY_MS);
//            t2.Join(MEDIUM_DELAY_MS);
//            Assert.IsTrue(!t1.IsAlive);
//            Assert.IsTrue(!t2.IsAlive);
//        }

//        [Test] 
//        [Ignore("Failing")]
//        public void ReadAfterWriterLock()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable9(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable10(lock_Renamed).Run));

//            t1.Start();
//            t2.Start();
//            Thread.Sleep(SHORT_DELAY_MS.Milliseconds*10000);
//            lock_Renamed.WriterLock.Unlock();
//            t1.Join(MEDIUM_DELAY_MS);
//            t2.Join(MEDIUM_DELAY_MS);
//            Assert.IsTrue(!t1.IsAlive);
//            Assert.IsTrue(!t2.IsAlive);
//        }


//        [Test] public void ReadHoldingWriterLock()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Assert.IsTrue(lock_Renamed.ReaderLock.TryLock());
//            lock_Renamed.ReaderLock.Unlock();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] 
//        [Ignore("Failing")]
//        public void ReadHoldingWriteLock2()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable11(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable12(lock_Renamed).Run));

//            t1.Start();
//            t2.Start();
//            lock_Renamed.ReaderLock.Lock();
//            lock_Renamed.ReaderLock.Unlock();
//            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds*10000));
//            lock_Renamed.ReaderLock.Lock();
//            lock_Renamed.ReaderLock.Unlock();
//            lock_Renamed.WriterLock.Unlock();
//            t1.Join(MEDIUM_DELAY_MS);
//            t2.Join(MEDIUM_DELAY_MS);
//            Assert.IsTrue(!t1.IsAlive);
//            Assert.IsTrue(!t2.IsAlive);
//        }

//        [Test] public void TryLockWhenReadLocked()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.ReaderLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable13(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.ReaderLock.Unlock();
//        }


//        [Test] public void WriteTryLockWhenReadLocked()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.ReaderLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable14(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.ReaderLock.Unlock();
//        }


//        [Test] public void WriteTryLock_Timeout()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable15(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void ReadTryLock_Timeout()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.Lock();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable16(lock_Renamed).Run));
//            t.Start();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void WriteLockInterruptibly()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.LockInterruptibly();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable17(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void ReadLockInterruptibly()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            lock_Renamed.WriterLock.LockInterruptibly();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable18(lock_Renamed).Run));
//            t.Start();
//            t.Interrupt();
//            t.Join();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void Await_IllegalMonitor()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            try
//            {
//                c.Await();
//                Assert.Fail("Should throw an exception.");
//            }

//            catch (SynchronizationLockException)
//            {
//            }
//        }


//        [Test] public void Signal_IllegalMonitor()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            try
//            {
//                c.Signal();
//                Assert.Fail("Should throw an exception.");
//            }

//            catch (SynchronizationLockException)
//            {
//            }
//        }


//        [Test] [Ignore("Failing")] public void AwaitNanos_Timeout()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            lock_Renamed.WriterLock.Lock();
//            Assert.IsTrue(c.Await(new TimeSpan(1)));
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void Await_Timeout()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            lock_Renamed.WriterLock.Lock();
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] public void AwaitUntil_Timeout()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            lock_Renamed.WriterLock.Lock();
//            DateTime d = DateTime.Now;
//            lock_Renamed.WriterLock.Unlock();
//        }


//        [Test] [Ignore("Failing")] public void Await()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable19(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            lock_Renamed.WriterLock.Lock();
//            c.Signal();
//            lock_Renamed.WriterLock.Unlock();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test] [Ignore("Failing")] public void AwaitUninterruptibly()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            UninterruptableThread thread = new UninterruptableThread(lock_Renamed.WriterLock, c);

//            thread.Start();

//            while (!thread.lockStarted)
//            {
//                Thread.Sleep(new TimeSpan((Int64) 10000*100));
//            }

//            lock_Renamed.WriterLock.Lock();
//            try
//            {
//                thread.Interrupt();
//                thread.canAwake = true;
//                c.Signal();
//            }
//            finally
//            {
//                lock_Renamed.WriterLock.Unlock();
//            }

//            thread.Join();
//            Assert.IsTrue(thread.interrupted);
//            Assert.IsFalse(thread.IsAlive);
//        }


//        [Test] public void Await_Interrupt()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable20(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test] public void AwaitNanos_Interrupt()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable21(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(new TimeSpan((Int64) 10000*SHORT_DELAY_MS.Milliseconds));
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test] public void AwaitUntil_Interrupt()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable22(lock_Renamed, c).Run));

//            t.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            t.Interrupt();
//            t.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t.IsAlive);
//        }


//        [Test][Ignore("Failing")] public void SignalAll()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            ICondition c = lock_Renamed.WriterLock.NewCondition();
//            Thread t1 = new Thread(new ThreadStart(new AnonymousClassRunnable23(lock_Renamed, c).Run));

//            Thread t2 = new Thread(new ThreadStart(new AnonymousClassRunnable24(lock_Renamed, c).Run));

//            t1.Start();
//            t2.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            lock_Renamed.WriterLock.Lock();
//            c.SignalAll();
//            lock_Renamed.WriterLock.Unlock();
//            t1.Join(SHORT_DELAY_MS);
//            t2.Join(SHORT_DELAY_MS);
//            Assert.IsFalse(t1.IsAlive);
//            Assert.IsFalse(t2.IsAlive);
//        }


//        [Test] public void Serialization()
//        {
//            ReentrantReadWriteLock l = new ReentrantReadWriteLock();
//            l.ReaderLock.Lock();
//            l.ReaderLock.Unlock();

//            MemoryStream bout = new MemoryStream(10000);

//            BinaryFormatter formatter = new BinaryFormatter();
//            formatter.Serialize(bout, l);

//            MemoryStream bin = new MemoryStream(bout.ToArray());
//            BinaryFormatter formatter2 = new BinaryFormatter();
//            ReentrantReadWriteLock r = (ReentrantReadWriteLock) formatter2.Deserialize(bin);
//            r.ReaderLock.Lock();
//            r.ReaderLock.Unlock();
//        }
//        [Test] public void GetQueueLength()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            Thread t1 = new Thread(new ThreadStart(new InterruptedLockRunnable(lock_Renamed).Run));
//            Thread t2 = new Thread(new ThreadStart(new InterruptibleLockRunnable(lock_Renamed).Run));
//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            lock_Renamed.WriterLock.Lock();
//            t1.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            t2.Start();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            Assert.AreEqual(2, lock_Renamed.QueueLength);
//            t1.Interrupt();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            Assert.AreEqual(1, lock_Renamed.QueueLength);
//            lock_Renamed.WriterLock.Unlock();

//            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
//            Assert.AreEqual(0, lock_Renamed.QueueLength);
//            t1.Join();
//            t2.Join();
//        }

//        [Test] public void ToStringTest()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();
//            String us = lock_Renamed.ToString();
//            Assert.IsTrue(us.IndexOf("Write locks = 0") >= 0);
//            Assert.IsTrue(us.IndexOf("Read locks = 0") >= 0);
//            lock_Renamed.WriterLock.Lock();
//            String ws = lock_Renamed.ToString();
//            Assert.IsTrue(ws.IndexOf("Write locks = 1") >= 0);
//            Assert.IsTrue(ws.IndexOf("Read locks = 0") >= 0);
//            lock_Renamed.WriterLock.Unlock();
//            lock_Renamed.ReaderLock.Lock();
//            lock_Renamed.ReaderLock.Lock();
//            String rs = lock_Renamed.ToString();
//            Assert.IsTrue(rs.IndexOf("Write locks = 0") >= 0);
//            Assert.IsTrue(rs.IndexOf("Read locks = 2") >= 0);
//        }


//        [Test] public void ReadLockToString()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            String us = lock_Renamed.ReaderLock.ToString();
//            Assert.IsTrue(us.IndexOf("Read locks = 0") >= 0);
//            lock_Renamed.ReaderLock.Lock();
//            lock_Renamed.ReaderLock.Lock();

//            String rs = lock_Renamed.ReaderLock.ToString();
//            Assert.IsTrue(rs.IndexOf("Read locks = 2") >= 0);
//        }


//        [Test] public void WriteLockToString()
//        {
//            ReentrantReadWriteLock lock_Renamed = new ReentrantReadWriteLock();

//            String us = lock_Renamed.WriterLock.ToString();
//            Assert.IsTrue(us.IndexOf("Unlocked") >= 0);
//            lock_Renamed.WriterLock.Lock();

//            String ls = lock_Renamed.WriterLock.ToString();
//            Assert.IsTrue(ls.IndexOf("Locked") >= 0);
//        }
//    }
//}