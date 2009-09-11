using System;
using NUnit.Framework;
using CollectionOptions = Spring.Collections.CollectionOptions;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="LinkedBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string), CollectionOptions.Unbounded)]
    [TestFixture(typeof(int), CollectionOptions.Unbounded)]
    [TestFixture(typeof(string), CollectionOptions.Bounded)]
    [TestFixture(typeof(int), CollectionOptions.Bounded)]
    public class LinkedBlockingQueueTest<T> : BlockingQueueTestFixture<T>
    {
        public LinkedBlockingQueueTest(CollectionOptions attributes)
            : base(attributes | CollectionOptions.Fifo)
        {
        }

        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return NewLinkedBlockingQueue(_isCapacityRestricted, _sampleSize, false);
        }

        protected sealed override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueueFilledWithSample();
        }

        protected virtual LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueue(_isCapacityRestricted, _sampleSize, true);
        }

        internal static LinkedBlockingQueue<T> NewLinkedBlockingQueue(bool isCapacityRestricted, int size, bool isFilled)
        {
            LinkedBlockingQueue<T> sut = isCapacityRestricted ? 
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
            SkipIfBoundedQueue();
            var queue = new LinkedBlockingQueue<T>();
            Assert.AreEqual(int.MaxValue, queue.RemainingCapacity);
            Assert.AreEqual(int.MaxValue, queue.Capacity);
        }

        [Test]
        public void ConstructorWelcomesNullElememtInCollectionArgument()
        {
            SkipIfBoundedQueue();
            T[] arrayWithDefaulValue = new T[_sampleSize];
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
            SkipIfBoundedQueue();
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void ConstructorCreatesQueueConstainsAllElementsInCollection()
        {
            SkipIfBoundedQueue();
            var q = new LinkedBlockingQueue<T>(_samples);
            foreach (T sample in _samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test]
        public void ConstructorCreatesQueueWithGivenCapacity()
        {
            SkipIfUnboundedQueue();
            var queue = new LinkedBlockingQueue<T>(_sampleSize);
            Assert.AreEqual(_sampleSize, queue.RemainingCapacity);
            Assert.AreEqual(_sampleSize, queue.Capacity);
        }

        [Test]
        public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            SkipIfUnboundedQueue();
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => { new LinkedBlockingQueue<T>(capacity); });
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
            Assert.That(e.ActualValue, Is.EqualTo(capacity));
        }

        [Test] public void ToArrayWritesAllElementsToNewArray() 
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
            T[] o = q.ToArray();
            for(int i = 0; i < o.Length; i++)
                Assert.AreEqual(o[i], q.Take());
        }

        [Test] public void ToArrayWritesAllElementsToExistingArray() 
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
            T[] ints = new T[_sampleSize];
            ints = q.ToArray(ints);
            for(int i = 0; i < ints.Length; i++)
                Assert.AreEqual(ints[i], q.Take());
        }

        [Test] public void ToArrayChokesOnNullArray() 
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
            var e = Assert.Throws<ArgumentNullException>(()=>q.ToArray(null));
            Assert.That(e.ParamName, Is.EqualTo("targetArray"));
        }

        [Test] public void ToArrayChokesOnIncompatibleArray()
        {
            LinkedBlockingQueue<object> q = new LinkedBlockingQueue<object>(2);
            q.Offer(new object());
            Assert.Throws<ArrayTypeMismatchException>(() => q.ToArray(new string[10]));
        }

        [Test] public void ToArrayWorksFineWithArrayOfSubType()
        {
            LinkedBlockingQueue<object> q = new LinkedBlockingQueue<object>(2);
            q.Offer(TestData<string>.One);
            var a = new string[10];
            q.ToArray(a);
            Assert.That(a[0], Is.EqualTo(TestData<string>.One));
        }

        [Test] public void ToArrayExpendsShorterArray()
        {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
            var a = new T[0];
            var a2 = q.ToArray(a);
            CollectionAssert.AreEqual(q, a2);
        }

        [Test] public void ToArrayExpendsShorterArrayWithSameType()
        {
            LinkedBlockingQueue<object> q = new LinkedBlockingQueue<object>();
            q.Offer(TestData<string>.One);
            var a = new string[0];
            var a2 = q.ToArray(a);
            CollectionAssert.AreEqual(q, a2);
            Assert.That(a2, Is.InstanceOf<string[]>());
        }

        [Test] public void ToStringContainsToStringOfElements() {
            LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
            string s = q.ToString();
            for (int i = 0; i < _sampleSize; ++i) {
                Assert.IsTrue(s.IndexOf(_samples[i].ToString()) >= 0);
            }
        }

        [TestFixture(typeof(string), CollectionOptions.Unbounded)]
        [TestFixture(typeof(int), CollectionOptions.Unbounded)]
        [TestFixture(typeof(string), CollectionOptions.Bounded)]
        [TestFixture(typeof(int), CollectionOptions.Bounded)]
        public class AsNonGeneric : Spring.Collections.TypedQueueTestFixture<T>
        {
            public AsNonGeneric(CollectionOptions options) 
                : base(options | CollectionOptions.Fifo) {}

            protected override Spring.Collections.IQueue NewQueue()
            {
                return NewLinkedBlockingQueue(_isCapacityRestricted, _sampleSize, false);
            }

            protected override Spring.Collections.IQueue NewQueueFilledWithSample()
            {
                return NewLinkedBlockingQueue(_isCapacityRestricted, _sampleSize, true);
            }
        }

    }
}
