#region License

/*
 * Copyright (C) 2009-2010 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spring.Collections.Generic;
using Spring.Utility;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// A thread-safe variant of <see cref="List{T}"/> in which all mutative
    /// operations (<see cref="Add"/>, <see cref="this"/>, and so on) are
    /// implemented by making a fresh copy of the underlying array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is ordinarily too costly, but may be <i>more</i> efficient
    /// than alternatives when traversal operations vastly outnumber
    /// mutations, and is useful when you cannot or don't want to
    /// synchronize traversals, yet need to preclude interference among
    /// concurrent threads.  The "snapshot" style enumerator method uses a
    /// reference to the state of the array at the point that the enumerator
    /// was created. This array never changes during the lifetime of the
    /// enumerator, so interference is impossible and the enumerator is
    /// guaranteed not to throw <see cref="InvalidOperationException"/>.
    /// The enumerator will not reflect additions, removals, or changes to
    /// the list since the enuemrator was created.
    /// </para>
    /// <para>
    /// All elements are permitted, including <c>null</c>.
    /// </para>
    /// <para>
    /// Memory consistency effects: As with other concurrent
    /// collections, actions in a thread prior to placing an object into a
    /// <see cref="CopyOnWriteList{T}"/> <i>happen-before</i> actions
    /// subsequent to the access or removal of that element from
    /// the <see cref="CopyOnWriteList{T}"/> in another thread.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Type of the element.</typeparam>
    /// <author>Kenneth Xu</author>
    [Serializable] public class CopyOnWriteList<T> : AbstractList<T> // BACKPORT_3_1 replaces CopyOnWriteArrayList
    {
        static readonly T[] _empty = new T[0];
        private volatile T[] _items;

        /// <summary>
        /// Creates an empty <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        public CopyOnWriteList()
        {
            _items = _empty;
        }

        /// <summary>
        /// Creates a <see cref="CopyOnWriteList{T}"/> containing the elements
        /// of the specified <paramref name="collection"/>, in the enumeration
        /// order.
        /// </summary>
        /// <param name="collection"></param>
        public CopyOnWriteList(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            _items = collection.ToArrayOptimized();
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// 
        /// <param name="index">
        /// The zero-based index of the element to get or set.</param>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        public override T this[int index]
        {
            get
            {
                var items = _items; // one volatile read only
                int size = items.Length;
                VerifyIndex(index, size, size);
                return items[index];
            }
            set
            {
                int size;
                lock (this)
                {
                    size = _items.Length;
                    if (index >= 0 && index < size)
                    {
                        T[] items = MakeCopy(size);
                        items[index] = value;
                        _items = items;
                        return;
                    }
                }
                VerifyIndex(index, size, size); // this throws exception.
            }
        }

        /// <summary>
        /// Searches the entire sorted <see cref="CopyOnWriteList{T}"/> for an
        /// element using the default comparer and returns the zero-based index
        /// of the element.
        /// </summary>
        /// <param name="item">
        /// The object to locate. The value can be <c>null</c> for reference types.
        /// </param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="CopyOnWriteList{T}"/>,
        /// if item is found; otherwise, a negative number that is the bitwise
        /// complement of the index of the next element that is larger than item or,
        /// if there is no larger element, the bitwise complement of <see cref="Count"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The default comparer <see cref="Comparer{T}.Default"/> cannot find an
        /// implementation of the <see cref="IComparable{T}"/> generic interface or
        /// the <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
        /// </exception>
        public int BinarySearch(T item)
        {
            var items = _items; // one volatile read only.
            return BinarySearch(items, 0, items.Length, item, null);
        }

        /// <summary>
        /// Searches the entire sorted <see cref="CopyOnWriteList{T}"/> for an
        /// element using the specified <paramref name="comparer"/> and returns
        /// the zero-based index of the element.
        /// </summary>
        /// <param name="item">
        /// The object to locate. The value can be <c>null</c> for reference types.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing
        /// elements.
        /// <br/>-or-<br/>
        /// <c>null</c> to use the default comparer <see cref="Comparer{T}.Default"/>.
        /// </param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="CopyOnWriteList{T}"/>,
        /// if item is found; otherwise, a negative number that is the bitwise
        /// complement of the index of the next element that is larger than item or,
        /// if there is no larger element, the bitwise complement of <see cref="Count"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is <c>null</c>, and the default comparer
        /// <see cref="Comparer{T}.Default"/> cannot find an implementation of the
        /// <see cref="IComparable{T}"/> generic interface or the
        /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
        /// </exception>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            var items = _items; // one volatile read only.
            return BinarySearch(items, 0, items.Length, item, comparer);
        }

        /// <summary>
        /// Searches the entire sorted <see cref="CopyOnWriteList{T}"/> for an
        /// element using the specified <see cref="Comparison{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate. The value can be <c>null</c> for reference types.
        /// </param>
        /// <param name="comparison">
        /// The <see cref="Comparison{T}"/> to use when comparing elements.
        /// </param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="CopyOnWriteList{T}"/>,
        /// if item is found; otherwise, a negative number that is the bitwise
        /// complement of the index of the next element that is larger than item or,
        /// if there is no larger element, the bitwise complement of <see cref="Count"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="comparison"/> is <c>null</c>.
        /// </exception>
        public int BinarySearch(T item, Comparison<T> comparison)
        {
            return BinarySearch(item, new ComparisonComparer<T>(comparison));
        }

        /// <summary>
        /// Searches the entire sorted <see cref="CopyOnWriteList{T}"/> for an
        /// element using the specified <paramref name="comparer"/> and returns
        /// the zero-based index of the element.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the range to search.
        /// </param>
        /// <param name="count">
        /// The length of the range to search.
        /// </param>
        /// <param name="item">
        /// The object to locate. The value can be <c>null</c> for reference types.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing
        /// elements.
        /// <br/>-or-<br/>
        /// <c>null</c> to use the default comparer <see cref="Comparer{T}.Default"/>.
        /// </param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="CopyOnWriteList{T}"/>,
        /// if item is found; otherwise, a negative number that is the bitwise
        /// complement of the index of the next element that is larger than item or,
        /// if there is no larger element, the bitwise complement of <see cref="Count"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// <br/>-or-<br/>
        /// <paramref name="count"/> is less than 0
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not
        /// specify a valid section in the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is <c>null</c>, and the default comparer
        /// <see cref="Comparer{T}.Default"/> cannot find an implementation of the
        /// <see cref="IComparable{T}"/> generic interface or the
        /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
        /// </exception>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return BinarySearch(_items, index, count, item, comparer);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        public override void Clear()
        {
            lock (this) _items = _empty;
        }

        /// <summary> 
        /// Returns <c>true</c> if this list contains all of the elements of the
        /// specified <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">
        /// Collection to be checked for containment in this list.
        /// </param>
        /// <returns>
        /// <c>true</c> if this list contains all of the elements of the
        /// specified <paramref name="collection"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="collection"/> is null.
        /// </exception>
        public bool ContainsAll(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            var items = _items;
            int size = items.Length;
            foreach (var e in collection)
            {
                if (Array.IndexOf(items, e, 0, size) < 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Converts the elements in the current <see cref="CopyOnWriteList{T}"/>
        /// to another type, and returns a list containing the converted elements.
        /// </summary>
        /// <typeparam name="TOutput">
        /// Type of the converted elements.
        /// </typeparam>
        /// <param name="converter">
        /// A <see cref="Converter{TInput,TOutput}"/> delegate that converts each
        /// element from one type to another type.
        /// </param>
        /// <returns>
        /// A <see cref="CopyOnWriteList{T}"/> of the target type containing the
        /// converted elements from the current <see cref="CopyOnWriteList{T}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="converter"/> is <c>null</c>.
        /// </exception>
        public CopyOnWriteList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            var items = _items; // one volatile read only.
            TOutput[] newItems = new TOutput[items.Length];
            for (int i = items.Length - 1; i >= 0; i--)
            {
                newItems[i] = converter(items[i]);
            }
            return new CopyOnWriteList<TOutput>{_items = newItems};
        }

        /// <summary>
        /// Determines whether the <see cref="CopyOnWriteList{T}"/> contains
        /// elements that match the conditions defined by the specified
        /// predicate.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the
        /// conditions of the elements to search for.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="CopyOnWriteList{T}"/> contains one or
        /// more elements that match the conditions defined by the specified
        /// predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public bool Exists(Predicate<T> match)
        {
            return (FindIndex(match) != -1);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the first occurrence within the
        /// entire <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The first element that matches the conditions defined by the
        /// specified predicate, if found; otherwise, the default value for
        /// type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public T Find(Predicate<T> match)
        {
            VerifyMatchArgument(match);

            var items = _items; // one volatile read only.
            for (int i = 0; i < items.Length; i++)
            {
                if (match(items[i])) return items[i];
            }
            return default(T);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the last occurrence within the
        /// entire <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The last element that matches the conditions defined by the
        /// specified predicate, if found; otherwise, the default value for
        /// type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public T FindLast(Predicate<T> match)
        {
            VerifyMatchArgument(match);

            var items = _items; // one volatile read only.
            for (int i = items.Length - 1; i >= 0; i--)
            {
                if (match(items[i])) return items[i];
            }
            return default(T);
        }

        /// <summary>
        /// Retrieves the all the elements that match the conditions defined
        /// by the specified predicate.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// A <see cref="CopyOnWriteList{T}"/> containing all the elements that
        /// match the conditions defined by the specified predicate, if found;
        /// otherwise, an empty <see cref="CopyOnWriteList{T}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public CopyOnWriteList<T> FindAll(Predicate<T> match)
        {
            VerifyMatchArgument(match);

            var items = _items; // one volatile read only.
            var array = new EnumerableToArrayBuffer<T>(items.Where(e => match(e))).ToArray();

            return new CopyOnWriteList<T> {_items = array};
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the first
        /// occurrence within the entire <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public int FindIndex(Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items;
            return FindIndex(items, 0, items.Length, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the last
        /// occurrence within the entire <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public int FindLastIndex(Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items;
            return FindLastIndex(items, items.Length, items.Length, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the first
        /// occurrence within the range of elements in the 
        /// <see cref="CopyOnWriteList{T}"/>  that extends from the specified
        /// <paramref name="index"/> to the last element.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public int FindIndex(int index, Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items;
            var size = items.Length;
            VerifyIndex(index, size, size);
            return FindIndex(items, index, size - index, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the last
        /// occurrence within the range of elements in the 
        /// <see cref="CopyOnWriteList{T}"/> that extends from the first
        /// element to specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        public int FindLastIndex(int index, Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items;
            var size = items.Length;
            VerifyIndex(index, size, size);
            return FindIndex(items, index, index, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the first
        /// occurrence within the range of elements in the 
        /// <see cref="CopyOnWriteList{T}"/> that starts at the specified
        /// <paramref name="index"/> and contains the specified number of elements.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <param name="count">
        /// The number of elements in the section to search.
        /// </param>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// <br/>-or-<br/>
        /// <paramref name="count"/> is less than 0
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not
        /// specify a valid section in the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        public int FindIndex(int index, int count, Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items;
            var size = items.Length;
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index, size);
            return FindIndex(items, index, count, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the
        /// specified predicate, and returns the zero-based index of the last
        /// occurrence within the range of elements in the 
        /// <see cref="CopyOnWriteList{T}"/> that contains the specified number
        /// of elements and starts at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <param name="count">
        /// The number of elements in the section to search.
        /// </param>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the element to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of an element that
        /// matches the conditions defined by <paramref name="match"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// <br/>-or-<br/>
        /// <paramref name="count"/> is less than 0
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not
        /// specify a valid section in the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        public int FindLastIndex(int index, int count, Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items; // one volatile read only.
            var size = items.Length;
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index);
            return FindLastIndex(items, index, count, match);
        }

        /// <summary>
        /// Performs the specified action on each element of the
        /// <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action{T}"/> delegate to perform on each element of
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is null.
        /// </exception>
        public void ForEach(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            var items = _items; // one volatile read only.
            for (int i = 0; i < items.Length; i++)
            {
                action(items[i]);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// Subclass must implement this method.
        /// </remarks>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate 
        /// through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)_items).GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// 
        /// <param name="item">
        /// The object to locate in the <see cref="IList{T}"/>.
        /// </param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public override int IndexOf(T item)
        {
            return Array.IndexOf(_items, item);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the first occurrence within the range of elements in the
        /// <see cref="CopyOnWriteList{T}"/> that extends from the specified 
        /// index to the last element.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="CopyOnWriteList{T}"/>. The
        /// value can be null for reference types.
        /// </param>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the
        /// range of elements in the <see cref="CopyOnWriteList{T}"/> that
        /// extends from index to the last element, if found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is outside the range of valid indexes for the 
        /// <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        public int IndexOf(T item, int index)
        {
            var items = _items; // one volatile read only.
            if (index > items.Length)
            {
                throw new ArgumentOutOfRangeException("index", index, 
                    "Value cannot be greater than list size.");
            }
            return Array.IndexOf(items, item, index, items.Length - index);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the first occurrence within the range of elements in the
        /// <see cref="CopyOnWriteList{T}"/> that starts at the specified index
        /// and contains the specified number of elements.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="CopyOnWriteList{T}"/>. The
        /// value can be null for reference types.
        /// </param>
        /// <param name="index">
        /// The zero-based starting index of the search.
        /// </param>
        /// <param name="count">
        /// The number of elements in the section to search.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the
        /// range of elements in the <see cref="CopyOnWriteList{T}"/> that
        /// starts at index and contains count number of elements, if found;
        /// otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.<br/>
        /// -or-<br/>
        /// <paramref name="count"/> is less than 0.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote
        /// a valid range of elements in the <see cref="CopyOnWriteList{T}"/>. 
        /// </exception>
        public int IndexOf(T item, int index, int count)
        {
            var items = _items; // one volatile read only.
            int size = items.Length;
            VerifyIndex(index, size+1, size);
            VerifyCountArgument(count, index, size);
            return Array.IndexOf(items, item, index, count);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        public override int Count
        {
            get { return _items.Length; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        /// 
        /// <returns>
        /// <c>false</c>.
        /// </returns>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the
        /// list.
        /// </summary>
        /// 
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ICollection{T}"/>; 
        /// otherwise, <c>false</c>. This method also returns false if item is not found in the 
        /// original <see cref="ICollection{T}"/>.
        /// </returns>
        /// 
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        public override bool Remove(T item)
        {
            lock(this)
            {
                var items = _items; // one volatile read only.
                var index = Array.IndexOf(items, item);
                if (index < 0) return false;
                _items = RemoveRange(items, index, 1);
                return true;
            }
        }

        /// <summary>
        /// Removes the all the elements that match the conditions defined by
        /// the specified predicate.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the elements to remove.
        /// </param>
        /// <returns>
        /// The number of elements removed from the <see cref="CopyOnWriteList{T}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <c>null</c>.
        /// </exception>
        public int RemoveAll(Predicate<T> match)
        {
            VerifyMatchArgument(match);
            lock (this)
            {
                int index = 0;
                var items = _items;
                var size = items.Length;
                while ((index < size) && !match(items[index]))
                {
                    index++;
                }
                if (index >= size) 
                    return 0;

                var newItems = new T[size - 1];
                if (index>0) Array.Copy(items, 0, newItems, 0, index);
                for (int i = index + 1; i < size; i++)
                {
                    if (!match(items[i])) newItems[index++] = items[i];
                }
                if (newItems.Length > index)
                    Array.Resize(ref newItems, index);
                _items = newItems;
                return size - index;
            }
        }

        /// <summary>
        /// Removes a range of elements from the <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the range of elements to remove.
        /// </param>
        /// <param name="count">
        /// The number of elements to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.<br/>
        /// -or-<br/>
        /// <paramref name="count"/> is less than 0.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote
        /// a valid range of elements in the <see cref="CopyOnWriteList{T}"/>. 
        /// </exception>
        public void RemoveRange(int index, int count)
        {
            int size;
            lock(this)
            {
                var items = _items;
                size = items.Length;
                if (index >= 0 && count >= 0 && count + index <= size)
                {
                    if (count != 0) _items = RemoveRange(items, index, count);
                    return;
                }
            }
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index, size);
        }

        /// <summary>
        /// Reverses the order of the elements in the entire
        /// <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        public void Reverse()
        {
            lock(this)
            {
                var items = _items; // only read volatile once.
                var size = items.Length;
                var newItems = new T[size];
                Array.Reverse(newItems, 0, size);
                _items = newItems;
            }
        }

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the range to reverse.
        /// </param>
        /// <param name="count">
        /// The number of elements in the range to reverse.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="count"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is greater than size of the
        /// <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote
        /// a valid range of elements in the <see cref="CopyOnWriteList{T}"/>. 
        /// </exception>
        public void Reverse(int index, int count)
        {
            int size;
            lock (this)
            {
                var items = _items; // only read volatile once.
                size = items.Length;
                if (index >= 0 && count >= 0 && count + index <= size)
                {
                    var newItems = new T[size];
                    Array.Copy(items, 0, newItems, 0, size);
                    Array.Reverse(newItems, index, count);
                    _items = newItems;
                    return;
                }
            }
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index, size);
        }

        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// 
        /// <param name="index">
        /// The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If index is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        public override void RemoveAt(int index)
        {
            int size;
            lock (this)
            {
                var items = _items; // one volatile read only.
                size = items.Length;
                if (index >= 0 && index < size)
                {
                    _items = RemoveRange(items, index, 1);
                }
            }
            VerifyIndex(index, size, size);
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="item">
        /// The object to insert into the <see cref="IList{T}"/>.</param>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If index is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        public override void Insert(int index, T item)
        {
            int size;
            lock (this)
            {
                var items = _items; // one volatile read only.
                if(InsertOne(ref items, index, item))
                {
                    _items = items;
                    return;
                }
                // failed to insert, record the size for error reporting.
                size = items.Length;
            }
            VerifyIndex(index, size+1, size);
        }

        /// <summary>
        /// Inserts the elements of a collection into the
        /// <see cref="CopyOnWriteList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the new elements should be inserted.
        /// </param>
        /// <param name="collection">
        /// The collection whose elements should be inserted into the
        /// <see cref="CopyOnWriteList{T}"/>. The collection itself cannot be
        /// <c>null</c>, but it can contain elements that are <c>null</c>, if
        /// type <typeparamref name="T"/> is a reference type.
        /// </param>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            int size;
            lock(this)
            {
                var items = _items; // one volatile read only.
                size = items.Length;
                if (index >= 0 && index <= size)
                {
                    if (InsertRange(ref items, index, collection))
                        _items = items;
                    return;
                }
            }
            VerifyIndex(index, size, size);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the last occurrence within the range of elements within the
        /// entire <see cref="CopyOnWriteList{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="CopyOnWriteList{T}"/>. The
        /// value can be <c>null</c> for reference types.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of item within the
        /// entire <see cref="CopyOnWriteList{T}"/>, if found; otherwise, –1.
        /// </returns>
        public int LastIndexOf(T item)
        {
            var items = _items; // only valatile read once
            return Array.LastIndexOf(items, item, _items.Length - 1, items.Length);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the last occurrence within the range of elements in the
        /// <see cref="CopyOnWriteList{T}"/> that extends from the first element
        /// to the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="CopyOnWriteList{T}"/>. The
        /// value can be <c>null</c> for reference types.
        /// </param>
        /// <param name="index">
        /// The zero-based starting index of the backward search.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of item within the
        /// range of elements in the <see cref="CopyOnWriteList{T}"/> that
        /// extends from the first element to <paramref name="index"/>, if
        /// found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.
        /// </exception>
        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, index + 1);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the last occurrence within the range of elements in the
        /// <see cref="CopyOnWriteList{T}"/> that contains the specified number
        /// of elements and ends at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="CopyOnWriteList{T}"/>. The
        /// value can be <c>null</c> for reference types.
        /// </param>
        /// <param name="index">
        /// The zero-based starting index of the backward search.
        /// </param>
        /// <param name="count">
        /// The number of elements in the section to search.
        /// </param>
        /// <returns>
        /// The zero-based index of the last occurrence of item within the
        /// range of elements in the <see cref="CopyOnWriteList{T}"/> that
        /// contains <paramref name="count"/> number of elements and ends at
        /// <paramref name="index"/>, if found; otherwise, –1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.<br/>
        /// -or-<br/>
        /// <paramref name="count"/> is less than 0.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote
        /// a valid range of elements in the <see cref="CopyOnWriteList{T}"/>. 
        /// </exception>
        public int LastIndexOf(T item, int index, int count)
        {
            var items = _items; // only volatile read once.
            int size = items.Length;
            VerifyIndex(index, size + 1, size);
            VerifyCountArgument(count, index, size);
            return Array.LastIndexOf(items, item, index, count);
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="ICollection{T}"/>.
        /// </param>
        public override void Add(T item)
        {
            lock(this)
            {
                var items = _items; // one volatile read only.
                InsertOne(ref items, items.Length, item);
                _items = items;
            }
        }

        /// <summary>
        /// Append the element if not present.
        /// </summary>
        /// <param name="item">
        /// Element to be added to this list, if absent.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element was added. Otherwise <c>false</c>.
        /// </returns>
        public bool AddIfAbsent(T item)
        {
            lock(this)
            {
                var items = _items; // one volatile read only.
                if (Array.IndexOf(items, item) >= 0) return false;
                InsertOne(ref items, items.Length, item);
                _items = items;
                return true;
            }
        }

        /// <summary>
        /// Appends all of the elements in the specified collection that
        /// are not already contained in this list, to the end of
        /// this list. Duplicated elements will be only added once.
        /// </summary>
        /// <param name="collection">
        /// Collection containing elements to be added to this list.
        /// </param>
        /// <returns>
        /// The number of elements added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="collection"/> is null.
        /// </exception>
        /// <seealso cref="AddIfAbsent"/>
        public int AddRangeAbsent(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            var cs = collection.ToArrayOptimized();
            if (cs.Length == 0) return 0;
            var uniq = new T[cs.Length];
            lock(this)
            {
                var items = _items;
                int size = items.Length;
                int added = 0;
                for (int i = 0; i < cs.Length; ++i)
                { // scan for duplicates
                    var e = cs[i];
                    if (Array.IndexOf(items, e, 0, size) < 0 &&
                        Array.IndexOf(uniq, e, 0, added) < 0)
                        uniq[added++] = e;
                }
                if (added > 0)
                {
                    var newElements = new T[size + added];
                    Array.Copy(items, 0, newElements, 0, size);
                    Array.Copy(uniq, 0, newElements, size, added);
                    _items = newElements;
                }
                return added;
            }
        }

        /// <summary>
        /// Called by <see cref="AbstractCollection{T}.AddRange"/> to add items
        /// in <paramref name="collection"/> to this list, after the parameter
        /// is validated to be neither <c>null</c> nor this collection itself.
        /// </summary>
        /// <param name="collection">Collection of items to be added.</param>
        /// <returns>
        /// <c>true</c> if this collection is modified, else <c>false</c>.
        /// </returns>
        protected override bool DoAddRange(IEnumerable<T> collection)
        {
            lock(this)
            {
                #pragma warning disable 420
                // OK to pass volatile as ref because we are inside lock.
                return AddRange(ref _items, collection);
                #pragma warning restore 420
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="CopyOnWriteList{T}"/>
        /// using the default comparer.
        /// </summary>
        public void Sort()
        {
            lock (this)
            {
                #pragma warning disable 420
                // OK to pass volatile as ref as we are in lock block.
                Sort(ref _items, 0, -1, null);
                #pragma warning restore 420
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="CopyOnWriteList{T}"/>
        /// using the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing
        /// elements, or <c>null</c> to use the default comparer
        /// <see cref="Comparer{T}.Default"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The implementation of <paramref name="comparer"/> caused an error
        /// during the sort. For example, <paramref name="comparer"/> might not
        /// return 0 when comparing an item with itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is <c>null</c>, and the default comparer
        /// <see cref="Comparer{T}.Default"/> cannot find implementation of the
        /// <see cref="IComparable{T}"/> generic interface or the 
        /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
        /// </exception>
        public void Sort(IComparer<T> comparer)
        {
            lock(this)
            {
                #pragma warning disable 420
                // OK to pass volatile as ref as we are in lock block.
                Sort(ref _items, 0, -1, comparer);
                #pragma warning restore 420
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="CopyOnWriteList{T}"/>
        /// using the specified.
        /// </summary>
        /// <param name="comparison">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing
        /// elements, or <c>null</c> to use the default comparer
        /// <see cref="Comparer{T}.Default"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The implementation of <paramref name="comparison"/> caused an error
        /// during the sort. For example, <paramref name="comparison"/> might not
        /// return 0 when comparing an item with itself.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="comparison"/> is <c>null</c>.
        /// </exception>
        public void Sort(Comparison<T> comparison)
        {
            Sort(new ComparisonComparer<T>(comparison));
        }

        /// <summary>
        /// Sorts the elements in a range of elements in
        /// <see cref="CopyOnWriteList{T}"/> using the specified
        /// <paramref name="comparer"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the range to sort.
        /// </param>
        /// <param name="count">
        /// The length of the range to sort.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing
        /// elements, or <c>null</c> to use the default comparer
        /// <see cref="Comparer{T}.Default"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the range of valid indexes for
        /// the <see cref="CopyOnWriteList{T}"/>.<br/>
        /// -or-<br/>
        /// <paramref name="count"/> is less than 0.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote
        /// a valid range of elements in the <see cref="CopyOnWriteList{T}"/>. 
        /// <br/>-or-<br/>
        /// The implementation of <paramref name="comparer"/> caused an error
        /// during the sort. For example, <paramref name="comparer"/> might not
        /// return 0 when comparing an item with itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is <c>null</c>, and the default comparer
        /// <see cref="Comparer{T}.Default"/> cannot find implementation of the
        /// <see cref="IComparable{T}"/> generic interface or the 
        /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
        /// </exception>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            int size;
            lock(this)
            {
                size = _items.Length;
                if (index >= 0 && count >= 0 && count + index <= size)
                {
                    #pragma warning disable 420
                    // OK to pass volatile as ref as we are in lock block.
                    Sort(ref _items, index, count, comparer);
                    #pragma warning restore 420
                    return;
                }
            }
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index, size);
        }

        /// <summary>
        /// Determines whether every element in the <see cref="CopyOnWriteList{T}"/>
        /// matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// to check against the elements.
        /// </param>
        /// <returns>
        /// <c>true</c> if every element in the <see cref="CopyOnWriteList{T}"/>
        /// matches the conditions defined by the specified predicate; otherwise,
        /// <c>false</c>. If the list has no elements, the return value is <c>true</c>.
        /// </returns>
        public bool TrueForAll(Predicate<T> match)
        {
            VerifyMatchArgument(match);
            var items = _items; // one volatile read only
            for (int i = 0; i < items.Length; i++)
            {
                if (!match(items[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to an 
        /// <see cref="Array"/>, starting at a particular <see cref="Array"/> 
        /// index.
        /// </summary>
        ///
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination 
        /// of the elements copied from <see cref="ICollection"/>. The 
        /// <see cref="Array"/> must have zero-based indexing. 
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins. 
        /// </param>
        /// <exception cref="ArgumentNullException">array is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is less than zero. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// array is multidimensional.-or- index is equal to or greater than 
        /// the length of array.
        /// -or- 
        /// The number of elements in the source <see cref="ICollection"/> 
        /// is greater than the available space from index to the end of the 
        /// destination array. 
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The type of the source <see cref="ICollection"/> cannot be cast 
        /// automatically to the type of the destination array. 
        /// </exception>
        /// <filterpriority>2</filterpriority>
        protected override void CopyTo(Array array, int index)
        {
            var items = _items; // this is important for threading.
            Array.Copy(items, 0, array, index, items.Length);
        }

        /// <summary>
        /// Does the actual work of copying to array. Subclass is recommended to 
        /// override this method instead of <see cref="AbstractCollection{T}.CopyTo(T[], int)"/> 
        /// method, which does all neccessary parameter checking and raises proper 
        /// exception before calling this method.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the 
        /// destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <param name="ensureCapacity">
        /// If is <c>true</c>, calls <see cref="AbstractCollection{T}.EnsureCapacity"/>
        /// </param>
        /// <returns>
        /// A new array of same runtime type as <paramref name="array"/> if 
        /// <paramref name="array"/> is too small to hold all elements and 
        /// <paramref name="ensureCapacity"/> is <c>false</c>. Otherwise
        /// the <paramref name="array"/> instance itself.
        /// </returns>
        protected override T[] DoCopyTo(T[] array, int arrayIndex, bool ensureCapacity)
        {
            var items = _items;
            if (ensureCapacity) array = EnsureCapacity(array, items.Length);
            try{Array.Copy(items, 0, array, arrayIndex, items.Length);}
            catch (InvalidCastException e) { throw Error.NewArrayTypeMismatchException(e); }
            return array;
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> 
        /// is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// <c>true</c>
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected override bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected override object SyncRoot
        {
            get { return this; }
        }

        private T[] MakeCopy(int size)
        {
            var items = new T[size];
            Array.Copy(_items, 0, items, 0, size);
            return items;
        }

        private static int BinarySearch(T[] items, int index, int count, T item, IComparer<T> comparer)
        {
            var size = items.Length;
            VerifyIndex(index, size, size);
            VerifyCountArgument(count, index, size);
            return Array.BinarySearch(items, index, count, item, comparer);
        }

        private static int FindIndex(T[] items, int index, int count, Predicate<T> match)
        {
            int end = index + count;
            for (int i = index; i < end; i++)
            {
                if (match(items[i])) return i;
            }
            return -1;
        }

        private static int FindLastIndex(T[] items, int index, int count, Predicate<T> match)
        {
            int end = index - count;
            for (int i = index; i > end; i--)
            {
                if (match(items[i])) return i;
            }
            return -1;
        }

        private static void Sort(ref T[] items, int index, int count, IComparer<T> comparer)
        {
            if (count == 0 ) return;
            int size = items.Length;
            if (count == -1) count = size - index;
            T[] newItems = new T[size];
            Array.Copy(items, 0, newItems, 0, size);
            Array.Sort(newItems, index, count, comparer);
            items = newItems;
        }

        private static bool AddRange(ref T[] items, IEnumerable<T> collection)
        {
            return InsertRange(ref items, items.Length, collection);
        }

        private static bool InsertRange(ref T[] items, int index, IEnumerable<T> collection)
        {
            EnumerableToArrayBuffer<T> buffer = new EnumerableToArrayBuffer<T>(collection);
            int count = buffer.Count;
            if (count == 0) return false;
            int size = items.Length;
            if (size == 0)
            {
                items = buffer.ToArray();
                return true;
            }
            var newItems = new T[size + count];
            buffer.CopyTo(newItems, index);
            if (index > 0)
                Array.Copy(items, 0, newItems, 0, index);
            if (index < size)
                Array.Copy(items, index, newItems, index + count, size - index);
            items = newItems;
            return true;
        }

        private static bool InsertOne(ref T[] items, int index, T item)
        {
            int size = items.Length;
            if (index < 0 || index > size)
                return false;
            var newItems = new T[size+1];
            if (index > 0)
                Array.Copy(items, 0, newItems, 0, index);
            newItems[index] = item;
            if (index < size)
                Array.Copy(items, index, newItems, index + 1, size - index);
            items = newItems;
            return true;
        }

        private static T[] RemoveRange(T[] items, int index, int count)
        {
            var size = items.Length - count;
            var newItems = new T[size];
            if (index > 0)
            {
                Array.Copy(items, 0, newItems, 0, index);
            }
            if (index < size)
            {
                Array.Copy(items, index+count, newItems, index, size-index);
            }
            return newItems;
        }

        private static void VerifyCountArgument(int count, int index, int size)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", count, "Value must be positive.");
            }
            if (count + index > size)
            {
                throw new ArgumentException(
                    string.Format("Parameter count({0}) + index({1}) must not be greater then list size({2}).",
                    count, index, size));
            }
        }

        private static void VerifyCountArgument(int count, int index)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", count, "Value must be positive.");
            }
            if (count > index)
            {
                throw new ArgumentException(
                    string.Format("Parameter count({0}) must not be greater then index({1}).",
                    count, index));
            }
        }

        private static void VerifyIndex(int index, int limit, int size)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(
                "index", index, "cannot be less then zero.");
            if (index >= limit) throw new ArgumentOutOfRangeException(
                "index", index, string.Format("Must be less then {0} in a list of size {1}.", limit, size));
        }

        private static void VerifyMatchArgument(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");
        }
    }
}