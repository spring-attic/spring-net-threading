#pragma warning disable 1574
//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using Spring.Threading;

/// <summary>
/// Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
public class SupportClass
{
    /// <summary>
    /// Represents a collection ob objects that contains no duplicate elements.
    /// </summary>	
    public interface SetSupport : System.Collections.ICollection, System.Collections.IList
    {
        /// <summary>
        /// Adds a new element to the Collection if it is not already present.
        /// </summary>
        /// <param name="obj">The object to add to the collection.</param>
        /// <returns>Returns true if the object was added to the collection, otherwise false.</returns>
        new bool Add(System.Object obj);

        /// <summary>
        /// Adds all the elements of the specified collection to the Set.
        /// </summary>
        /// <param name="c">Collection of objects to add.</param>
        /// <returns>true</returns>
        bool AddAll(System.Collections.ICollection c);
    }


    /*******************************/
    /// <summary>
    /// SupportClass for the HashSet class.
    /// </summary>
    [Serializable]
    public class HashSetSupport : System.Collections.ArrayList, SetSupport
    {
        public HashSetSupport()
            : base()
        {
        }

        public HashSetSupport(System.Collections.ICollection c)
        {
            this.AddAll(c);
        }

        public HashSetSupport(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Adds a new element to the ArrayList if it is not already present.
        /// </summary>		
        /// <param name="obj">Element to insert to the ArrayList.</param>
        /// <returns>Returns true if the new element was inserted, false otherwise.</returns>
        new public virtual bool Add(System.Object obj)
        {
            bool inserted;

            if ((inserted = this.Contains(obj)) == false)
            {
                base.Add(obj);
            }

            return !inserted;
        }

        /// <summary>
        /// Adds all the elements of the specified collection that are not present to the list.
        /// </summary>
        /// <param name="c">Collection where the new elements will be added</param>
        /// <returns>Returns true if at least one element was added, false otherwise.</returns>
        public bool AddAll(System.Collections.ICollection c)
        {
            System.Collections.IEnumerator e = new System.Collections.ArrayList(c).GetEnumerator();
            bool added = false;

            while (e.MoveNext() == true)
            {
                if (this.Add(e.Current) == true)
                    added = true;
            }

            return added;
        }

        /// <summary>
        /// Returns a copy of the HashSet instance.
        /// </summary>		
        /// <returns>Returns a shallow copy of the current HashSet.</returns>
        public override System.Object Clone()
        {
            return base.MemberwiseClone();
        }
    }


    /*******************************/
    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Ammount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    public static int URShift(int number, int bits)
    {
        if (number >= 0)
            return number >> bits;
        else
            return (number >> bits) + (2 << ~bits);
    }

    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Ammount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    public static int URShift(int number, long bits)
    {
        return URShift(number, (int)bits);
    }

    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Ammount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    public static long URShift(long number, int bits)
    {
        if (number >= 0)
            return number >> bits;
        else
            return (number >> bits) + (2L << ~bits);
    }

    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Ammount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    public static long URShift(long number, long bits)
    {
        return URShift(number, (int)bits);
    }

    /*******************************/
    /// <summary>
    /// This class provides functionality not found in .NET collection-related interfaces.
    /// </summary>
    public class ICollectionSupport
    {
        /// <summary>
        /// Adds a new element to the specified collection.
        /// </summary>
        /// <param name="c">Collection where the new element will be added.</param>
        /// <param name="obj">Object to add.</param>
        /// <returns>true</returns>
        public static bool Add(System.Collections.ICollection c, System.Object obj)
        {
            bool added = false;
            //Reflection. Invoke either the "add" or "Add" method.
            System.Reflection.MethodInfo method;
            try
            {
                //Get the "add" method for proprietary classes
                method = c.GetType().GetMethod("Add");
                if (method == null)
                    method = c.GetType().GetMethod("add");
                int index = (int)method.Invoke(c, new System.Object[] { obj });
                if (index >= 0)
                    added = true;
            }
            catch (System.Exception e)
            {
                throw e;
            }
            return added;
        }

        /// <summary>
        /// Adds all of the elements of the "c" collection to the "target" collection.
        /// </summary>
        /// <param name="target">Collection where the new elements will be added.</param>
        /// <param name="c">Collection whose elements will be added.</param>
        /// <returns>Returns true if at least one element was added, false otherwise.</returns>
        public static bool AddAll(System.Collections.ICollection target, System.Collections.ICollection c)
        {
            System.Collections.IEnumerator e = new System.Collections.ArrayList(c).GetEnumerator();
            bool added = false;

            //Reflection. Invoke "addAll" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("addAll");

                if (method != null)
                    added = (bool)method.Invoke(target, new System.Object[] { c });
                else
                {
                    method = target.GetType().GetMethod("Add");
                    while (e.MoveNext() == true)
                    {
                        bool tempBAdded = (int)method.Invoke(target, new System.Object[] { e.Current }) >= 0;
                        added = added ? added : tempBAdded;
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return added;
        }

        /// <summary>
        /// Removes all the elements from the collection.
        /// </summary>
        /// <param name="c">The collection to remove elements.</param>
        public static void Clear(System.Collections.ICollection c)
        {
            //Reflection. Invoke "Clear" method or "clear" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("Clear");

                if (method == null)
                    method = c.GetType().GetMethod("clear");

                method.Invoke(c, new System.Object[] { });
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Determines whether the collection contains the specified element.
        /// </summary>
        /// <param name="c">The collection to check.</param>
        /// <param name="obj">The object to locate in the collection.</param>
        /// <returns>true if the element is in the collection.</returns>
        public static bool Contains(System.Collections.ICollection c, System.Object obj)
        {
            bool contains = false;

            //Reflection. Invoke "contains" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("Contains");

                if (method == null)
                    method = c.GetType().GetMethod("contains");

                contains = (bool)method.Invoke(c, new System.Object[] { obj });
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return contains;
        }

        /// <summary>
        /// Determines whether the collection contains all the elements in the specified collection.
        /// </summary>
        /// <param name="target">The collection to check.</param>
        /// <param name="c">Collection whose elements would be checked for containment.</param>
        /// <returns>true id the target collection contains all the elements of the specified collection.</returns>
        public static bool ContainsAll(System.Collections.ICollection target, System.Collections.ICollection c)
        {
            System.Collections.IEnumerator e = c.GetEnumerator();

            bool contains = false;

            //Reflection. Invoke "containsAll" method for proprietary classes or "Contains" method for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("containsAll");

                if (method != null)
                    contains = (bool)method.Invoke(target, new Object[] { c });
                else
                {
                    method = target.GetType().GetMethod("Contains");
                    while (e.MoveNext() == true)
                    {
                        if ((contains = (bool)method.Invoke(target, new Object[] { e.Current })) == false)
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return contains;
        }

        /// <summary>
        /// Removes the specified element from the collection.
        /// </summary>
        /// <param name="c">The collection where the element will be removed.</param>
        /// <param name="obj">The element to remove from the collection.</param>
        public static bool Remove(System.Collections.ICollection c, System.Object obj)
        {
            bool changed = false;

            //Reflection. Invoke "remove" method for proprietary classes or "Remove" method
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("remove");

                if (method != null)
                    method.Invoke(c, new System.Object[] { obj });
                else
                {
                    method = c.GetType().GetMethod("Contains");
                    changed = (bool)method.Invoke(c, new System.Object[] { obj });
                    method = c.GetType().GetMethod("Remove");
                    method.Invoke(c, new System.Object[] { obj });
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return changed;
        }

        /// <summary>
        /// Removes all the elements from the specified collection that are contained in the target collection.
        /// </summary>
        /// <param name="target">Collection where the elements will be removed.</param>
        /// <param name="c">Elements to remove from the target collection.</param>
        /// <returns>true</returns>
        public static bool RemoveAll(System.Collections.ICollection target, System.Collections.ICollection c)
        {
            System.Collections.ArrayList al = ToArrayList(c);
            System.Collections.IEnumerator e = al.GetEnumerator();

            //Reflection. Invoke "removeAll" method for proprietary classes or "Remove" for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("removeAll");

                if (method != null)
                    method.Invoke(target, new System.Object[] { al });
                else
                {
                    method = target.GetType().GetMethod("Remove");
                    System.Reflection.MethodInfo methodContains = target.GetType().GetMethod("Contains");

                    while (e.MoveNext() == true)
                    {
                        while ((bool)methodContains.Invoke(target, new System.Object[] { e.Current }) == true)
                            method.Invoke(target, new System.Object[] { e.Current });
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// Retains the elements in the target collection that are contained in the specified collection
        /// </summary>
        /// <param name="target">Collection where the elements will be removed.</param>
        /// <param name="c">Elements to be retained in the target collection.</param>
        /// <returns>true</returns>
        public static bool RetainAll(System.Collections.ICollection target, System.Collections.ICollection c)
        {
            System.Collections.IEnumerator e = new System.Collections.ArrayList(target).GetEnumerator();
            System.Collections.ArrayList al = new System.Collections.ArrayList(c);

            //Reflection. Invoke "retainAll" method for proprietary classes or "Remove" for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("retainAll");

                if (method != null)
                    method.Invoke(target, new System.Object[] { c });
                else
                {
                    method = c.GetType().GetMethod("Remove");

                    while (e.MoveNext() == true)
                    {
                        if (al.Contains(e.Current) == false)
                            method.Invoke(target, new System.Object[] { e.Current });
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Returns an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection.</returns>
        public static System.Object[] ToArray(System.Collections.ICollection c)
        {
            int index = 0;
            System.Object[] objects = new System.Object[c.Count];
            System.Collections.IEnumerator e = c.GetEnumerator();

            while (e.MoveNext())
                objects[index++] = e.Current;

            return objects;
        }

        /// <summary>
        /// Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public static System.Object[] ToArray(System.Collections.ICollection c, System.Object[] objects)
        {
            int index = 0;

            System.Type type = objects.GetType().GetElementType();
            System.Object[] objs = (System.Object[])Array.CreateInstance(type, c.Count);

            System.Collections.IEnumerator e = c.GetEnumerator();

            while (e.MoveNext())
                objs[index++] = e.Current;

            //If objects is smaller than c then do not return the new array in the parameter
            if (objects.Length >= c.Count)
                objs.CopyTo(objects, 0);

            return objs;
        }

        /// <summary>
        /// Converts an ICollection instance to an ArrayList instance.
        /// </summary>
        /// <param name="c">The ICollection instance to be converted.</param>
        /// <returns>An ArrayList instance in which its elements are the elements of the ICollection instance.</returns>
        public static System.Collections.ArrayList ToArrayList(System.Collections.ICollection c)
        {
            System.Collections.ArrayList tempArrayList = new System.Collections.ArrayList();
            System.Collections.IEnumerator tempEnumerator = c.GetEnumerator();
            while (tempEnumerator.MoveNext())
                tempArrayList.Add(tempEnumerator.Current);
            return tempArrayList;
        }
    }


    /*******************************/
    /// <summary>
    /// Writes the serializable fields to the SerializationInfo object, which stores all the data needed to serialize the specified object object.
    /// </summary>
    /// <param name="info">SerializationInfo parameter from the GetObjectData method.</param>
    /// <param name="context">StreamingContext parameter from the GetObjectData method.</param>
    /// <param name="instance">Object to serialize.</param>
    public static void DefaultWriteObject(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, System.Object instance)
    {
        System.Type thisType = instance.GetType();
        System.Reflection.MemberInfo[] mi = System.Runtime.Serialization.FormatterServices.GetSerializableMembers(thisType, context);
        for (int i = 0; i < mi.Length; i++)
        {
            info.AddValue(mi[i].Name, ((System.Reflection.FieldInfo)mi[i]).GetValue(instance));
        }
    }


    /*******************************/
    /// <summary>
    /// Reads the serialized fields written by the DefaultWriteObject method.
    /// </summary>
    /// <param name="info">SerializationInfo parameter from the special deserialization constructor.</param>
    /// <param name="context">StreamingContext parameter from the special deserialization constructor</param>
    /// <param name="instance">Object to deserialize.</param>
    public static void DefaultReadObject(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, System.Object instance)
    {
        System.Type thisType = instance.GetType();
        System.Reflection.MemberInfo[] mi = System.Runtime.Serialization.FormatterServices.GetSerializableMembers(thisType, context);
        for (int i = 0; i < mi.Length; i++)
        {
            System.Reflection.FieldInfo fi = (System.Reflection.FieldInfo)mi[i];
            fi.SetValue(instance, info.GetValue(fi.Name, fi.FieldType));
        }
    }
    /*******************************/
    /// <summary>
    /// Converts an array of sbytes to an array of bytes
    /// </summary>
    /// <param name="sbyteArray">The array of sbytes to be converted</param>
    /// <returns>The new array of bytes</returns>
    public static byte[] ToByteArray(sbyte[] sbyteArray)
    {
        byte[] byteArray = null;

        if (sbyteArray != null)
        {
            byteArray = new byte[sbyteArray.Length];
            for (int index = 0; index < sbyteArray.Length; index++)
                byteArray[index] = (byte)sbyteArray[index];
        }
        return byteArray;
    }

    /// <summary>
    /// Converts a string to an array of bytes
    /// </summary>
    /// <param name="sourceString">The string to be converted</param>
    /// <returns>The new array of bytes</returns>
    public static byte[] ToByteArray(System.String sourceString)
    {
        return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
    }

    /// <summary>
    /// Converts a array of object-type instances to a byte-type array.
    /// </summary>
    /// <param name="tempObjectArray">Array to convert.</param>
    /// <returns>An array of byte type elements.</returns>
    public static byte[] ToByteArray(System.Object[] tempObjectArray)
    {
        byte[] byteArray = null;
        if (tempObjectArray != null)
        {
            byteArray = new byte[tempObjectArray.Length];
            for (int index = 0; index < tempObjectArray.Length; index++)
                byteArray[index] = (byte)tempObjectArray[index];
        }
        return byteArray;
    }

    /*******************************/
    /// <summary>
    /// This class manages array operations.
    /// </summary>
    public class ArraySupport
    {
        /// <summary>
        /// Compares the entire members of one array whith the other one.
        /// </summary>
        /// <param name="array1">The array to be compared.</param>
        /// <param name="array2">The array to be compared with.</param>
        /// <returns>True if both arrays are equals otherwise it returns false.</returns>
        /// <remarks>Two arrays are equal if they contains the same elements in the same order.</remarks>
        public static bool Equals(System.Array array1, System.Array array2)
        {
            bool result = false;
            if ((array1 == null) && (array2 == null))
                result = true;
            else if ((array1 != null) && (array2 != null))
            {
                if (array1.Length == array2.Length)
                {
                    int length = array1.Length;
                    result = true;
                    for (int index = 0; index < length; index++)
                    {
                        if (!(array1.GetValue(index).Equals(array2.GetValue(index))))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="fromindex">The first index to be filled.</param>
        /// <param name="toindex">The last index to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(System.Array array, System.Int32 fromindex, System.Int32 toindex, System.Object val)
        {
            System.Object Temp_Object = val;
            System.Type elementtype = array.GetType().GetElementType();
            if (elementtype != val.GetType())
                Temp_Object = System.Convert.ChangeType(val, elementtype);
            if (array.Length == 0)
                throw (new System.NullReferenceException());
            if (fromindex > toindex)
                throw (new System.ArgumentException());
            if ((fromindex < 0) || ((System.Array)array).Length < toindex)
                throw (new System.IndexOutOfRangeException());
            for (int index = (fromindex > 0) ? fromindex-- : fromindex; index < toindex; index++)
                array.SetValue(Temp_Object, index);
        }

        /// <summary>
        /// Fills the array with an specific value.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(System.Array array, System.Object val)
        {
            Fill(array, 0, array.Length, val);
        }
    }


    /*******************************/
    /// <summary>
    /// This class has static methods to manage collections.
    /// </summary>
    public class CollectionsSupport
    {
        /// <summary>
        /// Copies the IList to other IList.
        /// </summary>
        /// <param name="SourceList">IList source.</param>
        /// <param name="TargetList">IList target.</param>
        public static void Copy(System.Collections.IList SourceList, System.Collections.IList TargetList)
        {
            for (int i = 0; i < SourceList.Count; i++)
                TargetList[i] = SourceList[i];
        }

        /// <summary>
        /// Replaces the elements of the specified list with the specified element.
        /// </summary>
        /// <param name="List">The list to be filled with the specified element.</param>
        /// <param name="Element">The element with which to fill the specified list.</param>
        public static void Fill(System.Collections.IList List, System.Object Element)
        {
            for (int i = 0; i < List.Count; i++)
                List[i] = Element;
        }

        /// <summary>
        /// This class implements System.Collections.IComparer and is used for Comparing two String objects by evaluating 
        /// the numeric values of the corresponding Char objects in each string.
        /// </summary>
        class CompareCharValues : System.Collections.IComparer
        {
            public int Compare(System.Object x, System.Object y)
            {
                return System.String.CompareOrdinal((System.String)x, (System.String)y);
            }
        }

        /// <summary>
        /// Obtain the maximum element of the given collection with the specified comparator.
        /// </summary>
        /// <param name="Collection">Collection from which the maximum value will be obtained.</param>
        /// <param name="Comparator">The comparator with which to determine the maximum element.</param>
        /// <returns></returns>
        public static System.Object Max(System.Collections.ICollection Collection, System.Collections.IComparer Comparator)
        {
            System.Collections.ArrayList tempArrayList;

            if (((System.Collections.ArrayList)Collection).IsReadOnly)
                throw new System.NotSupportedException();

            if ((Comparator == null) || (Comparator is System.Collections.Comparer))
            {
                try
                {
                    tempArrayList = new System.Collections.ArrayList(Collection);
                    tempArrayList.Sort();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
                return (System.Object)tempArrayList[Collection.Count - 1];
            }
            else
            {
                try
                {
                    tempArrayList = new System.Collections.ArrayList(Collection);
                    tempArrayList.Sort(Comparator);
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
                return (System.Object)tempArrayList[Collection.Count - 1];
            }
        }

        /// <summary>
        /// Obtain the minimum element of the given collection with the specified comparator.
        /// </summary>
        /// <param name="Collection">Collection from which the minimum value will be obtained.</param>
        /// <param name="Comparator">The comparator with which to determine the minimum element.</param>
        /// <returns></returns>
        public static System.Object Min(System.Collections.ICollection Collection, System.Collections.IComparer Comparator)
        {
            System.Collections.ArrayList tempArrayList;

            if (((System.Collections.ArrayList)Collection).IsReadOnly)
                throw new System.NotSupportedException();

            if ((Comparator == null) || (Comparator is System.Collections.Comparer))
            {
                try
                {
                    tempArrayList = new System.Collections.ArrayList(Collection);
                    tempArrayList.Sort();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
                return (System.Object)tempArrayList[0];
            }
            else
            {
                try
                {
                    tempArrayList = new System.Collections.ArrayList(Collection);
                    tempArrayList.Sort(Comparator);
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
                return (System.Object)tempArrayList[0];
            }
        }


        /// <summary>
        /// Sorts an IList collections
        /// </summary>
        /// <param name="list">The System.Collections.IList instance that will be sorted</param>
        /// <param name="Comparator">The Comparator criteria, null to use natural comparator.</param>
        public static void Sort(System.Collections.IList list, System.Collections.IComparer Comparator)
        {
            if (((System.Collections.ArrayList)list).IsReadOnly)
                throw new System.NotSupportedException();

            if ((Comparator == null) || (Comparator is System.Collections.Comparer))
            {
                try
                {
                    ((System.Collections.ArrayList)list).Sort();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
            }
            else
            {
                try
                {
                    ((System.Collections.ArrayList)list).Sort(Comparator);
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
            }
        }

        /// <summary>
        /// Shuffles the list randomly.
        /// </summary>
        /// <param name="List">The list to be shuffled.</param>
        public static void Shuffle(System.Collections.IList List)
        {
            System.Random RandomList = new System.Random(unchecked((int)System.DateTime.Now.Ticks));
            Shuffle(List, RandomList);
        }

        /// <summary>
        /// Shuffles the list randomly.
        /// </summary>
        /// <param name="List">The list to be shuffled.</param>
        /// <param name="RandomList">The random to use to shuffle the list.</param>
        public static void Shuffle(System.Collections.IList List, System.Random RandomList)
        {
            System.Object source = null;
            int target = 0;

            for (int i = 0; i < List.Count; i++)
            {
                target = RandomList.Next(List.Count);
                source = (System.Object)List[i];
                List[i] = List[target];
                List[target] = source;
            }
        }
    }


    /*******************************/
    /// <summary>
    /// SupportClass for the SortedSet interface.
    /// </summary>
    public interface SortedSetSupport : SetSupport
    {
        /// <summary>
        /// Returns a portion of the list whose elements are less than the limit object parameter.
        /// </summary>
        /// <param name="limit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are less than the limit object parameter.</returns>
        SortedSetSupport HeadSet(System.Object limit);

        /// <summary>
        /// Returns a portion of the list whose elements are greater that the lowerLimit parameter less than the upperLimit parameter.
        /// </summary>
        /// <param name="lowerLimit">The start element of the portion to extract.</param>
        /// <param name="upperLimit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection.</returns>
        SortedSetSupport SubSet(System.Object lowerLimit, System.Object upperLimit);

        /// <summary>
        /// Returns a portion of the list whose elements are greater than the limit object parameter.
        /// </summary>
        /// <param name="limit">The start element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
        SortedSetSupport TailSet(System.Object limit);
    }


    /*******************************/
    /// <summary>
    /// SupportClass for the TreeSet class.
    /// </summary>
    [Serializable]
    public class TreeSetSupport : System.Collections.ArrayList, SetSupport, SortedSetSupport
    {
        private System.Collections.IComparer comparator = System.Collections.Comparer.Default;

        public TreeSetSupport()
            : base()
        {
        }

        public TreeSetSupport(System.Collections.ICollection c)
            : base()
        {
            this.AddAll(c);
        }

        public TreeSetSupport(System.Collections.IComparer c)
            : base()
        {
            this.comparator = c;
        }

        /// <summary>
        /// Gets the IComparator object used to sort this set.
        /// </summary>
        public System.Collections.IComparer Comparator
        {
            get
            {
                return this.comparator;
            }
        }

        /// <summary>
        /// Adds a new element to the ArrayList if it is not already present and sorts the ArrayList.
        /// </summary>
        /// <param name="obj">Element to insert to the ArrayList.</param>
        /// <returns>TRUE if the new element was inserted, FALSE otherwise.</returns>
        new public bool Add(System.Object obj)
        {
            bool inserted;
            if ((inserted = this.Contains(obj)) == false)
            {
                base.Add(obj);
                this.Sort(this.comparator);
            }
            return !inserted;
        }

        /// <summary>
        /// Adds all the elements of the specified collection that are not present to the list.
        /// </summary>		
        /// <param name="c">Collection where the new elements will be added</param>
        /// <returns>Returns true if at least one element was added to the collection.</returns>
        public bool AddAll(System.Collections.ICollection c)
        {
            System.Collections.IEnumerator e = new System.Collections.ArrayList(c).GetEnumerator();
            bool added = false;
            while (e.MoveNext() == true)
            {
                if (this.Add(e.Current) == true)
                    added = true;
            }
            this.Sort(this.comparator);
            return added;
        }

        /// <summary>
        /// Determines whether an element is in the the current TreeSetSupport collection. The IComparer defined for 
        /// the current set will be used to make comparisons between the elements already inserted in the collection and 
        /// the item specified.
        /// </summary>
        /// <param name="item">The object to be locatet in the current collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public override bool Contains(System.Object item)
        {
            System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (this.comparator.Compare(tempEnumerator.Current, item) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns a portion of the list whose elements are less than the limit object parameter.
        /// </summary>
        /// <param name="limit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are less than the limit object parameter.</returns>
        public SortedSetSupport HeadSet(System.Object limit)
        {
            SortedSetSupport newList = new TreeSetSupport();
            for (int i = 0; i < this.Count; i++)
            {
                if (this.comparator.Compare(this[i], limit) >= 0)
                    break;
                newList.Add(this[i]);
            }
            return newList;
        }

        /// <summary>
        /// Returns a portion of the list whose elements are greater that the lowerLimit parameter less than the upperLimit parameter.
        /// </summary>
        /// <param name="lowerLimit">The start element of the portion to extract.</param>
        /// <param name="upperLimit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection.</returns>
        public SortedSetSupport SubSet(System.Object lowerLimit, System.Object upperLimit)
        {
            SortedSetSupport newList = new TreeSetSupport();
            int i = 0;
            while (this.comparator.Compare(this[i], lowerLimit) < 0)
                i++;
            for (; i < this.Count; i++)
            {
                if (this.comparator.Compare(this[i], upperLimit) >= 0)
                    break;
                newList.Add(this[i]);
            }
            return newList;
        }

        /// <summary>
        /// Returns a portion of the list whose elements are greater than the limit object parameter.
        /// </summary>
        /// <param name="limit">The start element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
        public SortedSetSupport TailSet(System.Object limit)
        {
            SortedSetSupport newList = new TreeSetSupport();
            int i = 0;
            while (this.comparator.Compare(this[i], limit) < 0)
                i++;
            for (; i < this.Count; i++)
                newList.Add(this[i]);
            return newList;
        }
    }


    /*******************************/
    /// <summary>
    /// Converts the specified collection to its string representation.
    /// </summary>
    /// <param name="c">The collection to convert to string.</param>
    /// <returns>A string representation of the specified collection.</returns>
    public static System.String CollectionToString(System.Collections.ICollection c)
    {
        System.Text.StringBuilder s = new System.Text.StringBuilder();

        if (c != null)
        {

            System.Collections.ArrayList l = new System.Collections.ArrayList(c);

            bool isDictionary = (c is System.Collections.BitArray || c is System.Collections.Hashtable || c is System.Collections.IDictionary || c is System.Collections.Specialized.NameValueCollection || (l.Count > 0 && l[0] is System.Collections.DictionaryEntry));
            for (int index = 0; index < l.Count; index++)
            {
                if (l[index] == null)
                    s.Append("null");
                else if (!isDictionary)
                    s.Append(l[index]);
                else
                {
                    isDictionary = true;
                    if (c is System.Collections.Specialized.NameValueCollection)
                        s.Append(((System.Collections.Specialized.NameValueCollection)c).GetKey(index));
                    else
                        s.Append(((System.Collections.DictionaryEntry)l[index]).Key);
                    s.Append("=");
                    if (c is System.Collections.Specialized.NameValueCollection)
                        s.Append(((System.Collections.Specialized.NameValueCollection)c).GetValues(index)[0]);
                    else
                        s.Append(((System.Collections.DictionaryEntry)l[index]).Value);

                }
                if (index < l.Count - 1)
                    s.Append(", ");
            }

            if (isDictionary)
            {
                if (c is System.Collections.ArrayList)
                    isDictionary = false;
            }
            if (isDictionary)
            {
                s.Insert(0, "{");
                s.Append("}");
            }
            else
            {
                s.Insert(0, "[");
                s.Append("]");
            }
        }
        else
            s.Insert(0, "null");
        return s.ToString();
    }

    /// <summary>
    /// Tests if the specified object is a collection and converts it to its string representation.
    /// </summary>
    /// <param name="obj">The object to convert to string</param>
    /// <returns>A string representation of the specified object.</returns>
    public static System.String CollectionToString(System.Object obj)
    {
        System.String result = "";

        if (obj != null)
        {
            if (obj is System.Collections.ICollection)
                result = CollectionToString((System.Collections.ICollection)obj);
            else
                result = obj.ToString();
        }
        else
            result = "null";

        return result;
    }
    /*******************************/
    /// <summary>
    /// Summary description for EqualsSupport.
    /// </summary>
    public class EqualsSupport
    {
        /// <summary>
        /// Determines whether two Collections instances are equal.
        /// </summary>
        /// <param name="source">The first Collections to compare. </param>
        /// <param name="target">The second Collections to compare. </param>
        /// <returns>Return true if the first collection is the same instance as the second collection, otherwise returns false.</returns>
        public static bool Equals(System.Collections.ICollection source, System.Collections.ICollection target)
        {
            bool equal = true;

            System.Collections.ArrayList sourceInterfaces = new System.Collections.ArrayList(source.GetType().GetInterfaces());
            System.Collections.ArrayList targetInterfaces = new System.Collections.ArrayList(target.GetType().GetInterfaces());

            if (sourceInterfaces.Contains(System.Type.GetType("SupportClass+SetSupport")) &&
                !targetInterfaces.Contains(System.Type.GetType("SupportClass+SetSupport")))
                equal = false;
            else if (targetInterfaces.Contains(System.Type.GetType("SupportClass+SetSupport")) &&
                !sourceInterfaces.Contains(System.Type.GetType("SupportClass+SetSupport")))
                equal = false;

            if (equal)
            {
                System.Collections.IEnumerator sourceEnumerator = ReverseStack(source);
                System.Collections.IEnumerator targetEnumerator = ReverseStack(target);

                if (source.Count != target.Count)
                    equal = false;

                while (sourceEnumerator.MoveNext() && targetEnumerator.MoveNext())
                    if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
                        equal = false;
            }

            return equal;
        }

        /// <summary>
        /// Determines if a Collection is equal to the Object.
        /// </summary>
        /// <param name="source">The first Collections to compare.</param>
        /// <param name="target">The Object to compare.</param>
        /// <returns>Return true if the first collection contains the same values of the second Object, otherwise returns false.</returns>
        public static bool Equals(System.Collections.ICollection source, System.Object target)
        {
            return (target is System.Collections.ICollection) ? Equals(source, (System.Collections.ICollection)target) : false;
        }

        /// <summary>
        /// Determines if a IDictionaryEnumerator is equal to the Object.
        /// </summary>
        /// <param name="source">The first IDictionaryEnumerator to compare.</param>
        /// <param name="target">The second Object to compare.</param>
        /// <returns>Return true if the first IDictionaryEnumerator contains the same values of the second Object, otherwise returns false.</returns>
        public static bool Equals(System.Collections.IDictionaryEnumerator source, System.Object target)
        {
            return (target is System.Collections.IDictionaryEnumerator) ? Equals(source, (System.Collections.IDictionaryEnumerator)target) : false;
        }

        /// <summary>
        /// Determines if a IDictionary is equal to the Object.
        /// </summary>
        /// <param name="source">The first IDictionary to compare.</param>
        /// <param name="target">The second Object to compare.</param>
        /// <returns>Return true if the first IDictionary contains the same values of the second Object, otherwise returns false.</returns>
        public static bool Equals(System.Collections.IDictionary source, System.Object target)
        {
            return (target is System.Collections.IDictionary) ? Equals(source, (System.Collections.IDictionary)target) : false;
        }

        /// <summary>
        /// Determines whether two IDictionaryEnumerator instances are equals.
        /// </summary>
        /// <param name="source">The first IDictionaryEnumerator to compare.</param>
        /// <param name="target">The second IDictionaryEnumerator to compare.</param>
        /// <returns>Return true if the first IDictionaryEnumerator contains the same values as the second IDictionaryEnumerator, otherwise return false.</returns>
        public static bool Equals(System.Collections.IDictionaryEnumerator source, System.Collections.IDictionaryEnumerator target)
        {
            while (source.MoveNext() && target.MoveNext())
                if (source.Key.Equals(target.Key))
                    if (source.Value.Equals(target.Value))
                        return true;
            return false;
        }

        /// <summary>
        /// Reverses the Stack Collection received.
        /// </summary>
        /// <param name="collection">The collection to reverse.</param>
        /// <returns>The collection received in reverse order if it was a System.Collections.Stack type, otherwise it does 
        /// nothing to the collection.</returns>
        public static System.Collections.IEnumerator ReverseStack(System.Collections.ICollection collection)
        {
            if ((collection.GetType()) == (typeof(System.Collections.Stack)))
            {
                System.Collections.ArrayList collectionStack = new System.Collections.ArrayList(collection);
                collectionStack.Reverse();
                return collectionStack.GetEnumerator();
            }
            else
                return collection.GetEnumerator();
        }

        /// <summary>
        /// Determines whether two IDictionary instances are equal.
        /// </summary>
        /// <param name="source">The first Collection to compare.</param>
        /// <param name="target">The second Collection to compare.</param>
        /// <returns>Return true if the first collection is the same instance as the second collection, otherwise return false.</returns>
        public static bool Equals(System.Collections.IDictionary source, System.Collections.IDictionary target)
        {
            System.Collections.Hashtable targetAux = new System.Collections.Hashtable(target);

            if (source.Count == targetAux.Count)
            {
                System.Collections.IEnumerator sourceEnum = source.Keys.GetEnumerator();
                while (sourceEnum.MoveNext())
                    if (targetAux.Contains(sourceEnum.Current))
                        targetAux.Remove(sourceEnum.Current);
                    else
                        return false;
            }
            else
                return false;
            if (targetAux.Count == 0)
                return true;
            else
                return false;
        }
    }


    /*******************************/
    /// <summary>
    /// Provides functionality for classes that implements the IList interface.
    /// </summary>
    public class IListSupport
    {
        /// <summary>
        /// Ensures the capacity of the list to be greater or equal than the specified.
        /// </summary>
        /// <param name="list">The list to be checked.</param>
        /// <param name="capacity">The expected capacity.</param>
        public static void EnsureCapacity(System.Collections.ArrayList list, int capacity)
        {
            if (list.Capacity < capacity) list.Capacity = 2 * list.Capacity;
            if (list.Capacity < capacity) list.Capacity = capacity;
        }

        /// <summary>
        /// Adds all the elements contained into the specified collection, starting at the specified position.
        /// </summary>
        /// <param name="index">Position at which to add the first element from the specified collection.</param>
        /// <param name="list">The list used to extract the elements that will be added.</param>
        /// <param name="c"></param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public static bool AddAll(System.Collections.IList list, int index, System.Collections.ICollection c)
        {
            bool result = false;
            if (c != null)
            {
                System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(c).GetEnumerator();
                int tempIndex = index;

                while (tempEnumerator.MoveNext())
                {
                    list.Insert(tempIndex++, tempEnumerator.Current);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an enumerator of the collection starting at the specified position.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index">The position to set the iterator.</param>
        /// <returns>An IEnumerator at the specified position.</returns>
        public static System.Collections.IEnumerator GetEnumerator(System.Collections.IList list, int index)
        {
            if ((index < 0) || (index > list.Count))
                throw new System.IndexOutOfRangeException();

            System.Collections.IEnumerator tempEnumerator = list.GetEnumerator();
            if (index > 0)
            {
                int i = 0;
                while ((tempEnumerator.MoveNext()) && (i < index - 1))
                    i++;
            }
            return tempEnumerator;
        }
    }


    /*******************************/
    /// <summary>
    /// Provides functionality not found in .NET map-related interfaces.
    /// </summary>
    public class MapSupport
    {
        /// <summary>
        /// Determines whether the SortedList contains a specific value.
        /// </summary>
        /// <param name="d">The dictionary to check for the value.</param>
        /// <param name="obj">The object to locate in the SortedList.</param>
        /// <returns>Returns true if the value is contained in the SortedList, false otherwise.</returns>
        public static bool ContainsValue(System.Collections.IDictionary d, System.Object obj)
        {
            bool contained = false;
            System.Type type = d.GetType();

            //Classes that implement the SortedList class
            if (type == System.Type.GetType("System.Collections.SortedList"))
            {
                contained = (bool)((System.Collections.SortedList)d).ContainsValue(obj);
            }
            //Classes that implement the Hashtable class
            else if (type == System.Type.GetType("System.Collections.Hashtable"))
            {
                contained = (bool)((System.Collections.Hashtable)d).ContainsValue(obj);
            }
            else
            {
                //Reflection. Invoke "containsValue" method for proprietary classes
                try
                {
                    System.Reflection.MethodInfo method = type.GetMethod("containsValue");
                    contained = (bool)method.Invoke(d, new Object[] { obj });
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }

            return contained;
        }


        /// <summary>
        /// Determines whether the NameValueCollection contains a specific value.
        /// </summary>
        /// <param name="d">The dictionary to check for the value.</param>
        /// <param name="obj">The object to locate in the SortedList.</param>
        /// <returns>Returns true if the value is contained in the NameValueCollection, false otherwise.</returns>
        public static bool ContainsValue(System.Collections.Specialized.NameValueCollection d, System.Object obj)
        {
            bool contained = false;
            System.Type type = d.GetType();

            for (int i = 0; i < d.Count && !contained; i++)
            {
                System.String[] values = d.GetValues(i);
                if (values != null)
                {
                    foreach (System.String val in values)
                    {
                        if (val.Equals(obj))
                        {
                            contained = true;
                            break;
                        }
                    }
                }
            }
            return contained;
        }

        /// <summary>
        /// Copies all the elements of d to target.
        /// </summary>
        /// <param name="target">Collection where d elements will be copied.</param>
        /// <param name="d">Elements to copy to the target collection.</param>
        public static void PutAll(System.Collections.IDictionary target, System.Collections.IDictionary d)
        {
            if (d != null)
            {
                System.Collections.ArrayList keys = new System.Collections.ArrayList(d.Keys);
                System.Collections.ArrayList values = new System.Collections.ArrayList(d.Values);

                for (int i = 0; i < keys.Count; i++)
                    target[keys[i]] = values[i];
            }
        }

        /// <summary>
        /// Returns a portion of the list whose keys are less than the limit object parameter.
        /// </summary>
        /// <param name="l">The list where the portion will be extracted.</param>
        /// <param name="limit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are less than the limit object parameter.</returns>
        public static System.Collections.SortedList HeadMap(System.Collections.SortedList l, System.Object limit)
        {
            System.Collections.Comparer comparer = System.Collections.Comparer.Default;
            System.Collections.SortedList newList = new System.Collections.SortedList();

            for (int i = 0; i < l.Count; i++)
            {
                if (comparer.Compare(l.GetKey(i), limit) >= 0)
                    break;

                newList.Add(l.GetKey(i), l[l.GetKey(i)]);
            }

            return newList;
        }

        /// <summary>
        /// Returns a portion of the list whose keys are greater that the lowerLimit parameter less than the upperLimit parameter.
        /// </summary>
        /// <param name="list">The list where the portion will be extracted.</param>
        /// <param name="lowerLimit">The start element of the portion to extract.</param>
        /// <param name="upperLimit">The end element of the portion to extract.</param>
        /// <returns>The portion of the collection.</returns>
        public static System.Collections.SortedList SubMap(System.Collections.SortedList list, System.Object lowerLimit, System.Object upperLimit)
        {
            System.Collections.Comparer comparer = System.Collections.Comparer.Default;
            System.Collections.SortedList newList = new System.Collections.SortedList();

            if (list != null)
            {
                if ((list.Count > 0) && (!(lowerLimit.Equals(upperLimit))))
                {
                    int index = 0;
                    while (comparer.Compare(list.GetKey(index), lowerLimit) < 0)
                        index++;

                    for (; index < list.Count; index++)
                    {
                        if (comparer.Compare(list.GetKey(index), upperLimit) >= 0)
                            break;

                        newList.Add(list.GetKey(index), list[list.GetKey(index)]);
                    }
                }
            }

            return newList;
        }

        /// <summary>
        /// Returns a portion of the list whose keys are greater than the limit object parameter.
        /// </summary>
        /// <param name="list">The list where the portion will be extracted.</param>
        /// <param name="limit">The start element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
        public static System.Collections.SortedList TailMap(System.Collections.SortedList list, System.Object limit)
        {
            System.Collections.Comparer comparer = System.Collections.Comparer.Default;
            System.Collections.SortedList newList = new System.Collections.SortedList();

            if (list != null)
            {
                if (list.Count > 0)
                {
                    int index = 0;
                    while (comparer.Compare(list.GetKey(index), limit) < 0)
                        index++;

                    for (; index < list.Count; index++)
                        newList.Add(list.GetKey(index), list[list.GetKey(index)]);
                }
            }

            return newList;
        }
    }


    /*******************************/
    /// <summary>
    /// This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static long Identity(long literal)
    {
        return literal;
    }

    /// <summary>
    /// This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static ulong Identity(ulong literal)
    {
        return literal;
    }

    /// <summary>
    /// This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static float Identity(float literal)
    {
        return literal;
    }

    /// <summary>
    /// This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static double Identity(double literal)
    {
        return literal;
    }

    /*******************************/
    /// <summary>
    /// Copies all of the elements from the source Dictionary to target Dictionary. These elements will replace any elements that 
    /// target Dictionary had for any of the elements currently in the source dictionary.
    /// </summary>
    /// <param name="target">Target Dictionary.</param>
    /// <param name="source">Source Dictionary.</param>
    public static void PutAll(System.Collections.IDictionary target, System.Collections.IDictionary source)
    {
        System.Collections.ICollection tempCollection1 = source.Keys;
        System.Collections.ICollection tempCollection2 = source.Values;

        System.Array tempArray1 = System.Array.CreateInstance(typeof(System.Object), tempCollection1.Count);
        System.Array tempArray2 = System.Array.CreateInstance(typeof(System.Object), tempCollection2.Count);

        System.Int32 tempInt1 = new System.Int32();
        System.Int32 tempInt2 = new System.Int32();

        tempCollection1.CopyTo(tempArray1, tempInt1);
        tempCollection2.CopyTo(tempArray2, tempInt2);

        for (long index = 0; index < tempCollection1.Count; index++)
            target[tempArray1.GetValue(index)] = tempArray2.GetValue(index);
    }


    /*******************************/
    /// <summary>
    /// Support class used to handle threads
    /// </summary>
    public class ThreadClass : IRunnable
    {
        /// <summary>
        /// The instance of System.Threading.Thread
        /// </summary>
        private System.Threading.Thread threadField;

        /// <summary>
        /// Initializes a new instance of the ThreadClass class
        /// </summary>
        public ThreadClass()
        {
            threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(System.String Name)
        {
            threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
            this.Name = Name;
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        public ThreadClass(System.Threading.ThreadStart Start)
        {
            threadField = new System.Threading.Thread(Start);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(System.Threading.ThreadStart Start, System.String Name)
        {
            threadField = new System.Threading.Thread(Start);
            this.Name = Name;
        }

        /// <summary>
        /// This method has no functionality unless the method is overridden
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        /// Causes the operating system to change the state of the current thread instance to ThreadState.Running
        /// </summary>
        public virtual void Start()
        {
            threadField.Start();
        }

        /// <summary>
        /// Interrupts a thread that is in the WaitSleepJoin thread state
        /// </summary>
        public virtual void Interrupt()
        {
            threadField.Interrupt();
        }

        /// <summary>
        /// Gets the current thread instance
        /// </summary>
        public System.Threading.Thread Instance
        {
            get
            {
                return threadField;
            }
            set
            {
                threadField = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the thread
        /// </summary>
        public System.String Name
        {
            get
            {
                return threadField.Name;
            }
            set
            {
                if (threadField.Name == null)
                    threadField.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public System.Threading.ThreadPriority Priority
        {
            get
            {
                return threadField.Priority;
            }
            set
            {
                threadField.Priority = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the execution status of the current thread
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return threadField.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return threadField.IsBackground;
            }
            set
            {
                threadField.IsBackground = value;
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates
        /// </summary>
        public void Join()
        {
            threadField.Join();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        public void Join(long MiliSeconds)
        {
            lock (this)
            {
                threadField.Join(new System.TimeSpan(MiliSeconds * 10000));
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        /// <param name="NanoSeconds">Time of wait in nanoseconds</param>
        public void Join(long MiliSeconds, int NanoSeconds)
        {
            lock (this)
            {
                threadField.Join(new System.TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
            }
        }

        /// <summary>
        /// Resumes a thread that has been suspended
        /// </summary>
        public void Resume()
        {
#pragma warning disable 612,618
            threadField.Resume();
#pragma warning restore 612,618
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread. Calling this method 
        /// usually terminates the thread
        /// </summary>
        public void Abort()
        {
            threadField.Abort();
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread while also providing
        /// exception information about the thread termination. 
        /// Calling this method usually terminates the thread.
        /// </summary>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
        public void Abort(System.Object stateInfo)
        {
            lock (this)
            {
                threadField.Abort(stateInfo);
            }
        }

        /// <summary>
        /// Suspends the thread, if the thread is already suspended it has no effect
        /// </summary>
        public void Suspend()
        {
#pragma warning disable 612,618
            threadField.Suspend();
#pragma warning restore 612,618
        }

        /// <summary>
        /// Obtain a String that represents the current Object
        /// </summary>
        /// <returns>A String that represents the current Object</returns>
        public override System.String ToString()
        {
            return "Thread[" + Name + "," + Priority.ToString() + "," + "" + "]";
        }

        /// <summary>
        /// Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadClass Current()
        {
            ThreadClass CurrentThread = new ThreadClass();
            CurrentThread.Instance = System.Threading.Thread.CurrentThread;
            return CurrentThread;
        }
    }


    /*******************************/
    /// <summary>
    /// Lets you create an action to be performed with security privileges.
    /// </summary>
    public interface IPriviligedAction
    {
        /// <summary>
        /// Performs the priviliged action.
        /// </summary>
        /// <returns>A value that may represent the result of the action.</returns>
        System.Object Run();
    }


    /*******************************/
    /// <summary>
    /// Writes the exception stack trace to the received stream
    /// </summary>
    /// <param name="throwable">Exception to obtain information from</param>
    /// <param name="stream">Output sream used to write to</param>
    public static void WriteStackTrace(System.Exception throwable, System.IO.TextWriter stream)
    {
        stream.Write(throwable.StackTrace);
        stream.Flush();
    }

    /*******************************/
    /// <summary>
    /// Deserializes an object, or an entire graph of connected objects, and returns the object intance
    /// </summary>
    /// <param name="binaryReader">Reader instance used to read the object</param>
    /// <returns>The object instance</returns>
    public static System.Object Deserialize(System.IO.BinaryReader binaryReader)
    {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        return formatter.Deserialize(binaryReader.BaseStream);
    }

}
#pragma warning restore 1574
