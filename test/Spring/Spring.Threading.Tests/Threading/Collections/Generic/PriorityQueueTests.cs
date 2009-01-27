using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Spring.Threading.Collections;

namespace Spring.Threading.Tests.Collections.Generic
{
    [TestFixture]
    public class PriorityQueueTests : BaseThreadingTestCase
    {
        private class MyReverseComparator : IComparer<int>
        {
            #region IComparer<int> Members

            public int Compare(int x, int y)
            {
                if (x < y) return 1;
                if (x > y) return -1;
                return 0;
            }

            #endregion
        }

        private PriorityQueue<int> populatedQueue(int n)
        {
            var q = new PriorityQueue<int>(n);
            Assert.IsTrue(q.IsEmpty);
            for (int i = n - 1; i >= 0; i -= 2)
                Assert.IsTrue(q.Offer(i));
            for (int i = (n & 1); i < n; i += 2)
                Assert.IsTrue(q.Offer(i));
            Assert.IsFalse(q.IsEmpty);
            Assert.AreEqual(n, q.Count);
            return q;
        }

        [Test]
        public void testAdd()
        {
            var q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(i, q.Count);
                Assert.IsTrue(q.Add(i));
            }
        }

        [Test]
        public void testAddAll1()
        {
            try
            {
                var q = new PriorityQueue<int>(1);
                q.AddAll(null);
                Debug.Fail("Should throw exception.");
            }
            catch (NullReferenceException success)
            {
            }
        }

        /**
     * AddAll of a collection with null Elements throws NPE
     */

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void testAddAll2()
        {
            var q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            var ints = new int[DEFAULT_COLLECTION_SIZE];
            q.AddAll(ints);
            Debug.Fail("Should throw exception.");
        }

        /**
     * AddAll of a collection with any null Elements throws NPE after
     * possibly Adding some Elements
     */

        [Test]
        public void testAddAll3()
        {
            try
            {
                var q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
                var ints = new int[DEFAULT_COLLECTION_SIZE];
                for (int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i)
                    ints[i] = i;
                q.AddAll(ints);
                Debug.Fail("Should throw exception.");
            }
            catch (NullReferenceException success)
            {
            }
        }

        [Test]
        public void testAddAll5()
        {
                var empty = new int[0];
                var ints = new int[DEFAULT_COLLECTION_SIZE];
                for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                    ints[i] = DEFAULT_COLLECTION_SIZE - 1 - i;
                var q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
                Assert.IsFalse(q.AddAll(empty));
                Assert.IsTrue(q.AddAll(ints));
                for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                {
                    int output;
                    q.Poll(out output);
                    Assert.AreEqual(i, output);
                }
        }

        [Test]
        public void testClear()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            q.Clear();
            Assert.IsTrue(q.IsEmpty);
            Assert.AreEqual(0, q.Count);
            q.Add(1);
            Assert.IsFalse(q.IsEmpty);
            q.Clear();
            Assert.IsTrue(q.IsEmpty);
        }

        [Test]
        public void testConstructor1()
        {
            Assert.AreEqual(0, new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE).Count);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void testConstructor2()
        {
            var q = new PriorityQueue<int>(0);
        }

        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void testConstructor3()
        {
            var q = new PriorityQueue<int>(null);
        }

        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void testConstructor4()
        {
            var ints = new int[DEFAULT_COLLECTION_SIZE];
            var q = new PriorityQueue<int>(ints);
        }

        [Test]
        [ExpectedException(typeof (NullReferenceException))]
        public void testConstructor5()
        {
            var ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i)
                ints[i] = i;
            var q = new PriorityQueue<int>(ints);
        }

        [Test]
        public void testConstructor6()
        {
            var ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                ints[i] = i;
            var q = new PriorityQueue<int>(ints);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                int output;
                q.Poll(out output);
                Assert.AreEqual(ints[i], output);
            }
        }

        [Test]
        public void testConstructor7()
        {
            try
            {
                var cmp = new MyReverseComparator();
                var q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE, cmp);
                Assert.AreEqual(cmp, q.Comparator());
                var ints = new int[DEFAULT_COLLECTION_SIZE];
                for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                    ints[i] = i;
                q.AddAll(ints);
                for (int i = DEFAULT_COLLECTION_SIZE - 1; i >= 0; --i)
            {
                int output;
                q.Poll(out output);
                Assert.AreEqual(ints[i], output);
            }
            }
            finally
            {
            }
        }

        [Test]
        public void testContains()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.IsTrue(q.Contains(i));
                int output;
                q.Poll(out output);
                Assert.IsFalse(q.Contains(i));
            }
        }

        [Test]
        public void testContainsAll()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            var p = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.IsTrue(q.ContainsAll(p));
                Assert.IsFalse(p.ContainsAll(q));
                p.Add(i);
            }
            Assert.IsTrue(p.ContainsAll(q));
        }


        [Test]
        public void testCount()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.Count);
                q.Remove();
            }
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(i, q.Count);
                q.Add(i);
            }
        }

        [Test]
        public void testElement()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(i, ((int) q.Element()));
                int output;
                q.Poll(out output);
            }
            try
            {
                q.Element();
                Debug.Fail("Should throw exception.");
            }
            catch (NoSuchElementException success)
            {
            }
        }

        [Test]
        public void testEmpty()
        {
            var q = new PriorityQueue<int>(2);
            Assert.IsTrue(q.IsEmpty);
            q.Add(1);
            Assert.IsFalse(q.IsEmpty);
            q.Add(2);
            q.Remove();
            q.Remove();
            Assert.IsTrue(q.IsEmpty);
        }

        [Test]
        public void testIterator()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            int i = 0;
            var it = q.GetEnumerator();
            while (it.MoveNext())
            {
                Assert.IsTrue(q.Contains(it.Current));
                ++i;
            }
            Assert.AreEqual(i, DEFAULT_COLLECTION_SIZE);
        }
        [Test]
        public void testOffer()
        {
            var q = new PriorityQueue<int>(1);
            Assert.IsTrue(q.Offer(zero));
            Assert.IsTrue(q.Offer(one));
        }

        [Test]
        public void testPeek()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                int peekOut;
                Assert.AreEqual(i, ( q.Peek(out peekOut)));
                int pollOut;
                q.Poll(out pollOut);
                Assert.IsTrue(!q.Peek(out peekOut)  || i != (peekOut));
            }
            int finalOut;
            Assert.IsFalse(q.Peek(out finalOut));
        }

        [Test]
        public void testPoll()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                int pollOut;
                q.Poll(out pollOut);
                Assert.AreEqual(i, pollOut);
            }
            int finalOut;
            Assert.IsFalse(q.Poll(out finalOut));
        }

        [Test]
        public void testRemove()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(i, (q.Remove()));
            }
            try
            {
                q.Remove();
                Debug.Fail("Should throw exception.");
            }
            catch (NoSuchElementException success)
            {
            }
        }

        [Test]
        public void testRemoveAll()
        {
            for (int i = 1; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
                PriorityQueue<int> p = populatedQueue(i);
                Assert.IsTrue(q.RemoveAll(p));
                Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.());
                for (int j = 0; j < i; ++j)
                {
                    int I = (p.Remove());
                    Assert.IsFalse(q.Contains(I));
                }
            }
        }

        /**
     * Remove(x) removes x and returns true if present
     */

        [Test]
        public void testRemoveElement()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 1; i < DEFAULT_COLLECTION_SIZE; i += 2)
            {
                Assert.IsTrue(q.Remove(i));
            }
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; i += 2)
            {
                Assert.IsTrue(q.Remove(i));
                Assert.IsFalse(q.Remove(new int(i + 1)));
            }
            Assert.IsTrue(q.IsEmpty);
        }

        /**
     * Contains(x) reports true when Elements Added but not yet Removed
     */

        /**
     * retainAll(c) retains only those Elements of c and reports true if changed
     */

        [Test]
        public void testRetainAll()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            PriorityQueue<int> p = populatedQueue(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                boolean changed = q.retainAll(p);
                if (i == 0)
                    Assert.IsFalse(changed);
                else
                    Assert.IsTrue(changed);

                Assert.IsTrue(q.ContainsAll(p));
                Assert.AreEqual(DEFAULT_COLLECTION_SIZE - i, q.size());
                p.Remove();
            }
        }

        [Test]
        public void testSerialization()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            try
            {
                var bout = new ByteArrayOutputStream(10000);
                ObjectOutputStream out =
                new ObjectOutputStream(new BufferedOutputStream(bout));
                out.
                writeObject(q);
                out.
                close();

                var bin = new ByteArrayInputStream(bout.toByteArray());
                ObjectInputStream in =
                new ObjectInputStream(new BufferedInputStream(bin));
                PriorityQueue<int> r = (PriorityQueue)in.
                readObject();
                Assert.AreEqual(q.Count, r.size());
                while (!q.IsEmpty)
                    Assert.AreEqual(q.Remove(), r.remove());
            }
            catch (Exception e)
            {
                unexpectedException();
            }
        }

        /**
     * RemoveAll(c) removes only those Elements of c and reports true if changed
     */

        /**
     * toArray Contains all Elements
     */

        [Test]
        public void testToArray()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            Object[] o = q.toArray();
            Arrays.sort(o);
            for (int i = 0; i < o.length; i++)
                Assert.AreEqual(o[i], q.Poll());
        }

        /**
     * toArray(a) Contains all Elements
     */

        [Test]
        public void testToArray2()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            var ints = new int[DEFAULT_COLLECTION_SIZE];
            ints = (int[]) q.toArray(ints);
            Arrays.sort(ints);
            for (int i = 0; i < ints.length; i++)
                Assert.AreEqual(ints[i], q.Poll());
        }

        /**
     * iterator iterates through all Elements
     */


        /**
     * toString Contains toStrings of Elements
     */

        [Test]
        public void testToString()
        {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            String s = q.toString();
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.IsTrue(s.indexOf(String.valueOf(i)) >= 0);
            }
        }

        /**
     * A deserialized serialized queue has same Elements
     */
    }
}