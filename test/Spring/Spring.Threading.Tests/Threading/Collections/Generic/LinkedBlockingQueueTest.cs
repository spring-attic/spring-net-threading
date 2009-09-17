using System;
using NUnit.Framework;
using Spring.TestFixture.Collections;
using Spring.TestFixture.Collections.NonGeneric;
using Spring.TestFixture.Threading.Collections.Generic;

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
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueTest<T> : BlockingQueueTestFixture<T>
    {
        public LinkedBlockingQueueTest() : this(0) {}
        public LinkedBlockingQueueTest(CollectionOptions options)
            : base(options | CollectionOptions.Fifo) {}

        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return NewLinkedBlockingQueue(IsUnbounded, _sampleSize, false);
        }

        protected sealed override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueueFilledWithSample();
        }

        protected virtual LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueue(IsUnbounded, _sampleSize, true);
        }

        internal static LinkedBlockingQueue<T> NewLinkedBlockingQueue(bool isUnbounded, int size, bool isFilled)
        {
            LinkedBlockingQueue<T> sut = isUnbounded ? 
                new LinkedBlockingQueue<T>() : new LinkedBlockingQueue<T>(size);

            if (isFilled)
            {
                sut.AddRange(TestData<T>.MakeTestArray(size));
            }
            return sut;
        }

        [Test]
        public void ConstructorCreatesQueueWithUnlimitedCapacity()
        {
            Options.SkipWhenNot(CollectionOptions.Unbounded);
            var queue = new LinkedBlockingQueue<T>();
            Assert.AreEqual(int.MaxValue, queue.RemainingCapacity);
            Assert.AreEqual(int.MaxValue, queue.Capacity);
        }

        [Test]
        public void ConstructorWelcomesNullElememtInCollectionArgument()
        {
            Options.SkipWhenNot(CollectionOptions.Unbounded);
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
            Options.SkipWhenNot(CollectionOptions.Unbounded);
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void ConstructorCreatesQueueConstainsAllElementsInCollection()
        {
            Options.SkipWhenNot(CollectionOptions.Unbounded);
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
            Options.SkipWhen(CollectionOptions.Unbounded);
            var queue = new LinkedBlockingQueue<T>(_sampleSize);
            Assert.AreEqual(_sampleSize, queue.RemainingCapacity);
            Assert.AreEqual(_sampleSize, queue.Capacity);
        }

        [Test]
        public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            Options.SkipWhen(CollectionOptions.Unbounded);
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
            var q = NewCollectionFilledWithSample();
            string s = q.ToString();
            for (int i = 0; i < _sampleSize; ++i) {
                Assert.IsTrue(s.IndexOf(_samples[i].ToString()) >= 0);
            }
        }

        [TestFixture(typeof(string), CollectionOptions.Unbounded)]
        [TestFixture(typeof(int), CollectionOptions.Unbounded)]
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsNonGeneric : TypedQueueTestFixture<T>
        {
            public AsNonGeneric() :  this(0) {}
            public AsNonGeneric(CollectionOptions options) 
                : base(options | CollectionOptions.Fifo) {}

            protected override Spring.Collections.IQueue NewQueue()
            {
                return NewLinkedBlockingQueue(IsUnbounded, _sampleSize, false);
            }

            protected override Spring.Collections.IQueue NewQueueFilledWithSample()
            {
                return NewLinkedBlockingQueue(IsUnbounded, _sampleSize, true);
            }
        }

    }
}
