import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.locks.*;
import edu.emory.mathcs.backport.java.util.LinkedList;

import java.util.Collection;
import java.util.Iterator;
import java.util.AbstractCollection;

public class SynchronizedLinkedListQueue
    extends AbstractCollection implements Queue {
    private final Queue q = new LinkedList();

    public synchronized Iterator iterator() {
        return q.iterator();
    }

    public synchronized boolean isEmpty() {
        return q.isEmpty();
    }
    public synchronized int size() {
        return q.size();
    }
    public synchronized boolean offer(Object o) {
        return q.offer(o);
    }
    public synchronized boolean add(Object o) {
        return q.add(o);
    }
    public synchronized Object poll() {
        return q.poll();
    }
    public synchronized Object remove() {
        return q.remove();
    }
    public synchronized Object peek() {
        return q.peek();
    }
    public synchronized Object element() {
        return q.element();
    }

    public synchronized boolean contains(Object o) {
        return q.contains(o);
    }
    public synchronized Object[] toArray() {
        return q.toArray();
    }
    public synchronized Object[] toArray(Object[] a) {
        return q.toArray(a);
    }
    public synchronized boolean remove(Object o) {
        return q.remove(o);
    }

    public synchronized boolean containsAll(Collection coll) {
        return q.containsAll(coll);
    }
    public synchronized boolean addAll(Collection coll) {
        return q.addAll(coll);
    }
    public synchronized boolean removeAll(Collection coll) {
        return q.removeAll(coll);
    }
    public synchronized boolean retainAll(Collection coll) {
        return q.retainAll(coll);
    }
    public synchronized void clear() {
        q.clear();
    }
    public synchronized String toString() {
        return q.toString();
    }

}

