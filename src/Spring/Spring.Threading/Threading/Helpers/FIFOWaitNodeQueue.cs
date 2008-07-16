using System;
using System.Collections;
using System.Threading;

namespace Spring.Threading.Helpers
{
	/// <summary> 
	/// Simple linked list queue used in FIFOSemaphore.
	/// Methods are not locked; they depend on synch of callers.
	/// Must be public, since it is used by Semaphore (outside this package).
	/// </summary>
	[Serializable]
	public class FIFOWaitNodeQueue : IWaitNodeQueue
	{
		/// <summary>
		/// 
		/// </summary>
		[NonSerialized] protected WaitNode head;
		/// <summary>
		/// 
		/// </summary>
		[NonSerialized] protected WaitNode tail;
		/// <summary>
		/// 
		/// </summary>
		public FIFOWaitNodeQueue()
		{
		}
		/// <summary>
		/// 
		/// </summary>
		public int Count
		{
			get
			{
				int count = 0;
				WaitNode node = head;
				while (node != null)
				{
					if (node.IsWaiting)
						count++;
					node = node.NextWaitNode;
				}
				return count;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		public ICollection WaitingThreads
		{
			get
			{
				IList list = new ArrayList();
				WaitNode node = head;
				while (node != null)
				{
					if (node.IsWaiting)
						list.Add(node.Owner);
					node = node.NextWaitNode;
				}
				return list;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="waitNode"></param>
		public void Enqueue(WaitNode waitNode)
		{
			if (tail == null)
			    head = tail = waitNode;
			else
			{
			    tail.NextWaitNode = waitNode;
			    tail = waitNode;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public WaitNode Dequeue()
		{
			if (head == null)
				return null;
			else
			{
				WaitNode w = head;
			    head = w.NextWaitNode;
				if (head == null)
				    tail = null;
				w.NextWaitNode = null;
				return w;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool HasNodes
		{
			get
			{
				return head != null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="thread"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">If <paramref name="thread"/> is null.</exception>
		public bool IsWaiting(Thread thread)
		{
			if (thread == null)
				throw new ArgumentNullException("thread", "Thread cannot be null.");
			for (WaitNode node = head; node != null; node = node.NextWaitNode)
			{
				if (node.IsWaiting && node.Owner == thread)
					return true;
			}
			return false;
		}
	}
}