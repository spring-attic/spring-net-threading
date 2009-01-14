/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * http://creativecommons.org/licenses/publicdomain
 * Other contributors include Andrew Wright, Jeffrey Hayes,
 * Pat Fisher, Mike Judd.
 */

import junit.framework.*;
import edu.emory.mathcs.backport.java.util.concurrent.atomic.*;
import java.io.*;
import edu.emory.mathcs.backport.java.util.*;

public class AtomicReferenceArrayTest extends JSR166TestCase
{
    public static void main (String[] args) {
        junit.textui.TestRunner.run (suite());
    }
    public static Test suite() {
        return new TestSuite(AtomicReferenceArrayTest.class);
    }

    /**
     * constructor creates array of given size with all elements null
     */
    public void testConstructor(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            assertNull(ai.get(i));
        }
    }

    /**
     * constructor with null array throws NPE
     */
    public void testConstructor2NPE() {
        try {
            Integer[] a = null;
            AtomicReferenceArray ai = new AtomicReferenceArray(a);
        } catch (NullPointerException success) {
        } catch (Exception ex) {
            unexpectedException();
        }
    }

    /**
     * constructor with array is of same size and has all elements
     */
    public void testConstructor2() {
        Integer[] a = { two, one, three, four, seven};
        AtomicReferenceArray ai = new AtomicReferenceArray(a);
        assertEquals(a.length, ai.length());
        for (int i = 0; i < a.length; ++i)
            assertEquals(a[i], ai.get(i));
    }


    /**
     * get and set for out of bound indices throw IndexOutOfBoundsException
     */
    public void testIndexing(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        try {
            ai.get(SIZE);
        } catch(IndexOutOfBoundsException success){
        }
        try {
            ai.get(-1);
        } catch(IndexOutOfBoundsException success){
        }
        try {
            ai.set(SIZE, null);
        } catch(IndexOutOfBoundsException success){
        }
        try {
            ai.set(-1, null);
        } catch(IndexOutOfBoundsException success){
        }
    }

    /**
     * get returns the last value set at index
     */
    public void testGetSet(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            ai.set(i, one);
            assertEquals(one,ai.get(i));
            ai.set(i, two);
            assertEquals(two,ai.get(i));
            ai.set(i, m3);
            assertEquals(m3,ai.get(i));
        }
    }

    /**
     * get returns the last value lazySet at index by same thread
     */
    public void testGetLazySet(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            ai.lazySet(i, one);
            assertEquals(one,ai.get(i));
            ai.lazySet(i, two);
            assertEquals(two,ai.get(i));
            ai.lazySet(i, m3);
            assertEquals(m3,ai.get(i));
        }
    }

    /**
     * compareAndSet succeeds in changing value if equal to expected else fails
     */
    public void testCompareAndSet(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            ai.set(i, one);
            assertTrue(ai.compareAndSet(i, one,two));
            assertTrue(ai.compareAndSet(i, two,m4));
            assertEquals(m4,ai.get(i));
            assertFalse(ai.compareAndSet(i, m5,seven));
            assertFalse((seven.equals(ai.get(i))));
            assertTrue(ai.compareAndSet(i, m4,seven));
            assertEquals(seven,ai.get(i));
        }
    }

    /**
     * compareAndSet in one thread enables another waiting for value
     * to succeed
     */
    public void testCompareAndSetInMultipleThreads() {
        final AtomicReferenceArray a = new AtomicReferenceArray(1);
        a.set(0, one);
        Thread t = new Thread(new Runnable() {
                public void run() {
                    while(!a.compareAndSet(0, two, three)) Thread.yield();
                }});
        try {
            t.start();
            assertTrue(a.compareAndSet(0, one, two));
            t.join(LONG_DELAY_MS);
            assertFalse(t.isAlive());
            assertEquals(a.get(0), three);
        }
        catch(Exception e) {
            unexpectedException();
        }
    }

    /**
     * repeated weakCompareAndSet succeeds in changing value when equal
     * to expected
     */
    public void testWeakCompareAndSet(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            ai.set(i, one);
            while(!ai.weakCompareAndSet(i, one,two));
            while(!ai.weakCompareAndSet(i, two,m4));
            assertEquals(m4,ai.get(i));
            while(!ai.weakCompareAndSet(i, m4,seven));
            assertEquals(seven,ai.get(i));
        }
    }

    /**
     * getAndSet returns previous value and sets to given value at given index
     */
    public void testGetAndSet(){
        AtomicReferenceArray ai = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            ai.set(i, one);
            assertEquals(one,ai.getAndSet(i,zero));
            assertEquals(0,((Integer)ai.getAndSet(i,m10)).intValue());
            assertEquals(m10,ai.getAndSet(i,one));
        }
    }

    /**
     * a deserialized serialized array holds same values
     */
    public void testSerialization() {
        AtomicReferenceArray l = new AtomicReferenceArray(SIZE);
        for (int i = 0; i < SIZE; ++i) {
            l.set(i, new Integer(-i));
        }

        try {
            ByteArrayOutputStream bout = new ByteArrayOutputStream(10000);
            ObjectOutputStream out = new ObjectOutputStream(new BufferedOutputStream(bout));
            out.writeObject(l);
            out.close();

            ByteArrayInputStream bin = new ByteArrayInputStream(bout.toByteArray());
            ObjectInputStream in = new ObjectInputStream(new BufferedInputStream(bin));
            AtomicReferenceArray r = (AtomicReferenceArray) in.readObject();
            assertEquals(l.length(), r.length());
            for (int i = 0; i < SIZE; ++i) {
                assertEquals(r.get(i), l.get(i));
            }
        } catch(Exception e){
            unexpectedException();
        }
    }


    /**
     * toString returns current value.
     */
    public void testToString() {
        Integer[] a = { two, one, three, four, seven};
        AtomicReferenceArray ai = new AtomicReferenceArray(a);
        assertEquals(toString(a), ai.toString());
    }

    private static String toString(Integer[] array) {
        if (array.length == 0)
            return "[]";

        StringBuffer buf = new StringBuffer();
        buf.append('[');
        buf.append(array[0]);

        for (int i = 1; i < array.length; i++) {
            buf.append(", ");
            buf.append(array[i]);
        }

        buf.append("]");
        return buf.toString();
    }
}
