/*
 * Written by Doug Lea and released to the public domain, as explained at
 * http://creativecommons.org/licenses/publicdomain
 */

/*
 * Estimates the difference in time for compareAndSet and CAS-like
 * operations versus unsynchronized, non-volatile pseudo-CAS when
 * updating random numbers. These estimates thus give the cost
 * of atomicity/barriers/exclusion over and above the time to
 * just compare and conditionally store (int) values, so are
 * not intended to measure the "raw" cost of a CAS.
 *
 * Outputs, in nanoseconds:
 *  "Atomic CAS"      AtomicInteger.compareAndSet
 *  "Updater CAS"     CAS first comparing args
 *  "Volatile"        pseudo-CAS using volatile store if comparison succeeds
 *  "Mutex"           emulated compare and set done under AQS-based mutex lock
 *  "Synchronized"    emulated compare and set done under a synchronized block.
 *
 * By default, these are printed for 1..#cpus threads, but you can
 * change the upper bound number of threads by providing the
 * first argument to this program.
 *
 * The last two kinds of runs (mutex and synchronized) are done only
 * if this program is called with (any) second argument
 */
using System;
using Spring.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Locks;

public class CASLoops
{

    internal const int TRIALS = 2;
    internal const long BASE_SECS_PER_RUN = 4;
    internal static readonly int NCPUS = 1; 
    internal static int maxThreads;

    internal static bool includeLocks = false;

    static CASLoops()
    {
        System.Management.SelectQuery query = new System.Management.SelectQuery(
            "SELECT NumberOfProcessors FROM Win32_ComputerSystem");

        System.Management.ManagementObjectSearcher searcher =
            new System.Management.ManagementObjectSearcher(query); 

        System.Management.ManagementObjectCollection results = searcher.Get();

        foreach (System.Management.ManagementBaseObject obj in results)
        {
            foreach (System.Management.PropertyData data in obj.Properties)
            {
                if (data.Value != null) NCPUS = Convert.ToInt32(data.Value);     
            }
        }
    }

    [STAThread]
    public static void main(System.String[] args)
    {
        maxThreads = NCPUS;
        if (args.Length > 0)
            maxThreads = System.Int32.Parse(args[0]);

        loopIters = new long[maxThreads + 1];

        if (args.Length > 1)
            includeLocks = true;

        System.Console.Out.WriteLine("Warmup...");
        for (int i = maxThreads; i > 0; --i)
        {
            runCalibration(i, 10);
            oneRun(i, loopIters[i] / 4, false);
            System.Console.Out.Write(".");
        }

        for (int i = 1; i <= maxThreads; ++i)
            loopIters[i] = 0;

        for (int j = 0; j < 2; ++j)
        {
            for (int i = 1; i <= maxThreads; ++i)
            {
                runCalibration(i, 1000);
                oneRun(i, loopIters[i] / 8, false);
                System.Console.Out.Write(".");
            }
        }

        for (int i = 1; i <= maxThreads; ++i)
            loopIters[i] = 0;

        for (int j = 0; j < TRIALS; ++j)
        {
            System.Console.Out.WriteLine("Trial " + j);
            for (int i = 1; i <= maxThreads; ++i)
            {
                runCalibration(i, BASE_SECS_PER_RUN * 1000L);
                oneRun(i, loopIters[i], true);
            }
        }
    }

    //UPGRADE_NOTE: Final was removed from the declaration of 'totalIters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal static readonly AtomicLong totalIters = new AtomicLong(0);
    //UPGRADE_NOTE: Final was removed from the declaration of 'successes '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal static readonly AtomicLong successes = new AtomicLong(0);
    //UPGRADE_NOTE: Final was removed from the declaration of 'sum '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal static readonly AtomicInteger sum = new AtomicInteger(0);

    //UPGRADE_NOTE: Final was removed from the declaration of 'rng '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal static readonly LoopHelpers.MarsagliaRandom rng = new LoopHelpers.MarsagliaRandom();

    internal static long[] loopIters;

    internal sealed class NonAtomicInteger
    {
        internal volatile int readBarrier;
        internal int value_Renamed;

        internal NonAtomicInteger()
        {
        }
        internal int get_Renamed()
        {
            int junk = readBarrier;
            return value_Renamed;
        }
        internal bool compareAndSet(int cmp, int val)
        {
            if (value_Renamed == cmp)
            {
                value_Renamed = val;
                return true;
            }
            return false;
        }
        internal void set_Renamed(int val)
        {
            value_Renamed = val;
        }
    }

    //    static final class UpdaterAtomicInteger {
    //        volatile int value;
    //
    //        static final AtomicIntegerFieldUpdater<UpdaterAtomicInteger>
    //                valueUpdater = AtomicIntegerFieldUpdater.newUpdater
    //                (UpdaterAtomicInteger.class, "value");
    //
    //
    //        UpdaterAtomicInteger() {}
    //        int get() {
    //            return value;
    //        }
    //        boolean compareAndSet(int cmp, int val) {
    //            return valueUpdater.compareAndSet(this, cmp, val);
    //        }
    //
    //        void set(int val) { value = val; }
    //    }
    //
    internal sealed class VolatileInteger
    {
        internal volatile int readBarrier;
        internal volatile int value_Renamed;

        internal VolatileInteger()
        {
        }
        internal int get_Renamed()
        {
            int junk = readBarrier;
            return value_Renamed;
        }
        internal bool compareAndSet(int cmp, int val)
        {
            if (value_Renamed == cmp)
            {
                value_Renamed = val;
                return true;
            }
            return false;
        }
        internal void set_Renamed(int val)
        {
            value_Renamed = val;
        }
    }

    internal sealed class SynchedInteger
    {
        internal int value_Renamed;

        internal SynchedInteger()
        {
        }
        internal int get_Renamed()
        {
            return value_Renamed;
        }
        //UPGRADE_NOTE: Synchronized keyword was removed from method 'compareAndSet'. Lock expression was added. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1027'"
        internal bool compareAndSet(int cmp, int val)
        {
            lock (this)
            {
                if (value_Renamed == cmp)
                {
                    value_Renamed = val;
                    return true;
                }
                return false;
            }
        }
        //UPGRADE_NOTE: Synchronized keyword was removed from method 'set'. Lock expression was added. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1027'"
        internal void set_Renamed(int val)
        {
            lock (this)
            {
                value_Renamed = val;
            }
        }
    }


    internal sealed class LockedInteger : ReentrantLock
    {
        internal int value_Renamed;
        internal LockedInteger()
        {
        }

        internal int get_Renamed()
        {
            return value_Renamed;
        }
        internal bool compareAndSet(int cmp, int val)
        {
            Lock();
            try
            {
                if (value_Renamed == cmp)
                {
                    value_Renamed = val;
                    return true;
                }
                return false;
            }
            finally
            {
                Unlock();
            }
        }
        internal void set_Renamed(int val)
        {
            Lock();
            try
            {
                value_Renamed = val;
            }
            finally
            {
                Unlock();
            }
        }
    }

    // All these versions are copy-paste-hacked to avoid
    // contamination with virtual call resolution etc.

    // Use fixed-length unrollable inner loops to reduce safepoint checks
    internal const int innerPerOuter = 16;

    internal sealed class NonAtomicLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'iters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long iters;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal NonAtomicInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal NonAtomicLoop(long iters, NonAtomicInteger obj, CyclicBarrier b)
        {
            this.iters = iters;
            this.obj = obj;
            this.barrier = b;
            obj.set_Renamed(CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long i = iters;
                int y = 0;
                int succ = 0;
                while (i > 0)
                {
                    for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                    {
                        int x = obj.get_Renamed();
                        int z = y + LoopHelpers.compute6(x);
                        if (obj.compareAndSet(x, z))
                            ++succ;
                        y = LoopHelpers.compute7(z);
                    }
                    i -= CASLoops.innerPerOuter;
                }
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.get_Renamed());
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    internal sealed class AtomicLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'iters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long iters;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal AtomicInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal AtomicLoop(long iters, AtomicInteger obj, CyclicBarrier b)
        {
            this.iters = iters;
            this.obj = obj;
            this.barrier = b;
            obj.Value = (CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long i = iters;
                int y = 0;
                int succ = 0;
                while (i > 0)
                {
                    for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                    {
                        int x = obj.Value;
                        int z = y + LoopHelpers.compute6(x);
                        if (obj.CompareAndSet(x, z))
                            ++succ;
                        y = LoopHelpers.compute7(z);
                    }
                    i -= CASLoops.innerPerOuter;
                }
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.Value);
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    //    static final class UpdaterAtomicLoop implements Runnable {
    //        final long iters;
    //        final UpdaterAtomicInteger obj;
    //        final CyclicBarrier barrier;
    //        UpdaterAtomicLoop(long iters, UpdaterAtomicInteger obj, CyclicBarrier b) {
    //            this.iters = iters;
    //            this.obj = obj;
    //            this.barrier = b;
    //            obj.set(rng.next());
    //        }
    //
    //        public void run() {
    //            try {
    //                barrier.Await();
    //                long i = iters;
    //                int y = 0;
    //                int succ = 0;
    //                while (i > 0) {
    //                    for (int k = 0; k < innerPerOuter; ++k) {
    //                        int x = obj.get();
    //                        int z = y + LoopHelpers.compute6(x);
    //                        if (obj.compareAndSet(x, z))
    //                            ++succ;
    //                        y = LoopHelpers.compute7(z);
    //                    }
    //                    i -= innerPerOuter;
    //                }
    //                sum.AddDeltaAndReturnPreviousValue(obj.get());
    //                successes.AddDeltaAndReturnPreviousValue(succ);
    //                barrier.Await();
    //            }
    //            catch (Exception ie) {
    //                return;
    //            }
    //        }
    //    }

    internal sealed class VolatileLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'iters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long iters;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal VolatileInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal VolatileLoop(long iters, VolatileInteger obj, CyclicBarrier b)
        {
            this.iters = iters;
            this.obj = obj;
            this.barrier = b;
            obj.set_Renamed(CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long i = iters;
                int y = 0;
                int succ = 0;
                while (i > 0)
                {
                    for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                    {
                        int x = obj.get_Renamed();
                        int z = y + LoopHelpers.compute6(x);
                        if (obj.compareAndSet(x, z))
                            ++succ;
                        y = LoopHelpers.compute7(z);
                    }
                    i -= CASLoops.innerPerOuter;
                }
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.get_Renamed());
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    internal sealed class SynchedLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'iters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long iters;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal SynchedInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal SynchedLoop(long iters, SynchedInteger obj, CyclicBarrier b)
        {
            this.iters = iters;
            this.obj = obj;
            this.barrier = b;
            obj.set_Renamed(CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long i = iters;
                int y = 0;
                int succ = 0;
                while (i > 0)
                {
                    for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                    {
                        int x = obj.get_Renamed();
                        int z = y + LoopHelpers.compute6(x);
                        if (obj.compareAndSet(x, z))
                            ++succ;
                        y = LoopHelpers.compute7(z);
                    }
                    i -= CASLoops.innerPerOuter;
                }
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.get_Renamed());
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    internal sealed class LockedLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'iters '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long iters;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal LockedInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal LockedLoop(long iters, LockedInteger obj, CyclicBarrier b)
        {
            this.iters = iters;
            this.obj = obj;
            this.barrier = b;
            obj.set_Renamed(CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long i = iters;
                int y = 0;
                int succ = 0;
                while (i > 0)
                {
                    for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                    {
                        int x = obj.get_Renamed();
                        int z = y + LoopHelpers.compute6(x);
                        if (obj.compareAndSet(x, z))
                            ++succ;
                        y = LoopHelpers.compute7(z);
                    }
                    i -= CASLoops.innerPerOuter;
                }
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.get_Renamed());
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    internal const int loopsPerTimeCheck = 2048;

    internal sealed class NACalibrationLoop : IRunnable
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'endTime '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal long endTime;
        //UPGRADE_NOTE: Final was removed from the declaration of 'obj '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal NonAtomicInteger obj;
        //UPGRADE_NOTE: Final was removed from the declaration of 'barrier '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal CyclicBarrier barrier;
        internal NACalibrationLoop(long endTime, NonAtomicInteger obj, CyclicBarrier b)
        {
            this.endTime = endTime;
            this.obj = obj;
            this.barrier = b;
            obj.set_Renamed(CASLoops.rng.next());
        }

        public void Run()
        {
            try
            {
                barrier.Await();
                long iters = 0;
                int y = 0;
                int succ = 0;
                do
                {
                    int i = CASLoops.loopsPerTimeCheck;
                    while (i > 0)
                    {
                        for (int k = 0; k < CASLoops.innerPerOuter; ++k)
                        {
                            int x = obj.get_Renamed();
                            int z = y + LoopHelpers.compute6(x);
                            if (obj.compareAndSet(x, z))
                                ++succ;
                            y = LoopHelpers.compute7(z);
                        }
                        i -= CASLoops.innerPerOuter;
                    }
                    iters += CASLoops.loopsPerTimeCheck;
                }
                while ((System.DateTime.Now.Ticks - 621355968000000000) / 10000 < endTime);
                CASLoops.totalIters.AddDeltaAndReturnPreviousValue(iters);
                CASLoops.sum.AddDeltaAndReturnPreviousValue(obj.get_Renamed());
                CASLoops.successes.AddDeltaAndReturnPreviousValue(succ);
                barrier.Await();
            }
            catch (System.Exception ie)
            {
                return;
            }
        }
    }

    internal static void runCalibration(int n, long nms)
    {
        long now = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
        long endTime = now + nms;
        CyclicBarrier b = new CyclicBarrier(n + 1);
        totalIters.Value = (0);
        NonAtomicInteger a = new NonAtomicInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new NACalibrationLoop(endTime, a, b).Run)).Start();
        b.Await();
        b.Await();
        long ipt = totalIters.Value / n;
        if (ipt > loopIters[n])
            loopIters[n] = ipt;
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
    }

    internal static long runNonAtomic(int n, long iters)
    {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier b = new CyclicBarrier(n + 1, timer);
        NonAtomicInteger a = new NonAtomicInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new NonAtomicLoop(iters, a, b).Run)).Start();
        b.Await();
        b.Await();
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
        return timer.Time;
    }

    //    static long runUpdaterAtomic(int n, long iters) throws Exception {
    //        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
    //        CyclicBarrier b = new CyclicBarrier(n+1, timer);
    //        UpdaterAtomicInteger a = new UpdaterAtomicInteger();
    //        for (int j = 0; j < n; ++j)
    //            new Thread(new UpdaterAtomicLoop(iters, a, b)).start();
    //        b.await();
    //        b.await();
    //        if (sum.get() == 0) System.out.print(" ");
    //        return timer.getTime();
    //    }
    //
    internal static long runAtomic(int n, long iters)
    {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier b = new CyclicBarrier(n + 1, timer);
        AtomicInteger a = new AtomicInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new AtomicLoop(iters, a, b).Run)).Start();
        b.Await();
        b.Await();
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
        return timer.Time;
    }

    internal static long runVolatile(int n, long iters)
    {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier b = new CyclicBarrier(n + 1, timer);
        VolatileInteger a = new VolatileInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new VolatileLoop(iters, a, b).Run)).Start();
        b.Await();
        b.Await();
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
        return timer.Time;
    }


    internal static long runSynched(int n, long iters)
    {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier b = new CyclicBarrier(n + 1, timer);
        SynchedInteger a = new SynchedInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new SynchedLoop(iters, a, b).Run)).Start();
        b.Await();
        b.Await();
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
        return timer.Time;
    }

    internal static long runLocked(int n, long iters)
    {
        LoopHelpers.BarrierTimer timer = new LoopHelpers.BarrierTimer();
        CyclicBarrier b = new CyclicBarrier(n + 1, timer);
        LockedInteger a = new LockedInteger();
        for (int j = 0; j < n; ++j)
            new SupportClass.ThreadClass(new System.Threading.ThreadStart(new LockedLoop(iters, a, b).Run)).Start();
        b.Await();
        b.Await();
        if (sum.Value == 0)
            System.Console.Out.Write(" ");
        return timer.Time;
    }

    internal static void report(System.String tag, long runtime, long basetime, int nthreads, long iters)
    {
        System.Console.Out.Write(tag);
        long t = (runtime - basetime) / iters;
        if (nthreads > NCPUS)
            t = t * NCPUS / nthreads;
        System.Console.Out.Write(LoopHelpers.rightJustify(t));
        //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
        double secs = (double)(runtime) / 1000000000.0;
        System.Console.Out.WriteLine("\t " + secs + "s run time");
    }


    internal static void oneRun(int i, long iters, bool print)
    {
        if (print)
            System.Console.Out.WriteLine("threads : " + i + " base iters per thread per run : " + LoopHelpers.rightJustify(loopIters[i]));
        long ntime = runNonAtomic(i, iters);
        if (print)
            report("Base        : ", ntime, ntime, i, iters);
        //UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
        System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64)10000 * 100L));
        long atime = runAtomic(i, iters);
        if (print)
            report("Atomic CAS  : ", atime, ntime, i, iters);
        //UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
        System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64)10000 * 100L));
        //        long gtime = runUpdaterAtomic(i, iters);
        //        if (print)
        //            report("Updater CAS : ", gtime, ntime, i, iters);
        //        Thread.sleep(100L);
        long vtime = runVolatile(i, iters);
        if (print)
            report("Volatile    : ", vtime, ntime, i, iters);

        //UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
        System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64)10000 * 100L));
        long stime = runSynched(i, iters);
        if (print)
            report("Synchronized: ", stime, ntime, i, iters);
        //UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
        System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64)10000 * 100L));
        if (!includeLocks)
            return;
        long mtime = runLocked(i, iters);
        if (print)
            report("Mutex       : ", mtime, ntime, i, iters);
        //UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
        System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64)10000 * 100L));
    }
}