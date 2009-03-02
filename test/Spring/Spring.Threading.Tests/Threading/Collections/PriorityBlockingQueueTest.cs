/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * http://creativecommons.org/licenses/publicdomain
 * Other contributors include Andrew Wright, Jeffrey Hayes,
 * Pat Fisher, Mike Judd.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spring.Collections;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;

namespace Spring.Threading.Tests.Collections {
    /// <author>Doug Lea>author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class PriorityBlockingQueueTest : BaseThreadingTestCase {

        private const int SIZE = 4;
        private const int NOCAP = Int32.MaxValue;

        /** Sample Comparator */
        public class MyReverseComparator : IComparer<int> {
            public int Compare(int i, int j) {
                if(i < j) return 1;
                if(i > j) return -1;
                return 0;
            }
        }

        /// <summary>
        /// Create a queue of given size containing consecutive Integers 0 ... n.
        /// </summary>
        private static PriorityBlockingQueue<int> PopulatedQueue(int n) {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(n);
            Assert.IsTrue(q.IsEmpty);
            for(int i = n - 1; i >= 0; i -= 2)
                Assert.IsTrue(q.Offer(i));
            for(int i = (n & 1); i < n; i += 2)
                Assert.IsTrue(q.Offer(i));
            Assert.IsFalse(q.IsEmpty);
            Assert.That(q.RemainingCapacity, Is.EqualTo(NOCAP));
            Assert.That(q.Count, Is.EqualTo(n));
            return q;
        }

        /// <summary>
        /// A new queue has unbounded capacity
        /// </summary>
        [Test]
        public void TestConstructor1() {
            Assert.That(new PriorityBlockingQueue<int>(SIZE).RemainingCapacity, Is.EqualTo(NOCAP));
        }

        /// <summary>
        /// Constructor throws IAE if  capacity argument nonpositive
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestConstructor2() {
            new PriorityBlockingQueue<int>(0);
        }

        /// <summary>
        /// Initializing from null Collection throws <see cref="ArgumentNullException"/>
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor3() {
            new PriorityBlockingQueue<object>(null);
        }

        /// <summary>
        /// Initializing from Collection of null elements throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor4() {
            object[] objects = new object[4];
            new PriorityBlockingQueue<object>(objects);
        }

        /// <summary>
        /// Initializing from Collection with some null elements throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor5() {
            string[] strings = new string[SIZE];
            for(int i = 0; i < SIZE - 1; ++i)
                strings[i] = i.ToString();
            new PriorityBlockingQueue<string>(strings);
        }

        /// <summary>
        /// Queue contains all elements of collection used to initialize
        /// </summary>
        [Test]
        public void TestConstructor6() {
            string[] strings = new string[SIZE];
            for(int i = 0; i < SIZE; ++i)
                strings[i] = i.ToString();
            PriorityBlockingQueue<string> q = new PriorityBlockingQueue<string>(strings);
            for(int i = 0; i < SIZE; ++i) {
                string result;
                Assert.IsTrue(q.Poll(out result));
                Assert.That(result, Is.EqualTo(i.ToString()));
            }
        }

        /// <summary>
        /// The comparator used in constructor is used
        /// </summary>
        [Test]
        public void TestConstructor7() {
            MyReverseComparator cmp = new MyReverseComparator();
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(SIZE, cmp);
            Assert.That(q.Comparator, Is.EqualTo(cmp));
            int[] ints = new int[SIZE];
            for(int i = 0; i < SIZE; ++i)
                ints[i] = i;
            q.AddAll(ints);
            for(int i = SIZE - 1; i >= 0; --i) {
                int result;
                Assert.IsTrue(q.Poll(out result));
                Assert.That(result, Is.EqualTo(ints[i]));
            }
        }

        /// <summary>
        /// isEmpty is true before add, false after
        /// </summary>
        [Test]
        public void TestEmpty() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.RemainingCapacity, Is.EqualTo(NOCAP));
            q.Add(one);
            Assert.IsFalse(q.IsEmpty);
            q.Add(two);
            q.Remove();
            q.Remove();
            Assert.IsTrue(q.IsEmpty);
        }

        /// <summary>
        /// remainingCapacity does not change when elements added or removed, but size does
        /// </summary>
        [Test]
        public void TestRemainingCapacity() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.RemainingCapacity, Is.EqualTo(NOCAP));
                Assert.That(q.Count, Is.EqualTo(SIZE - i));
                q.Remove();
            }
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.RemainingCapacity, Is.EqualTo(NOCAP));
                Assert.That(q.Count, Is.EqualTo(i));
                q.Add(i);
            }
        }

        /// <summary>
        /// Offer(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestOfferNull() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(1);
            q.Offer(null);
        }

        /// <summary>
        /// add(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddNull() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(1);
            q.Add(null);
        }

        /// <summary>
        /// Offer of comparable element succeeds
        /// </summary>
        [Test]
        public void TestOffer() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(1);
            Assert.IsTrue(q.Offer(zero));
            Assert.IsTrue(q.Offer(one));
        }

        /// <summary>
        /// Offer of non-Comparable throws CCE
        /// </summary>
        [Test, ExpectedException(typeof(InvalidCastException))]
        public void TestOfferNonComparable() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(1);
            q.Offer(new object());
            q.Offer(new object());
            q.Offer(new object());
        }

        /// <summary>
        /// add of comparable succeeds
        /// </summary>
        [Test]
        public void TestAdd() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.Count, Is.EqualTo(i));
                q.Add(i);
                Assert.That(q.Count, Is.EqualTo(i + 1));
            }
        }

        /// <summary>
        /// addAll(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAll1() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(1);
            q.AddAll(null);
        }

        /// <summary>
        /// addAll(this) throws IAE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddAllSelf() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            q.AddAll(q);
        }

        /// <summary>
        /// addAll of a collection with null elements throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAll2() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(SIZE);
            object[] objs = new object[SIZE];
            q.AddAll(objs);
        }

        /// <summary>
        /// addAll of a collection with any null elements throws NPE after possibly adding some elements
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAll3() {
            PriorityBlockingQueue<string> q = new PriorityBlockingQueue<string>(SIZE);
            string[] objs = new string[SIZE];
            for(int i = 0; i < SIZE - 1; ++i)
                objs[i] = "";
            q.AddAll(objs);
        }

        /// <summary>
        /// Queue contains all elements of successful addAll
        /// </summary>
        [Test]
        public void TestAddAll5() {
            int[] empty = new int[0];
            int[] ints = new int[SIZE];
            for(int i = SIZE - 1; i >= 0; --i)
                ints[i] = i;
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(SIZE);
            Assert.IsFalse(q.AddAll(empty));
            Assert.IsTrue(q.AddAll(ints));
            for(int i = 0; i < SIZE; ++i) {
                int result;
                Assert.IsTrue(q.Poll(out result));
                Assert.That(result, Is.EqualTo(ints[i]));
            }
        }

        /// <summary>
        /// put(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestPutNull() {
            PriorityBlockingQueue<object> q = new PriorityBlockingQueue<object>(SIZE);
            q.Put(null);
        }

        /// <summary>
        /// all elements successfully put are contained
        /// </summary>
        [Test]
        public void TestPut() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                q.Put(i);
                Assert.IsTrue(q.Contains(i));
            }
            Assert.That(q.Count, Is.EqualTo(SIZE));
        }

        /// <summary>
        /// put doesn't block waiting for take
        /// </summary>
        [Test]
        public void TestPutWithTake() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            Thread t = new Thread(new ThreadStart(delegate {
                int added = 0;
                try {
                    q.Put(0);
                    ++added;
                    q.Put(0);
                    ++added;
                    q.Put(0);
                    ++added;
                    q.Put(0);
                    ++added;
                    Assert.IsTrue(added == 4);
                }
                finally { }
            }));

            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            q.Take();
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed Offer does not time out
        /// </summary>
        [Test]
        public void TestTimedOffer() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    q.Put(0);
                    q.Put(0);
                    Assert.IsTrue(q.Offer(0, SHORT_DELAY_MS));
                    Assert.IsTrue(q.Offer(0, LONG_DELAY_MS));
                }
                finally { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            q.Take();
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// take retrieves elements in priority order
        /// </summary>
        [Test]
        public void TestTake() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.Take(), Is.EqualTo(i));
            }
        }

        /// <summary>
        /// take blocks interruptibly when empty
        /// </summary>
        [Test]
        public void TestTakeFromEmpty() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    q.Take();
                }
                catch(ThreadInterruptedException) { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// Take removes existing elements until empty, then blocks interruptibly
        /// </summary>
        [Test]
        public void TestBlockingTake() {
            AtomicBoolean isInterupted = new AtomicBoolean(false);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
                    for(int i = 0; i < SIZE; ++i) {
                        Assert.That(q.Take(), Is.EqualTo(i));
                    }
                    q.Take();
                }
                catch(ThreadInterruptedException success) {
                    isInterupted.Value = true;
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
            Assert.IsTrue(isInterupted.Value);
        }


        /// <summary>
        /// poll succeeds unless empty
        /// </summary>
        [Test]
        public void TestPoll() {
            int item;
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {

                Assert.IsTrue(q.Poll(out item));
                Assert.That(item, Is.EqualTo(i));
            }
            Assert.IsFalse(q.Poll(out item));
        }

        /// <summary>
        /// timed pool with zero timeout succeeds when non-empty, else times out
        /// </summary>
        [Test]
        public void TestTimedPoll0() {
            int item;
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.IsTrue(q.Poll(TimeSpan.Zero, out item));
                Assert.That(item, Is.EqualTo(i));
            }
            Assert.IsFalse(q.Poll(TimeSpan.Zero, out item));
        }

        /// <summary>
        /// timed pool with nonzero timeout succeeds when non-empty, else times out
        /// </summary>
        [Test]
        public void TestTimedPoll() {
            int item;
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.IsTrue(q.Poll(SHORT_DELAY_MS, out item));
                Assert.That(item, Is.EqualTo(i));
            }
            Assert.IsFalse(q.Poll(SHORT_DELAY_MS, out item));
        }

        /// <summary>
        /// Interrupted timed poll throws InterruptedException instead of returning timeout status
        /// </summary>
        [Test]
        public void TestInterruptedTimedPoll() {
            AtomicBoolean isInterupted = new AtomicBoolean(false);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    int item;
                    PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
                    for(int i = 0; i < SIZE; ++i) {
                        Assert.IsTrue(q.Poll(SHORT_DELAY_MS, out item));
                        Assert.That(item, Is.EqualTo(i));
                    }
                    Assert.IsFalse(q.Poll(SHORT_DELAY_MS, out item));
                }
                catch(ThreadInterruptedException) {
                    isInterupted.Value = true;
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed poll before a delayed Offer fails; after Offer succeeds; on interruption throws
        /// </summary>
        [Test]
        public void testTimedPollWithOffer() {
            AtomicBoolean isInterupted = new AtomicBoolean(false);
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    int item;
                    Assert.IsFalse(q.Poll(SHORT_DELAY_MS, out item));
                    q.Poll(LONG_DELAY_MS, out item);
                    q.Poll(LONG_DELAY_MS, out item);
                }
                catch(ThreadInterruptedException) {
                    isInterupted.Value = true;
                }
            }));
            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            Assert.IsTrue(q.Offer(0, SHORT_DELAY_MS));
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// peek returns next element, or null if empty
        /// </summary>
        [Test]
        public void TestPeek() {
            int item;
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.IsTrue(q.Peek(out item));
                Assert.That(item, Is.EqualTo(i));
                q.Poll(out item);
                Assert.IsTrue(q.Peek(out item) || i != item);
            }
            Assert.IsFalse(q.Peek(out item));
        }

        /// <summary>
        /// element returns next element, or throws NSEE if empty
        /// </summary>
        [Test, ExpectedException(typeof(NoElementsException))]
        public void TestElement() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.Element(), Is.EqualTo(i));
                int item;
                q.Poll(out item);
            }
            q.Element();
        }

        /// <summary>
        /// remove removes next element, or throws NSEE if empty
        /// </summary>
        [Test, ExpectedException(typeof(NoElementsException))]
        public void TestRemove() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.That(q.Remove(), Is.EqualTo(i));
            }
            q.Remove();
        }

        /// <summary>
        /// remove(x) removes x and returns true if present
        /// </summary>
        [Test]
        public void TestRemoveElement() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 1; i < SIZE; i += 2) {
                Assert.IsTrue(q.Remove(i));
            }
            for(int i = 0; i < SIZE; i += 2) {
                Assert.IsTrue(q.Remove(i));
                Assert.IsFalse(q.Remove(i + 1));
            }
            Assert.IsTrue(q.IsEmpty);
        }

        /// <summary>
        /// contains(x) reports true when elements added but not yet removed
        /// </summary>
        [Test]
        public void TestContains() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            for(int i = 0; i < SIZE; ++i) {
                Assert.IsTrue(q.Contains(i));
                int item;
                q.Poll(out item);
            }
            // cannot test for Contains(0) !!!
            for(int i = 1; i < SIZE; ++i) {
                Assert.IsFalse(q.Contains(i));
            }
        }

        /// <summary>
        /// clear removes all elements
        /// </summary>
        [Test]
        public void TestClear() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            q.Clear();
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            q.Add(one);
            Assert.IsFalse(q.IsEmpty);
            Assert.IsTrue(q.Contains(one));
            q.Clear();
            Assert.IsTrue(q.IsEmpty);
        }

        /// <summary>
        /// TODO containsAll(c) is true when c contains a subset of elements
        /// </summary>
        //[Test]
        //public void TestContainsAll() {
        //    PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
        //    PriorityBlockingQueue<int> p = new PriorityBlockingQueue<int>(SIZE);
        //    for(int i = 0; i < SIZE; ++i) {
        //        Assert.IsTrue(q.ContainsAll(p));
        //        Assert.IsFalse(p.ContainsAll(q));
        //        p.Add(i);
        //    }
        //    Assert.IsTrue(p.ContainsAll(q));
        //}

        ///**
        // * TODO retainAll(c) retains only those elements of c and reports true if changed
        // */
        //public void TestRetainAll() {
        //    PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
        //    PriorityBlockingQueue<int> p = PopulatedQueue(SIZE);
        //    for(int i = 0; i < SIZE; ++i) {
        //        bool changed = q.RetainAll(p);
        //        if(i == 0)
        //            Assert.IsFalse(changed);
        //        else
        //            Assert.IsTrue(changed);

        //        Assert.IsTrue(q.ContainsAll(p));
        //        Assert.That(q.Count, Is.EqualTo(SIZE - i));
        //        p.Remove();
        //    }
        //}

        /// <summary>
        /// TODO removeAll(c) removes only those elements of c and reports true if changed
        /// </summary>
        //[Test]
        //public void TestRemoveAll() {
        //    for(int i = 1; i < SIZE; ++i) {
        //        PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
        //        PriorityBlockingQueue<int> p = PopulatedQueue(i);
        //        Assert.IsTrue(q.RemoveAll(p));
        //        assertEquals(SIZE - i, q.size());
        //        for(int j = 0; j < i; ++j) {
        //            Integer I = (Integer)(p.remove());
        //            Assert.IsFalse(q.contains(I));
        //        }
        //    }
        //}

        /// <summary>
        /// toArray contains all elements
        /// </summary>
        [Test]
        public void TestToArray() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            int[] o = q.ToArray();
            Array.Sort(o);
            for(int i = 0; i < o.Length; i++)
                Assert.That(q.Take(), Is.EqualTo(o[i]));
        }

        /// <summary>
        /// toArray(a) contains all elements
        /// </summary>
        [Test]
        public void TestToArray2() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            int[] ints = new int[SIZE];
            ints = q.ToArray(ints);
            Array.Sort(ints);
            for(int i = 0; i < ints.Length; i++)
                Assert.That(q.Take(), Is.EqualTo(ints[i]));
        }

        /// <summary>
        /// toArray(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestToArray_BadArg() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            q.ToArray(null);
        }

        /// <summary>
        /// iterator iterates through all elements
        /// </summary>
        [Test]
        public void TestIterator() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            int i = 0;
            IEnumerator<int> it = q.GetEnumerator();
            while(it.MoveNext()) {
                Assert.IsTrue(q.Contains(it.Current));
                ++i;
            }
            Assert.That(SIZE, Is.EqualTo(i));
        }

        /// <summary>
        /// toString contains toStrings of elements
        /// </summary>
        [Test]
        public void TestToString() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            string s = q.ToString();
            for(int i = 0; i < SIZE; ++i) {
                Assert.IsTrue(s.IndexOf(i.ToString()) >= 0);
            }
        }

        /// <summary>
        /// Offer transfers elements across Executor tasks
        /// </summary>
        [Test, Ignore("check JoinPool()")]
        public void TestPollInExecutor() {
            AtomicBoolean isInterupted = new AtomicBoolean(false);
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(2);
            IExecutorService executor = Executors.NewFixedThreadPool(2);
            executor.Execute(delegate {
                int item;
                Assert.IsFalse(q.Poll(out item));
                try {
                    Assert.IsTrue(q.Poll(MEDIUM_DELAY_MS, out item));
                    Assert.IsTrue(q.IsEmpty);
                }
                catch(ThreadInterruptedException) {
                    isInterupted.Value = true;
                }
            });

            executor.Execute(delegate {
                try {
                    Thread.Sleep(SMALL_DELAY_MS);
                    q.Put(1);
                }
                catch(ThreadInterruptedException e) {
                    isInterupted.Value = true;
                }
            });

            JoinPool(executor);
        }

        /// <summary>
        /// A deserialized serialized queue has same elements
        /// </summary>
        [Test]
        public void TestSerialization() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);

            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream sout = new MemoryStream();
            bf.Serialize(sout, q);
            Byte[] data = sout.ToArray();
            sout.Close();

            MemoryStream sin = new MemoryStream(data);
            PriorityBlockingQueue<int> r = (PriorityBlockingQueue<int>)bf.Deserialize(sin);
            sin.Close();

            Assert.That(r.Count, Is.EqualTo(q.Count));
            while(!q.IsEmpty)
                Assert.That(r.Remove(), Is.EqualTo(q.Remove()));
        }

        /// <summary>
        /// drainTo(null) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestDrainToNull() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            q.DrainTo(null);
        }

        /// <summary>
        /// drainTo(this) throws IAE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestDrainToSelf() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            q.DrainTo(q);
        }

        /// <summary>
        /// drainTo(c) empties queue into another collection c
        /// </summary>
        [Test]
        public void TestDrainTo() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            IList<int> l = new List<int>();
            q.DrainTo(l);
            Assert.That(q.Count,Is.EqualTo(0));
            Assert.That(l.Count, Is.EqualTo(SIZE));
            for(int i = 0; i < SIZE; ++i)
                Assert.That(l[i], Is.EqualTo((i)));
            q.Add(zero);
            q.Add(one);
            Assert.IsFalse(q.IsEmpty);
            Assert.IsTrue(q.Contains(zero));
            Assert.IsTrue(q.Contains(one));
            l.Clear();
            q.DrainTo(l);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(l.Count, Is.EqualTo(2));
            for(int i = 0; i < 2; ++i)
                Assert.That(l[i], Is.EqualTo((i)));
        }

        /// <summary>
        /// drainTo empties queue
        /// </summary>
        [Test]
        public void TestDrainToWithActivePut() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
            Thread t = new Thread(new ThreadStart(delegate {
                                                      q.Put(SIZE + 1);
                                                  }));

                t.Start();
                IList<int> l = new List<int>();
                q.DrainTo(l);
                Assert.IsTrue(l.Count >= SIZE);
                for (int i = 0; i < SIZE; ++i)
                    Assert.That(l[i], Is.EqualTo((i)));
                t.Join();
                Assert.IsTrue(q.Count + l.Count >= SIZE);
        }

        /// <summary>
        /// drainTo(null, n) throws NPE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestDrainToNullN() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
                q.DrainTo(null, 0);
        }

        /// <summary>
        /// drainTo(this, n) throws IAE
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void testDrainToSelfN() {
            PriorityBlockingQueue<int> q = PopulatedQueue(SIZE);
                q.DrainTo(q, 0);
        }

        /// <summary>
        /// drainTo(c, n) empties first max {n, size} elements of queue into c
        /// </summary>
        [Test]
        public void testDrainToN() {
            PriorityBlockingQueue<int> q = new PriorityBlockingQueue<int>(SIZE * 2);
            for(int i = 0; i < SIZE + 2; ++i) {
                for(int j = 0; j < SIZE; j++)
                    Assert.IsTrue(q.Offer(j));
                IList<int> l = new List<int>();
                q.DrainTo(l, i);
                int k = (i < SIZE) ? i : SIZE;
                Assert.That(l.Count, Is.EqualTo(k));
                Assert.That(q.Count, Is.EqualTo(SIZE - k));
                for(int j = 0; j < k; ++j)
                    Assert.That(l[j], Is.EqualTo(j));
                int item;
                while(q.Poll(out item)) {}
            }
        }
    }
}