/*
 * @test %I% %E%
 * @bug 4486658
 * @compile -source 1.5 ConcurrentDequeLoops.java
 * @run main/timeout=230 ConcurrentDequeLoops
 * @summary Checks that a set of threads can repeatedly get and modify items
 */
/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain. Use, modify, and
 * redistribute this code in any way without acknowledgement.
 */

import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.atomic.*;
import edu.emory.mathcs.backport.java.util.concurrent.helpers.*;
import java.util.ArrayList;

public class ConcurrentDequeLoops {
    static final ExecutorService pool = Executors.newCachedThreadPool();
    static AtomicInteger totalItems;
    static boolean print = false;

    public static void main(String[] args) throws Exception {
        int maxStages = 8;
        int items = 1000000;

        Class klass = null;
        if (args.length > 0) {
            try {
                klass = Class.forName(args[0]);
            } catch(ClassNotFoundException e) {
                throw new RuntimeException("Class " + args[0] + " not found.");
            }
        }
        else
            throw new Error();

        if (args.length > 1)
            maxStages = Integer.parseInt(args[1]);

        System.out.print("Class: " + klass.getName());
        System.out.println(" stages: " + maxStages);

        print = false;
        System.out.println("Warmup...");
        oneRun(klass, 1, items);
        Thread.sleep(100);
        oneRun(klass, 1, items);
        Thread.sleep(100);
        print = true;

        int k = 1;
        for (int i = 1; i <= maxStages;) {
            oneRun(klass, i, items);
            if (i == k) {
                k = i << 1;
                i = i + (i >>> 1);
            }
            else
                i = k;
        }
        pool.shutdown();
   }

    static class Stage implements Callable {
        final Deque queue;
        final CyclicBarrier barrier;
        final LoopHelpers.SimpleRandom rng = new LoopHelpers.SimpleRandom();
        int items;
        Stage (Deque q, CyclicBarrier b, int items) {
            queue = q;
            barrier = b;
            this.items = items;
        }

        public Object call() {
            // Repeatedly take something from queue if possible,
            // transform it, and put back in.
            try {
                barrier.await();
                int l = (int)Utils.nanoTime();
                int takes = 0;
                for (;;) {
                    Integer item;
                    int rnd = rng.next();
                    if ((rnd & 1) == 0)
                        item = (Integer)queue.pollFirst();
                    else
                        item = (Integer)queue.pollLast();
                    if (item != null) {
                        ++takes;
                        l += LoopHelpers.compute2(item.intValue());
                    }
                    else if (takes != 0) {
                        totalItems.getAndAdd(-takes);
                        takes = 0;
                    }
                    else if (totalItems.get() <= 0)
                        break;
                    l = LoopHelpers.compute1(l);
                    if (items > 0) {
                        --items;
                        Integer res = new Integer(l);
                        if ((rnd & 16) == 0)
                            queue.addFirst(res);
                        else
                            queue.addLast(res);
                    }
                    else { // spinwait
                        for (int k = 1 + (l & 15); k != 0; --k)
                            l = LoopHelpers.compute1(LoopHelpers.compute2(l));
                        if ((l & 3) == 3) {
                            Thread.sleep(1);
                        }
                    }
                }
                return new Integer(l);
            }
            catch (Exception ie) {
                ie.printStackTrace();
                throw new Error("Call loop failed");
            }
        }
    }

    static void oneRun(Class klass, int n, int items) throws Exception {
        Deque q = (Deque)klass.newInstance();
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier barrier = new CyclicBarrier(n + 1, timer);
        totalItems = new AtomicInteger(n * items);
        ArrayList results = new ArrayList(n);
        for (int i = 0; i < n; ++i)
            results.add(pool.submit(new Stage(q, barrier, items)));

        if (print)
            System.out.print("Threads: " + n + "\t:");
        barrier.await();
        int total = 0;
        for (int i = 0; i < n; ++i) {
            Future f = (Future)results.get(i);
            Integer r = (Integer)f.get();
            total += r.intValue();
        }
        long endTime = Utils.nanoTime();
        long time = endTime - timer.startTime;
        if (print)
            System.out.println(LoopHelpers.rightJustify(time / (items * n)) + " ns per item");
        if (total == 0) // avoid overoptimization
            System.out.println("useless result: " + total);

    }
}
