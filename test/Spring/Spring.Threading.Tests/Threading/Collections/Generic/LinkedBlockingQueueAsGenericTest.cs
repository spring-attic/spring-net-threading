using System;
using NUnit.Framework;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Functional test case for bounded <see cref="LinkedBlockingQueue{T}"/> as a generic
    /// <see cref="IBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueBoundAsGenericTest<T> : LinkedBlockingQueueTestBase<T>
    {
        public LinkedBlockingQueueBoundAsGenericTest()
        {
            _isCapacityRestricted = true;
            _isFifoQueue = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new LinkedBlockingQueue<T>(_sampleSize);
        }

        protected override LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample()
        {
            var sut = new LinkedBlockingQueue<T>(_sampleSize);
            sut.AddRange(TestData<T>.MakeTestArray(_sampleSize));
            return sut;
        }

        [Test] public void ConstructorCreatesQueueWithGivenCapacity()
        {
            var queue = new LinkedBlockingQueue<T>(_sampleSize);
            Assert.AreEqual(_sampleSize, queue.RemainingCapacity);
            Assert.AreEqual(_sampleSize, queue.Capacity);
        }

        [Test] public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => { new LinkedBlockingQueue<T>(capacity); });
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
            Assert.That(e.ActualValue, Is.EqualTo(capacity));
        }

    }

    /// <summary>
    /// Functional test case for unbounded <see cref="LinkedBlockingQueue{T}"/> as a generic
    /// <see cref="IBlockingQueue{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueUnboundAsGenericTest<T> : LinkedBlockingQueueTestBase<T>
    {
        public LinkedBlockingQueueUnboundAsGenericTest()
        {
            _isCapacityRestricted = false;
            _isFifoQueue = true;
        }
        protected override IBlockingQueue<T> NewBlockingQueue()
        {
            return new LinkedBlockingQueue<T>();
        }

        protected override LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample()
        {
            return new LinkedBlockingQueue<T>(TestData<T>.MakeTestArray(_sampleSize));
        }

        [Test] public void ConstructorCreatesQueueWithUnlimitedCapacity()
        {
            var queue = new LinkedBlockingQueue<T>();
            Assert.AreEqual(int.MaxValue, queue.RemainingCapacity);
            Assert.AreEqual(int.MaxValue, queue.Capacity);
        }

        [Test] public void ConstructorWelcomesNullElememtInCollectionArgument()
        {
            T[] arrayWithDefaulValue = new T[_sampleSize];
            var q = new LinkedBlockingQueue<T>(arrayWithDefaulValue);
            foreach (T sample in arrayWithDefaulValue)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

        [Test] public void ConstructorChokesOnNullCollectionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName,Is.EqualTo("collection"));
        }

        [Test] public void ConstructorCreatesQueueConstainsAllElementsInCollection()
        {
            var q = new LinkedBlockingQueue<T>(_samples);
            foreach (T sample in _samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

    }

    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    public abstract class LinkedBlockingQueueTestBase<T> : BlockingQueueTestFixture<T>
    {
        protected abstract LinkedBlockingQueue<T> NewLinkedBlockingQueueFilledWithSample();

        protected sealed override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
        {
            return NewLinkedBlockingQueueFilledWithSample();
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

		[Test] public void ToStringContainsToStringOfElements() {
		    LinkedBlockingQueue<T> q = NewLinkedBlockingQueueFilledWithSample();
			string s = q.ToString();
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(s.IndexOf(_samples[i].ToString()) >= 0);
			}
		}

    }
}
