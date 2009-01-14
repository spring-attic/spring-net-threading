using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spring.Threading.Collections;

namespace Spring.Threading.Tests.Collections.Generic
{
    [TestFixture]
    public class PriorityQueueTests : BaseThreadingTestCase
    {

    private class MyReverseComparator : IComparer<int> {
        public int Compare(int x, int y) {
            if (x < y) return 1;
            if (x > y) return -1;
            return 0;
        }
    }

    private PriorityQueue<int> populatedQueue(int n) {
        PriorityQueue<int> q = new PriorityQueue<int>(n);
        Assert.IsTrue(q.IsEmpty);
	for(int i = n-1; i >= 0; i-=2)
	    Assert.IsTrue(q.Offer(i));
	for(int i = (n & 1); i < n; i+=2)
	    Assert.IsTrue(q.Offer(i));
        Assert.IsFalse(q.IsEmpty);
	Assert.AreEqual(n, q.Count);
        return q;
    }

    [Test] public void testConstructor1() {
        Assert.AreEqual(0, new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE).Count);
    }

    [Test] 
       [ExpectedException(typeof(ArgumentException))] 
        public void testConstructor2() {
            PriorityQueue<int> q = new PriorityQueue<int>(0);
    }

    [Test] 
        [ExpectedException(typeof(NullReferenceException))]
        public void testConstructor3() {
            PriorityQueue<int> q = new PriorityQueue<int>(null);
    }

    [Test] 
        [ExpectedException(typeof(NullReferenceException))]
        public void testConstructor4() {
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            PriorityQueue<int> q = new PriorityQueue<int>(Arrays.asList(ints));
    }
    [Test] 
        [ExpectedException(typeof(NullReferenceException))]
        public void testConstructor5() {
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE-1; ++i)
                ints[i] = new int(i);
            PriorityQueue<int> q = new PriorityQueue<int>(Arrays.asList(ints));
    }

    [Test] public void testConstructor6() {
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                ints[i] = i;
            PriorityQueue<int> q = new PriorityQueue<int>(Arrays.asList(ints));
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                Assert.AreEqual(ints[i], q.Poll());
      
    }

    [Test] public void testConstructor7() {
        try {
            MyReverseComparator cmp = new MyReverseComparator();
            PriorityQueue<int> q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE, cmp);
            Assert.AreEqual(cmp, q.comparator());
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                ints[i] = new int(i);
            q.AddAll(Arrays.asList(ints));
            for (int i = DEFAULT_COLLECTION_SIZE-1; i >= 0; --i)
                Assert.AreEqual(ints[i], q.Poll());
        }
        finally {}
    }

    [Test] public void testEmpty() {
        PriorityQueue<int> q = new PriorityQueue<int>(2);
        Assert.IsTrue(q.IsEmpty);
        q.Add(1);
        Assert.IsFalse(q.IsEmpty);
        q.Add(2);
        q.Remove();
        q.Remove();
        Assert.IsTrue(q.IsEmpty);
    }

   
    [Test] public void testCount() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE-i, q.Count);
            q.Remove();
        }
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, q.Count);
            q.Add(i);
        }
    }

    /**
     * Offer(null) throws NPE
     */
    [Test] public void testOfferNull() {
	try {
            PriorityQueue<int> q = new PriorityQueue<int>(1);
            q.Offer(null);
            shouldThrow();
        } catch (NullReferenceException success) { }
    }

    /**
     * Add(null) throws NPE
     */
    [Test] public void testAddNull() {
	try {
            PriorityQueue<int> q = new PriorityQueue<int>(1);
            q.Add(null);
            shouldThrow();
        } catch (NullReferenceException success) { }
    }

    /**
     * Offer of comparable element succeeds
     */
    [Test] public void testOffer() {
        PriorityQueue<int> q = new PriorityQueue<int>(1);
        Assert.IsTrue(q.Offer(zero));
        Assert.IsTrue(q.Offer(one));
    }


    /**
     * Add of comparable succeeds
     */
    [Test] public void testAdd() {
        PriorityQueue<int> q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, q.Count);
            Assert.IsTrue(q.Add(i));
        }
    }

    /**
     * AddAll(null) throws NPE
     */
    [Test] public void testAddAll1() {
        try {
            PriorityQueue<int> q = new PriorityQueue<int>(1);
            q.AddAll(null);
            shouldThrow();
        }
        catch (NullReferenceException success) {}
    }
    /**
     * AddAll of a collection with null elements throws NPE
     */
    [Test] public void testAddAll2() {
        try {
            PriorityQueue<int> q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            q.AddAll(Arrays.asList(ints));
            shouldThrow();
        }
        catch (NullReferenceException success) {}
    }
    /**
     * AddAll of a collection with any null elements throws NPE after
     * possibly Adding some elements
     */
    [Test] public void testAddAll3() {
        try {
            PriorityQueue<int> q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE-1; ++i)
                ints[i] = new int(i);
            q.AddAll(Arrays.asList(ints));
            shouldThrow();
        }
        catch (NullReferenceException success) {}
    }

    /**
     * Queue contains all elements of successful AddAll
     */
    [Test] public void testAddAll5() {
        try {
            int[] empty = new int[0];
            int[] ints = new int[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                ints[i] = new int(DEFAULT_COLLECTION_SIZE-1-i);
            PriorityQueue<int> q = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
            Assert.IsFalse(q.AddAll(Arrays.asList(empty)));
            Assert.IsTrue(q.AddAll(Arrays.asList(ints)));
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                Assert.AreEqual(new int(i), q.Poll());
        }
        finally {}
    }

    /**
     * Poll succeeds unless empty
     */
    [Test] public void testPoll() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, ((int)q.Poll()).intValue());
        }
	assertNull(q.Poll());
    }

    /**
     * peek returns next element, or null if empty
     */
    [Test] public void testPeek() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, ((int)q.peek()).intValue());
            q.Poll();
            Assert.IsTrue(q.peek() == null ||
                       i != ((int)q.peek()).intValue());
        }
	assertNull(q.peek());
    }

    /**
     * element returns next element, or throws NSEE if empty
     */
    [Test] public void testElement() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, ((int)q.element()).intValue());
            q.Poll();
        }
        try {
            q.element();
            shouldThrow();
        }
        catch (NoSuchElementException success) {}
    }

    /**
     * Remove removes next element, or throws NSEE if empty
     */
    [Test] public void testRemove() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.AreEqual(i, ((int)q.Remove()).intValue());
        }
        try {
            q.Remove();
            shouldThrow();
        } catch (NoSuchElementException success){
	}
    }

    /**
     * Remove(x) removes x and returns true if present
     */
    [Test] public void testRemoveElement() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 1; i < DEFAULT_COLLECTION_SIZE; i+=2) {
            Assert.IsTrue(q.Remove(new int(i)));
        }
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; i+=2) {
            Assert.IsTrue(q.Remove(new int(i)));
            Assert.IsFalse(q.Remove(new int(i+1)));
        }
        Assert.IsTrue(q.IsEmpty);
    }

    /**
     * contains(x) reports true when elements Added but not yet Removed
     */
    [Test] public void testContains() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.IsTrue(q.contains(new int(i)));
            q.Poll();
            Assert.IsFalse(q.contains(new int(i)));
        }
    }

    /**
     * clear Removes all elements
     */
    [Test] public void testClear() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        q.clear();
        Assert.IsTrue(q.IsEmpty);
        Assert.AreEqual(0, q.Count);
        q.Add(new int(1));
        Assert.IsFalse(q.IsEmpty);
        q.clear();
        Assert.IsTrue(q.IsEmpty);
    }

    /**
     * containsAll(c) is true when c contains a subset of elements
     */
    [Test] public void testContainsAll() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        PriorityQueue<int> p = new PriorityQueue<int>(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.IsTrue(q.containsAll(p));
            Assert.IsFalse(p.containsAll(q));
            p.Add(new int(i));
        }
        Assert.IsTrue(p.containsAll(q));
    }

    /**
     * retainAll(c) retains only those elements of c and reports true if changed
     */
    [Test] public void testRetainAll() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        PriorityQueue<int> p = populatedQueue(DEFAULT_COLLECTION_SIZE);
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            boolean changed = q.retainAll(p);
            if (i == 0)
                Assert.IsFalse(changed);
            else
                Assert.IsTrue(changed);

            Assert.IsTrue(q.containsAll(p));
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE-i, q.size());
            p.Remove();
        }
    }

    /**
     * RemoveAll(c) removes only those elements of c and reports true if changed
     */
    [Test] public void testRemoveAll() {
        for (int i = 1; i < DEFAULT_COLLECTION_SIZE; ++i) {
            PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
            PriorityQueue<int> p = populatedQueue(i);
            Assert.IsTrue(q.RemoveAll(p));
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE-i, q.size());
            for (int j = 0; j < i; ++j) {
                int I = (Integer)(p.Remove());
                Assert.IsFalse(q.contains(I));
            }
        }
    }

    /**
     * toArray contains all elements
     */
    [Test] public void testToArray() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
	Object[] o = q.toArray();
        Arrays.sort(o);
	for(int i = 0; i < o.length; i++)
	    Assert.AreEqual(o[i], q.Poll());
    }

    /**
     * toArray(a) contains all elements
     */
    [Test] public void testToArray2() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
	int[] ints = new int[DEFAULT_COLLECTION_SIZE];
	ints = (int[])q.toArray(ints);
        Arrays.sort(ints);
        for(int i = 0; i < ints.length; i++)
            Assert.AreEqual(ints[i], q.Poll());
    }

    /**
     * iterator iterates through all elements
     */
    [Test] public void testIterator() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        int i = 0;
	Iterator it = q.iterator();
        while(it.hasNext()) {
            Assert.IsTrue(q.contains(it.next()));
            ++i;
        }
        Assert.AreEqual(i, DEFAULT_COLLECTION_SIZE);
    }

    /**
     * iterator.Remove removes current element
     */
    [Test] public void testIteratorRemove () {
        final PriorityQueue<int> q = new PriorityQueue<int>(3);
        q.Add(new int(2));
        q.Add(new int(1));
        q.Add(new int(3));

        Iterator it = q.iterator();
        it.next();
        it.Remove();

        it = q.iterator();
        Assert.AreEqual(it.next(), new int(2));
        Assert.AreEqual(it.next(), new int(3));
        Assert.IsFalse(it.hasNext());
    }


    /**
     * toString contains toStrings of elements
     */
    [Test] public void testToString() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        String s = q.toString();
        for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
            Assert.IsTrue(s.indexOf(String.valueOf(i)) >= 0);
        }
    }

    /**
     * A deserialized serialized queue has same elements
     */
    [Test] public void testSerialization() {
        PriorityQueue<int> q = populatedQueue(DEFAULT_COLLECTION_SIZE);
        try {
            ByteArrayOutputStream bout = new ByteArrayOutputStream(10000);
            ObjectOutputStream out = new ObjectOutputStream(new BufferedOutputStream(bout));
            out.writeObject(q);
            out.close();

            ByteArrayInputStream bin = new ByteArrayInputStream(bout.toByteArray());
            ObjectInputStream in = new ObjectInputStream(new BufferedInputStream(bin));
            PriorityQueue<int> r = (PriorityQueue)in.readObject();
            Assert.AreEqual(q.Count, r.size());
            while (!q.IsEmpty)
                Assert.AreEqual(q.Remove(), r.remove());
        } catch(Exception e){
            unexpectedException();
        }
    }
}
    }
}
