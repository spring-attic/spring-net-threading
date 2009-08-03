using System;
using System.Threading;
using NUnit.Framework;
using System.Collections.Generic;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for fair <see cref="SynchronousQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class SynchronousQueueFairTest<T> : SynchronousQueueTestBase<T>
    {
        public SynchronousQueueFairTest()
        {
            _isFair = true;
        }
        protected override SynchronousQueue<T> NewSynchronousQueue()
        {
            return new SynchronousQueue<T>(true);
        }
    }

    /// <summary>
    /// Test cases for no fair <see cref="SynchronousQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class SynchronousQueueNoFairTest<T> : SynchronousQueueTestBase<T>
    {
        protected override SynchronousQueue<T>  NewSynchronousQueue()
        {
            return new SynchronousQueue<T>();
        }
    }

    public abstract class SynchronousQueueTestBase<T> : BlockingQueueTestFixture<T>
    {
        protected SynchronousQueueTestBase()
        {
            _isCapacityRestricted = true;
            _sampleSize = 0;
            _isFifoQueue = true;
        }

        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return NewSynchronousQueue();
        }

        protected abstract SynchronousQueue<T> NewSynchronousQueue();

        [Test] public override void EnumeratorFailsWhenCollectionIsModified()
        {
            SkipForCurrentQueueImplementation();
        }

        [Test] public override void RemoveOnlyOneOfDuplicatesWhenSupported()
        {
            SkipForCurrentQueueImplementation();
        }

        [Test] public override void DrainToEmptiesFullQueueAndUnblocksWaitingPut()
        {
            var q = NewSynchronousQueue();
            ThreadManager.StartAndAssertRegistered("T1", () => q.Put(TestData<T>.Zero));
            ThreadManager.StartAndAssertRegistered("T2", () => q.Put(TestData<T>.One));
            var l = new List<T>();
            Thread.Sleep(TestData.ShortDelay);
            q.DrainTo(l);
            Assert.That(l.Count, Is.EqualTo(2));
            CollectionAssert.Contains(l, TestData<T>.Zero);
            CollectionAssert.Contains(l, TestData<T>.One);
            ThreadManager.JoinAndVerify();
        }

        [Test] public override void SelectiveDrainToMovesSelectedElementsIntoCollection()
        {
            var q = NewSynchronousQueue();
            var t = ThreadManager.StartAndAssertRegistered("T1",
                () => {
                    try { q.Put(TestData<T>.Zero); }
                    catch (ThreadInterruptedException){} // ignore
                });
            var l = new List<T>();
            Thread.Sleep(TestData.ShortDelay);
            q.DrainTo(l, e=>true);
            Assert.That(l.Count, Is.EqualTo(0));
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test]
        public void SynchronousQueueIsBothEmptyAndFull()
        {
            var q = NewSynchronousQueue();
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(q.Capacity, Is.EqualTo(0));
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            Assert.IsFalse(q.Offer(TestData<T>.One));
        }

        [Test]
        public void AddRangeChokesWhenNoActiveTaker()
        {
            var q = NewSynchronousQueue();
            Assert.Throws<InvalidOperationException>(
                () => q.AddRange(TestData<T>.MakeTestArray(1)));
        }

        [Test]
        public void OfferSucceedsWithActiveTaker()
        {
            var q = NewSynchronousQueue();
            T[] values = new T[2];
            ThreadManager.StartAndAssertRegistered(
                "T1", () => q.Poll(TestData.SmallDelay, out values[0]));
            ThreadManager.StartAndAssertRegistered(
                "T1", () => q.Poll(TestData.SmallDelay, out values[1]));
            Thread.Sleep(TestData.ShortDelay);
            q.Offer(TestData<T>.One);
            q.Offer(TestData<T>.Two);
            ThreadManager.JoinAndVerify();
            if (_isFair)
            {
                Assert.That(values[0], Is.EqualTo(TestData<T>.One));
                Assert.That(values[1], Is.EqualTo(TestData<T>.Two));
            }
            else
            {
                CollectionAssert.Contains(values, TestData<T>.One);
                CollectionAssert.Contains(values, TestData<T>.Two);
            }
        }

        [Test]
        public void TimedPollGetsElementsInExpectedOrderOfActivePutter()
        {
            var q = NewSynchronousQueue();
            const int size = 2;
            var position = new AtomicInteger(0);
            for (int i = 0; i < size; i++)
            {
                int index = i;
                T e = TestData<T>.MakeData(i);
                ThreadManager.StartAndAssertRegistered(
                    "T" + i, () =>
                                 {
                                     while (position.CompareAndSet(index, index + 1)) Thread.Sleep(1);
                                     q.Offer(e, TestData.SmallDelay);
                                 });
            }
            Thread.Sleep(TestData.ShortDelay);
            T[] values = new T[size];
            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(q.Poll(TestData.ShortDelay, out values[i]));
            }
            ThreadManager.JoinAndVerify();
            for (int i = 0; i < size; i++)
            {

                if (_isFair)
                {
                    Assert.That(values[i], Is.EqualTo(TestData<T>.MakeData(i)));
                }
                else
                {
                    CollectionAssert.Contains(values, TestData<T>.MakeData(i));
                }
            }
        }
    }
}