/*
* Written by Doug Lea with assistance from members of JCP JSR-166
* Expert Group and released to the public domain, as explained at
* http://creativecommons.org/licenses/publicdomain
*/
/// <summary> Misc utilities in JSR166 performance tests</summary>
//UPGRADE_TODO: The package 'edu.emory.mathcs.backport.java.util.concurrent' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//UPGRADE_TODO: The package 'edu.emory.mathcs.backport.java.util.concurrent.atomic' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//UPGRADE_TODO: The type 'edu.emory.mathcs.backport.java.util.concurrent.helpers.Utils' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
using System.Threading;
using Spring.Threading;
using Spring.Threading.AtomicTypes;

class LoopHelpers
{

    //UPGRADE_NOTE: Final was removed from the declaration of 'staticRNG '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal static readonly SimpleRandom staticRNG = new SimpleRandom();

    // Some mindless computation to do between synchronizations...

    /// <summary> generates 32 bit pseudo-random numbers.
    /// Adapted from http://www.snippets.org
    /// </summary>
    public static int compute1(int x)
    {
        int lo = 16807 * (x & 0xFFFF);
        int hi = 16807 * (SupportClass.URShift(x, 16));
        lo += ((hi & 0x7FFF) << 16);
        if ((lo & unchecked((int)0x80000000)) != 0)
        {
            lo &= 0x7fffffff;
            ++lo;
        }
        lo += SupportClass.URShift(hi, 15);
        if (lo == 0 || (lo & unchecked((int)0x80000000)) != 0)
        {
            lo &= 0x7fffffff;
            ++lo;
        }
        return lo;
    }

    /// <summary>  Computes a linear congruential random number a random number
    /// of times.
    /// </summary>
    public static int compute2(int x)
    {
        int loops = (SupportClass.URShift(x, 4)) & 7;
        while (loops-- > 0)
        {
            x = (x * 2147483647) % 16807;
        }
        return x;
    }

    /// <summary> Yet another random number generator</summary>
    public static int compute3(int x)
    {
        int t = (x % 127773) * 16807 - (x / 127773) * 2836;
        return (t > 0) ? t : t + 0x7fffffff;
    }

    /// <summary> Yet another random number generator</summary>
    public static int compute4(int x)
    {
        return x * 134775813 + 1;
    }


    /// <summary> Yet another random number generator</summary>
    public static int compute5(int x)
    {
        return 36969 * (x & 65535) + (x >> 16);
    }

    /// <summary> Marsaglia xorshift (1, 3, 10)</summary>
    public static int compute6(int seed)
    {
        seed ^= seed << 1;
        seed ^= SupportClass.URShift(seed, 3);
        seed ^= (seed << 10);
        return seed;
    }

    /// <summary> Marsaglia xorshift (6, 21, 7)</summary>
    public static int compute7(int y)
    {
        y ^= y << 6;
        y ^= SupportClass.URShift(y, 21);
        y ^= (y << 7);
        return y;
    }


    /// <summary> Marsaglia xorshift for longs</summary>
    public static long compute8(long x)
    {
        x ^= x << 13;
        x ^= SupportClass.URShift(x, 7);
        x ^= (x << 17);
        return x;
    }

    public sealed class XorShift32Random
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'seq '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly AtomicInteger seq = new AtomicInteger(8862213);
        internal int x = -1831433054;
        public XorShift32Random(int seed)
        {
            x = seed;
        }
        public XorShift32Random()
            : this((int)Utils.CurrentNanoSeconds() + seq.AddDeltaAndReturnPreviousValue(129))
        {
        }
        public int next()
        {
            x ^= x << 6;
            x ^= SupportClass.URShift(x, 21);
            x ^= (x << 7);
            return x;
        }
    }


    /// <summary>Multiplication-free RNG from Marsaglia "Xorshift RNGs" paper </summary>
    public sealed class MarsagliaRandom
    {
        //UPGRADE_NOTE: Final was removed from the declaration of 'seq '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly AtomicInteger seq = new AtomicInteger(3122688);
        internal int x;
        internal int y = 842502087;
        internal int z = -715159705;
        internal int w = 273326509;
        public MarsagliaRandom(int seed)
        {
            x = seed;
        }
        public MarsagliaRandom()
            : this((int)Utils.CurrentNanoSeconds() + seq.AddDeltaAndReturnPreviousValue(129))
        {
        }
        public int next()
        {
            int t = x ^ (x << 11);
            x = y;
            y = z;
            z = w;
            return w = (w ^ (SupportClass.URShift(w, 19)) ^ (t ^ (SupportClass.URShift(t, 8))));
        }
    }

    /// <summary> Unsynchronized version of java.util.Random algorithm.</summary>
    public sealed class SimpleRandom
    {
        public long Seed
        {
            set
            {
                seed = value;
            }

        }
        private const long multiplier = 0x5DEECE66DL;
        private const long addend = 0xBL;
        private const long mask = (1L << 48) - 1;
        //UPGRADE_NOTE: Final was removed from the declaration of 'seq '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly AtomicLong seq = new AtomicLong(-715159705);
        private long seed;

        internal SimpleRandom(long s)
        {
            seed = s;
        }

        internal SimpleRandom()
        {
            seed = Utils.CurrentNanoSeconds() + seq.AddDeltaAndReturnPreviousValue(129);
        }

        public int next()
        {
            long nextseed = (seed * multiplier + addend) & mask;
            seed = nextseed;
            return ((int)(SupportClass.URShift(nextseed, 17))) & 0x7FFFFFFF;
        }
    }

    public class BarrierTimer : IRunnable
    {
        virtual public long Time
        {
            get
            {
                return endTime - startTime;
            }

        }
        internal volatile bool started;
        private long _startTime;
        internal long startTime
        {
            get { return Interlocked.Read(ref _startTime); }
            set { Interlocked.Exchange(ref _startTime, value); }
        }

        private long _endTime;
        internal long endTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        public virtual void Run()
        {
            long t = Utils.CurrentNanoSeconds();
            if (!started)
            {
                started = true;
                startTime = t;
            }
            else
                endTime = t;
        }
        public virtual void clear()
        {
            started = false;
        }
    }

    public static System.String rightJustify(long n)
    {
        // There's probably a better way to do this...
        System.String field = "         ";
        System.String num = System.Convert.ToString(n);
        if (num.Length >= field.Length)
            return num;
        System.Text.StringBuilder b = new System.Text.StringBuilder(field);
        b.Replace(b.ToString(b.Length - num.Length, b.Length - (b.Length - num.Length)), num, b.Length - num.Length, b.Length - (b.Length - num.Length));
        return b.ToString();
    }
}