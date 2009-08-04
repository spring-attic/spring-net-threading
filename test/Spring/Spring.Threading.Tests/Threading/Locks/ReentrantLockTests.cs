using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Locks
{
    /// <summary>
    /// Test cases for <see cref="ReentrantLock"/>.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu (Interlocked)</author>
    [TestFixture]
    public class ReentrantLockTests : ThreadingTestFixture
    {
        private ReentrantLock _lock;

        private class UninterruptableThread
        {
            public readonly Thread InternalThread;
            private readonly ICondition _condition;
            private readonly ReentrantLock _myLock;

            public volatile bool CanAwake;
            public volatile bool Interrupted;
            public volatile bool LockStarted;

            public UninterruptableThread(ReentrantLock myLock, ICondition c)
            {
                _myLock = myLock;
                _condition = c;
                InternalThread = new Thread(Run);
            }

            private void Run()
            {
                using(_myLock.Lock())
                {
                    LockStarted = true;

                    while (!CanAwake)
                    {
                        _condition.AwaitUninterruptibly();
                    }
                    Interrupted = TestThreadManager.IsCurrentThreadInterrupted();
                }
            }
        }

        [Serializable]
        private class PublicReentrantLock : ReentrantLock
        {
            internal PublicReentrantLock(bool fair) : base(fair)
            {
            }

            public ICollection<Thread> GetWaitingThreadsPublic(ICondition c)
            {
                return base.GetWaitingThreads(c);
            }
        }

        [Test] public void DefaultConstructorIsNotFair()
        {
            Assert.IsFalse(new ReentrantLock().IsFair);
        }

        [Test] public void ConstructorSetsGivenFairness()
        {
            Assert.IsTrue(new ReentrantLock(true).IsFair);
            Assert.IsFalse(new ReentrantLock(false).IsFair);
        }

        [Test] public void LockIsNotInterruptibe([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.Lock();
            Thread t1 = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        _lock.Lock();
                        Assert.IsTrue(TestThreadManager.IsCurrentThreadInterrupted());
                        Assert.IsTrue(_lock.IsLocked);
                        Assert.IsTrue(_lock.IsHeldByCurrentThread);
                        _lock.Unlock();
                    });
            t1.Interrupt();
            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(t1.IsAlive);
            Assert.IsTrue(_lock.IsLocked);
            Assert.IsTrue(_lock.IsHeldByCurrentThread);
            _lock.Unlock();
            ThreadManager.JoinAndVerify();
        }

        [Test] public void LockSucceedsOnUnlockedLock([Values(true, false)] bool isFair)
        {
            ReentrantLock rl = new ReentrantLock(isFair);
            rl.Lock();
            Assert.IsTrue(rl.IsLocked);
            rl.Unlock();
        }

        [Test] public void UnlockChokesWhenAttemptOnUnownedLock([Values(true, false)] bool isFair)
        {
            ReentrantLock rl = new ReentrantLock(isFair);
            Assert.Throws<SynchronizationLockException>(rl.Unlock);
        }

        [Test] public void TryLockSucceedsOnUnlockedLock([Values(true, false)] bool isFair)
        {
            ReentrantLock rl = new ReentrantLock(isFair);
            Assert.IsTrue(rl.TryLock());
            Assert.IsTrue(rl.IsLocked);
            rl.Unlock();
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void HasQueuedThreadsReportsExistenceOfWaitingThreads(bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            Assert.IsFalse(_lock.HasQueuedThreads);
            _lock.Lock();
            Thread t1 = ThreadManager.StartAndAssertRegistered("T1", InterruptedLock);
            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.HasQueuedThreads);
            Thread t2 = ThreadManager.StartAndAssertRegistered("T2", InterruptibleLock);
            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.HasQueuedThreads);
            t1.Interrupt();
            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.HasQueuedThreads);
            _lock.Unlock();
            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.HasQueuedThreads);

            ThreadManager.JoinAndVerify(t1, t2);
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetQueueLengthReportsNumberOfWaitingThreads(bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            Assert.AreEqual(0, _lock.QueueLength);
            _lock.Lock();
            Thread t1 = ThreadManager.StartAndAssertRegistered("T1", InterruptedLock);

            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(1, _lock.QueueLength);
            Thread t2 = ThreadManager.StartAndAssertRegistered("T2", InterruptibleLock);

            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(2, _lock.QueueLength);
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(1, _lock.QueueLength);
            _lock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(0, _lock.QueueLength);

            ThreadManager.JoinAndVerify(t1, t2);
        }

        [TestCase(true, ExpectedException = typeof(ArgumentNullException))]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void IsQueuedThreadChokesOnNullParameter(bool isFair)
        {
            ReentrantLock sync = new ReentrantLock(isFair);
            sync.IsQueuedThread(null);
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void IsQueuedThreadReportsWhetherThreadIsQueued(bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            Thread t1 = ThreadManager.NewVerifiableThread(InterruptedLock, "T1");
            Thread t2 = ThreadManager.NewVerifiableThread(InterruptibleLock, "T2");
            Assert.IsFalse(_lock.IsQueuedThread(t1));
            Assert.IsFalse(_lock.IsQueuedThread(t2));
            _lock.Lock();
            t1.Start();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.IsQueuedThread(t1));
            t2.Start();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.IsQueuedThread(t1));
            Assert.IsTrue(_lock.IsQueuedThread(t2));
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.IsQueuedThread(t1));
            Assert.IsTrue(_lock.IsQueuedThread(t2));
            _lock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.IsQueuedThread(t1));

            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.IsQueuedThread(t2));

            ThreadManager.JoinAndVerify(t1, t2);
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetQueuedThreadsIncludeWaitingThreads(bool isFair)
        {
            _lock = new PublicReentrantLock(isFair);

            Assert.That(_lock.QueuedThreads.Count, Is.EqualTo(0));
            _lock.Lock();
            Assert.IsTrue((_lock.QueuedThreads.Count == 0));
            Thread t1 = ThreadManager.StartAndAssertRegistered("T1", InterruptedLock);

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.QueuedThreads.Contains(t1));
            Thread t2 = ThreadManager.StartAndAssertRegistered("T2", InterruptibleLock);

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.QueuedThreads.Contains(t1));
            Assert.IsTrue(_lock.QueuedThreads.Contains(t2));
            t1.Interrupt();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.QueuedThreads.Contains(t1));
            Assert.IsTrue(_lock.QueuedThreads.Contains(t2));
            _lock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue((_lock.QueuedThreads.Count == 0));

            ThreadManager.JoinAndVerify(t1, t2);
        }

        [Test] public void TimedTryLockIsInterruptible([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.Lock();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                () => Assert.Throws<ThreadInterruptedException>(
                          () => _lock.TryLock(MEDIUM_DELAY)));

            t.Interrupt();
            ThreadManager.JoinAndVerify(t);
        }

        [Test] public void TimedTryLockAllSucceedsFromMultipleThreads([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.LockInterruptibly();
            ThreadStart action =
                delegate
                    {
                        Assert.IsTrue(_lock.TryLock(LONG_DELAY));
                        try
                        {
                            Thread.Sleep(SHORT_DELAY);
                        }
                        finally
                        {
                            _lock.Unlock();
                        }

                    };
            ThreadManager.StartAndAssertRegistered("T", action, action);
            Assert.IsTrue(_lock.IsLocked);
            Assert.IsTrue(_lock.IsHeldByCurrentThread);
            _lock.Unlock();
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TryLockChokesWhenIsLocked([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.Lock();
            ThreadManager.StartAndAssertRegistered(
                "T1", delegate { Assert.IsFalse(_lock.TryLock());});
            ThreadManager.JoinAndVerify();
            _lock.Unlock();
        }

        [Test] public void TimedTryLockTimesOutOnLockedLock([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.Lock();
            ThreadManager.StartAndAssertRegistered(
                "T1",
                () => Assert.IsFalse(_lock.TryLock(TimeSpan.FromMilliseconds(1))));
            ThreadManager.JoinAndVerify();
            _lock.Unlock();
        }

        [Test] public void GetHoldCountReturnsNumberOfRecursiveHolds([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            RecursiveHolds(delegate { _lock.Lock(); });
            RecursiveHolds((delegate
            {
                Assert.IsTrue(_lock.TryLock());
            }));
            RecursiveHolds(delegate
            {
                Assert.IsTrue(_lock.TryLock(SHORT_DELAY));
            });
            RecursiveHolds(delegate { _lock.LockInterruptibly(); });
        }

        private void RecursiveHolds(Action lockTask)
        {
            for (int i = 1; i <= DEFAULT_COLLECTION_SIZE; i++)
            {
                lockTask();
                Assert.AreEqual(i, _lock.HoldCount);
            }
            for (int i = DEFAULT_COLLECTION_SIZE; i > 0; i--)
            {
                _lock.Unlock();
                Assert.AreEqual(i - 1, _lock.HoldCount);
            }
        }

        [Test] public void IsLockedReportsIfLockOwnedByAnyThread([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.Lock();
            Assert.IsTrue(_lock.IsLocked);
            _lock.Unlock();
            Assert.IsFalse(_lock.IsLocked);
            Thread t1 = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        _lock.Lock();
                        Assert.Throws<ThreadInterruptedException>(()=>Thread.Sleep(LONG_DELAY));
                        _lock.Unlock();
                    });

            Thread.Sleep(SHORT_DELAY);
            Assert.IsTrue(_lock.IsLocked);
            t1.Interrupt();
            ThreadManager.JoinAndVerify();
            Assert.IsFalse(_lock.IsLocked);
        }

        [Test] public void LockInterruptiblySucceedsWhenUnlockedElseIsInterruptible([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.LockInterruptibly();
            var threads = ThreadManager.StartAndAssertRegistered("T", 
                InterruptedLock,
                delegate { using (_lock.LockInterruptibly()) { } }, 
                delegate { using (_lock.LockInterruptibly()) { } });
            threads[0].Interrupt();
            ThreadManager.JoinAndVerify(threads[0]);
            Assert.IsTrue(_lock.IsLocked);
            Assert.IsTrue(_lock.IsHeldByCurrentThread);
            _lock.Unlock();
            ThreadManager.JoinAndVerify();
        }

        [Test, ExpectedException(typeof(SynchronizationLockException))]
        public void AwaitChokesWhenLockIsNotOwned([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            c.Await();
        }

        [Test, ExpectedException(typeof(SynchronizationLockException))]
        public void SignalChokesWhenLockIsNotOwned([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            c.Signal();
        }

        [Test] public void AwaitTimeoutInNanosWithoutSignal([Values(true, false)] bool isFair)
        {
            TimeSpan timeToWait = new TimeSpan(1);
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            _lock.Lock();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool result = c.Await(timeToWait);
            sw.Stop();
            Assert.IsFalse(result);
            Assert.That(sw.Elapsed, Is.Not.LessThan(timeToWait));
            _lock.Unlock();
        }

        [Test] public void AwaitTimeoutWithoutSignal([Values(true, false)] bool isFair)
        {
            TimeSpan timeToWait = SHORT_DELAY;
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            _lock.Lock();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool result = c.Await(timeToWait);
            sw.Stop();
            Assert.IsFalse(result);
            Assert.That(sw.Elapsed, Is.Not.LessThan(timeToWait));
            _lock.Unlock();
        }

        [Test] public void AwaitUntilTimeoutWithoutSignal([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            _lock.Lock();
            DateTime until = DateTime.Now.AddMilliseconds(10);
            bool result = c.AwaitUntil(until);
            Assert.That(DateTime.Now, Is.GreaterThanOrEqualTo(until));
            Assert.IsFalse(result);
            _lock.Unlock();
        }

        [Test] public void AwaitReturnsWhenSignalled([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            ThreadManager.StartAndAssertRegistered(
                "T1",
                () => { using (_lock.Lock()) c.Await(); });

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            c.Signal();
            _lock.Unlock();
            ThreadManager.JoinAndVerify();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void HasWaitersChokesOnNullParameter([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.HasWaiters(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetWaitQueueLengthChokesOnNullParameter([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            _lock.GetWaitQueueLength(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetWaitingThreadsChokesOnNullParameter([Values(true, false)] bool isFair)
        {
            PublicReentrantLock myLock = new PublicReentrantLock(isFair);
            myLock.GetWaitingThreadsPublic(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void HasWaitersChokesWhenNotOwned([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            ReentrantLock lock2 = new ReentrantLock(isFair);
            lock2.HasWaiters(c);
        }

        [TestCase(true, ExpectedException = typeof(SynchronizationLockException))]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void HasWaitersChokesWhenNotLocked(bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            _lock.HasWaiters(c);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void GetWaitQueueLengthChokesWhenNotOwned([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = (_lock.NewCondition());
            ReentrantLock lock2 = new ReentrantLock(isFair);
            lock2.GetWaitQueueLength(c);
        }

        [TestCase(true, ExpectedException = typeof(SynchronizationLockException))]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetWaitQueueLengthChokesWhenNotLocked(bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = (_lock.NewCondition());
            _lock.GetWaitQueueLength(c);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void GetWaitingThreadsChokesWhenNotOwned([Values(true, false)] bool isFair)
        {
            PublicReentrantLock myLock = new PublicReentrantLock(isFair);
            ICondition c = (myLock.NewCondition());
            PublicReentrantLock lock2 = new PublicReentrantLock(isFair);
            lock2.GetWaitingThreadsPublic(c);
        }

        [TestCase(true, ExpectedException = typeof(SynchronizationLockException))]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetWaitingThreadsChokesWhenNotLocked(bool isFair)
        {
            PublicReentrantLock myLock = new PublicReentrantLock(isFair);
            ICondition c = (myLock.NewCondition());
            myLock.GetWaitingThreadsPublic(c);
        }

        [Test] public void HasWaitersReturnTrueWhenThreadIsWaitingElseFalse([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(true);
            ICondition c = _lock.NewCondition();
            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        using (_lock.Lock())
                        {
                            Assert.IsFalse(_lock.HasWaiters(c));
                            Assert.That(_lock.GetWaitQueueLength(c), Is.EqualTo(0));
                            c.Await();
                        }
                    });

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            Assert.IsTrue(_lock.HasWaiters(c));
            Assert.AreEqual(1, _lock.GetWaitQueueLength(c));
            c.Signal();
            _lock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            Assert.IsFalse(_lock.HasWaiters(c));
            Assert.AreEqual(0, _lock.GetWaitQueueLength(c));
            _lock.Unlock();
            ThreadManager.JoinAndVerify();
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetWaitQueueLengthReturnsNumberOfThreads(bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            ICondition c = _lock.NewCondition();
            ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        using (_lock.Lock())
                        {
                            Assert.IsFalse(_lock.HasWaiters(c));
                            Assert.AreEqual(0, _lock.GetWaitQueueLength(c));
                            c.Await();
                        }
                    });

            Thread.Sleep(SHORT_DELAY);
            ThreadManager.StartAndAssertRegistered(
                "T2",
                delegate
                    {
                        using (_lock.Lock())
                        {
                            Assert.IsTrue(_lock.HasWaiters(c));
                            Assert.AreEqual(1, _lock.GetWaitQueueLength(c));
                            c.Await();
                        }
                    });

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            Assert.IsTrue(_lock.HasWaiters(c));
            Assert.AreEqual(2, _lock.GetWaitQueueLength(c));
            c.SignalAll();
            _lock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            Assert.IsFalse(_lock.HasWaiters(c));
            Assert.AreEqual(0, _lock.GetWaitQueueLength(c));
            _lock.Unlock();

            ThreadManager.JoinAndVerify();
        }

        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void GetWaitingThreadsIncludesWaitingThreads(bool isFair)
        {
            PublicReentrantLock myLock = new PublicReentrantLock(isFair);

            ICondition c = myLock.NewCondition();

            myLock.Lock();
            Assert.That(myLock.GetWaitingThreadsPublic(c).Count, Is.EqualTo(0));
            myLock.Unlock();
            Thread t1 = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                {
                    myLock.Lock();
                    Assert.That(myLock.GetWaitingThreadsPublic(c).Count, Is.EqualTo(0));
                    c.Await();
                    myLock.Unlock();
                });

            Thread.Sleep(SHORT_DELAY);
            Thread t2 = ThreadManager.StartAndAssertRegistered(
                "T2",
                delegate
                {
                    myLock.Lock();
                    Assert.That(myLock.GetWaitingThreadsPublic(c).Count, Is.Not.EqualTo(0));
                    c.Await();
                    myLock.Unlock();
                });

            Thread.Sleep(SHORT_DELAY);
            myLock.Lock();
            Assert.IsTrue(myLock.HasWaiters(c));
            Assert.IsTrue(myLock.GetWaitingThreadsPublic(c).Contains(t1));
            Assert.IsTrue(myLock.GetWaitingThreadsPublic(c).Contains(t2));
            c.SignalAll();
            myLock.Unlock();

            Thread.Sleep(SHORT_DELAY);
            myLock.Lock();
            Assert.IsFalse(myLock.HasWaiters(c));
            Assert.IsTrue((myLock.GetWaitingThreadsPublic(c).Count == 0));
            myLock.Unlock();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void AwaitUninterruptiblyCannotBeInterrupted([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            UninterruptableThread thread = new UninterruptableThread(_lock, c);

            thread.InternalThread.Start();

            while (!thread.LockStarted)
            {
                Thread.Sleep(100);
            }

            _lock.Lock();
            try
            {
                thread.InternalThread.Interrupt();
                thread.CanAwake = true;
                c.Signal();
            }
            finally
            {
                _lock.Unlock();
            }

            ThreadManager.JoinAndVerify(thread.InternalThread);
            Assert.IsTrue(thread.Interrupted);
        }

        [Test] public void AwaitIsInterruptible([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            ICondition c = _lock.NewCondition();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                () => Assert.Throws<ThreadInterruptedException>(
                          () => { using (_lock.Lock()) c.Await(); }));

            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void AwaitNanosIsInterruptible([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            ICondition c = _lock.NewCondition();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                () => Assert.Throws<ThreadInterruptedException>(
                          () => { using (_lock.Lock()) c.Await(new TimeSpan(0, 0, 0, 1)); }));

            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void AwaitUntilIsInterruptible([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);

            ICondition c = _lock.NewCondition();
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", 
                () => Assert.Throws<ThreadInterruptedException>(
                          () => { using (_lock.Lock()) c.AwaitUntil(DateTime.Now.AddMilliseconds(10000)); }));

            Thread.Sleep(SHORT_DELAY);
            t.Interrupt();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void SignalAllWakesUpAllThreads([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            ThreadStart cAwait = delegate { using (_lock.Lock()) c.Await(); };

            ThreadManager.StartAndAssertRegistered("T", cAwait, cAwait);

            Thread.Sleep(SHORT_DELAY);
            _lock.Lock();
            c.SignalAll();
            _lock.Unlock();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void AwaitAfterMultipleReentrantLockingPreservesLockCount([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            ICondition c = _lock.NewCondition();
            ThreadManager.StartAndAssertRegistered(
                "T",
                delegate
                    {
                        using (_lock.Lock())
                        {
                            Assert.That(_lock.HoldCount, Is.EqualTo(1));
                            c.Await();
                            Assert.That(_lock.HoldCount, Is.EqualTo(1));
                        }
                    },
                delegate
                    {
                        using (_lock.Lock())
                        using (_lock.Lock())
                        {
                            Assert.That(_lock.HoldCount, Is.EqualTo(2));
                            c.Await();
                            Assert.That(_lock.HoldCount, Is.EqualTo(2));
                        }
                    });

            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(_lock.IsLocked);
            using(_lock.Lock()) c.SignalAll();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void SerializationDeserializesAsUnlocked([Values(true, false)] bool isFair)
        {
            ReentrantLock l = new ReentrantLock(isFair);
            l.Lock();
            l.Unlock();
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            ReentrantLock r = (ReentrantLock)formatter2.Deserialize(bin);
            r.Lock();
            r.Unlock();
        }

        [Test] public void ToStringVerification([Values(true, false)] bool isFair)
        {
            _lock = new ReentrantLock(isFair);
            StringAssert.Contains("Unlocked", _lock.ToString());
            using (_lock.Lock())
            {
                StringAssert.Contains("Locked by thread", _lock.ToString());
            }
            StringAssert.Contains("Unlocked", _lock.ToString());
        }

        #region Private Methods

        private void InterruptedLock()
        {
            Assert.Throws<ThreadInterruptedException>(delegate
            {
                _lock.LockInterruptibly();
            });
        }

        private void InterruptibleLock()
        {
            try
            {
                _lock.LockInterruptibly();
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        #endregion
    }
}