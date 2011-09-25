using System;
using System.Collections.Generic;

namespace Spring.Utility
{
    internal class ComparableComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }

        public static ComparableComparer<T> Default = new ComparableComparer<T>();
    }
}