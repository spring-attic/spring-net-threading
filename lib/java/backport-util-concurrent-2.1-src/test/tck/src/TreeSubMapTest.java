/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * https://creativecommons.org/licenses/publicdomain
 */

import junit.framework.*;
import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import java.io.*;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.Map;
import java.util.Set;
import java.util.SortedMap;

public class TreeSubMapTest extends JSR166TestCase {
    public static void main(String[] args) {
	junit.textui.TestRunner.run (suite());	
    }
    public static Test suite() {
	return new TestSuite(TreeSubMapTest.class);
    }

    /**
     * Create a map from Integers 1-5 to Strings "A"-"E".
     */
    private static NavigableMap map5() {   
	TreeMap map = new TreeMap();
        assertTrue(map.isEmpty());
	map.put(zero, "Z");
	map.put(one, "A");
	map.put(five, "E");
	map.put(three, "C");
	map.put(two, "B");
	map.put(four, "D");
	map.put(seven, "F");
        assertFalse(map.isEmpty());
        assertEquals(7, map.size());
        return map.navigableSubMap(one, seven);
    }

    private static NavigableMap map0() {   
	TreeMap map = new TreeMap();
        assertTrue(map.isEmpty());
        return map.navigableTailMap(one);
    }

    /**
     *  clear removes all pairs
     */
    public void testClear() {
        NavigableMap map = map5();
	map.clear();
	assertEquals(map.size(), 0);
    }


    /**
     *  Maps with same contents are equal
     */
    public void testEquals() {
        NavigableMap map1 = map5();
        NavigableMap map2 = map5();
        assertEquals(map1, map2);
        assertEquals(map2, map1);
	map1.clear();
        assertFalse(map1.equals(map2));
        assertFalse(map2.equals(map1));
    }

    /**
     *  containsKey returns true for contained key
     */
    public void testContainsKey() {
        NavigableMap map = map5();
	assertTrue(map.containsKey(one));
        assertFalse(map.containsKey(zero));
    }

    /**
     *  containsValue returns true for held values
     */
    public void testContainsValue() {
        NavigableMap map = map5();
	assertTrue(map.containsValue("A"));
        assertFalse(map.containsValue("Z"));
    }

    /**
     *  get returns the correct element at the given key,
     *  or null if not present
     */
    public void testGet() {
        NavigableMap map = map5();
	assertEquals("A", (String)map.get(one));
        NavigableMap empty = map0();
        assertNull(empty.get(one));
    }

    /**
     *  isEmpty is true of empty map and false for non-empty
     */
    public void testIsEmpty() {
        NavigableMap empty = map0();
        NavigableMap map = map5();
	assertTrue(empty.isEmpty());
        assertFalse(map.isEmpty());
    }

    /**
     *   firstKey returns first key
     */
    public void testFirstKey() {
        NavigableMap map = map5();
	assertEquals(one, map.firstKey());
    }

    /**
     *   lastKey returns last key
     */
    public void testLastKey() {
        NavigableMap map = map5();
	assertEquals(five, map.lastKey());
    }


    /**
     *   keySet returns a Set containing all the keys
     */
    public void testKeySet() {
        NavigableMap map = map5();
	Set s = map.keySet();
	assertEquals(5, s.size());
	assertTrue(s.contains(one));
	assertTrue(s.contains(two));
	assertTrue(s.contains(three));
	assertTrue(s.contains(four));
	assertTrue(s.contains(five));
    }

    /**
     *   keySet is ordered
     */
    public void testKeySetOrder() {
        NavigableMap map = map5();
	Set s = map.keySet();
        Iterator i = s.iterator();
        Integer last = (Integer)i.next();
        assertEquals(last, one);
        while (i.hasNext()) {
            Integer k = (Integer)i.next();
            assertTrue(last.compareTo(k) < 0);
            last = k;
        }
    }

    /**
     * values collection contains all values
     */
    public void testValues() {
        NavigableMap map = map5();
	Collection s = map.values();
	assertEquals(5, s.size());
	assertTrue(s.contains("A"));
	assertTrue(s.contains("B"));
	assertTrue(s.contains("C"));
	assertTrue(s.contains("D"));
	assertTrue(s.contains("E"));
    }

    /**
     * entrySet contains all pairs
     */
    public void testEntrySet() {
        NavigableMap map = map5();
	Set s = map.entrySet();
	assertEquals(5, s.size());
        Iterator it = s.iterator();
        while (it.hasNext()) {
            Map.Entry e = (Map.Entry) it.next();
            assertTrue( 
                       (e.getKey().equals(one) && e.getValue().equals("A")) ||
                       (e.getKey().equals(two) && e.getValue().equals("B")) ||
                       (e.getKey().equals(three) && e.getValue().equals("C")) ||
                       (e.getKey().equals(four) && e.getValue().equals("D")) ||
                       (e.getKey().equals(five) && e.getValue().equals("E")));
        }
    }

    /**
     *   putAll  adds all key-value pairs from the given map
     */
    public void testPutAll() {
        NavigableMap empty = map0();
        NavigableMap map = map5();
	empty.putAll(map);
	assertEquals(5, empty.size());
	assertTrue(empty.containsKey(one));
	assertTrue(empty.containsKey(two));
	assertTrue(empty.containsKey(three));
	assertTrue(empty.containsKey(four));
	assertTrue(empty.containsKey(five));
    }

    /**
     *   remove removes the correct key-value pair from the map
     */
    public void testRemove() {
        NavigableMap map = map5();
	map.remove(five);
	assertEquals(4, map.size());
	assertFalse(map.containsKey(five));
    }

    /**
     * lowerEntry returns preceding entry.
     */
    public void testLowerEntry() {
        NavigableMap map = map5();
        Map.Entry e1 = map.lowerEntry(three);
        assertEquals(two, e1.getKey());

        Map.Entry e2 = map.lowerEntry(six);
        assertEquals(five, e2.getKey());

        Map.Entry e3 = map.lowerEntry(one);
        assertNull(e3);

        Map.Entry e4 = map.lowerEntry(zero);
        assertNull(e4);

    }

    /**
     * higherEntry returns next entry.
     */
    public void testHigherEntry() {
        NavigableMap map = map5();
        Map.Entry e1 = map.higherEntry(three);
        assertEquals(four, e1.getKey());

        Map.Entry e2 = map.higherEntry(zero);
        assertEquals(one, e2.getKey());

        Map.Entry e3 = map.higherEntry(five);
        assertNull(e3);

        Map.Entry e4 = map.higherEntry(six);
        assertNull(e4);

    }

    /**
     * floorEntry returns preceding entry.
     */
    public void testFloorEntry() {
        NavigableMap map = map5();
        Map.Entry e1 = map.floorEntry(three);
        assertEquals(three, e1.getKey());

        Map.Entry e2 = map.floorEntry(six);
        assertEquals(five, e2.getKey());

        Map.Entry e3 = map.floorEntry(one);
        assertEquals(one, e3.getKey());

        Map.Entry e4 = map.floorEntry(zero);
        assertNull(e4);

    }

    /**
     * ceilingEntry returns next entry.
     */
    public void testCeilingEntry() {
        NavigableMap map = map5();
        Map.Entry e1 = map.ceilingEntry(three);
        assertEquals(three, e1.getKey());

        Map.Entry e2 = map.ceilingEntry(zero);
        assertEquals(one, e2.getKey());

        Map.Entry e3 = map.ceilingEntry(five);
        assertEquals(five, e3.getKey());

        Map.Entry e4 = map.ceilingEntry(six);
        assertNull(e4);

    }

    /**
     * pollFirstEntry returns entries in order
     */
    public void testPollFirstEntry() {
        NavigableMap map = map5();
        Map.Entry e = map.pollFirstEntry();
        assertEquals(one, e.getKey());
        assertEquals("A", e.getValue());
        e = map.pollFirstEntry();
        assertEquals(two, e.getKey());
        map.put(one, "A");
        e = map.pollFirstEntry();
        assertEquals(one, e.getKey());
        assertEquals("A", e.getValue());
        e = map.pollFirstEntry();
        assertEquals(three, e.getKey());
        map.remove(four);
        e = map.pollFirstEntry();
        assertEquals(five, e.getKey());
        try {
            e.setValue("A");
            shouldThrow();
        } catch (Exception ok) {
        }
        assertTrue(map.isEmpty());
        Map.Entry f = map.firstEntry();
        assertNull(f);
        e = map.pollFirstEntry();
        assertNull(e);
    }

    /**
     * pollLastEntry returns entries in order
     */
    public void testPollLastEntry() {
        NavigableMap map = map5();
        Map.Entry e = map.pollLastEntry();
        assertEquals(five, e.getKey());
        assertEquals("E", e.getValue());
        e = map.pollLastEntry();
        assertEquals(four, e.getKey());
        map.put(five, "E");
        e = map.pollLastEntry();
        assertEquals(five, e.getKey());
        assertEquals("E", e.getValue());
        e = map.pollLastEntry();
        assertEquals(three, e.getKey());
        map.remove(two);
        e = map.pollLastEntry();
        assertEquals(one, e.getKey());
        try {
            e.setValue("E");
            shouldThrow();
        } catch (Exception ok) {
        }
        e = map.pollLastEntry();
        assertNull(e);
    }

    /**
     *   size returns the correct values
     */
    public void testSize() {
        NavigableMap map = map5();
        NavigableMap empty = map0();
	assertEquals(0, empty.size());
	assertEquals(5, map.size());
    }

    /**
     * toString contains toString of elements
     */
    public void testToString() {
        NavigableMap map = map5();
        String s = map.toString();
        for (int i = 1; i <= 5; ++i) {
            assertTrue(s.indexOf(String.valueOf(i)) >= 0);
        }
    }        

    // Exception tests

    /**
     * get(null) of nonempty map throws NPE
     */
    public void testGet_NullPointerException() {
        try {
            NavigableMap c = map5();
            c.get(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * containsKey(null) of nonempty map throws NPE
     */
    public void testContainsKey_NullPointerException() {
        try {
            NavigableMap c = map5();
            c.containsKey(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * put(null,x) throws NPE
     */
    public void testPut1_NullPointerException() {
        try {
            NavigableMap c = map5();
            c.put(null, "whatever");
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * remove(null) throws NPE
     */
    public void testRemove1_NullPointerException() {
        try {
            NavigableMap c = map5();
            c.remove(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * A deserialized map equals original
     */
    public void testSerialization() {
        NavigableMap q = map5();

        try {
            ByteArrayOutputStream bout = new ByteArrayOutputStream(10000);
            ObjectOutputStream out = new ObjectOutputStream(new BufferedOutputStream(bout));
            out.writeObject(q);
            out.close();

            ByteArrayInputStream bin = new ByteArrayInputStream(bout.toByteArray());
            ObjectInputStream in = new ObjectInputStream(new BufferedInputStream(bin));
            NavigableMap r = (NavigableMap)in.readObject();
            assertFalse(r.isEmpty());
            assertEquals(q.size(), r.size());
            assertTrue(q.equals(r));
            assertTrue(r.equals(q));
        } catch(Exception e){
            e.printStackTrace();
            unexpectedException();
        }
    }



    /**
     * subMap returns map with keys in requested range
     */
    public void testSubMapContents() {
        NavigableMap map = map5();
        SortedMap sm = map.subMap(two, four);
        assertEquals(two, sm.firstKey());
        assertEquals(three, sm.lastKey());
        assertEquals(2, sm.size());
        assertFalse(sm.containsKey(one));
        assertTrue(sm.containsKey(two));
        assertTrue(sm.containsKey(three));
        assertFalse(sm.containsKey(four));
        assertFalse(sm.containsKey(five));
        Iterator i = sm.keySet().iterator();
        Object k;
        k = (Integer)(i.next());
        assertEquals(two, k);
        k = (Integer)(i.next());
        assertEquals(three, k);
        assertFalse(i.hasNext());
        Iterator j = sm.keySet().iterator();
        j.next();
        j.remove();
        assertFalse(map.containsKey(two));
        assertEquals(4, map.size());
        assertEquals(1, sm.size());
        assertEquals(three, sm.firstKey());
        assertEquals(three, sm.lastKey());
        assertTrue(sm.remove(three) != null);
        assertTrue(sm.isEmpty());
        assertEquals(3, map.size());
    }

    public void testSubMapContents2() {
        NavigableMap map = map5();
        SortedMap sm = map.subMap(two, three);
        assertEquals(1, sm.size());
        assertEquals(two, sm.firstKey());
        assertEquals(two, sm.lastKey());
        assertFalse(sm.containsKey(one));
        assertTrue(sm.containsKey(two));
        assertFalse(sm.containsKey(three));
        assertFalse(sm.containsKey(four));
        assertFalse(sm.containsKey(five));
        Iterator i = sm.keySet().iterator();
        Object k;
        k = (Integer)(i.next());
        assertEquals(two, k);
        assertFalse(i.hasNext());
        Iterator j = sm.keySet().iterator();
        j.next();
        j.remove();
        assertFalse(map.containsKey(two));
        assertEquals(4, map.size());
        assertEquals(0, sm.size());
        assertTrue(sm.isEmpty());
        assertTrue(sm.remove(three) == null);
        assertEquals(4, map.size());
    }

    /**
     * headMap returns map with keys in requested range
     */
    public void testHeadMapContents() {
        NavigableMap map = map5();
        SortedMap sm = map.headMap(four);
        assertTrue(sm.containsKey(one));
        assertTrue(sm.containsKey(two));
        assertTrue(sm.containsKey(three));
        assertFalse(sm.containsKey(four));
        assertFalse(sm.containsKey(five));
        Iterator i = sm.keySet().iterator();
        Object k;
        k = (Integer)(i.next());
        assertEquals(one, k);
        k = (Integer)(i.next());
        assertEquals(two, k);
        k = (Integer)(i.next());
        assertEquals(three, k);
        assertFalse(i.hasNext());
        sm.clear();
        assertTrue(sm.isEmpty());
        assertEquals(2, map.size());
        assertEquals(four, map.firstKey());
    }

    /**
     * headMap returns map with keys in requested range
     */
    public void testTailMapContents() {
        NavigableMap map = map5();
        SortedMap sm = map.tailMap(two);
        assertFalse(sm.containsKey(one));
        assertTrue(sm.containsKey(two));
        assertTrue(sm.containsKey(three));
        assertTrue(sm.containsKey(four));
        assertTrue(sm.containsKey(five));
        Iterator i = sm.keySet().iterator();
        Object k;
        k = (Integer)(i.next());
        assertEquals(two, k);
        k = (Integer)(i.next());
        assertEquals(three, k);
        k = (Integer)(i.next());
        assertEquals(four, k);
        k = (Integer)(i.next());
        assertEquals(five, k);
        assertFalse(i.hasNext());

        Iterator ei = sm.entrySet().iterator();
        Map.Entry e;
        e = (Map.Entry)(ei.next());
        assertEquals(two, e.getKey());
        assertEquals("B", e.getValue());
        e = (Map.Entry)(ei.next());
        assertEquals(three, e.getKey());
        assertEquals("C", e.getValue());
        e = (Map.Entry)(ei.next());
        assertEquals(four, e.getKey());
        assertEquals("D", e.getValue());
        e = (Map.Entry)(ei.next());
        assertEquals(five, e.getKey());
        assertEquals("E", e.getValue());
        assertFalse(i.hasNext());

        SortedMap ssm = sm.tailMap(four);
        assertEquals(four, ssm.firstKey());
        assertEquals(five, ssm.lastKey());
        assertTrue(ssm.remove(four) != null);
        assertEquals(1, ssm.size());
        assertEquals(3, sm.size());
        assertEquals(4, map.size());
    }
    
}
