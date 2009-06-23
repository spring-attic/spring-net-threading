/*
* Written by Dawid Kurzyniec, based on code written by Doug Lea with assistance
* from members of JCP JSR-166 Expert Group. Released to the public domain,
* as explained at http://creativecommons.org/licenses/publicdomain.
*/
using System;
namespace edu.emory.mathcs.backport.java.util
{

    public class Arrays
    {

        private Arrays()
        {
        }

        public static void sort(long[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(long[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(int[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(int[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(short[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(short[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(char[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(char[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(sbyte[] a)
        {
            System.Array.Sort(SupportClass.ToByteArray(a));
        }

        public static void sort(sbyte[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(SupportClass.ToByteArray(a), fromIndex, toIndex - fromIndex);
        }

        public static void sort(double[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(double[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(float[] a)
        {
            System.Array.Sort(a);
        }

        public static void sort(float[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }


        public static void sort(System.Object[] a)
        {
            //UPGRADE_TODO: Method 'java.util.Arrays.sort' was converted to 'System.Array.Sort' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilArrayssort_javalangObject[]'"
            System.Array.Sort(a);
        }

        public static void sort(System.Object[] a, int fromIndex, int toIndex)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex);
        }

        public static void sort(System.Object[] a, System.Collections.IComparer c)
        {
            System.Array.Sort(a, c);
        }

        public static void sort(System.Object[] a, int fromIndex, int toIndex, System.Collections.IComparer c)
        {
            System.Array.Sort(a, fromIndex, toIndex - fromIndex, c);
        }


        // Searching

        public static int binarySearch(long[] a, long key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(int[] a, int key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(short[] a, short key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(char[] a, char key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(sbyte[] a, sbyte key)
        {
            return System.Array.BinarySearch(SupportClass.ToByteArray(a), (byte)key);
        }

        public static int binarySearch(double[] a, double key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(float[] a, float key)
        {
            return System.Array.BinarySearch(a, (System.Object)key);
        }

        public static int binarySearch(System.Object[] a, System.Object key)
        {
            return System.Array.BinarySearch(a, key);
        }

        public static int binarySearch(System.Object[] a, System.Object key, System.Collections.IComparer c)
        {
            return System.Array.BinarySearch(a, key, c);
        }


        // Equality Testing

        public static bool equals(long[] a, long[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(int[] a, int[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(short[] a, short[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(char[] a, char[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(sbyte[] a, sbyte[] a2)
        {
            return SupportClass.ArraySupport.Equals(SupportClass.ToByteArray(a), SupportClass.ToByteArray(a2));
        }

        public static bool equals(bool[] a, bool[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(double[] a, double[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(float[] a, float[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }

        public static bool equals(System.Object[] a, System.Object[] a2)
        {
            return SupportClass.ArraySupport.Equals(a, a2);
        }


        // Filling

        public static void fill(long[] a, long val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(long[] a, int fromIndex, int toIndex, long val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(int[] a, int val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(int[] a, int fromIndex, int toIndex, int val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(short[] a, short val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(short[] a, int fromIndex, int toIndex, short val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(char[] a, char val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(char[] a, int fromIndex, int toIndex, char val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(sbyte[] a, sbyte val)
        {
            SupportClass.ArraySupport.Fill(SupportClass.ToByteArray(a), (byte)val);
        }

        public static void fill(sbyte[] a, int fromIndex, int toIndex, sbyte val)
        {
            SupportClass.ArraySupport.Fill(SupportClass.ToByteArray(a), fromIndex, toIndex, (byte)val);
        }

        public static void fill(bool[] a, bool val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(bool[] a, int fromIndex, int toIndex, bool val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(double[] a, double val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(double[] a, int fromIndex, int toIndex, double val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(float[] a, float val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(float[] a, int fromIndex, int toIndex, float val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }

        public static void fill(System.Object[] a, System.Object val)
        {
            SupportClass.ArraySupport.Fill(a, val);
        }

        public static void fill(System.Object[] a, int fromIndex, int toIndex, System.Object val)
        {
            SupportClass.ArraySupport.Fill(a, fromIndex, toIndex, val);
        }


        // Cloning

        /// <since> 1.6
        /// </since>
        public static System.Object[] copyOf(System.Object[] original, int newLength)
        {
            return copyOf(original, newLength, original.GetType());
        }

        /// <since> 1.6
        /// </since>
        public static System.Object[] copyOf(System.Object[] original, int newLength, System.Type newType)
        {
            System.Object[] arr = (newType == typeof(System.Object[])) ? new System.Object[newLength] : (System.Object[])System.Array.CreateInstance(newType.GetElementType(), newLength);
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static sbyte[] copyOf(sbyte[] original, int newLength)
        {
            sbyte[] arr = new sbyte[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static short[] copyOf(short[] original, int newLength)
        {
            short[] arr = new short[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static int[] copyOf(int[] original, int newLength)
        {
            int[] arr = new int[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static long[] copyOf(long[] original, int newLength)
        {
            long[] arr = new long[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static char[] copyOf(char[] original, int newLength)
        {
            char[] arr = new char[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static float[] copyOf(float[] original, int newLength)
        {
            float[] arr = new float[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static double[] copyOf(double[] original, int newLength)
        {
            double[] arr = new double[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static bool[] copyOf(bool[] original, int newLength)
        {
            bool[] arr = new bool[newLength];
            int len = (original.Length < newLength ? original.Length : newLength);
            Array.Copy(original, 0, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static System.Object[] copyOfRange(System.Object[] original, int from, int to)
        {
            return copyOfRange(original, from, to, original.GetType());
        }

        /// <since> 1.6
        /// </since>
        public static System.Object[] copyOfRange(System.Object[] original, int from, int to, System.Type newType)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            System.Object[] arr = (newType == typeof(System.Object[])) ? new System.Object[newLength] : (System.Object[])System.Array.CreateInstance(newType.GetElementType(), newLength);
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static sbyte[] copyOfRange(sbyte[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            sbyte[] arr = new sbyte[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static short[] copyOfRange(short[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            short[] arr = new short[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static int[] copyOfRange(int[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            int[] arr = new int[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static long[] copyOfRange(long[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            long[] arr = new long[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static char[] copyOfRange(char[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            char[] arr = new char[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static float[] copyOfRange(float[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            float[] arr = new float[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static double[] copyOfRange(double[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            double[] arr = new double[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }

        /// <since> 1.6
        /// </since>
        public static bool[] copyOfRange(bool[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new System.ArgumentException(from + " > " + to);
            bool[] arr = new bool[newLength];
            int ceil = original.Length - from;
            int len = (ceil < newLength) ? ceil : newLength;
            Array.Copy(original, from, arr, 0, len);
            return arr;
        }


        public static System.Collections.IList asList(System.Object[] a)
        {
            //UPGRADE_TODO: Method 'java.util.Arrays.asList' was converted to 'System.Collections.ArrayList' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilArraysasList_javalangObject[]'"
            return new System.Collections.ArrayList(a);
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(long[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                long e = a[i];
                hash = 31 * hash + (int)(e ^ (SupportClass.URShift(e, 32)));
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(int[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + a[i];
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(short[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + a[i];
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(char[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + a[i];
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(sbyte[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + a[i];
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(bool[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + (a[i] ? 1231 : 1237);
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(float[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31*hash + a[i].GetHashCode();
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(double[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                hash = 31 * hash + a[i].GetHashCode();
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int hashCode(System.Object[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                System.Object e = a[i];
                hash = 31 * hash + (e == null ? 0 : e.GetHashCode());
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static int deepHashCode(System.Object[] a)
        {
            if (a == null)
                return 0;
            int hash = 1;
            for (int i = 0; i < a.Length; i++)
            {
                System.Object e = a[i];
                hash = 31 * hash + (e is System.Object[] ? deepHashCode((System.Object[])e) : (e is sbyte[] ? hashCode((sbyte[])e) : (e is short[] ? hashCode((short[])e) : (e is int[] ? hashCode((int[])e) : (e is long[] ? hashCode((long[])e) : (e is char[] ? hashCode((char[])e) : (e is bool[] ? hashCode((bool[])e) : (e is float[] ? hashCode((float[])e) : (e is double[] ? hashCode((double[])e) : (e != null ? e.GetHashCode() : 0))))))))));
            }
            return hash;
        }

        /// <since> 1.5
        /// </since>
        public static bool deepEquals(System.Object[] a1, System.Object[] a2)
        {
            if (a1 == a2)
                return true;
            if (a1 == null || a2 == null)
                return false;
            int len = a1.Length;
            if (len != a2.Length)
                return false;
            for (int i = 0; i < len; i++)
            {
                System.Object e1 = a1[i];
                System.Object e2 = a2[i];
                if (e1 == e2)
                    continue;
                if (e1 == null)
                    return false;
                bool eq = (e1.GetType() != e2.GetType() || e1.GetType().IsArray) ? e1.Equals(e2) : ((e1 is System.Object[] && e2 is System.Object[]) ? deepEquals((System.Object[])e1, (System.Object[])e2) : ((e1 is sbyte[] && e2 is sbyte[]) ? equals((sbyte[])e1, (sbyte[])e2) : ((e1 is short[] && e2 is short[]) ? equals((short[])e1, (short[])e2) : ((e1 is int[] && e2 is int[]) ? equals((int[])e1, (int[])e2) : ((e1 is long[] && e2 is long[]) ? equals((long[])e1, (long[])e2) : ((e1 is char[] && e2 is char[]) ? equals((char[])e1, (char[])e2) : ((e1 is bool[] && e2 is bool[]) ? equals((bool[])e1, (bool[])e2) : ((e1 is float[] && e2 is float[]) ? equals((float[])e1, (float[])e2) : ((e1 is double[] && e2 is double[]) ? equals((double[])e1, (double[])e2) : e1.Equals(e2))))))))));

                if (!eq)
                    return false;
            }
            return true;
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(long[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(int[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(short[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(char[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(sbyte[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append((byte)a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append((byte)a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(bool[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(float[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(double[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String toString(System.Object[] a)
        {
            if (a == null)
                return "null";
            if (a.Length == 0)
                return "[]";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('[').Append(a[0]);
            for (int i = 1; i < a.Length; i++)
                buf.Append(", ").Append(a[i]);
            buf.Append(']');
            return buf.ToString();
        }

        /// <since> 1.5
        /// </since>
        public static System.String deepToString(System.Object[] a)
        {
            if (a == null)
                return "null";
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            deepToString(a, buf, new System.Collections.ArrayList());
            return buf.ToString();
        }

        private static void deepToString(System.Object[] a, System.Text.StringBuilder buf, System.Collections.IList seen)
        {
            seen.Add(a);
            buf.Append('[');
            for (int i = 0; i < a.Length; i++)
            {
                if (i > 0)
                    buf.Append(", ");
                System.Object e = a[i];
                if (e == null)
                {
                    buf.Append("null");
                }
                else if (!e.GetType().IsArray)
                {
                    //UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
                    buf.Append(e.ToString());
                }
                else if (e is System.Object[])
                {
                    if (seen.Contains(e))
                        buf.Append("[...]");
                    else
                        deepToString((System.Object[])e, buf, seen);
                }
                else
                {
                    // primitive arr
                    buf.Append((e is sbyte[]) ? toString((sbyte[])e) : ((e is short[]) ? toString((short[])e) : ((e is int[]) ? toString((int[])e) : ((e is long[]) ? toString((long[])e) : ((e is char[]) ? toString((char[])e) : ((e is bool[]) ? toString((bool[])e) : ((e is float[]) ? toString((float[])e) : ((e is double[]) ? toString((double[])e) : ""))))))));
                }
            }
            buf.Append(']');
            seen.RemoveAt(seen.Count - 1);
        }
    }
}