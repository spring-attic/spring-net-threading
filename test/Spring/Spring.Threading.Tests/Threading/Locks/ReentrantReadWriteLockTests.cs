using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Locks
{
    [TestFixture]
    public class ReentrantReadWriteLockTests : BaseThreadingTestCase
    {
        private class AnonymousClassRunnable : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.LockInterruptibly();
                    myLock.WriterLock.Unlock();
                    myLock.WriterLock.LockInterruptibly();
                    myLock.WriterLock.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable1 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable1(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.TryLock(new TimeSpan(0, 0, 0, 0, 1000));
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable2 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable2(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.ReaderLock.LockInterruptibly();
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable3 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable3(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.ReaderLock.TryLock(new TimeSpan(0, 0, 0, 0, 1000));
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable4 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable4(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                Debug.Assert(!myLock.WriterLock.TryLock());
            }

            #endregion
        }

        private class AnonymousClassRunnable5 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable5(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                Debug.Assert(!myLock.ReaderLock.TryLock());
            }

            #endregion
        }

        private class AnonymousClassRunnable6 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable6(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                Debug.Assert(myLock.ReaderLock.TryLock());
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable7 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable7(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.ReaderLock.Lock();
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable8 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable8(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.WriterLock.Lock();
                myLock.WriterLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable9 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable9(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.ReaderLock.Lock();
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable10 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable10(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.ReaderLock.Lock();
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable11 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable11(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.ReaderLock.Lock();
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable12 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable12(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                myLock.ReaderLock.Lock();
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable13 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable13(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                Debug.Assert(myLock.ReaderLock.TryLock());
                myLock.ReaderLock.Unlock();
            }

            #endregion
        }

        private class AnonymousClassRunnable14 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable14(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                Debug.Assert(!myLock.WriterLock.TryLock());
            }

            #endregion
        }

        private class AnonymousClassRunnable15 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable15(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    Debug.Assert(!myLock.WriterLock.TryLock(new TimeSpan()));
                }
                catch (Exception)
                {
                    Assert.Fail("Should not throw an exception");
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable16 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable16(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    Debug.Assert(!myLock.ReaderLock.TryLock(new TimeSpan(0, 0, 0, 0, 1)));
                }
                catch (Exception)
                {
                    Assert.Fail("Should not throw an exception");
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable17 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable17(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.LockInterruptibly();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable18 : IRunnable
        {
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable18(ReentrantReadWriteLock myLock)
            {
                this.myLock = myLock;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.ReaderLock.LockInterruptibly();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable19 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable19(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.Await();
                    myLock.WriterLock.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    Assert.Fail("Should not throw an exception.");
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable20 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable20(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.Await();
                    myLock.WriterLock.Unlock();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable21 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable21(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.Await(MEDIUM_DELAY_MS);
                    myLock.WriterLock.Unlock();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable22 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable22(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.AwaitUntil(DateTime.Now.AddMilliseconds(10000));
                    myLock.WriterLock.Unlock();
                    Assert.Fail("Should throw an exception");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable23 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable23(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.Await();
                    myLock.WriterLock.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    Assert.Fail("Should not throw an exception.");
                }
            }

            #endregion
        }

        private class AnonymousClassRunnable24 : IRunnable
        {
            private readonly ICondition c;
            private readonly ReentrantReadWriteLock myLock;

            public AnonymousClassRunnable24(ReentrantReadWriteLock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.Lock();
                    c.Await();
                    myLock.WriterLock.Unlock();
                }
                catch (ThreadInterruptedException)
                {
                    Assert.Fail("Should not throw an exception.");
                }
            }

            #endregion
        }

        internal class InterruptibleLockRunnable : IRunnable
        {
            internal ReentrantReadWriteLock myLock;

            internal InterruptibleLockRunnable(ReentrantReadWriteLock l)
            {
                myLock = l;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.LockInterruptibly();
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }


        internal class InterruptedLockRunnable : IRunnable
        {
            internal ReentrantReadWriteLock myLock;

            internal InterruptedLockRunnable(ReentrantReadWriteLock l)
            {
                myLock = l;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    myLock.WriterLock.LockInterruptibly();
                    Assert.Fail("Should throw an exception.");
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            #endregion
        }

        internal class UninterruptableThread : IRunnable
        {
            public readonly Thread InternalThread;
            private readonly ICondition c;
            private readonly ILock myLock;

            public volatile bool canAwake;
            public volatile bool interrupted;
            public volatile bool lockStarted;

            public UninterruptableThread(ILock myLock, ICondition c)
            {
                this.myLock = myLock;
                this.c = c;
                InternalThread = new Thread(Run);
            }
            #region IRunnable Members

            public void Run()
            {
                myLock.Lock();
                lockStarted = true;
                while (!canAwake)
                {
                    c.AwaitUninterruptibly();
                }

                interrupted = InternalThread.IsAlive;
                myLock.Unlock();
            }

            #endregion
        }

        [Serializable]
        internal class PublicReentrantReadWriteLock2 : ReentrantReadWriteLock
        {
            internal PublicReentrantReadWriteLock2()
            {
            }

            protected PublicReentrantReadWriteLock2(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [Test]
        public void Await()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            var t = new Thread(new AnonymousClassRunnable19(myLock, c).Run);

            t.Start();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            myLock.WriterLock.Lock();
            c.Signal();
            myLock.WriterLock.Unlock();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }

        [Test]
        public void Await_IllegalMonitor()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            try
            {
                c.Await();
                Assert.Fail("Should throw an exception.");
            }

            catch (SynchronizationLockException)
            {
            }
        }

        [Test]
        public void Await_Interrupt()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            var t = new Thread(new AnonymousClassRunnable20(myLock, c).Run);

            t.Start();

            Thread.Sleep(SHORT_DELAY_MS.Milliseconds);
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }

        [Test]
        public void Await_Timeout()
        {
            var myLock = new ReentrantReadWriteLock();

            var c = myLock.WriterLock.NewCondition();
            myLock.WriterLock.Lock();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void AwaitNanos_Interrupt()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            var t = new Thread(new AnonymousClassRunnable21(myLock, c).Run);

            t.Start();

            Thread.Sleep(new TimeSpan((Int64) 10000*SHORT_DELAY_MS.Milliseconds));
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }

        [Test]
        public void AwaitNanos_Timeout()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            myLock.WriterLock.Lock();
            Assert.IsTrue(c.Await(new TimeSpan(1)));
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void AwaitUninterruptibly()
        {
            var myLock = new ReentrantReadWriteLock();

            var c = myLock.WriterLock.NewCondition();
            var thread = new UninterruptableThread(myLock.WriterLock, c);

            thread.InternalThread.Start();

            while (!thread.lockStarted)
            {
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
            }

            myLock.WriterLock.Lock();
            try
            {
                thread.InternalThread.Interrupt();
                thread.canAwake = true;
                c.Signal();
            }
            finally
            {
                myLock.WriterLock.Unlock();
            }

            thread.InternalThread.Join();
            Assert.IsTrue(thread.interrupted);
            Assert.IsFalse(thread.InternalThread.IsAlive);
        }

        [Test]
        public void AwaitUntil_Interrupt()
        {
            var myLock = new ReentrantReadWriteLock();

            var c = myLock.WriterLock.NewCondition();
            var t = new Thread(new AnonymousClassRunnable22(myLock, c).Run);

            t.Start();

            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds));
            t.Interrupt();
            t.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
        }

        [Test]
        public void AwaitUntil_Timeout()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            myLock.WriterLock.Lock();
            DateTime d = DateTime.Now;
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void Constructor()
        {
            var rl = new ReentrantReadWriteLock();
            Assert.IsFalse(rl.IsFair);
            Assert.IsFalse(rl.IsWriteLockHeld);
            Assert.AreEqual(0, rl.ReadLockCount);
        }

        [Test]
        public void GetQueueLength()
        {
            var myLock = new ReentrantReadWriteLock();
            var t1 = new Thread(new InterruptedLockRunnable(myLock).Run);
            var t2 = new Thread(new InterruptibleLockRunnable(myLock).Run);
            Assert.AreEqual(0, myLock.QueueLength);
            myLock.WriterLock.Lock();
            t1.Start();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            Assert.AreEqual(1, myLock.QueueLength);
            t2.Start();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            Assert.AreEqual(2, myLock.QueueLength);
            t1.Interrupt();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            Assert.AreEqual(1, myLock.QueueLength);
            myLock.WriterLock.Unlock();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            Assert.AreEqual(0, myLock.QueueLength);
            t1.Join();
            t2.Join();
        }


        [Test]
        public void GetWriteHoldCount()
        {
            var myLock = new ReentrantReadWriteLock();
            for (int i = 1; i <= DEFAULT_COLLECTION_SIZE; i++)
            {
                myLock.WriterLock.Lock();
                Assert.AreEqual(i, myLock.WriteHoldCount);
            }
            for (int i = DEFAULT_COLLECTION_SIZE; i > 0; i--)
            {
                myLock.WriterLock.Unlock();
                Assert.AreEqual(i - 1, myLock.WriteHoldCount);
            }
        }

        [Test]
        public void Lock()
        {
            var rl = new ReentrantReadWriteLock();
            rl.WriterLock.Lock();
            Assert.IsTrue(rl.IsWriteLockHeld);
            Assert.IsTrue(rl.WriterLockedByCurrentThread);
//        assertTrue(((ReentrantReadWriteLock.WriteLock)rl.writeLock()).isHeldByCurrentThread());
            Assert.AreEqual(0, rl.ReadLockCount);
            rl.WriterLock.Unlock();
            Assert.IsFalse(rl.IsWriteLockHeld);
            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//        assertFalse(((ReentrantReadWriteLock.WriteLock)rl.writeLock()).isHeldByCurrentThread());
            Assert.AreEqual(0, rl.ReadLockCount);
            rl.ReaderLock.Lock();
            Assert.IsFalse(rl.IsWriteLockHeld);
            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//        assertFalse(((ReentrantReadWriteLock.WriteLock)rl.writeLock()).isHeldByCurrentThread());
            Assert.AreEqual(1, rl.ReadLockCount);
            rl.ReaderLock.Unlock();
            Assert.IsFalse(rl.IsWriteLockHeld);
            Assert.IsFalse(rl.WriterLockedByCurrentThread);
//        assertFalse(((ReentrantReadWriteLock.WriteLock)rl.writeLock()).isHeldByCurrentThread());
            Assert.AreEqual(0, rl.ReadLockCount);
        }

//    public void testGetHoldCount() {
//        ReentrantReadWriteLock lock = new ReentrantReadWriteLock();
//        for(int i = 1; i <= SIZE; i++) {
//            lock.writeLock().lock();
//            assertEquals(i,((ReentrantReadWriteLock.WriteLock)lock.writeLock()).getHoldCount());
//        }
//        for(int i = SIZE; i > 0; i--) {
//            lock.writeLock().unlock();
//            assertEquals(i-1,((ReentrantReadWriteLock.WriteLock)lock.writeLock()).getHoldCount());
//        }
//    }


        [Test]
        public void MultipleReadLocks()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.ReaderLock.Lock();
            var t = new Thread(new AnonymousClassRunnable6(myLock).Run);
            t.Start();
            t.Join();
            myLock.ReaderLock.Unlock();
        }


        [Test]
        public void ReadAfterWriterLock()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t1 = new Thread(new AnonymousClassRunnable9(myLock).Run);
            var t2 = new Thread(new AnonymousClassRunnable10(myLock).Run);

            t1.Start();
            t2.Start();
            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds*10000));
            myLock.WriterLock.Unlock();
            t1.Join(MEDIUM_DELAY_MS);
            t2.Join(MEDIUM_DELAY_MS);
            Assert.IsTrue(!t1.IsAlive);
            Assert.IsTrue(!t2.IsAlive);
        }


        [Test]
        public void ReadHoldingWriteLock2()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t1 = new Thread(new AnonymousClassRunnable11(myLock).Run);
            var t2 = new Thread(new AnonymousClassRunnable12(myLock).Run);

            t1.Start();
            t2.Start();
            myLock.ReaderLock.Lock();
            myLock.ReaderLock.Unlock();
            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds*10000));
            myLock.ReaderLock.Lock();
            myLock.ReaderLock.Unlock();
            myLock.WriterLock.Unlock();
            t1.Join(MEDIUM_DELAY_MS);
            t2.Join(MEDIUM_DELAY_MS);
            Assert.IsTrue(!t1.IsAlive);
            Assert.IsTrue(!t2.IsAlive);
        }

        [Test]
        public void ReadHoldingWriterLock()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            Assert.IsTrue(myLock.ReaderLock.TryLock());
            myLock.ReaderLock.Unlock();
            myLock.WriterLock.Unlock();
        }


        [Test]
        public void ReadLockInterruptibly()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.LockInterruptibly();
            var t = new Thread(new AnonymousClassRunnable18(myLock).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void ReadLockInterruptibly_Interrupted()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable2(myLock).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            Thread.Sleep(SHORT_DELAY_MS);
            myLock.WriterLock.Unlock();
            t.Join();
        }

        [Test]
        public void ReadLockToString()
        {
            var myLock = new ReentrantReadWriteLock();

            String us = myLock.ReaderLock.ToString();
            Assert.IsTrue(us.IndexOf("Read locks = 0") >= 0);
            myLock.ReaderLock.Lock();
            myLock.ReaderLock.Lock();

            String rs = myLock.ReaderLock.ToString();
            Assert.IsTrue(rs.IndexOf("Read locks = 2") >= 0);
        }

        [Test]
        public void ReadTryLock_Interrupted()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable3(myLock).Run);
            t.Start();
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void ReadTryLock_Timeout()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable16(myLock).Run);
            t.Start();
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void ReadTryLockWhenLocked()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable5(myLock).Run);
            t.Start();
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void Serialization()
        {
            var l = new ReentrantReadWriteLock();
            l.ReaderLock.Lock();
            l.ReaderLock.Unlock();

            var bout = new MemoryStream(10000);

            var formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            var bin = new MemoryStream(bout.ToArray());
            var formatter2 = new BinaryFormatter();
            var r = (ReentrantReadWriteLock) formatter2.Deserialize(bin);
            r.ReaderLock.Lock();
            r.ReaderLock.Unlock();
        }


        [Test]
        public void Signal_IllegalMonitor()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            try
            {
                c.Signal();
                Assert.Fail("Should throw an exception.");
            }

            catch (SynchronizationLockException)
            {
            }
        }


        [Test]
        public void SignalAll()
        {
            var myLock = new ReentrantReadWriteLock();

            ICondition c = myLock.WriterLock.NewCondition();
            var t1 = new Thread(new AnonymousClassRunnable23(myLock, c).Run);

            var t2 = new Thread(new AnonymousClassRunnable24(myLock, c).Run);

            t1.Start();
            t2.Start();

            Thread.Sleep(new TimeSpan(10000*SHORT_DELAY_MS.Milliseconds));
            myLock.WriterLock.Lock();
            c.SignalAll();
            myLock.WriterLock.Unlock();
            t1.Join(SHORT_DELAY_MS);
            t2.Join(SHORT_DELAY_MS);
            Assert.IsFalse(t1.IsAlive);
            Assert.IsFalse(t2.IsAlive);
        }


        [Test]
        public void ToStringTest()
        {
            var myLock = new ReentrantReadWriteLock();
            String us = myLock.ToString();
            Assert.IsTrue(us.IndexOf("Write locks = 0") >= 0);
            Assert.IsTrue(us.IndexOf("Read locks = 0") >= 0);
            myLock.WriterLock.Lock();
            String ws = myLock.ToString();
            Assert.IsTrue(ws.IndexOf("Write locks = 1") >= 0);
            Assert.IsTrue(ws.IndexOf("Read locks = 0") >= 0);
            myLock.WriterLock.Unlock();
            myLock.ReaderLock.Lock();
            myLock.ReaderLock.Lock();
            String rs = myLock.ToString();
            Assert.IsTrue(rs.IndexOf("Write locks = 0") >= 0);
            Assert.IsTrue(rs.IndexOf("Read locks = 2") >= 0);
        }

        [Test]
        public void TryLockWhenReadLocked()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.ReaderLock.Lock();
            var t = new Thread(new AnonymousClassRunnable13(myLock).Run);
            t.Start();
            t.Join();
            myLock.ReaderLock.Unlock();
        }

        [Test]
        public void Unlock_IllegalMonitorStateException()
        {
            var rl = new ReentrantReadWriteLock();
            try
            {
                rl.WriterLock.Unlock();
                Assert.Fail("Should throw an exception.");
            }
            catch (SynchronizationLockException)
            {
            }
        }

        [Test]
        public void WriteAfterMultipleReadLocks()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.ReaderLock.Lock();
            var t1 = new Thread(new AnonymousClassRunnable7(myLock).Run);
            var t2 = new Thread(new AnonymousClassRunnable8(myLock).Run);

            t1.Start();
            t2.Start();
            Thread.Sleep(new TimeSpan(SHORT_DELAY_MS.Milliseconds*10000));
            myLock.ReaderLock.Unlock();
            t1.Join(MEDIUM_DELAY_MS);
            t2.Join(MEDIUM_DELAY_MS);
            Assert.IsTrue(!t1.IsAlive);
            Assert.IsTrue(!t2.IsAlive);
        }

        [Test]
        public void WriteLockInterruptibly()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.LockInterruptibly();
            var t = new Thread(new AnonymousClassRunnable17(myLock).Run);
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void WriteLockInterruptibly_Interrupted()
        {
            var myLock = new ReentrantReadWriteLock();
            var t = new Thread(new AnonymousClassRunnable(myLock).Run);
            myLock.WriterLock.Lock();
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            Thread.Sleep(SHORT_DELAY_MS);
            myLock.WriterLock.Unlock();
            t.Join();
        }


        [Test]
        public void WriteLockToString()
        {
            var myLock = new ReentrantReadWriteLock();

            String us = myLock.WriterLock.ToString();
            Assert.IsTrue(us.IndexOf("Unlocked") >= 0);
            myLock.WriterLock.Lock();

            String ls = myLock.WriterLock.ToString();
            Assert.IsTrue(ls.IndexOf("Locked") >= 0);
        }

        [Test]
        public void WriteTryLock_Interrupted()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable1(myLock).Run);
            t.Start();
            t.Interrupt();
            myLock.WriterLock.Unlock();
            t.Join();
        }

        [Test]
        public void WriteTryLock_Timeout()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable15(myLock).Run);
            t.Start();
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void WriteTryLockWhenLocked()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.WriterLock.Lock();
            var t = new Thread(new AnonymousClassRunnable4(myLock).Run);
            t.Start();
            t.Join();
            myLock.WriterLock.Unlock();
        }

        [Test]
        public void WriteTryLockWhenReadLocked()
        {
            var myLock = new ReentrantReadWriteLock();
            myLock.ReaderLock.Lock();
            var t = new Thread(new AnonymousClassRunnable14(myLock).Run);
            t.Start();
            t.Join();
            myLock.ReaderLock.Unlock();
        }
    }
}