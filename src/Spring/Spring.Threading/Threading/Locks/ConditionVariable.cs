using System;
using System.Collections;
using System.Threading;

namespace Spring.Threading.Locks
{
	[Serializable]
	internal class ConditionVariable : ICondition
	{
		protected internal IExclusiveLock _internalExclusiveLock;

		protected internal virtual IExclusiveLock Lock
		{
			get { return _internalExclusiveLock; }

		}

		protected internal virtual int WaitQueueLength
		{
			get { throw new NotSupportedException("Use FAIR version"); }

		}

		protected internal virtual ICollection WaitingThreads
		{
			get { throw new NotSupportedException("Use FAIR version"); }

		}

		/// <summary> 
		/// Create a new <see cref="Spring.Threading.Locks.ConditionVariable"/> that relies on the given mutual
		/// exclusion lock.
		/// </summary>
		/// <param name="exclusiveLock">
		/// A non-reentrant mutual exclusion lock.
		/// </param>
		internal ConditionVariable(IExclusiveLock exclusiveLock)
		{
			this._internalExclusiveLock = exclusiveLock;
		}

		public virtual void AwaitUninterruptibly()
		{
			bool wasInterrupted = false;
			try
			{
				lock (this)
				{
					_internalExclusiveLock.Unlock();
					while (true)
					{
						try
						{
							Monitor.Wait(this);
							break;
						}
						catch (ThreadInterruptedException)
						{
							wasInterrupted = true;
						}
					}
				}
			}
			finally
			{
				_internalExclusiveLock.Lock();
				if (wasInterrupted)
				{
					Thread.CurrentThread.Interrupt();
				}
			}
		}

		public virtual void Await()
		{
			try
			{
				lock (this)
				{
					_internalExclusiveLock.Unlock();
					try
					{
						Monitor.Wait(this);
					}
					catch (ThreadInterruptedException ex)
					{
						Monitor.Pulse(this);
						throw ex;
					}
				}
			}
			finally
			{
				_internalExclusiveLock.Lock();
			}
		}

		public virtual bool Await(TimeSpan durationToWait)
		{
			TimeSpan duration = durationToWait;
			bool success = false;
			try
			{
				lock (this)
				{
					_internalExclusiveLock.Unlock();
					try
					{
						if ( duration.TotalMilliseconds > 0)
						{
							DateTime start = DateTime.Now;
							Monitor.Wait(this, durationToWait);
							success = DateTime.Now.Subtract(start).TotalMilliseconds < duration.TotalMilliseconds;
						}
					}
					catch (ThreadInterruptedException ex)
					{
						Monitor.Pulse(this);
						throw ex;
					}
				}
			}
			finally
			{
				_internalExclusiveLock.Lock();
			}
			return success;
		}

		public virtual bool AwaitUntil(DateTime deadline)
		{
			if (deadline == DateTime.MinValue || deadline == DateTime.MaxValue)
			{
				throw new NullReferenceException();
			}
			long abstime = deadline.Ticks;
			bool deadlineHasPassed = false;
			try
			{
				lock (this)
				{
					_internalExclusiveLock.Unlock();
					try
					{
						long start = DateTime.Now.Ticks;
						long msecs = abstime - start;
						if (msecs > 0)
						{
							Monitor.Wait(this, deadline.Subtract(DateTime.Now));
							// DK: due to coarse-grained (millis) clock, it seems
							// preferable to acknowledge timeout (success == false)
							// when the equality holds (timing is exact)
							deadlineHasPassed =  ( DateTime.Now.Ticks - start) < msecs;
						}
						else
						{
							deadlineHasPassed = true;
						}
					}
					catch (ThreadInterruptedException ex)
					{
						Monitor.Pulse(this);
						throw ex;
					}
				}
			}
			finally
			{
				_internalExclusiveLock.Lock();
			}
			return deadlineHasPassed;
		}

		public virtual void Signal()
		{
			lock (this)
			{
				checkifLockIsHeldByCurrentThread();
				Monitor.Pulse(this);
			}
		}


		public virtual void SignalAll()
		{
			lock (this)
			{
				checkifLockIsHeldByCurrentThread();
				Monitor.PulseAll(this);
			}
		}

		protected internal virtual bool hasWaiters()
		{
			throw new NotSupportedException("Use FAIR version");
		}

		private void checkifLockIsHeldByCurrentThread()
		{
			if (!_internalExclusiveLock.HeldByCurrentThread)
			{
				throw new SynchronizationLockException();
			}
		}

	}
}