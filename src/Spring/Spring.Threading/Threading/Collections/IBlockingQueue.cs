using System;
using System.Collections;
using Spring.Collections;

namespace Spring.Threading.Collections
{
	/// <summary> 
	/// A <see cref="Spring.Collections.IQueue"/> that additionally supports operations
	/// that wait for the queue to become non-empty when retrieving an
	/// element, and wait for space to become available in the queue when
	/// storing an element.
	/// </summary>
	/// <remarks>
	/// <see cref="Spring.Threading.Collections.IBlockingQueue"/> methods come in four forms, with different ways
	/// of handling operations that cannot be satisfied immediately, but may be
	/// satisfied at some point in the future:
	/// one throws an exception, the second returns a special value (either
	/// <see lang="null"/> or <see lang="false"/>, depending on the operation), the third
	/// blocks the current thread indefinitely until the operation can succeed,
	/// and the fourth blocks for only a given maximum time limit before giving
	/// up.  These methods are summarized in the following table:
	/// 
	/// <p/>
	/// A <see cref="Spring.Threading.Collections.IBlockingQueue"/> does not accept <see lang="null"/> elements.
	/// Implementations throw <see cref="System.ArgumentNullException"/> on attempts
	/// to call <see cref="Spring.Collections.IQueue.Add(object)"/>, <see cref="Spring.Threading.Collections.IBlockingQueue.Put(object)"/>
	/// or  <see cref="Spring.Collections.IQueue.Offer(object)"/> a <see lang="null"/>.  A
	/// <see lang="null"/> is used as a sentinel value to indicate failure of
	/// <see cref="Spring.Collections.IQueue.Poll()"/> operations.
	/// 
	/// <p/>
	/// A <see cref="Spring.Threading.Collections.IBlockingQueue"/> may be capacity bounded. At any given
	/// time it may have a <see cref="Spring.Threading.Collections.IBlockingQueue.RemainingCapacity"/> beyond which no
	/// additional elements can be <see cref="Spring.Threading.Collections.IBlockingQueue.Put(object)"/> without blocking.
	/// A <see cref="Spring.Threading.Collections.IBlockingQueue"/> without any intrinsic capacity constraints always
	/// reports a remaining capacity of <see cref="System.Int32.MaxValue"/>.
	/// 
	/// <p/>
	/// <see cref="Spring.Threading.Collections.IBlockingQueue"/> implementations are designed to be used
	/// primarily for producer-consumer queues, but additionally support
	/// the <see cref="System.Collections.ICollection"/> interface.  So, for example, it is
	/// possible to remove an arbitrary element from a queue using
	/// <see cref="Spring.Collections.IQueue.Remove()"/>. 
	/// However, such operations are in general
	/// <b>not</b> performed very efficiently, and are intended for only
	/// occasional use, such as when a queued message is cancelled.
	/// 
	/// <p/>
	/// A <see cref="Spring.Threading.Collections.IBlockingQueue"/> does <b>not</b> intrinsically support
	/// any kind of 'close' or 'shutdown' operation to
	/// indicate that no more items will be added.  The needs and usage of
	/// such features tend to be implementation-dependent. For example, a
	/// common tactic is for producers to insert special
	/// <b>end-of-stream</b> or <b>poison</b> objects, that are
	/// interpreted accordingly when taken by consumers.
	/// 
	/// <p/>
	/// Usage example, based on a typical producer-consumer scenario.
	/// Note that a <see cref="Spring.Threading.Collections.IBlockingQueue"/> can safely be used with multiple
	/// producers and multiple consumers.
	/// 
	/// <code>
	/// class Producer : IRunnable {
	///		private IBlockingQueue queue;
	/// 	Producer(IBlockingQueue q) { queue = q; }
	/// 	public void Run() {
	/// 		try {
	/// 			while (true) { 
	/// 				queue.Put(produce()); 
	/// 			}
	/// 		} catch (InterruptedException ex) { 
	/// 			... handle ...
	/// 		}
	/// 	}
	/// 	Object Produce() { ... }
	/// }
	/// 
	/// class Consumer : IRunnable {
	///		private IBlockingQueue queue;
	/// 	Consumer(IBlockingQueue q) { queue = q; }
	/// 	public void Run() {
	/// 		try {
	/// 			while (true) { Consume(queue.Take()); }
	/// 		} catch (InterruptedException ex) { ... handle ...}
	/// 	}
	/// 	void Consume(object x) { ... }
	/// }
	/// 
	/// class Setup {
	///		void Main() {
	/// 		IBlockingQueue q = new SomeQueueImplementation();
	/// 		Producer p = new Producer(q);
	/// 		Consumer c1 = new Consumer(q);
	/// 		Consumer c2 = new Consumer(q);
	/// 		new Thread(new ThreadStart(p.Run)).Start();
	/// 		new Thread(new ThreadStart(c1.Run)).Start();
	/// 		new Thread(new ThreadStart(c2.Run)).Start();
	/// 	}
	/// }
	/// </code>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio(.NET)</author>
	public interface IBlockingQueue : IQueue
	{
		/// <summary> 
		/// Inserts the specified element into this queue, waiting if necessary
		/// for space to become available.
		/// </summary>
		/// <param name="objectToAdd">the element to add</param>
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
		void Put(object objectToAdd);

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
		bool Offer(object objectToAdd, TimeSpan duration);

		/// <summary> 
		/// Retrieves and removes the head of this queue, waiting if necessary
		/// until an element becomes available.
		/// </summary>
		/// <returns> the head of this queue</returns>
		object Take();

		/// <summary> 
		/// Retrieves and removes the head of this queue, waiting up to the
		/// specified wait time if necessary for an element to become available.
		/// </summary>
		/// <param name="duration">how long to wait before giving up</param>
		/// <returns> 
		/// the head of this queue, or <see lang="null"/> if the
		/// specified waiting time elapses before an element is available
		/// </returns>
		object Poll(TimeSpan duration);

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
		int RemainingCapacity {get;}

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
		/// <see cref="System.ArgumentException"/>. Further, the behavior of
		/// this operation is undefined if the specified collection is
		/// modified while the operation is in progress.
		/// </remarks>
		/// <param name="collection">the collection to transfer elements into</param>
		/// <returns> the number of elements transferred</returns>
		/// <exception cref="System.InvalidOperationException">
		/// If the queue cannot be drained at this time.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// If the class of the supplied <paramref name="collection"/> prevents it
		/// from being used for the elemetns from the queue.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the specified collection is <see lang="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="collection"/> represents the queue itself.
		/// </exception>
		int DrainTo(ICollection collection);

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
		/// <see cref="System.ArgumentException"/>. Further, the behavior of
		/// this operation is undefined if the specified collection is
		/// modified while the operation is in progress.
		/// </remarks>
		/// <param name="collection">the collection to transfer elements into</param>
		/// <param name="maxElements">the maximum number of elements to transfer</param>
		/// <returns> the number of elements transferred</returns>
		/// <exception cref="System.InvalidOperationException">
		/// If the queue cannot be drained at this time.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// If the class of the supplied <paramref name="collection"/> prevents it
		/// from being used for the elemetns from the queue.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the specified collection is <see lang="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="collection"/> represents the queue itself.
		/// </exception>
		int DrainTo(ICollection collection, int maxElements);
	}
}