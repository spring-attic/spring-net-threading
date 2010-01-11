using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Spring.Collections.Generic;
using Spring.Threading.Future;
using Spring.Utility;

namespace Spring.Threading.Collections.Generic
{
	/// <summary>
	/// An unbounded <see cref="IBlockingQueue{T}"/> of
	/// <see cref="IDelayed"/> elements, in which an element can only be taken
	/// when its delay has expired. 
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	[Serializable]
    public class DelayQueue<T> : AbstractBlockingQueue<T>, IDeserializationCallback //BACKPORT_2_2
        where T : IDelayed
    {
        [NonSerialized]
        private object _lock = new object();

        private readonly PriorityQueue<T> _queue;

        /// <summary>
        /// Creates a new, empty <see cref="DelayQueue{T}"/>
        /// </summary>
        public DelayQueue()
        {
            _queue = new PriorityQueue<T>();
        }

        /// <summary>
        ///Creates a <see cref="DelayQueue{T}"/> initially containing the elements of the
        ///given collection of <see cref="IDelayed"/> instances.
        /// </summary>
        /// <param name="source">collection of elements to populate queue with.</param>
        /// <exception cref="ArgumentNullException">If the collection is null.</exception>
        /// <exception cref="NullReferenceException">if any of the elements of the collection are null</exception>
        public DelayQueue(IEnumerable<T> source)
        {
            _queue = new PriorityQueue<T>(source);
        }

        /// <summary>
        /// Inserts the specified element into this delay queue.
        /// </summary>
        /// <param name="element">element to add</param>
        /// <returns>Always <see lang="true"/></returns>
        /// <exception cref="NullReferenceException">
        /// If the specified element is <see lang="null"/>.
        /// </exception>
        public override bool Offer(T element)
        {
            lock (_lock)
            {
                T first;
                bool emptyBeforeOffer = !_queue.Peek(out first);
                _queue.Offer(element);
                if (emptyBeforeOffer || element.CompareTo(first) < 0)
                {
                    Monitor.PulseAll(_lock);
                }
                return true;
            }
        }

        /// <summary>
        ///	Inserts the specified element into this delay queue. As the queue is
        ///	unbounded this method will never block.
        /// </summary>
        /// <param name="element">element to add</param>
        /// <exception cref="NullReferenceException">if the element is <see lang="null"/></exception>
        public override void Put(T element)
        {
            Offer(element);
        }

        /// <summary>
        /// Returns the capacity of this queue. Since this is a unbounded queue, <see cref="int.MaxValue"/> is returned.
        /// </summary>
        public override int Capacity
        {
            get { return Int32.MaxValue; }
        }

        #region IBlockingQueue Members

        /// <summary> 
        /// Inserts the specified element into this queue, waiting up to the
        /// specified wait time if necessary for space to become available.
        /// </summary>
        /// <param name="objectToAdd">the element to add</param>
        /// <param name="duration">how long to wait before giving up</param>
        /// <returns> <see lang="true"/> if successful, or <see lang="false"/> if
        /// the specified waiting time elapses before space is available
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the element cannot be added at this time due to capacity restrictions.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// If the class of the supplied <paramref name="objectToAdd"/> prevents it
        /// from being added to this queue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified element is <see lang="null"/> and this queue does not
        /// permit <see lang="null"/> elements.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If some property of the supplied <paramref name="objectToAdd"/> prevents
        /// it from being added to this queue.
        /// </exception>
        public override bool Offer(T objectToAdd, TimeSpan duration)
        {
            return Offer(objectToAdd);
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element becomes available and/or expired.
        /// </summary>
        /// <returns> the head of this queue</returns>
        public override T Take()
        {
            lock (_lock)
            {
                for (; ; )
                {
                    T first;
                    if (!_queue.Peek(out first))
                    {
                        Monitor.Wait(_lock);
                    }
                    else
                    {
                        TimeSpan delay = first.GetRemainingDelay();
                        if (delay.Ticks > 0)
                        {
                            Monitor.Wait(_lock, delay);
                        }
                        else
                        {
                            T x;
                            bool hasOne = _queue.Poll(out x);
                            Debug.Assert(hasOne);
                            if (_queue.Count != 0)
                            {
                                Monitor.PulseAll(_lock);
                            }
                            return x;
                        }
                    }
                }
            }
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue
        /// or returns <see lang="null"/> if this queue is empty or if the head has not expired.
        /// </summary>
        /// <returns> 
        /// The head of this queue, or <see lang="null"/> if this queue is empty or if the head has not expired.
        /// </returns>
        public override bool Poll(out T element)
        {
            lock (_lock)
            {
                T first;
                if (!_queue.Peek(out first) || first.GetRemainingDelay().Ticks > 0)
                {
                    element = default(T);
                    return false;
                }
                T x;
                bool hasOne = _queue.Poll(out x);
                Debug.Assert(hasOne);
                if (_queue.Count != 0)
                {
                    Monitor.PulseAll(_lock);
                }
                element = x;
                return true;
            }
        }

	    /// <summary>
	    /// Retrieves, but does not remove, the head of this queue into out
	    /// parameter <paramref name="element"/>.
	    /// </summary>
	    /// <param name="element">
	    /// The head of this queue. <c>default(T)</c> if queue is empty.
	    /// </param>
	    /// <returns>
	    /// <c>false</c> is the queue is empty. Otherwise <c>true</c>.
	    /// </returns>
	    public override bool Peek(out T element)
        {
            lock (_lock)
            {
                T first;
                if (!_queue.Peek(out first) || first.GetRemainingDelay().Ticks > 0)
                {
                    element = default(T);
                    return false;
                }
                T x;
                bool hasOne = _queue.Peek(out x);
                Debug.Assert(hasOne);
                if (_queue.Count != 0)
                {
                    Monitor.PulseAll(_lock);
                }
                element = x;
                return true;
            }
        }
        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element with an expired delay is available on this queue,
        /// or the specified wait time expires.
        /// </summary>
        /// <param name="duration">how long to wait before giving up</param>a
        /// <param name="element"></param>
        /// <returns> 
        /// the head of this queue, or <see lang="null"/> if the
        /// specified waiting time elapses before an element is available
        /// </returns>
        public override bool Poll(TimeSpan duration, out T element)
        {
            lock (_lock)
            {
                DateTime deadline = WaitTime.Deadline(duration);
                for (; ; )
                {
                    T first;
                    if (!_queue.Peek(out first))
                    {
                        if (duration.Ticks <= 0)
                        {
                            element = default(T);
                            return false;
                        }
                        Monitor.Wait(_lock, WaitTime.Cap(duration));
                        duration = deadline.Subtract(DateTime.UtcNow);
                    }
                    else
                    {
                        TimeSpan delay = first.GetRemainingDelay();
                        if (delay.Ticks > 0)
                        {
                            if (duration.Ticks <= 0)
                            {
                                element = default(T);
                                return false;
                            }
                            if (delay > duration)
                            {
                                delay = duration;
                            }
                            Monitor.Wait(_lock, WaitTime.Cap(delay));
                            duration = deadline.Subtract(DateTime.UtcNow);
                        }
                        else
                        {
                            T x;
                            bool hasOne = _queue.Poll(out x);
                            Debug.Assert(hasOne);
                            if (_queue.Count != 0)
                            {
                                Monitor.PulseAll(_lock);
                            }
                            element = x;
                            return true;
                        }
                    }
                }
            }
        }

        /// <summary> 
        /// Returns the number of additional elements that this queue can ideally
        /// (in the absence of memory or resource constraints) accept without
        /// blocking. <see cref="DelayQueue{T}"/> is unbounded so this always
        /// return <see cref="int.MaxValue"/>.
        /// </summary>
        /// <returns><see cref="int.MaxValue"/></returns>
        public override int RemainingCapacity
        {
            get { return Int32.MaxValue; }
        }

	    /// <summary> 
	    /// Does the real work for all drain methods. Caller must
	    /// guarantee the <paramref name="action"/> is not <c>null</c> and
	    /// <paramref name="maxElements"/> is greater then zero (0).
	    /// </summary>
	    /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
	    /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int)"/>
	    /// <seealso cref="IQueue{T}.Drain(System.Action{T}, Predicate{T})"/>
	    /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int, Predicate{T})"/>
	    internal protected override int DoDrain(Action<T> action, int maxElements, Predicate<T> criteria)
        {
            lock (_lock)
            {
                int n = _queue.Drain(action, maxElements, criteria, (e => e.GetRemainingDelay().Ticks > 0) );
                if (n > 0)
                {
                    Monitor.PulseAll(_lock);
                }
                return n;
            }
        }

        #endregion

        #region ICollection Members


        /// <summary>
        /// Returns the current number of elements in this queue.
        /// </summary>
        public override int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
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
	        return new ToArrayEnumerator<T>(_queue);
	    }

	    /// <summary>
        /// When implemented by a class, copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins. </param>
        protected override void CopyTo(Array array, int index)
        {
            lock (_lock)
            {
                ((ICollection)_queue).CopyTo(array, index);
            }
        }

	    /// <summary>
	    /// Does the actual work of copying to array.
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
            lock (_lock)
            {
                if (array == null || ensureCapacity) array = EnsureCapacity(array, Count);
                _queue.CopyTo(array, arrayIndex);
                return array;
            }
        }

        #endregion

        /// <summary> 
        /// Removes all of the elements from this queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The queue will be empty after this call returns.
        /// </para>
        /// </remarks>
        public override void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }

        /// <summary>
        /// Removes a single instance of the specified element from this
        /// queue, if it is present, whether or not it has expired.
        /// </summary>
        /// <param name="element">element to remove</param>
        /// <returns><see lang="true"/> if element was remove, <see lang="false"/> if not.</returns>
        public override bool Remove(T element) {
            lock (_lock)
            {
                return _queue.Remove(element);
            }
        }

        #region IDeserializationCallback Members

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            _lock = new object();
        }

        #endregion
    }
}