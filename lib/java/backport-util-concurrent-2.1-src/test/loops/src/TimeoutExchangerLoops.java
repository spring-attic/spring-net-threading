/*
 * Written by Bill Scherer and Doug Lea with assistance from members
 * of JCP JSR-166 Expert Group and released to the public domain. Use,
 * modify, and redistribute this code in any way without
 * acknowledgement.
 */

import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.atomic.*;
import edu.emory.mathcs.backport.java.util.concurrent.locks.*;

public class TimeoutExchangerLoops {
    static final int  DEFAULT_THREADS        = 32;
    static final long DEFAULT_TRIAL_MILLIS   = 5000;
    static final long DEFAULT_PATIENCE_NANOS = 500000;

    static final ExecutorService pool = Executors.newCachedThreadPool();

    public static void main(String[] args) throws Exception {
        int maxThreads = DEFAULT_THREADS;
        long trialMillis = DEFAULT_TRIAL_MILLIS;
        long patienceNanos = DEFAULT_PATIENCE_NANOS;

        // Parse and check args
        int argc = 0;
        try {
            while (argc < args.length) {
                String option = args[argc++];
		if (option.equals("-t"))
		    trialMillis = Integer.parseInt(args[argc]);
		else if (option.equals("-p"))
		    patienceNanos = Long.parseLong(args[argc]);
                else
                    maxThreads = Integer.parseInt(option);
                argc++;
            }
        }
        catch (Exception e) {
            e.printStackTrace();
            System.exit(0);
        }

	// Display runtime parameters
	System.out.print("TimeoutExchangerTest");
	System.out.print(" -t " + trialMillis);
	System.out.print(" -p " + patienceNanos);
	System.out.print(" max threads " + maxThreads);
	System.out.println();

        // warmup
        System.out.print("Threads: " + 2 + "\t");
        oneRun(2, trialMillis, patienceNanos);
        Thread.sleep(100);

        int k = 4;
        for (int i = 2; i <= maxThreads;) {
            System.out.print("Threads: " + i + "\t");
            oneRun(i, trialMillis, patienceNanos);
            Thread.sleep(100);
            if (i == k) {
                k = i << 1;
                i = i + (i >>> 1);
            }
            else
                i = k;
        }
        pool.shutdown();
    }

    static void oneRun(int nThreads, long trialMillis, long patienceNanos)
        throws Exception {
        CyclicBarrier barrier = new CyclicBarrier(nThreads+1);
        long stopTime = System.currentTimeMillis() + trialMillis;
        Exchanger x = new Exchanger();
        Runner[] runners = new Runner[nThreads];
        for (int i = 0; i < nThreads; ++i)
            runners[i] = new Runner(x, stopTime, patienceNanos, barrier);
        for (int i = 0; i < nThreads; ++i)
            pool.execute(runners[i]);
        barrier.await();
        barrier.await();
        long iters = 0;
        long fails = 0;
        long check = 0;
        for (int i = 0; i < nThreads; ++i) {
            iters += runners[i].iterations;
            fails += runners[i].failures;
            check += runners[i].mine.value;
        }
        if (check != iters)
            throw new Error("bad checksum " + iters + "/" + check);
        long rate = (iters * 1000) / trialMillis;
        double failRate = (fails * 100.0) / (double)iters;
        System.out.print(LoopHelpers.rightJustify(rate) + " iterations/s ");
        System.out.print(failRate);
        System.out.print("% timeouts");
        System.out.println();
    }

    static final class MutableInt {
	int value;
    }

    static final class Runner implements Runnable {
        final Exchanger x;
        volatile long iterations;
        volatile long failures;
        volatile MutableInt mine;
        final long stopTime;
        final long patience;
        final CyclicBarrier barrier;
        Runner(Exchanger x, long stopTime,
               long patience, CyclicBarrier b) {
            this.x = x;
            this.stopTime = stopTime;
            this.patience = patience;
            this.barrier = b;
            mine = new MutableInt();
        }

        public void run() {
            try {
                barrier.await();
                MutableInt m = mine;
                int i = 0;
                int fails = 0;
                do {
                    try {
                        ++i;
                        m.value++;
                        m = (MutableInt)x.exchange(m, patience, TimeUnit.NANOSECONDS);
                    } catch (TimeoutException to) {
                        if (System.currentTimeMillis() >= stopTime)
                            break;
                        else
                            ++fails;
                    }
                } while ((i & 127) != 0 || // only check time periodically
                         System.currentTimeMillis() < stopTime);

                mine = m;
                iterations = i;
                failures = fails;
                barrier.await();
            } catch(Exception e) {
                e.printStackTrace();
                return;
            }
        }
    }
}

