using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Spring
{
    public static class TestData
    {
        public const long ShortDelayMillis = 300;
        public const long SmallDelayMillis = ShortDelayMillis * 5;
        public const long MediumDelayMillis = ShortDelayMillis * 10;
        public const long LongDelayMillis = ShortDelayMillis * 50;
        public static readonly TimeSpan ShortDelay = TimeSpan.FromMilliseconds(ShortDelayMillis);
        public static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(SmallDelayMillis);
        public static readonly TimeSpan MediumDelay = TimeSpan.FromMilliseconds(MediumDelayMillis);
        public static readonly TimeSpan LongDelay = TimeSpan.FromMilliseconds(LongDelayMillis);

    }

    public static class TestData<T>
    {
        public static readonly T One = MakeData(1);
        public static readonly T Two = MakeData(2);
        public static readonly T Three = MakeData(3);
        public static readonly T Four = MakeData(4);
        public static readonly T Five = MakeData(5);

        public static readonly T M1 = MakeData(-1);
        public static readonly T M2 = MakeData(-2);
        public static readonly T M3 = MakeData(-3);
        public static readonly T M4 = MakeData(-4);
        public static readonly T M5 = MakeData(-5);

        public static T MakeData(int i)
        {
            return (T) Convert.ChangeType(i, typeof (T));
        }

        public static T[] MakeTestArray(int count)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = MakeData(i);
            }
            return result;
        }

        public static IList<T> MakeTestList(int count)
        {
            return MakeTestArray(count);
        }
    }
}
