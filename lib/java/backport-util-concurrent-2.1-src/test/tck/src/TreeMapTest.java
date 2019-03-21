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

public class TreeMapTest extends JSR166TestCase {
    public static void main(String[] args) {
	junit.textui.TestRunner.run (suite());	
    }
    public static Test suite() {
	return new TestSuite(TreeMapTest.class);
    }

    /**
     * Create a map from Integers 1-5 to Strings "A"-"E".
     */
    private static TreeMap map5() {   
	TreeMap map = new TreeMap();
        assertTrue(map.isEmpty());
	map.put(one, "A");
	map.put(five, "E");
	map.put(three, "C");
	map.put(two, "B");
	map.put(four, "D");
        assertFalse(map.isEmpty());
        assertEquals(5, map.size());
	return map;
    }

    /**
     *  clear removes all pairs
     */
    public void testClear() {
        TreeMap map = map5();
	map.clear();
	assertEquals(map.size(), 0);
    }

    /**
     *  
     */
    public void testConstructFromSorted() {
        TreeMap map = map5();
        TreeMap map2 = new TreeMap(map);
        assertEquals(map, map2);
    }

    /**
     *  Maps with same contents are equal
     */
    public void testEquals() {
        TreeMap map1 = map5();
        TreeMap map2 = map5();
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
        TreeMap map = map5();
	assertTrue(map.containsKey(one));
        assertFalse(map.containsKey(zero));
    }

    /**
     *  containsValue returns true for held values
     */
    public void testContainsValue() {
        TreeMap map = map5();
	assertTrue(map.containsValue("A"));
        assertFalse(map.containsValue("Z"));
    }

    /**
     *  get returns the correct element at the given key,
     *  or null if not present
     */
    public void testGet() {
        TreeMap map = map5();
	assertEquals("A", (String)map.get(one));
        TreeMap empty = new TreeMap();
        assertNull(empty.get(one));
    }

    /**
     *  isEmpty is true of empty map and false for non-empty
     */
    public void testIsEmpty() {
        TreeMap empty = new TreeMap();
        TreeMap map = map5();
	assertTrue(empty.isEmpty());
        assertFalse(map.isEmpty());
    }

    /**
     *   firstKey returns first key
     */
    public void testFirstKey() {
        TreeMap map = map5();
	assertEquals(one, map.firstKey());
    }

    /**
     *   lastKey returns last key
     */
    public void testLastKey() {
        TreeMap map = map5();
	assertEquals(five, map.lastKey());
    }


    /**
     *  keySet.toArray returns contains all keys
     */
    public void testKeySetToArray() {
        TreeMap map = map5();
	Set s = map.keySet();
        Object[] ar = s.toArray();
        assertTrue(s.containsAll(Arrays.asList(ar)));
	assertEquals(5, ar.length);
        ar[0] = m10;
        assertFalse(s.containsAll(Arrays.asList(ar)));
    }

    /**
     *  descendingkeySet.toArray returns contains all keys
     */
    public void testDescendingKeySetToArray() {
        TreeMap map = map5();
	Set s = map.descendingKeySet();
        Object[] ar = s.toArray();
	assertEquals(5, ar.length);
        assertTrue(s.containsAll(Arrays.asList(ar)));
        ar[0] = m10;
        assertFalse(s.containsAll(Arrays.asList(ar)));
    }

    /**
     *   keySet returns a Set containing all the keys
     */
    public void testKeySet() {
        TreeMap map = map5();
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
        TreeMap map = map5();
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
     *   descendingKeySet is ordered
     */
    public void testDescendingKeySetOrder() {
        TreeMap map = map5();
	Set s = map.descendingKeySet();
        Iterator i = s.iterator();
        Integer last = (Integer)i.next();
        assertEquals(last, five);
        while (i.hasNext()) {
            Integer k = (Integer)i.next();
            assertTrue(last.compareTo(k) > 0);
            last = k;
        }
    }

    /**
     * values collection contains all values
     */
    public void testValues() {
        TreeMap map = map5();
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
        TreeMap map = map5();
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
     * descendingEntrySet contains all pairs
     */
    public void testDescendingEntrySet() {
        TreeMap map = map5();
	Set s = map.descendingEntrySet();
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
     *  entrySet.toArray contains all entries
     */
    public void testEntrySetToArray() {
        TreeMap map = map5();
	Set s = map.entrySet();
        Object[] ar = s.toArray();
        assertEquals(5, ar.length);
        for (int i = 0; i < 5; ++i) {
            assertTrue(map.containsKey(((Map.Entry)(ar[i])).getKey()));
            assertTrue(map.containsValue(((Map.Entry)(ar[i])).getValue()));
        }
    }

    /**
     *  descendingEntrySet.toArray contains all entries
     */
    public void testDescendingEntrySetToArray() {
        TreeMap map = map5();
	Set s = map.descendingEntrySet();
        Object[] ar = s.toArray();
        assertEquals(5, ar.length);
        for (int i = 0; i < 5; ++i) {
            assertTrue(map.containsKey(((Map.Entry)(ar[i])).getKey()));
            assertTrue(map.containsValue(((Map.Entry)(ar[i])).getValue()));
        }
    }

    /**
     *   putAll  adds all key-value pairs from the given map
     */
    public void testPutAll() {
        TreeMap empty = new TreeMap();
        TreeMap map = map5();
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
        TreeMap map = map5();
	map.remove(five);
	assertEquals(4, map.size());
	assertFalse(map.containsKey(five));
    }

    /**
     * lowerEntry returns preceding entry.
     */
    public void testLowerEntry() {
        TreeMap map = map5();
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
        TreeMap map = map5();
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
        TreeMap map = map5();
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
        TreeMap map = map5();
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
     * lowerKey returns preceding element
     */
    public void testLowerKey() {
        TreeMap q = map5();
        Object e1 = q.lowerKey(three);
        assertEquals(two, e1);

        Object e2 = q.lowerKey(six);
        assertEquals(five, e2);

        Object e3 = q.lowerKey(one);
        assertNull(e3);

        Object e4 = q.lowerKey(zero);
        assertNull(e4);

    }

    /**
     * higherKey returns next element
     */
    public void testHigherKey() {
        TreeMap q = map5();
        Object e1 = q.higherKey(three);
        assertEquals(four, e1);

        Object e2 = q.higherKey(zero);
        assertEquals(one, e2);

        Object e3 = q.higherKey(five);
        assertNull(e3);

        Object e4 = q.higherKey(six);
        assertNull(e4);

    }

    /**
     * floorKey returns preceding element
     */
    public void testFloorKey() {
        TreeMap q = map5();
        Object e1 = q.floorKey(three);
        assertEquals(three, e1);

        Object e2 = q.floorKey(six);
        assertEquals(five, e2);

        Object e3 = q.floorKey(one);
        assertEquals(one, e3);

        Object e4 = q.floorKey(zero);
        assertNull(e4);

    }

    /**
     * ceilingKey returns next element
     */
    public void testCeilingKey() {
        TreeMap q = map5();
        Object e1 = q.ceilingKey(three);
        assertEquals(three, e1);

        Object e2 = q.ceilingKey(zero);
        assertEquals(one, e2);

        Object e3 = q.ceilingKey(five);
        assertEquals(five, e3);

        Object e4 = q.ceilingKey(six);
        assertNull(e4);

    }

    /**
     * pollFirstEntry returns entries in order
     */
    public void testPollFirstEntry() {
        TreeMap map = map5();
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
        e = map.pollFirstEntry();
        assertNull(e);
    }

    /**
     * pollLastEntry returns entries in order
     */
    public void testPollLastEntry() {
        TreeMap map = map5();
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
        TreeMap map = map5();
        TreeMap empty = new TreeMap();
	assertEquals(0, empty.size());
	assertEquals(5, map.size());
    }

    /**
     * toString contains toString of elements
     */
    public void testToString() {
        TreeMap map = map5();
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
            TreeMap c = map5();
            c.get(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * containsKey(null) of nonempty map throws NPE
     */
    public void testContainsKey_NullPointerException() {
        try {
            TreeMap c = map5();
            c.containsKey(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * remove(null) throws NPE for nonempty map
     */
    public void testRemove1_NullPointerException() {
        try {
            TreeMap c = new TreeMap();
            c.put("sadsdf", "asdads");
            c.remove(null);
            shouldThrow();
        } catch(NullPointerException e){}
    }

    /**
     * A deserialized map equals original
     */
    public void testSerialization() {
        TreeMap q = map5();

        try {
            ByteArrayOutputStream bout = new ByteArrayOutputStream(10000);
            ObjectOutputStream out = new ObjectOutputStream(new BufferedOutputStream(bout));
            out.writeObject(q);
            out.close();

            ByteArrayInputStream bin = new ByteArrayInputStream(bout.toByteArray());
            ObjectInputStream in = new ObjectInputStream(new BufferedInputStream(bin));
            TreeMap r = (TreeMap)in.readObject();
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
        TreeMap map = map5();
        NavigableMap sm = map.navigableSubMap(two, four);
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
        Iterator r = sm.descendingKeySet().iterator();
        k = (Integer)(r.next());
        assertEquals(three, k);
        k = (Integer)(r.next());
        assertEquals(two, k);
        assertFalse(r.hasNext());
        
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
        TreeMap map = map5();
        NavigableMap sm = map.navigableSubMap(two, three);
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
        Iterator r = sm.descendingKeySet().iterator();
        k = (Integer)(r.next());
        assertEquals(two, k);
        assertFalse(r.hasNext());
        
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
        TreeMap map = map5();
        NavigableMap sm = map.navigableHeadMap(four);
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
        TreeMap map = map5();
        NavigableMap sm = map.navigableTailMap(two);
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
        Iterator r = sm.descendingKeySet().iterator();
        k = (Integer)(r.next());
        assertEquals(five, k);
        k = (Integer)(r.next());
        assertEquals(four, k);
        k = (Integer)(r.next());
        assertEquals(three, k);
        k = (Integer)(r.next());
        assertEquals(two, k);
        assertFalse(r.hasNext());

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

        NavigableMap ssm = sm.navigableTailMap(four);
        assertEquals(four, ssm.firstKey());
        assertEquals(five, ssm.lastKey());
        assertTrue(ssm.remove(four) != null);
        assertEquals(1, ssm.size());
        assertEquals(3, sm.size());
        assertEquals(4, map.size());
    }
    
}
