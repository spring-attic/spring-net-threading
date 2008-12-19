using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Some common utilities used in varous test cases.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public static class CollectionTestUtils
    {
        public static void VerifyData<T>(ICollection<T> expected, AbstractCollection<T> actual)
        {
            foreach (T item in expected)
            {
                Assert.IsTrue(actual.Contains(item));
            }

            Assert.AreEqual(expected, actual);

            T[] array;

            array = new T[expected.Count];
            actual.CopyTo(array, 0);
            Assert.AreEqual(expected, array);

            array = new T[expected.Count];
            ICollection nonGeneric = actual;
            nonGeneric.CopyTo(array, 0);
            Assert.AreEqual(expected, array);

        }

        public static ICollection<T> MakeTestCollection<T>(int count)
        {
            return MakeTestArray<T>(count);
        }

        public static ICollection MakeTestCollection(int count)
        {
            return MakeTestArray<object>(count);
        }

        public static T[] MakeTestArray<T>(int count)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (T)Convert.ChangeType(i, typeof(T));
            }
            return result;
        }

        public static IList<T> MakeTestList<T>(int count)
        {
            return MakeTestArray<T>(count);
        }
    }
}
