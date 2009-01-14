/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * http://creativecommons.org/licenses/publicdomain
 */
import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.locks.*;
import java.util.Collection;
import java.util.Iterator;
import java.util.ArrayList;


/**
 * This is an incomplete implementation of a wrapper class
 * that places read-write locks around unsynchronized Collections.
 * Exists as a sample input for CollectionLoops test.
 */

public final class RWCollection implements Collection {
    private final Collection c;
    private final ReentrantReadWriteLock rwl = new ReentrantReadWriteLock();

    public RWCollection(Collection c) {
        if (c == null)
            throw new NullPointerException();
        this.c = c;
    }

    public RWCollection() {
        this(new ArrayList());
    }

    public final int size() {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.size();} finally { l.unlock(); }
    }
    public final boolean isEmpty(){
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.isEmpty();} finally { l.unlock(); }
    }

    public final boolean contains(Object o) {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.contains(o);} finally { l.unlock(); }
    }

    public final boolean equals(Object o) {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.equals(o);} finally { l.unlock(); }
    }
    public final int hashCode() {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.hashCode();} finally { l.unlock(); }
    }
    public final String toString() {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.toString();} finally { l.unlock(); }
    }

    public final Iterator iterator() {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try {return c.iterator();} finally { l.unlock(); }
    }

    public final Object[] toArray() {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try  {return c.toArray();} finally { l.unlock(); }
    }
    public final Object[] toArray(Object[] a) {
        final ReentrantReadWriteLock.ReadLock l =  (ReentrantReadWriteLock.ReadLock)rwl.readLock();
        l.lock(); try  {return c.toArray(a);} finally { l.unlock(); }
    }

    public final boolean add(Object e) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.add(e);} finally { l.unlock(); }
    }
    public final boolean remove(Object o) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.remove(o);} finally { l.unlock(); }
    }

    public final boolean containsAll(Collection coll) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.containsAll(coll);} finally { l.unlock(); }
    }
    public final boolean addAll(Collection coll) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.addAll(coll);} finally { l.unlock(); }
    }
    public final boolean removeAll(Collection coll) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.removeAll(coll);} finally { l.unlock(); }
    }
    public final boolean retainAll(Collection coll) {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {return c.retainAll(coll);} finally { l.unlock(); }
    }
    public final void clear() {
        final ReentrantReadWriteLock.WriteLock l =  (ReentrantReadWriteLock.WriteLock)rwl.writeLock();
        l.lock(); try  {c.clear();} finally { l.unlock(); }
    }

}
