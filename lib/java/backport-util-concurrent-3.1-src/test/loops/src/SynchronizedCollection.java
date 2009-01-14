// Stand-alone version of java.util.Collections.synchronizedCollection
import edu.emory.mathcs.backport.java.util.*;
import java.io.*;
import java.util.Collection;
import java.util.Iterator;
import java.util.ArrayList;

public final class SynchronizedCollection implements Collection, Serializable {
    // use serialVersionUID from JDK 1.2.2 for interoperability
    private static final long serialVersionUID = 3053995032091335093L;

    final Collection c;	   // Backing Collection
    final Object	   mutex;  // Object on which to synchronize

    public SynchronizedCollection(Collection c) {
        if (c==null)
            throw new NullPointerException();
        this.c = c;
        mutex = this;
    }
    public SynchronizedCollection(Collection c, Object mutex) {
        this.c = c;
        this.mutex = mutex;
    }

    public SynchronizedCollection() {
        this(new ArrayList());
    }

    public final int size() {
        synchronized(mutex) {return c.size();}
    }
    public final boolean isEmpty() {
        synchronized(mutex) {return c.isEmpty();}
    }
    public final boolean contains(Object o) {
        synchronized(mutex) {return c.contains(o);}
    }
    public final Object[] toArray() {
        synchronized(mutex) {return c.toArray();}
    }
    public final Object[] toArray(Object[] a) {
        synchronized(mutex) {return c.toArray(a);}
    }

    public final Iterator iterator() {
        return c.iterator(); // Must be manually synched by user!
    }

    public final boolean add(Object e) {
        synchronized(mutex) {return c.add(e);}
    }
    public final boolean remove(Object o) {
        synchronized(mutex) {return c.remove(o);}
    }

    public final boolean containsAll(Collection coll) {
        synchronized(mutex) {return c.containsAll(coll);}
    }
    public final boolean addAll(Collection coll) {
        synchronized(mutex) {return c.addAll(coll);}
    }
    public final boolean removeAll(Collection coll) {
        synchronized(mutex) {return c.removeAll(coll);}
    }
    public final boolean retainAll(Collection coll) {
        synchronized(mutex) {return c.retainAll(coll);}
    }
    public final void clear() {
        synchronized(mutex) {c.clear();}
    }
    public final String toString() {
        synchronized(mutex) {return c.toString();}
    }
    private void writeObject(ObjectOutputStream s) throws IOException {
        synchronized(mutex) {s.defaultWriteObject();}
    }
}
