
import java.rmi.MarshalledObject;
import java.io.*;
import java.util.List;
import java.util.ArrayList;

import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.PriorityQueue;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.atomic.*;
import edu.emory.mathcs.backport.java.util.concurrent.locks.*;
import java.util.HashMap;
import java.util.Map;
import java.util.Iterator;
import java.util.Collection;

/**
 * Class to test serial compatibility between backport versions
 */
public class SerializationTest {

    final static List ldata;
    final static Map mdata;

    static {
        ldata = new ArrayList();
        ldata.add("s1");
        ldata.add("s2");
        ldata.add("s3");
        mdata = new HashMap();
        mdata.put("key 1", "value 1");
        mdata.put("key 2", new Long(4));
    }

    public static void main(String[] args) throws IOException {
        if (args.length < 2) throw new IllegalArgumentException("Need 2 arguments");
        String op       = args[0];
        String filename = args[1];
        File f = new File(filename);
        if ("-serialize".equals(op) || "serialize".equals(op)) {
            FileOutputStream fout = new FileOutputStream(f);
            OutputStream out = new BufferedOutputStream(fout);
            List objs = createObjects();
            for (Iterator itr = objs.iterator(); itr.hasNext(); ) {
                serializeObject(itr.next(), out);
            }
            out.flush();
            out.close();
        }
        else {
            FileInputStream fin = new FileInputStream(f);
            InputStream in = new BufferedInputStream(fin);
            deserializeObjects(in);
        }
    }

    private static List createObjects() {
        List objs = new ArrayList();

        // collections

        objs.add(new ArrayBlockingQueue(100, false, ldata));
        objs.add(new ArrayDeque(ldata));
        objs.add(new LinkedBlockingDeque(ldata));
        objs.add(new LinkedBlockingQueue(ldata));
        objs.add(new LinkedList(ldata));
        objs.add(new PriorityQueue(ldata));
        objs.add(new PriorityBlockingQueue(ldata));
        CopyOnWriteArrayList cowl = new CopyOnWriteArrayList(ldata);
        objs.add(cowl);
        objs.add(cowl.subList(1, 2));
        objs.add(new CopyOnWriteArraySet(ldata));
        objs.add(new SynchronousQueue(false));
        objs.add(new SynchronousQueue(true));

        ConcurrentHashMap m = new ConcurrentHashMap(mdata);
        objs.add(m);
        //objs.add(m.keySet());
        //objs.add(m.values());
        objs.add(new ConcurrentLinkedQueue(ldata));
        NavigableMap nm = new ConcurrentSkipListMap(mdata);
        objs.add(nm);
        objs.add(nm.subMap("key 0", "key 3"));
        NavigableSet ns = new ConcurrentSkipListSet(mdata.keySet());
        objs.add(ns);
        objs.add(ns.subSet("key 0", "key 3"));
        nm = new TreeMap(mdata);
        objs.add(nm);
        objs.add(nm.subMap("key 0", "key 3"));
        ns = new TreeSet(mdata.keySet());
        objs.add(ns);
        objs.add(ns.subSet("key 0", "key 3"));

        // atomics

        objs.add(new AtomicBoolean(true));
        objs.add(new AtomicInteger(123));
        objs.add(new AtomicIntegerArray(new int[] { 1, 2, 3}));
        objs.add(new AtomicLong(123L));
        objs.add(new AtomicLongArray(new long[] { 1L, 2L, 3L}));
        objs.add(new AtomicReference(new Integer(3)));
        objs.add(new AtomicReferenceArray(new Integer[] {
            new Integer(1), new Integer(2), new Integer(3)}));

        // locks

        serializeLock(objs, new ReentrantLock(false));
        serializeLock(objs, new ReentrantLock(true));
        ReentrantReadWriteLock rr = new ReentrantReadWriteLock();
        objs.add(rr);
        serializeLock(objs, rr.readLock());
        serializeLock(objs, rr.writeLock());
        serializeSemaphore(objs, new Semaphore(10, false));
        serializeSemaphore(objs, new Semaphore(10, true));

        // other
        objs.add(TimeUnit.DAYS);
        objs.add(TimeUnit.HOURS);
        objs.add(TimeUnit.MINUTES);
        objs.add(TimeUnit.SECONDS);
        objs.add(TimeUnit.MILLISECONDS);
        objs.add(TimeUnit.MICROSECONDS);
        objs.add(TimeUnit.NANOSECONDS);

        return objs;
    }

    private static void serializeLock(List objs, Lock l) {
        l.lock();
        try {
            objs.add(l);
            objs.add(l.newCondition());
        }
        catch (UnsupportedOperationException e) {}
        finally {
            l.unlock();
        }
    }

    private static void serializeSemaphore(List objs, Semaphore s) {
        s.acquireUninterruptibly();
        try {
            objs.add(s);
        }
        finally {
            s.release();
        }
    }

    private static void serializeObject(Object obj, OutputStream out) {
        try {
            ByteArrayOutputStream bos = new ByteArrayOutputStream();
            ObjectOutputStream oos = new ObjectOutputStream(bos);
            oos.writeObject(obj);
            oos.flush();
            oos.close();
            int size = bos.size();
            DataOutputStream dout = new DataOutputStream(out);
            dout.writeInt(size);
            bos.writeTo(dout);
        }
        catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    private static void deserializeObjects(InputStream in) throws IOException {
        DataInput din = new DataInputStream(in);
        while (true) {
            int size;
            try {
                size = din.readInt();
            }
            catch (EOFException e) {
                return;
            }
            byte[] arr = new byte[size];
            din.readFully(arr);
            ByteArrayInputStream bin = new ByteArrayInputStream(arr);
            ObjectInputStream oin = new ObjectInputStream(bin);
            try {
                Object obj = oin.readObject();
                System.out.println(obj);
                if (obj instanceof Lock) {
                    Lock l = (Lock)obj;
                    l.lock();
                    l.unlock();
                }
                else if (obj instanceof ReadWriteLock) {
                    ReadWriteLock rl = (ReadWriteLock)obj;
                    Lock r = rl.readLock();
                    Lock w = rl.writeLock();
                    r.lock();
                    r.unlock();
                    w.newCondition();
                    w.lock();
                    w.unlock();
                }
            }
            catch (Throwable e) {
                e.printStackTrace();
            }
        }
    }
}
