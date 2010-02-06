using System;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.TestFixtures.Collections;
using Spring.TestFixtures.Collections.NonGeneric;
using Spring.TestFixtures.Threading.Collections.Generic;
using Spring.Threading.AtomicTypes;
#if !PHASED
using IQueue = Spring.Collections.IQueue;
#else
using IQueue = System.Collections.ICollection;
#endif

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="LinkedBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string), CollectionContractOptions.Bounded)]
    [TestFixture(typeof(int), CollectionContractOptions.Bounded)]
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueTest<T> : BlockingQueueContract<T>
    {
        private const CollectionContractOptions _defaultContractOptions =
            CollectionContractOptions.Fifo |
            CollectionContractOptions.ToStringPrintItems |
            CollectionContractOptions.WeaklyConsistentEnumerator;

        public LinkedBlockingQueueTest() : this(0) {}
        public LinkedBlockingQueueTest(CollectionContractOptions options)
            : base(options | _defaultContractOptions) { }

        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return NewLinkedBlockingQueue(IsBounded, SampleSize, false);
        }

        protected sealed override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueueFilledWithSample();
        }

        protected virtual LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueue(IsBounded, SampleSize, true);
        }

        protected virtual LinkedBlockingQueue<T> NewLinkedBlockingQueue()
        {
            return NewLinkedBlockingQueue(IsBounded, SampleSize, false);
        }

        internal static LinkedBlockingQueue<T> NewLinkedBlockingQueue(bool isBounded, int size, bool isFilled)
        {
            LinkedBlockingQueue<T> sut = isBounded ? 
                new LinkedBlockingQueue<T>(size) : new LinkedBlockingQueue<T>();

            if (isFilled)
            {
                sut.AddRange(TestData<T>.MakeTestArray(size));
            }
            return sut;
        }

        [Test]
        public void ConstructorCreatesQueueWithUnlimitedCapacity()
        {
            Options.SkipWhen(CollectionContractOptions.Bounded);
            var queue = new LinkedBlockingQueue<T>();
            Assert.AreEqual(int.MaxValue, queue.RemainingCapacity);
            Assert.AreEqual(int.MaxValue, queue.Capacity);
        }

        [Test]
        public void ConstructorWelcomesNullElememtInCollectionArgument()
        {
            Options.SkipWhen(CollectionContractOptions.Bounded);
            T[] arrayWithDefaulValue = new T[SampleSize];
            var q = new LinkedBlockingQueue<T>(arrayWithDefaulValue);
            foreach (T sample in arrayWithDefaulValue)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test]
        public void ConstructorChokesOnNullCollectionArgument()
        {
            Options.SkipWhen(CollectionContractOptions.Bounded);
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void ConstructorCreatesQueueConstainsAllElementsInCollection()
        {
            Options.SkipWhen(CollectionContractOptions.Bounded);
            var q = new LinkedBlockingQueue<T>(Samples);
            foreach (T sample in Samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test]
        public void ConstructorCreatesQueueWithGivenCapacity()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var queue = new LinkedBlockingQueue<T>(SampleSize);
            Assert.AreEqual(SampleSize, queue.RemainingCapacity);
            Assert.AreEqual(SampleSize, queue.Capacity);
        }

        [Test]
        public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => { new LinkedBlockingQueue<T>(capacity); });
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
            Assert.That(e.ActualValue, Is.EqualTo(capacity));
        }

        [Test] public void IsBrokenIndicatesIfQueueIsBroken()
        {
            var q = NewLinkedBlockingQueue();
            Assert.IsFalse(q.IsBroken);
            q.Break();
            Assert.IsTrue(q.IsBroken);
            q.Clear();
            Assert.IsFalse(q.IsBroken);
        }

        [Test] public void StopBreaksAndEmptiesQueue()
        {
            var q = NewLinkedBlockingQueueFilledWithSample();
            Assert.That(q.Count, Is.GreaterThan(0));
            Assert.IsFalse(q.IsBroken);
            q.Stop();
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.IsTrue(q.IsBroken);
        }

        [Test] public void TimedOfferReturnsFalseWhenQueueIsBroken()
        {
            var q = NewLinkedBlockingQueue();
            q.Break();
            Assert.IsFalse(q.Offer(TestData<T>.One, Delays.Short));
        }

        [Test] public void BlockedTimedOfferReturnsWhenQueueIsBroken()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var q = NewLinkedBlockingQueueFilledWithSample();
            var isOfferReturned = new AtomicBoolean();
            ThreadManager.StartAndAssertRegistered(
                "T1", () =>
                {
                    Assert.IsFalse(q.Offer(TestData<T>.One, Delays.Long));
                    isOfferReturned.Value = true;
                });
            Thread.Sleep(Delays.Short);
            Assert.IsFalse(isOfferReturned);
            q.Break();
            Thread.Sleep(Delays.Short);
            Assert.IsTrue(isOfferReturned);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TryPutReturnFalseWhenQueueIsBroken()
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueue();
            q.Break();
            Assert.IsFalse(q.TryPut(TestData<T>.One));
        }

        [Test] public void PutChokesWhenQueueIsBroken()
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueue();
            q.Break();
            Assert.Throws<QueueBrokenException>(()=>q.Put(TestData<T>.One));
        }

        [Test] public void BlockedTryPutPuturnsFalseWhenQueueIsBroken()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var q = NewLinkedBlockingQueueFilledWithSample();
            var isTryPutReturned = new AtomicBoolean();
            ThreadManager.StartAndAssertRegistered(
                "T1", () =>
                          {
                              Assert.IsFalse(q.TryPut(TestData<T>.One));
                              isTryPutReturned.Value = true;
                          });
            Thread.Sleep(Delays.Short);
            Assert.IsFalse(isTryPutReturned);
            q.Break();
            Thread.Sleep(Delays.Short);
            Assert.IsTrue(isTryPutReturned);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void BlockedTryPutPuturnsFalseWhenQueueIsStopped()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var q = NewLinkedBlockingQueueFilledWithSample();
            var isTryPutReturned = new AtomicBoolean();
            ThreadManager.StartAndAssertRegistered(
                "T1", () =>
                          {
                              Assert.IsFalse(q.TryPut(TestData<T>.One));
                              isTryPutReturned.Value = true;
                          });
            Thread.Sleep(Delays.Short);
            Assert.IsFalse(isTryPutReturned);
            q.Stop();
            Thread.Sleep(Delays.Short);
            Assert.IsTrue(isTryPutReturned);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TryTakeReturnsFalseOnlyWhenQueueIsBrokenAndEmpty()
        {
            var q = NewLinkedBlockingQueue();
            var one = TestData<T>.One;
            q.Put(one);
            q.Break();
            T result;
            Assert.IsTrue(q.TryTake(out result));
            Assert.That(result, Is.EqualTo(one));
            Assert.IsFalse(q.TryTake(out result));
        }

        [Test] public void TakeChokesOnlyWhenQueueIsBrokenAndEmpty()
        {
            var q = NewLinkedBlockingQueue();
            var one = TestData<T>.One;
            q.Put(one);
            q.Break();
            Assert.That(q.Take(), Is.EqualTo(one));
            Assert.Throws<QueueBrokenException>(()=>q.Take());
        }

        [Test] public void BlockedTryTakePuturnsWhenQueueIsBroken()
        {
            Options.SkipWhenNot(CollectionContractOptions.Bounded);
            var q = NewLinkedBlockingQueue();
            var isTryTakeReturned = new AtomicBoolean();
            ThreadManager.StartAndAssertRegistered(
                "T1", () =>
                {
                    T result;
                    Assert.IsFalse(q.TryTake(out result));
                    isTryTakeReturned.Value = true;
                });
            Thread.Sleep(Delays.Short);
            Assert.IsFalse(isTryTakeReturned);
            q.Break();
            Thread.Sleep(Delays.Short);
            Assert.IsTrue(isTryTakeReturned);
            ThreadManager.JoinAndVerify();
        }

        [TestFixture(typeof(string), CollectionContractOptions.Bounded)]
        [TestFixture(typeof(int), CollectionContractOptions.Bounded)]
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsNonGeneric : TypedQueueContract<T>
        {
            public AsNonGeneric() :  this(0) {}
            public AsNonGeneric(CollectionContractOptions options) 
                : base(options | _defaultContractOptions) {}

            protected override IQueue NewQueue()
            {
                return NewLinkedBlockingQueue(IsBounded, SampleSize, false);
            }

            protected override IQueue NewQueueFilledWithSample()
            {
                return NewLinkedBlockingQueue(IsBounded, SampleSize, true);
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsAbstractCollection : AbstractCollectionContract<T>
        {
            /// <summary>
            /// Return a new empty <see cref="AbstractCollection{TE}"/>.
            /// </summary>
            /// <returns></returns>
            protected override AbstractCollection<TE> NewCollection<TE>()
            {
                return new LinkedBlockingQueue<TE>(SampleSize);
            }
        }
    }
}
