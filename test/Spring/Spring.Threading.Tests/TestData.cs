using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Spring
{
    public static class TestData<T>
    {
        public static readonly T one = MakeData(1);
        public static readonly T two = MakeData(2);
        public static readonly T three = MakeData(3);
        public static readonly T four = MakeData(4);
        public static readonly T five = MakeData(5);

        public static readonly T m1 = MakeData(-1);
        public static readonly T m2 = MakeData(-2);
        public static readonly T m3 = MakeData(-3);
        public static readonly T m4 = MakeData(-4);
        public static readonly T m5 = MakeData(-5);

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
