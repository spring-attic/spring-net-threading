using System;
using NUnit.Framework;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="LinkedBlockingQueue{T}"/>
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class LinkedBlockingQueueTest<T>
    {
        private int _sampleSize = 9;
        private T[] _samples;

        TestThreadManager ThreadManager { get; set; }

        [SetUp] public void SetUp()
        {
            _samples = TestData<T>.MakeTestArray(_sampleSize);
            ThreadManager = new TestThreadManager();
        }

        [TearDown] public void TearDown()
        {
            ThreadManager.TearDown();
        }

        /// <summary>
        /// Create a queue of given size containing objects of type 
        /// <typeparamref name="T"/> converted from consecutive integer 
        /// 0 ... n-1.
        /// </summary>
        private LinkedBlockingQueue<T> PopulatedQueue(int n)
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(n);
            Assert.IsTrue(q.IsEmpty);
            for (int i = 0; i < n; i++)
                Assert.IsTrue(q.Offer(TestData<T>.MakeData(i)));
            Assert.IsFalse(q.IsEmpty);
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.AreEqual(n, q.Count);
            return q;
        }

        [Test] public void ConstructorCreatesQueueWithGivenCapacityOrMaxInteger()
        {
            Assert.AreEqual(_sampleSize, new LinkedBlockingQueue<T>(_sampleSize).RemainingCapacity);
            Assert.AreEqual(int.MaxValue, new LinkedBlockingQueue<T>().RemainingCapacity);
        }

        [Test] public void ConstructorChokesOnNonPositiveCapacityArgument([Values(0, -1)] int capacity)
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => { new LinkedBlockingQueue<T>(capacity); });
            Assert.That(e.ParamName, Is.EqualTo("capacity"));
            Assert.That(e.ActualValue, Is.EqualTo(capacity));
        }

        [Test] public void ConstructorChokesOnNullCollectionArgument()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => { new LinkedBlockingQueue<T>(null); });
            Assert.That(e.ParamName,Is.EqualTo("collection"));
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

        [Test] public void TransitionsFromEmptyToFullWhenElementsAdded()
        {
            var q = new LinkedBlockingQueue<T>(2);
            Assert.IsTrue(q.IsEmpty);
            Assert.AreEqual(2, q.RemainingCapacity, "should have room for 2");
            q.Add(_samples[0]);
            Assert.IsFalse(q.IsEmpty);
            q.Add(_samples[1]);
            Assert.IsFalse(q.IsEmpty);
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.IsFalse(q.Offer(_samples[2]));
        }

		[Test] public void ToArrayWritesAllElementsToNewArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T[] o = q.ToArray();
			for(int i = 0; i < o.Length; i++)
				Assert.AreEqual(o[i], q.Take());
		}

		[Test] public void ToArrayWritesAllElementsToExistingArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			T[] ints = new T[_sampleSize];
			ints = q.ToArray(ints);
				for(int i = 0; i < ints.Length; i++)
					Assert.AreEqual(ints[i], q.Take());
		}

		[Test] public void ToArrayChokesOnNullArray() 
        {
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
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
			LinkedBlockingQueue<T> q =PopulatedQueue(_sampleSize);
			String s = q.ToString();
			for (int i = 0; i < _sampleSize; ++i) {
				Assert.IsTrue(s.IndexOf(_samples[i].ToString()) >= 0);
			}
		}

    }
}