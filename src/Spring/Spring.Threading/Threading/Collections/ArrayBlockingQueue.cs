using System;
using System.Collections;
using System.Threading;
using Spring.Collections;
using Spring.Threading.Collections;
using Spring.Threading.Locks;
using Spring.Util;

namespace Spring.Threading.Collections
{
	/// <summary> A bounded <see cref="Spring.Threading.Collections.IBlockingQueue"/> backed by an
	/// array.  This queue orders elements FIFO (first-in-first-out).  The
	/// <i>head</i> of the queue is that element that has been on the
	/// queue the longest time.  The <i>tail</i> of the queue is that
	/// element that has been on the queue the shortest time. New elements
	/// are inserted at the tail of the queue, and the queue retrieval
	/// operations obtain elements at the head of the queue.
	/// 
	/// <p/>
	/// This is a classic &quot;bounded buffer&quot;, in which a
	/// fixed-sized array holds elements inserted by producers and
	/// extracted by consumers.  Once created, the capacity cannot be
	/// increased.  Attempts to <see cref="Spring.Threading.Collections.IBlockingQueue.Put(object)"/>
	/// an element into a full queue
	/// will result in the operation blocking; attempts to <see cref="Spring.Threading.Collections.IBlockingQueue.Take()"/>
	/// an element from an empty queue will similarly block.
	/// 
	/// <p/> 
	/// This class supports an optional fairness policy for ordering
	/// waiting producer and consumer threads.  By default, this ordering
	/// is not guaranteed. However, a queue constructed with fairness set
	/// to <see lang="true"/> grants threads access in FIFO order. Fairness
	/// generally decreases throughput but reduces variability and avoids
	/// starvation.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class ArrayBlockingQueue : AbstractQueue, IBlockingQueue
	{
		/// <summary>
		/// The intial capacity of this queue.
		/// </summary>
		private int _capacity;
		/// <summary>Number of items in the queue </summary>
		private int _count;

		/// <summary>The queued items  </summary>
		private object[] _items;

		/// <summary>items index for next take, poll or remove </summary>
		[NonSerialized] private int _takeIndex;

		/// <summary>items index for next put, offer, or add. </summary>
		[NonSerialized] private int _putIndex;

		/// <summary>Main lock guarding all access </summary>
		private ReentrantLock _lock;

		/// <summary>Condition for waiting takes </summary>
		private ICondition _notEmptyCondition;

		/// <summary>Condition for waiting puts </summary>
		private ICondition _notFullCondition;
		/// <summary>
		/// Provides synchronized access to the collection.
		/// </summary>
		[NonSerialized]
		private object _syncRoot;

		#region Private Methods

		/// <summary> 
		/// Utility for remove and iterator.remove: Delete item at position <paramref name="index"/>.
		/// Call only when holding lock.
		/// </summary>
		internal virtual void removeAt(int index)
		{
			object[] items = _items;
			if (index == _takeIndex)
			{
				items[_takeIndex] = null;
				_takeIndex = increment(_takeIndex);
			}
			else
			{
				for (;; )
				{
					int nextIndex = increment(index);
					if (nextIndex != _putIndex)
					{
						items[index] = items[nextIndex];
						index = nextIndex;
					}
					else
					{
						items[index] = null;
						_putIndex = index;
						break;
					}
				}
			}
			--_count;
			_notFullCondition.Signal();
		}

		/// <summary> Circularly increment i.</summary>
		private int increment(int index)
		{
			return (++index == _items.Length) ? 0 : index;
		}

		/// <summary> 
		/// Inserts element at current put position, advances, and signals.
		/// Call only when holding lock.
		/// </summary>
		private void insert(object x)
		{
			_items[_putIndex] = x;
			_putIndex = increment(_putIndex);
			++_count;
			_notEmptyCondition.Signal();
		}

		/// <summary> 
		/// Extracts element at current take position, advances, and signals.
		/// Call only when holding lock.
		/// </summary>
		private object extract()
		{
			Object[] items = _items;
			Object x = items[_takeIndex];
			items[_takeIndex] = null;
			_takeIndex = increment(_takeIndex);
			--_count;
			_notFullCondition.Signal();
			return x;
		}

		#endregion

		#region Constructors

		/// <summary> 
		/// Creates an <see cref="Spring.Threading.Collections.ArrayBlockingQueue"/> with the given (fixed)
		/// <paramref name="capacity"/> and the specified <paramref name="isFair"/> fairness access policy.
		/// </summary>
		/// <param name="capacity">the capacity of this queue</param>
		/// <param name="isFair">if <see lang="true"/> then queue accesses for threads blocked
		/// on insertion or removal, are processed in FIFO order; if <see lang="false"/> the access order is unspecified.
		/// </param>
		/// <exception cref="System.ArgumentOutOfRangeException">if <paramref name="capacity"/> is less than 1.</exception>
		public ArrayBlockingQueue(int capacity, bool isFair)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException("capacity");
			_capacity = capacity;
			_items = new object[capacity];
			_lock = new ReentrantLock(isFair);
			_notEmptyCondition = _lock.NewCondition();
			_notFullCondition = _lock.NewCondition();
		}

		/// <summary> 
		/// Creates an <see cref="Spring.Threading.Collections.ArrayBlockingQueue"/> with the given (fixed)
		/// <paramref name="capacity"/> and the specified <paramref name="isFair"/> fairness access policy
		/// and initially containing the  elements of the given collection,
		/// added in traversal order of the collection's iterator.
		/// </summary>
		/// <param name="capacity">the capacity of this queue</param>
		/// <param name="isFair">if <see lang="true"/> then queue accesses for threads blocked
		/// on insertion or removal, are processed in FIFO order; if <see lang="false"/> the access order is unspecified.
		/// </param>
		/// <param name="collection">the collection of elements to initially contain</param>
		/// <exception cref="System.ArgumentOutOfRangeException">if <paramref name="capacity"/> 
		/// is less than 1 or is less than the size of <pararef name="collection"/>.</exception>
		/// <exception cref="System.ArgumentNullException">If <paramref name="collection"/> or any of its elements
		/// are <see lang="null"/></exception> 
		public ArrayBlockingQueue(int capacity, bool isFair, ICollection collection) : this(capacity, isFair)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (capacity < collection.Count)
				throw new ArgumentOutOfRangeException("Collection size greater than queue capacity");
			foreach (object currentObject in collection)
			{
				Add(currentObject);
			}
		}

		/// <summary> 
		/// Creates an <see cref="Spring.Threading.Collections.ArrayBlockingQueue"/> with the given (fixed)
		/// <paramref name="capacity"/> and default fairness access policy.
		/// </summary>
		/// <param name="capacity">the capacity of this queue</param>
		/// <exception cref="System.ArgumentOutOfRangeException">if <paramref name="capacity"/> is less than 1.</exception>
		public ArrayBlockingQueue(int capacity) : this(capacity, false)
		{
		}

		#endregion
		/// <summary> 
		/// Atomically removes all of the elements from this queue.
		/// The queue will be empty after this call returns.
		/// </summary>
		public override void  Clear()
		{
			System.Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				int i = _takeIndex;
				int k = _count;
				while (k-- > 0)
				{
					items[i] = null;
					i = increment(i);
				}
				_count = 0;
				_putIndex = 0;
				_takeIndex = 0;
				_notFullCondition.SignalAll();
			}
			finally
			{
				currentLock.Unlock();
			}
		}
		/// <summary>
		/// Returns <see cref="System.String"/> representation of this <see cref="Spring.Threading.Collections.ArrayBlockingQueue"/>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				return StringUtils.CollectionToCommaDelimitedString(this);
			}
			finally
			{
				currentLock.Unlock();
			}
		}
		/// <summary> 
		/// Returns <see lang="true"/> if this queue contains the specified element.
		/// </summary>
		/// <remarks>
		/// More formally, returns <see lang="true"/> if and only if this queue contains
		/// at least one element <i>element</i> such that <i>objectToSearchFor.equals(element)</i>.
		/// </remarks>
		/// <param name="objectToSearchFor">object to be checked for containment in this queue</param>
		/// <returns> <see lang="true"/> if this queue contains the specified element</returns>
		public bool Contains(object objectToSearchFor)
		{
			if (objectToSearchFor == null)
				return false;
			object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				int i = _takeIndex;
				int k = 0;
				while (k++ < _count)
				{
					if (objectToSearchFor.Equals(items[i]))
						return true;
					i = increment(i);
				}
				return false;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Removes a single instance of the specified element from this queue,
		/// if it is present.  More formally, removes an <i>element</i> such
		/// that <i>objectToRemove.Equals(element)</i>, if this queue contains one or more such
		/// elements.
		/// </summary>
		/// <param name="objectToRemove">element to be removed from this queue, if present
		/// </param>
		/// <returns> <see lang="true"/> if this queue contained the specified element or
		///  if this queue changed as a result of the call, <see lang="false"/> otherwise
		/// </returns>
		public bool Remove(object objectToRemove)
		{
			if (objectToRemove == null)
				return false;
			object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				int currentIndex = _takeIndex;
				int currentStep = 0;
				for (;; )
				{
					if (currentStep++ >= _count)
						return false;
					if (objectToRemove.Equals(items[currentIndex]))
					{
						removeAt(currentIndex);
						return true;
					}
					currentIndex = increment(currentIndex);
				}
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary>
		/// Copies the elements of the 
		/// <see cref="System.Collections.ICollection"/> to an <see cref="System.Array"/> , 
		/// starting at a particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="System.Array"/> that is the destination of 
		/// the elements copied from <see cref="System.Collections.ICollection"/>. 
		/// The <see cref="System.Array"/> must have zero-based indexing. </param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		/// <exception cref="System.InvalidCastException">if any elemens of this <see cref="System.Collections.ICollection"/>
		/// cannot be copied into <paramref name="array"/> because of a type mismatch.</exception>
		/// <exception cref="System.ArgumentException">if <paramref name="array"/> isn't larger enough 
		/// to hold all elements from this <see cref="System.Collections.ICollection"/></exception>
		public override void CopyTo(Array array, int index)
		{
			lock ( this )
			{
				if ( _count > array.Length ) throw new ArgumentException("Destination array too small.", "array");
				if ( index > array.Length - 1 ) throw new ArgumentException("Starting index outside bounds of target array.", "index");
				if ( index + _count > array.Length ) throw new IndexOutOfRangeException("Destination array not long enough to begin copying at index " + index + ".");

				for ( int queueElementCount = 0; queueElementCount < _count; queueElementCount++)
				{
					array.SetValue(_items[queueElementCount], index );
					index++;
				}
				Array.Sort(array);	
			}
		}
		/// <summary>
		/// Gets the capacity of the queue.
		/// </summary>
		public override int Capacity
		{
			get { return _capacity; }
		}

		/// <summary> 
		/// Returns the number of elements in this queue.
		/// </summary>
		/// <returns> the number of elements in this queue</returns>
		public override int Count
		{
			get
			{
				ReentrantLock lock_Renamed = _lock;
				lock_Renamed.Lock();
				try
				{
					return _count;
				}
				finally
				{
					lock_Renamed.Unlock();
				}
			}

		}

		/// <summary>
		/// When implemented by a class, gets an object 
		/// that can be used to synchronize access to the <see cref="System.Collections.ICollection"/>.
		/// </summary>
		public override object SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		/// <summary> 
		/// Inserts the specified element into this queue if it is possible to do
		/// so immediately without violating capacity restrictions.
		/// </summary>
		/// <remarks>
		/// <p/>
		/// When using a capacity-restricted queue, this method is generally
		/// preferable to <see cref="ArgumentException"/>,
		/// which can fail to insert an element only by throwing an exception.
		/// </remarks>
		/// <param name="objectToAdd">
		/// The element to add.
		/// </param>
		/// <returns>
		/// <see lang="true"/> if the element was added to this queue.
		/// </returns>
		/// <exception cref="object">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="objectToAdd"/> is
		/// <see lang="null"/> and this queue does not permit <see lang="null"/>
		/// elements.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		public override bool Offer(object objectToAdd)
		{
			if (objectToAdd == null)
				throw new ArgumentNullException("objectToAdd");
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				if (_count == _items.Length)
					return false;
				else
				{
					insert(objectToAdd);
					return true;
				}
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Retrieves and removes the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p/>
		/// This method differs from <see cref="Spring.Collections.IQueue.Poll()"/>
		/// only in that it throws an exception if this queue is empty.
		/// </remarks>
		/// <returns> 
		/// The head of this queue
		/// </returns>
		/// <exception cref="Spring.Collections.NoElementsException">if this queue is empty</exception>
		public override object Remove()
		{
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				if (_count == 0)
					throw new NoElementsException("Queue is empty.");
				object x = extract();
				return x;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Retrieves and removes the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public override object Poll()
		{
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				if (_count == 0)
					return null;
				object x = extract();
				return x;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Inserts the specified element into this queue, waiting if necessary
		/// for space to become available.
		/// </summary>
		/// <param name="objectToAdd">the element to add</param>
		/// <exception cref="ArgumentException">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If the class of the supplied <paramref name="objectToAdd"/> prevents it
		/// from being added to this queue.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// If the specified element is <see lang="null"/> and this queue does not
		/// permit <see lang="null"/> elements.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		public void Put(object objectToAdd)
		{
			if (objectToAdd == null)
				throw new ArgumentNullException("objectToAdd");
			Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.LockInterruptibly();
			try
			{
				try
				{
					while (_count == items.Length)
						_notFullCondition.Await();
				}
				catch (ThreadInterruptedException)
				{
					_notFullCondition.Signal();
					throw;
				}
				insert(objectToAdd);
			}
			finally
			{
				currentLock.Unlock();
			}
		}

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
		/// <exception cref="ArgumentException">
		/// If the specified element is <see lang="null"/> and this queue does not
		/// permit <see lang="null"/> elements.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		public bool Offer(object objectToAdd, TimeSpan duration)
		{
			if (objectToAdd == null)
				throw new ArgumentNullException("objectToAdd");
			ReentrantLock currentLock = _lock;
			currentLock.LockInterruptibly();
			try
			{
				TimeSpan durationToWait = duration;
				DateTime deadline = DateTime.Now.Add(durationToWait);
				for (;; )
				{
					if (_count != _items.Length)
					{
						insert(objectToAdd);
						return true;
					}
					if (durationToWait.Ticks <= 0)
						return false;
					try
					{
						_notFullCondition.Await(durationToWait);
						durationToWait = deadline.Subtract(DateTime.Now);
					}
					catch (ThreadInterruptedException)
					{
						_notFullCondition.Signal();
						throw;
					}
				}
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Retrieves and removes the head of this queue, waiting if necessary
		/// until an element becomes available.
		/// </summary>
		/// <returns> the head of this queue</returns>
		public object Take()
		{
			ReentrantLock currentLock = _lock;
			currentLock.LockInterruptibly();
			try
			{
				try
				{
					while (_count == 0)
						_notEmptyCondition.Await();
				}
				catch (ThreadInterruptedException)
				{
					_notEmptyCondition.Signal();
					throw;
				}
				Object x = extract();
				return x;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Retrieves and removes the head of this queue, waiting up to the
		/// specified wait time if necessary for an element to become available.
		/// </summary>
		/// <param name="duration">how long to wait before giving up</param>
		/// <returns> 
		/// the head of this queue, or <see lang="null"/> if the
		/// specified waiting time elapses before an element is available
		/// </returns>
		public object Poll(TimeSpan duration)
		{
			ReentrantLock currentLock = _lock;
			currentLock.LockInterruptibly();
			try
			{
				TimeSpan durationToWait = duration;
				DateTime deadline = DateTime.Now.Add(durationToWait);
				for (;; )
				{
					if (_count != 0)
					{
						object x = extract();
						return x;
					}
					if (durationToWait.Ticks <= 0)
						return null;
					try
					{
						_notEmptyCondition.Await(durationToWait);
						durationToWait = deadline.Subtract(DateTime.Now);
					}
					catch (ThreadInterruptedException)
					{
						_notEmptyCondition.Signal();
						throw;
					}
				}
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Returns the number of additional elements that this queue can ideally
		/// (in the absence of memory or resource constraints) accept without
		/// blocking, or <see cref="System.Int32.MaxValue"/> if there is no intrinsic
		/// limit.
		/// 
		/// <p/>
		/// Note that you <b>cannot</b> always tell if an attempt to insert
		/// an element will succeed by inspecting <see cref="Spring.Threading.Collections.IBlockingQueue.RemainingCapacity"/>
		/// because it may be the case that another thread is about to
		/// insert or remove an element.
		/// </summary>
		/// <returns> the remaining capacity</returns>
		public int RemainingCapacity
		{
			get
			{
				ReentrantLock currentLock = _lock;
				currentLock.Lock();
				try
				{
					return _items.Length - _count;
				}
				finally
				{
					currentLock.Unlock();
				}
			}
		}

		/// <summary> 
		/// Removes all available elements from this queue and adds them
		/// to the given collection.  
		/// </summary>
		/// <remarks>
		/// This operation may be more
		/// efficient than repeatedly polling this queue.  A failure
		/// encountered while attempting to add elements to
		/// collection <paramref name="collection"/> may result in elements being in neither,
		/// either or both collections when the associated exception is
		/// thrown.  Attempts to drain a queue to itself result in
		/// <see cref="InvalidCastException"/>. Further, the behavior of
		/// this operation is undefined if the specified collection is
		/// modified while the operation is in progress.
		/// </remarks>
		/// <param name="collection">the collection to transfer elements into</param>
		/// <returns> the number of elements transferred</returns>
		/// <exception cref="ArgumentNullException">
		/// If the queue cannot be drained at this time.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If the class of the supplied <paramref name="collection"/> prevents it
		/// from being used for the elemetns from the queue.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If the specified collection is <see lang="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If <paramref name="collection"/> represents the queue itself.
		/// </exception>
		public int DrainTo(ICollection collection)
		{
			if (collection == null)
				throw new System.ArgumentNullException("collection");
			if (collection == this)
				throw new System.InvalidOperationException("Cannot drain queue to itself.");
			System.Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				int i = _takeIndex;
				int n = 0;
				int max = _count;
				while (n < max)
				{
					CollectionUtils.Add(collection, items[i]);
					items[i] = null;
					i = increment(i);
					++n;
				}
				if (n > 0)
				{
					_count = 0;
					_putIndex = 0;
					_takeIndex = 0;
					_notFullCondition.SignalAll();
				}
				return n;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> Removes at most the given number of available elements from
		/// this queue and adds them to the given collection.  
		/// </summary>
		/// <remarks> 
		/// This operation may be more
		/// efficient than repeatedly polling this queue.  A failure
		/// encountered while attempting to add elements to
		/// collection <paramref name="collection"/> may result in elements being in neither,
		/// either or both collections when the associated exception is
		/// thrown.  Attempts to drain a queue to itself result in
		/// <see cref="InvalidOperationException"/>. Further, the behavior of
		/// this operation is undefined if the specified collection is
		/// modified while the operation is in progress.
		/// </remarks>
		/// <param name="collection">the collection to transfer elements into</param>
		/// <param name="maxElements">the maximum number of elements to transfer</param>
		/// <returns> the number of elements transferred</returns>
		/// <exception cref="InvalidCastException">
		/// If the queue cannot be drained at this time.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If the class of the supplied <paramref name="collection"/> prevents it
		/// from being used for the elemetns from the queue.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If the specified collection is <see lang="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="collection"/> represents the queue itself.
		/// </exception>
		public int DrainTo(ICollection collection, int maxElements)
		{
			if (collection == null)
				throw new System.ArgumentNullException("collection");
			if (collection == this)
				throw new System.InvalidOperationException("Cannot drain queue to itself.");
			if (maxElements <= 0)
				return 0;
			System.Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				int i = _takeIndex;
				int n = 0;
				int max = (maxElements < _count)?maxElements:_count;
				while (n < max)
				{
					CollectionUtils.Add(collection, items[i]);
					items[i] = null;
					i = increment(i);
					++n;
				}
				if (n > 0)
				{
					_count -= n;
					_takeIndex = i;
					_notFullCondition.SignalAll();
				}
				return n;
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Retrieves, but does not remove, the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public override object Peek()
		{
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				return (_count == 0) ? null : _items[_takeIndex];
			}
			finally
			{
				currentLock.Unlock();
			}
		}

		/// <summary> 
		/// Returns an <see cref="System.Array"/> containing all of the elements in this queue, in
		/// proper sequence.
		/// </summary>
		/// <remarks>
		/// The returned array will be "safe" in that no references to it are
		/// maintained by this queue.  (In other words, this method must allocate
		/// a new array).  The caller is thus free to modify the returned array.
		/// </remarks>
		/// <returns> an <see cref="System.Array"/> containing all of the elements in this queue</returns>
		public Object[] ToArray()
		{
			Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				Object[] a = new Object[_count];
				int k = 0;
				int i = _takeIndex;
				while (k < _count)
				{
					a[k++] = items[i];
					i = increment(i);
				}
				return a;
			}
			finally
			{
				currentLock.Unlock();
			}
		}
		/// <summary> 
		/// Returns an array containing all of the elements in this queue, in
		/// proper sequence; the runtime type of the returned array is that of
		/// the specified array.  
		/// </summary>
		/// <remarks>
		/// If the queue fits in the specified array, it
		/// is returned therein.  Otherwise, a new array is allocated with the
		/// runtime type of the specified array and the size of this queue.
		/// 
		/// <p/>
		/// If this queue fits in the specified array with room to spare
		/// (i.e., the array has more elements than this queue), the element in
		/// the array immediately following the end of the queue is set to
		/// <see lang="null"/>.
		/// 
		/// <p/>
		/// Like the <see cref="Spring.Threading.Collections.ArrayBlockingQueue.ToArray()"/> method, 
		/// this method acts as bridge between
		/// array-based and collection-based APIs.  Further, this method allows
		/// precise control over the runtime type of the output array, and may,
		/// under certain circumstances, be used to save allocation costs.
		/// 
		/// <p/>
		/// Suppose <i>x</i> is a queue known to contain only strings.
		/// The following code can be used to dump the queue into a newly
		/// allocated array of <see cref="System.String"/> objects:
		/// 
		/// <code>
		///		string[] y = x.ToArray(new string[0]);
		/// </code>
		/// 
		/// Note that <see cref="Spring.Threading.Collections.ArrayBlockingQueue.ToArray(object[])"/> with an empty
		/// arry is identical in function to
		/// <see cref="Spring.Threading.Collections.ArrayBlockingQueue.ToArray()"/>.
		/// </remarks>
		/// <param name="targetArray">
		/// the array into which the elements of the queue are to
		/// be stored, if it is big enough; otherwise, a new array of the
		/// same runtime type is allocated for this purpose
		/// </param>
		/// <returns> an array containing all of the elements in this queue</returns>
		/// <exception cref="ArrayTypeMismatchException">if the runtime type of the <pararef name="targetArray"/> 
		/// is not a super tyoe of the runtime tye of every element in this queue.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">If the <paramref name="targetArray"/> is <see lang="null"/>
		/// </exception>
		public System.Object[] ToArray(System.Object[] targetArray)
		{
			if ( targetArray == null )
				throw new ArgumentNullException("targetArray");
			System.Object[] items = _items;
			ReentrantLock currentLock = _lock;
			currentLock.Lock();
			try
			{
				if (targetArray.Length < _count)
					targetArray = (System.Object[]) System.Array.CreateInstance(targetArray.GetType().GetElementType(), _count);
				
				int k = 0;
				int i = _takeIndex;
				while (k < _count)
				{
					targetArray[k++] = items[i];
					i = increment(i);
				}
				if (targetArray.Length > _count)
					targetArray[_count] = null;
				return targetArray;
			}
			finally
			{
				currentLock.Unlock();
			}
		}
		/// <summary>
		/// When implemented by a class, gets a value indicating whether access to the 
		/// <see cref="System.Collections.ICollection"/> is synchronized (thread-safe).
		/// </summary>
		public override bool IsSynchronized
		{
			get { return true; }
		}
		/// <summary>
		/// Returns <see lang="true"/> if there are no elements in the <see cref="IQueue"/>, <see lang="false"/> otherwise.
		/// </summary>
		public override bool IsEmpty
		{
			get { return _count == 0;}
		}

		/// <summary> 
		/// Returns an <see cref="System.Collections.IEnumerator"/> over the elements in this queue in proper sequence.
		/// </summary>
		/// <remarks>
		/// The returned <see cref="System.Collections.IEnumerator"/> is a "weakly consistent" iterator that
		/// will never throw {@link ConcurrentModificationException},
		/// and guarantees to traverse elements as they existed upon
		/// construction of the iterator, and may (but is not guaranteed to)
		/// reflect any modifications subsequent to construction.
		/// </remarks>
		/// <returns> an iterator over the elements in this queue in proper sequence
		/// </returns>
		public override IEnumerator GetEnumerator()
		{
			ReentrantLock lock_Renamed = _lock;
			lock_Renamed.Lock();
			try
			{
				return new ArrayBlockingQueueEnumerator(this);
			}
			finally
			{
				lock_Renamed.Unlock();
			}
		}
		private class ArrayBlockingQueueEnumerator : IEnumerator
		{
			/// <summary> 
			/// Index of element to be returned by next,
			/// or a negative number if no such element.
			/// </summary>
			private int _nextIndex;
			/// <summary>
			/// Parent <see cref="Spring.Threading.Collections.ArrayBlockingQueue"/> 
			/// for this <see cref="System.Collections.IEnumerator"/>
			/// </summary>
			private ArrayBlockingQueue _enclosingInstance;
			/// <summary> 
			/// nextItem holds on to item fields because once we claim
			/// that an element exists in hasNext(), we must return it in
			/// the following next() call even if it was in the process of
			/// being removed when hasNext() was called.
			/// </summary>
			private object _nextItem;
			/// <summary> 
			/// Index of element returned by most recent call to next.
			/// Reset to -1 if this element is deleted by a call to remove.
			/// </summary>
			private int _lastReturnIndex;
			public object Current
			{
				get
				{
					ReentrantLock currentLock = _enclosingInstance._lock;
					currentLock.Lock();
					try
					{
						if (_nextIndex < 0)
							throw new NoElementsException();
						_lastReturnIndex = _nextIndex;
						System.Object x = _nextItem;
						_nextIndex = _enclosingInstance.increment(_nextIndex);
						checkNext();
						return x;
					}
					finally
					{
						currentLock.Unlock();
					}
				}
				
			}
			
			
				
			internal ArrayBlockingQueueEnumerator(ArrayBlockingQueue enclosingInstance)
			{
				_enclosingInstance = enclosingInstance;
				setInitialState();
			}
			
			public bool MoveNext()
			{
				return _nextIndex >= 0;
			}

			public void Reset()
			{
				setInitialState();
			}

			private void setInitialState()
			{
				_lastReturnIndex = - 1;
				if (_enclosingInstance.Count == 0)
					_nextIndex = - 1;
				else
				{
					_nextIndex = _enclosingInstance._takeIndex;
					_nextItem = _enclosingInstance._items[_enclosingInstance._takeIndex];
				}
			}

			/// <summary> 
			/// Checks whether nextIndex is valid; if so setting nextItem.
			/// Stops iterator when either hits putIndex or sees null item.
			/// </summary>
			private void checkNext()
			{
				if (_nextIndex == _enclosingInstance._putIndex)
				{
					_nextIndex = - 1;
					_nextItem = null;
				}
				else
				{
					_nextItem = _enclosingInstance._items[_nextIndex];
					if (_nextItem == null)
						_nextIndex = - 1;
				}
			}
		}
	}
}