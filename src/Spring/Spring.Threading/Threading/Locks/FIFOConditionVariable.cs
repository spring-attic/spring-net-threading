using System;
using System.Collections;
using System.Threading;
using Spring.Threading.Helpers;

namespace Spring.Threading.Locks
{
	/// <summary>
	/// 
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	internal class FIFOConditionVariable : ConditionVariable
	{
		static FIFOConditionVariable()
		{
			sync = new AnonymousClassQueuedSync();
		}

		public class AnonymousClassQueuedSync : IQueuedSync
		{
			public virtual bool Recheck(WaitNode node)
			{
				return false;
			}

			public virtual void TakeOver(WaitNode node)
			{
			}
		}

		protected internal override int WaitQueueLength
		{
			get
			{
				if (!Lock.HeldByCurrentThread)
				{
					throw new SynchronizationLockException();
				}
				return wq.Count;
			}

		}

		protected internal override ICollection WaitingThreads
		{
			get
			{
				if (!Lock.HeldByCurrentThread)
				{
					throw new SynchronizationLockException();
				}
				return wq.WaitingThreads;
			}

		}

		private static readonly IQueuedSync sync;

		private IWaitNodeQueue wq = new FIFOWaitNodeQueue();

		internal FIFOConditionVariable(IExclusiveLock exclusiveLock) : base(exclusiveLock)
		{
		}

		public override void AwaitUninterruptibly()
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
			WaitNode n = new WaitNode();
			wq.Enqueue(n);
			Lock.Unlock();
			try
			{
				n.DoWaitUninterruptibly(sync);
			}
			finally
			{
				Lock.Lock();
			}
		}

		public override void Await()
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
			WaitNode n = new WaitNode();
			wq.Enqueue(n);
			Lock.Unlock();
			try
			{
				n.DoWait(sync);
			}
			finally
			{
				Lock.Lock();
			}
		}

		public override bool Await(TimeSpan timespan)
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
          
			WaitNode n = new WaitNode();
			wq.Enqueue(n);
			bool success;
			Lock.Unlock();
			try
			{
				success = n.DoTimedWait(sync, timespan);
			}
			finally
			{
				Lock.Lock();
			}
			return success;
		}

		public override bool AwaitUntil(DateTime deadline)
		{
			if (deadline == DateTime.MinValue || deadline == DateTime.MaxValue)
			{
				throw new NullReferenceException();
			}
			return Await(deadline.Subtract(DateTime.Now));
		}

		public override void Signal()
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
			for (;; )
			{
				WaitNode w = wq.Dequeue();
				if (w == null)
					return;
				if (w.Signal(sync))
					return;
			}
		}

		public override void SignalAll()
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
			for (;; )
			{
				WaitNode w = wq.Dequeue();
				if (w == null)
					return; 
				w.Signal(sync);
			}
		}

		protected internal override bool hasWaiters()
		{
			if (!Lock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
			return wq.HasNodes;
		}

	}
}