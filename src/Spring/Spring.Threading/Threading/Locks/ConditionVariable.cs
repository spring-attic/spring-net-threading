using System;
using System.Collections;
using System.Threading;

namespace Spring.Threading.Locks
{
    [Serializable]
    internal class ConditionVariable : ICondition
    {
        protected internal IExclusiveLock _internalExclusiveLock;

        /// <summary> 
        /// Create a new <see cref="Spring.Threading.Locks.ConditionVariable"/> that relies on the given mutual
        /// exclusion lock.
        /// </summary>
        /// <param name="exclusiveLock">
        /// A non-reentrant mutual exclusion lock.
        /// </param>
        internal ConditionVariable(IExclusiveLock exclusiveLock)
        {
            _internalExclusiveLock = exclusiveLock;
        }

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

        #region ICondition Members

        public virtual void AwaitUninterruptibly()
        {
            var holdCount = _internalExclusiveLock.HoldCount;
            if (holdCount == 0)
            {
                throw new SynchronizationLockException();
            }
            // avoid instant spurious wakeup if thread already interrupted
            var wasInterrupted = Thread.CurrentThread.IsAlive;

            try
            {
                lock (this)
                {
                    for (var i = holdCount; i > 0; i--) _internalExclusiveLock.Unlock();
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch (ThreadInterruptedException)
                    {
                        wasInterrupted = true;
                        // may have masked the signal and there is no way
                        // to tell; defensively propagate the signal
                        Monitor.Pulse(this);
                    }
                }
            }
            finally
            {
                for (var i = holdCount; i > 0; i--) _internalExclusiveLock.Lock();
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
                    catch (ThreadInterruptedException)
                    {
                        Monitor.Pulse(this);
                        throw;
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
            var duration = durationToWait;
            var success = false;
            try
            {
                lock (this)
                {
                    _internalExclusiveLock.Unlock();
                    try
                    {
                        if (duration.TotalMilliseconds > 0)
                        {
                            var start = DateTime.Now;
                            Monitor.Wait(this, durationToWait);
                            success = DateTime.Now.Subtract(start).TotalMilliseconds < duration.TotalMilliseconds;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        Monitor.Pulse(this);
                        throw;
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
            var abstime = deadline.Ticks;
            bool deadlineHasPassed;
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
                            deadlineHasPassed = (DateTime.Now.Ticks - start) < msecs;
                        }
                        else
                        {
                            deadlineHasPassed = true;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        Monitor.Pulse(this);
                        throw;
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

        #endregion

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