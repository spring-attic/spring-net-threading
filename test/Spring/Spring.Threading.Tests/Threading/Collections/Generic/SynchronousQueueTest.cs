using System;
using System.Threading;
using NUnit.Framework;
using System.Collections.Generic;
using Spring.Collections;
using Spring.TestFixture.Collections;
using Spring.TestFixture.Collections.NonGeneric;
using Spring.TestFixture.Threading.Collections.Generic;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="SynchronousQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int), CollectionOptions.Fair)]
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string), CollectionOptions.Fair)]
    [TestFixture(typeof(string))]
    public class SynchronousQueueTest<T> : BlockingQueueTestFixture<T>
    {
        public SynchronousQueueTest() : this(0) {}
        public SynchronousQueueTest(CollectionOptions options) 
            : base(options | CollectionOptions.Fifo)
        {
            _sampleSize = 0;
        }

        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return NewSynchronousQueue();
        }

        private SynchronousQueue<T> NewSynchronousQueue()
        {
            return IsFair ? new SynchronousQueue<T>(true) : new SynchronousQueue<T>();
        }

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
            Thread.Sleep(TestData.ShortDelay);
            ThreadManager.StartAndAssertRegistered(
                "T2", () => q.Poll(TestData.SmallDelay, out values[1]));
            Thread.Sleep(TestData.ShortDelay);
            q.Offer(TestData<T>.One);
            q.Offer(TestData<T>.Two);
            ThreadManager.JoinAndVerify();
            if (IsFair)
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
            for (int i = 0; i < size; i++)
            {
                if(i>0) Thread.Sleep(TestData.ShortDelay);
                T e = TestData<T>.MakeData(i);
                ThreadManager.StartAndAssertRegistered(
                    "T" + i, () => q.Offer(e, TestData.MediumDelay));
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

                if (IsFair)
                {
                    Assert.That(values[i], Is.EqualTo(TestData<T>.MakeData(i)));
                }
                else
                {
                    CollectionAssert.Contains(values, TestData<T>.MakeData(i));
                }
            }
        }

        [TestFixture(typeof(int), CollectionOptions.Fair)]
        [TestFixture(typeof(int))]
        [TestFixture(typeof(string), CollectionOptions.Fair)]
        [TestFixture(typeof(string))]
        public class AsNonGeneric : TypedQueueTestFixture<T>
        {
            private readonly bool _isFair;

            public AsNonGeneric() : this(0) {}
            public AsNonGeneric(CollectionOptions options) : base(options)
            {
                _isFair = options.Has(CollectionOptions.Fair);
                _sampleSize = 0;
            }
            protected override IQueue NewQueue()
            {
                return _isFair ? new SynchronousQueue<T>(true) : new SynchronousQueue<T>();
            }
        }
    }
}