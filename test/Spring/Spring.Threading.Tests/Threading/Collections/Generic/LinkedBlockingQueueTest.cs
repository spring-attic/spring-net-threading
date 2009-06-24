using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Collections.Generic
{
    [TestFixture]
    public class LinkedBlockingQueueTest {
        [Test]
        public void TestPollOnEmptyQueue() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            string retval;
            Assert.IsFalse(queue.Poll(TimeSpan.Zero, out retval));
        }

        [Test]
        public void TestEnumerator() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            Assert.That(queue.Count, Is.EqualTo(3));

            int count = 0;
            foreach(String s in queue) {
                ++count;
            }

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void TestEnumeratorWithConcurrentTakeBetweenMoveAndGetCurrent() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            iter.MoveNext();
            queue.Take();
            Assert.That(iter.Current, Is.EqualTo("test1"));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestEnumeratorWithChangingCollectionBetweenTwoMoves() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            iter.MoveNext();
            queue.Take();
            iter.MoveNext();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestEnumeratorWithChangingCollectionBeforeRest() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            queue.Take();
            iter.Reset();
        }
    }

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

        [Test] public void RemainingCapacityDecreasesOnAddIncreasesOnRemove()
        {
            var q = PopulatedQueue(_sampleSize);
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.AreEqual(i, q.RemainingCapacity);
                Assert.AreEqual(_sampleSize - i, q.Count);
                q.Remove();
            }
            for (int i = 0; i < _sampleSize; ++i)
            {
                Assert.AreEqual(_sampleSize - i, q.RemainingCapacity);
                Assert.AreEqual(i, q.Count);
                q.Add(_samples[i]);
            }
        }

        [Test] public void OfferWelcomesNull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            q.Offer(default(T));
        }

        [Test] public void AddWelcomesNull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            q.Add(default(T));
        }

        [Test] public void OfferSucceedsIfNotFullFailsIfFull()
        {
            var q = new LinkedBlockingQueue<T>(1);
            Assert.IsTrue(q.Offer(_samples[0]));
            Assert.IsFalse(q.Offer(_samples[1]));
        }

        [Test] public void AddSucceedsIfNotFullChokesIfFull()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(_sampleSize);
            for (int i = 0; i < _sampleSize; ++i)
            {
                q.Add(_samples[i]);
            }
            Assert.AreEqual(0, q.RemainingCapacity);
            Assert.Throws<InvalidOperationException>(
                () => q.Add(TestData<T>.MakeData(_sampleSize)));
        }

        [Test] public void AddRangeChokesOnNullArgument() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(1);
            var e = Assert.Throws<ArgumentNullException>(()=>q.AddRange(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeChokesWhenAddItself()
        {
            LinkedBlockingQueue<T> q = PopulatedQueue(_sampleSize);
            var e = Assert.Throws<ArgumentException>(()=>q.AddRange(q));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeWelcomesNullElements() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            T[] arrayWithDefaulValue = new T[_sampleSize];
            q.AddRange(arrayWithDefaulValue);
            foreach (T sample in arrayWithDefaulValue)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

    	[Test] public void AddRangeChokesWhenNotEnoughRoom() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(1);
            Assert.Throws<InvalidOperationException>(()=>q.AddRange(_samples));
    	}

    	[Test] public void AddRangeAddAllElementsInTraversalOrder() {
            T[] empty = new T[0];
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            Assert.IsFalse(q.AddRange(empty));
            Assert.IsTrue(q.AddRange(_samples));
            foreach (T sample in _samples)
            {
                T value;
                Assert.IsTrue(q.Poll(out value));
                Assert.That(value, Is.EqualTo(sample));
            }
        }

     	[Test] public void PutWelcomesNull() {
            LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
            q.Put(default(T));
     	    Assert.That(q.Remove(), Is.EqualTo(default(T)));
     	}

    	[Test] public void PutAddsElementsToQueue() {
             LinkedBlockingQueue<T> q =new LinkedBlockingQueue<T>(_sampleSize);
             for (int i = 0; i < _sampleSize; ++i) {
                 q.Put(_samples[i]);
                 Assert.IsTrue(q.Contains(_samples[i]));
             }
             Assert.AreEqual(0, q.RemainingCapacity);
	    }

        [Test] public void PutBlocksInterruptiblyWhenFull()
        {
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", 
                delegate
                    {
                        int added = 0;
                        LinkedBlockingQueue<T> q =
                            new LinkedBlockingQueue<T>(_sampleSize);
                        for (int i = 0; i < _sampleSize; ++i)
                        {
                            q.Put(_samples[i]);
                            ++added;
                        }
                        Assert.Throws<ThreadInterruptedException>(
                            () => q.Put(TestData<T>.MakeData(_sampleSize)));
                        Assert.AreEqual(added, _sampleSize);
                    });
            Thread.Sleep(TestData.SmallDelay);
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }

        [Test] public void PutBlocksWaitingForTakeWhenFull()
        {
            LinkedBlockingQueue<T> q = new LinkedBlockingQueue<T>(2);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1",
                delegate
                    {
                        int added = 0;
                        q.Put(_samples[0]);
                        ++added;
                        q.Put(_samples[1]);
                        ++added;
                        q.Put(_samples[2]);
                        ++added;
                        Assert.Throws<ThreadInterruptedException>(
                            delegate
                                {
                                    q.Put(_samples[3]);
                                    ++added;
                                });
                        Assert.IsTrue(added >= 2);
                    });
            Thread.Sleep(TestData.ShortDelay);
            q.Take();
            t.Interrupt();
            ThreadManager.JoinAndVerify();
        }
    }
}