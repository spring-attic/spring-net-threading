/*
 * @test
 * @summary checks to make sure a pipeline of exchangers passes data.
 */
/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain. Use, modify, and
 * redistribute this code in any way without acknowledgement.
 */

import edu.emory.mathcs.backport.java.util.concurrent.*;

public class ExchangeLoops {
    static final ExecutorService pool = Executors.newCachedThreadPool();
    static boolean print = false;

    static class Int {
        public int value;
        Int(int i) { value = i; }
    }


    public static void main(String[] args) throws Exception {
        int maxStages = 100;
        int iters = 100000;

        if (args.length > 0)
            maxStages = Integer.parseInt(args[0]);

        print = false;
        System.out.println("Warmup...");
        oneRun(2, 100000);
        print = true;

        for (int i = 2; i <= maxStages; i += (i+1) >>> 1) {
            System.out.print("Threads: " + i + "\t: ");
            oneRun(i, iters);
        }
        pool.shutdown();
   }

    static class Stage implements Runnable {
        final int iters;
        final Exchanger left;
        final Exchanger right;
        final CyclicBarrier barrier;
        volatile int result;
        Stage (Exchanger left,
               Exchanger right,
               CyclicBarrier b, int iters) {
            this.left = left;
            this.right = right;
            barrier = b;
            this.iters = iters;
        }

        public void run() {
            try {
                barrier.await();
                Int item = new Int(hashCode());
                for (int i = 0; i < iters; ++i) {
                    if (left != null) {
                        item.value = LoopHelpers.compute1(item.value);
                        Int other = (Int)left.exchange(item);
                        if (other == item || other == null)
                            throw new Error("Failed Exchange");
                        item = other;

                    }
                    if (right != null) {
                        item.value = LoopHelpers.compute2(item.value);
                        Int other = (Int)right.exchange(item);
                        if (other == item || other == null)
                            throw new Error("Failed Exchange");
                        item = other;
                    }
                }
                barrier.await();

            }
            catch (Exception ie) {
                ie.printStackTrace();
                return;
            }
        }
    }

    static void oneRun(int nthreads, int iters) throws Exception {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier barrier = new CyclicBarrier(nthreads + 1, timer);
        Exchanger l = null;
        Exchanger r = new Exchanger();
        for (int i = 0; i < nthreads; ++i) {
            pool.execute(new Stage(l, r, barrier, iters));
            l = r;
            r = (i+2 < nthreads) ? new Exchanger() : null;
        }
        barrier.await();
        barrier.await();
        long time = timer.getTime();
        if (print)
            System.out.println(LoopHelpers.rightJustify(time / (iters * nthreads + iters * (nthreads-2))) + " ns per transfer");
    }

}
