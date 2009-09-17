using System;
using NUnit.Framework;
using Spring.Collections;
using Spring.Collections.Generic;
using Spring.TestFixture.Collections;
using Spring.TestFixture.Collections.NonGeneric;
using Spring.TestFixture.Threading.Collections.Generic;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for no fair <see cref="ArrayBlockingQueue{T}"/> as a generic
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ArrayBlockingQueueTest<T>
    {
        private readonly T[] _c = new T[1];

        [Test] public void ConstructorChokesOnNegativeCapacity()
        {
            AssertChokesOnNagativeCapacityArgument(() => new ArrayBlockingQueue<T>(-1));
            AssertChokesOnNagativeCapacityArgument(() => new ArrayBlockingQueue<T>(-1, true));
            AssertChokesOnNagativeCapacityArgument(() => new ArrayBlockingQueue<T>(-1, true, _c));
        }

        [Test] public void ConstructorChokesOnNullCollection()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new ArrayBlockingQueue<T>(1, true, null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void ConstructorDefaultToNofair()
        {
            Assert.IsFalse(new ArrayBlockingQueue<T>(1).IsFair);
        }

        [Test] public void IsFairReturnsTheValueSetInConstructor()
        {
            Assert.IsTrue(new ArrayBlockingQueue<T>(1, true).IsFair);
            Assert.IsFalse(new ArrayBlockingQueue<T>(1, false).IsFair);
            Assert.IsTrue(new ArrayBlockingQueue<T>(1, true, _c).IsFair);
            Assert.IsFalse(new ArrayBlockingQueue<T>(1, false, _c).IsFair);
        }

        [Test] public void CapacityReturnsTheValueSetInConstructor()
        {
            Assert.That(new ArrayBlockingQueue<T>(5).Capacity, Is.EqualTo(5));
            Assert.That(new ArrayBlockingQueue<T>(8, true).Capacity, Is.EqualTo(8));
            Assert.That(new ArrayBlockingQueue<T>(10, false, _c).Capacity, Is.EqualTo(10));
        }

        private static void AssertChokesOnNagativeCapacityArgument(TestDelegate action)
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(action);
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
        }

        [TestFixture(typeof(int), CollectionOptions.Fair)]
        [TestFixture(typeof(int))]
        [TestFixture(typeof(string), CollectionOptions.Fair)]
        [TestFixture(typeof(string))]
        public class AsGeneric : BlockingQueueTestFixture<T>
        {
            public AsGeneric() : this(0) {}
            public AsGeneric(CollectionOptions options)
                : base(options | CollectionOptions.Fifo | CollectionOptions.ToStringPrintItems) { }

            protected override IBlockingQueue<T> NewBlockingQueue()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, IsFair);
            }

            protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, IsFair, TestData<T>.MakeTestArray(_sampleSize));
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
            public AsNonGeneric(CollectionOptions options)
                : base(options | CollectionOptions.Fifo)
            {
                _isFair = options.Has(CollectionOptions.Fair);
            }

            protected override IQueue NewQueue()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair);
            }

            protected override IQueue NewQueueFilledWithSample()
            {
                return new ArrayBlockingQueue<T>(_sampleSize, _isFair, TestData<T>.MakeTestArray(_sampleSize));
            }

            [Test] public void BlockingQueueIsSynchronized()
            {
                Assert.IsTrue(NewQueue().IsSynchronized);
            }
        }
    }
}
