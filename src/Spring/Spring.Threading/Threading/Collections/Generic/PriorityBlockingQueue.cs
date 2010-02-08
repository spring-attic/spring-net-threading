/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * http://creativecommons.org/licenses/publicdomain
 */

#region License

/*
 * Copyright (C) 2002-2008 the original author or authors.
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

#region Imports
using System;
using System.Collections.Generic;
using Spring.Collections.Generic;

#endregion

namespace Spring.Threading.Collections.Generic {

    /// <summary>
    /// An unbounded <see cref="IBlockingQueue{T}">blocking queue</see> that
    /// uses the same ordering rules as class <see cref="PriorityQueue{T}"/>
    /// and supplies blocking retrieval operations.  While this queue is
    /// logically unbounded, attempted additions may fail due to resource
    /// exhaustion (causing <see cref="OutOfMemoryException"/>). This class
    /// does not permit <c>null</c> elements.  A priority queue relying on
    /// <see cref="IComparable{T}">natural ordering</see> also does not
    /// permit insertion of non-comparable objects (doing so results in
    /// <see cref="InvalidCastException"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implement all of the <i>optional</i> methods of the
    /// <see cref="ICollection{T}"/> interfaces.  The enumerator provided in
    /// method <see cref="GetEnumerator"/> is <i>not</i> guaranteed to traverse
    /// the elements of the <see cref="PriorityBlockingQueue{T}"/> in any
    /// particular order. If you need ordered traversal, consider using
    /// <c>Array.Sort(pq.ToArray())</c>.  Also, method
    /// <see cref="AbstractBlockingQueue{T}.DrainTo(ICollection{T},int)"/>
    /// can be used to <i>remove</i> some or all elements in priority
    /// order and place them in another collection.
    /// </para>
    /// <para>
    /// Operations on this class make no guarantees about the ordering
    /// of elements with equal priority. If you need to enforce an
    /// ordering, you can define custom classes or comparers that use a
    /// secondary key to break ties in primary priority values.
    /// </para>
    /// <example>
    /// For example, here is a class that applies first-in-first-out
    /// tie-breaking to comparable elements. To use it, you would insert a
    /// <c>new FIFOEntry&lt;Entry>(anEntry)</c> instead of a plain entry object.
    ///  
    /// <code language="c#">
    /// public class FIFOEntry&lt;T> : IComparable&lt;FIFOEntry&lt;T>> 
    ///     where T : class, IComparable&lt;T>
    /// {
    ///   readonly static AtomicLong seq = new AtomicLong();
    ///   readonly long seqNum;
    ///   readonly T entry;
    ///   public FIFOEntry(T entry) {
    ///     seqNum = seq.ReturnValueAndDecrement();
    ///     this.entry = entry;
    ///   }
    ///   public object Entry { get { return entry; } }
    ///   public int CompareTo(FIFOEntry&lt;T> other) {
    ///     int res = entry.CompareTo(other.entry);
    ///     if (res == 0 &amp;&amp; !ReferenceEquals(other.entry, entry))
    ///       res = (seqNum &lt; other.seqNum ? -1 : 1);
    ///     return res;
    ///   }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Andreas Döhring (.NET)</author>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public class PriorityBlockingQueue<T> : BlockingQueueWrapper<T> // BACKPORT_2_2
    {
        /// <summary>
        /// Creates a <see cref="PriorityBlockingQueue{T}"/> with the default
        /// initial capacity (11) that orders its elements according to
        /// their <see cref="IComparable{T}">natural ordering</see>.
        /// </summary>
        public PriorityBlockingQueue() : base(new PriorityQueue<T>()) { }

        /// <summary>
        /// Creates a <see cref="PriorityBlockingQueue{T}"/> with the specified
        /// initial capacity that orders its elements according to their
        /// <see cref="IComparable{T}">natural ordering</see>.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityBlockingQueue(int initialCapacity)
            : base(new PriorityQueue<T>(initialCapacity)) { }

        /// <summary>
        /// Creates a <see cref="PriorityBlockingQueue{T}"/> with the specified
        /// initial capacity that orders its elements according to the specified
        /// comparer.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <param name="comparer">
        /// The comparer that will be used to order this priority queue.  
        /// If <c>null</c>, the <see cref="IComparable{T}">natural ordering</see>
        /// of the elements will be used.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityBlockingQueue(int initialCapacity, IComparer<T> comparer)
            : base(new PriorityQueue<T>(initialCapacity, comparer)) { }

        /// <summary>
        /// Creates a <see cref="PriorityBlockingQueue{T}"/> with the specified
        /// initial capacity that orders its elements according to the specified
        /// comparison.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <param name="comparison">
        /// The comparison that will be used to order this priority queue.  
        /// If <c>null</c>, the <see cref="IComparable{T}">natural ordering</see>
        /// of the elements will be used.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityBlockingQueue(int initialCapacity, Comparison<T> comparison)
            : base(new PriorityQueue<T>(initialCapacity, comparison)) {}

        /// <summary>
        /// Creates a <see cref="PriorityBlockingQueue{T}"/> initially populated
        /// with <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are to be placed into this priority queue.
        /// </param>
        public PriorityBlockingQueue(IEnumerable<T> collection)
            : base(new PriorityQueue<T>(collection)) { }

        /// <summary>
        /// Returns the comparer used to order the elements in this queue,
        /// or <c>null</c> if this queue uses the <see cref="IComparable{T}">
        /// natural ordering</see> of its elements.
        /// </summary>
        public IComparer<T> Comparer
        {
            get { return ((PriorityQueue<T>)_wrapped).Comparer; }
        }

        /// <summary> 
        /// Returns an enumerator over all the elements (both expired and
        /// unexpired) in this queue. The enumerator does not return the
        /// elements in any particular order.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IEnumerator{T}"/> is a "weakly consistent" 
        /// enumerator that will not throw <see cref="InvalidOperationException"/> 
        /// when the queue is concurrently modified, and guarantees to traverse
        /// elements as they existed upon construction of the enumerator, and
        /// may (but is not guaranteed to) reflect any modifications subsequent
        /// to construction.
        /// </remarks>
        /// <returns>
        /// An enumerator over the elements in this queue.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return new ToArrayEnumerator<T>((PriorityQueue<T>)_wrapped);
        }
    }
}
