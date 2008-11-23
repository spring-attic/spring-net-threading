#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using System.Collections.Generic;

namespace Spring.Threading.Collections {
    /// <summary>
    /// A collection designed for holding elements prior to processing.
    /// Besides basic <see cref="System.Collections.Generic.ICollection{T}"/> operations,
    /// queues provide additional insertion, extraction, and inspection
    /// operations.  Each of these methods exists in two forms: one throws
    /// an exception if the operation fails, the other returns a special
    /// value (either <code>null</code> or <code>false</code>, depending on the
    /// operation).  The latter form of the insert operation is designed
    /// specifically for use with capacity-restricted <code>IQueue</code>
    /// implementations; in most implementations, insert operations cannot
    /// fail.
    ///
    /// <p>Queues typically, but do not necessarily, order elements in a
    /// FIFO (first-in-first-out) manner.  Among the exceptions are
    /// priority queues, which order elements according to a supplied
    /// comparator, or the elements' natural ordering, and LIFO queues (or
    /// stacks) which order the elements LIFO (last-in-first-out).
    /// Whatever the ordering used, the <em>head</em> of the queue is that
    /// element which would be removed by a call to <see cref="Remove"/> or
    /// <see cref="Poll"/>.  In a FIFO queue, all new elements are inserted at
    /// the <em> tail</em> of the queue. Other kinds of queues may use
    /// different placement rules.  Every <code>IQueue</code> implementation
    /// must specify its ordering properties.</p>
    ///
    /// <p>The  <see cref="Offer"/> method inserts an element if possible,
    /// otherwise returning <code>false</code>.  This differs from the <see cref="ICollection{T}.Add"/>
    /// method, which can fail to add an element only by throwing an exception.
    /// The <code>offer</code> method is designed for use when failure is a normal,
    /// rather than exceptional occurrence, for example, in fixed-capacity
    /// (or &quot;bounded&quot;) queues.</p>
    ///
    /// <p>The <see cref="Remove"/> and <see cref="Poll"/> methods remove and
    /// return the head of the queue.
    /// Exactly which element is removed from the queue is a
    /// function of the queue's ordering policy, which differs from
    /// implementation to implementation. The <code>remove()</code> and
    /// <code>poll()</code> methods differ only in their behavior when the
    /// queue is empty: the <code>remove()</code> method throws an exception,   
    /// while the <code>poll()</code> method returns <code>null</code>.</p>
    ///
    /// <p>The <see cref="Element"/> and <see cref="Peek"/> methods return, but do
    /// not remove, the head of the queue.</p>
    ///
    /// <p>The <code>Queue</code> interface does not define the <i>blocking queue
    /// methods</i>, which are common in concurrent programming.  These methods,
    /// which wait for elements to appear or for space to become available, are
    /// defined in the {@link edu.emory.mathcs.backport.java.util.concurrent.BlockingQueue} interface, which
    /// extends this interface.</p>
    ///
    /// <p><code>Queue</code> implementations generally do not allow insertion
    /// of <code>null</code> elements, although some implementations, such as
    /// {@link LinkedList}, do not prohibit insertion of <code>null</code>.
    /// Even in the implementations that permit it, <code>null</code> should
    /// not be inserted into a <code>Queue</code>, as <code>null</code> is also
    /// used as a special return value by the <code>poll</code> method to
    /// indicate that the queue contains no elements.</p>
    ///
    /// <p><code>Queue</code> implementations generally do not define
    /// element-based versions of methods <code>equals</code> and
    /// <code>hashCode</code> but instead inherit the identity based versions
    /// from class <code>Object</code>, because element-based equality is not
    /// always well-defined for queues with the same elements but different
    /// ordering properties.</p>
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <see cref="System.Collections.Generic.ICollection{T}"/>
    /// <author>Doug Lea</author>
    /// <author>Andreas Doehring (.NET)</author>
    public interface IQueue<T> : ICollection<T>, IEnumerable<T> {
        /// <summary>
        /// Inserts the specified element into this queue if it is possible to do
        /// so immediately without violating capacity restrictions.
        /// When using a capacity-restricted queue, this method is generally
        /// preferable to <see cref="ICollection{T}.Add"/>, which can fail to insert an element only
        /// by throwing an exception.
        /// </summary>
        /// <param name="element">the element to add</param>
        /// <returns><code>true</code>if the element was added to this queue, else <code>false</code></returns>
        /// <exception cref="ArgumentNullException"/>
        bool Offer(T element);

        /// <summary>
        /// Retrieves and removes the head of this queue.  This method differs
        /// from {@link #poll poll} only in that it throws an exception if this
        /// queue is empty.
        /// </summary>
        /// <returns>the head of this queue</returns>
        /// <exception cref="InvalidOperationException">if the queue is empty</exception>
        T Remove();

        /// <summary>
        /// Retrieves and removes the head of this queue,
        /// or returns <code>null</code> if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue, or <code>null</code> if this queue is empty</returns>
        T Poll();

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue.  This method
        /// differs from <see cref="Peek"/> only in that it throws an exception
        /// if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue</returns>
        /// <exception cref="InvalidOperationException">if the queue is empty</exception>
        T Element();

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue, 
        /// or returns <code>null</code> if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue, or <code>null</code> if this queue is empty</returns>
        T Peek();
    }
}