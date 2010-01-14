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
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Spring.Collections.Generic;
using Spring.Threading.Locks;
#endregion

namespace Spring.Threading.Collections.Generic {

    /// <summary>
    /// An unbounded {@linkplain BlockingQueue blocking queue} that uses
    /// the same ordering rules as class {@link PriorityQueue} and supplies
    /// blocking retrieval operations.  While this queue is logically
    /// unbounded, attempted additions may fail due to resource exhaustion
    /// (causing <tt>OutOfMemoryError</tt>). This class does not permit
    /// <tt>null</tt> elements.  A priority queue relying on {@linkplain
    /// Comparable natural ordering} also does not permit insertion of
    /// non-comparable objects (doing so results in
    /// <tt>ClassCastException</tt>).
    /// 
    /// <p>This class and its iterator implement all of the
    /// <i>optional</i> methods of the {@link Collection} and {@link
    /// Iterator} interfaces.  The Iterator provided in method {@link
    /// #iterator()} is <i>not</i> guaranteed to traverse the elements of
    /// the PriorityBlockingQueue in any particular order. If you need
    /// ordered traversal, consider using
    /// <tt>Arrays.sort(pq.toArray())</tt>.  Also, method <tt>drainTo</tt>
    /// can be used to <i>remove</i> some or all elements in priority
    /// order and place them in another collection.</p>
    /// 
    /// <p>Operations on this class make no guarantees about the ordering
    /// of elements with equal priority. If you need to enforce an
    /// ordering, you can define custom classes or comparators that use a
    /// secondary key to break ties in primary priority values.  For
    /// example, here is a class that applies first-in-first-out
    /// tie-breaking to comparable elements. To use it, you would insert a
    /// <tt>new FIFOEntry(anEntry)</tt> instead of a plain entry object.</p>
    /// 
    /// <pre>
    /// class FIFOEntry implements Comparable {
    ///   final static AtomicLong seq = new AtomicLong();
    ///   final long seqNum;
    ///   final Object entry;
    ///   public FIFOEntry(Object entry) {
    ///     seqNum = seq.getAndIncrement();
    ///     this.entry = entry;
    ///   }
    ///   public Object getEntry() { return entry; }
    ///   public int compareTo(FIFOEntr other) {
    ///     int res = entry.compareTo(other.entry);
    ///     if (res == 0 &amp;&amp; other.entry != this.entry)
    ///       res = (seqNum &lt; other.seqNum ? -1 : 1);
    ///     return res;
    ///   }
    /// }</pre>
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Andreas Döhring (.NET)</author>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public class PriorityBlockingQueue<T> : BlockingQueueWrapper<T> // BACKPORT_2_2
    {
        /// <summary>
        /// Creates a <tt>PriorityBlockingQueue</tt> with the default
        /// initial capacity (11) that orders its elements according to
        /// their {@linkplain Comparable natural ordering}.
        /// </summary>
        public PriorityBlockingQueue() : base(new PriorityQueue<T>()) { }

        /// <summary>
        /// Creates a <tt>PriorityBlockingQueue</tt> with the specified
        /// initial capacity that orders its elements according to their
        /// {@linkplain Comparable natural ordering}.
        /// </summary>
        /// <param name="initialCapacity">the initial capacity for this priority queue</param>
        /// <exception cref="ArgumentException">if <tt>initialCapacity</tt> is less than 1</exception>
        public PriorityBlockingQueue(int initialCapacity)
            : base(new PriorityQueue<T>(initialCapacity)) { }

        /// <summary>
        /// Creates a <tt>PriorityBlockingQueue</tt> with the specified initial
        /// capacity that orders its elements according to the specified
        /// comparator.
        /// </summary>
        /// <param name="initialCapacity">the initial capacity for this priority queue</param>
        /// <param name="comparator">comparator the comparator that will be used to order this priority queue.  
        /// If {@code null}, the {@linkplain Comparable natural ordering} of the elements will be used.</param>
        /// <exception cref="ArgumentException">if <tt>initialCapacity</tt> is less than 1</exception>
        public PriorityBlockingQueue(int initialCapacity, IComparer<T> comparator)
            : base(new PriorityQueue<T>(initialCapacity, comparator)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <param name="comparison"></param>
        public PriorityBlockingQueue(int initialCapacity, Comparison<T> comparison)
            : base(new PriorityQueue<T>(initialCapacity, comparison)) {}

        /// <summary>
        /// Creates the inner <see cref="PriorityQueue{T}"/> from the specified collection
        /// </summary>
        /// <param name="collection">the collection whose elements are to be placed into this priority queue</param>
        public PriorityBlockingQueue(IEnumerable<T> collection)
            : base(new PriorityQueue<T>(collection)) { }

        /// <summary>
        /// Returns the comparator used to order the elements in this queue,
        /// or <tt>null</tt> if this queue uses the {@linkplain Comparable
        /// natural ordering} of its elements.
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
