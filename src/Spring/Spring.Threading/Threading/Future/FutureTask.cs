//using System;
//using System.Threading;
//using Spring.Threading.Execution;

//namespace Spring.Threading.Future
//{
//    /// <summary>
//    /// Enumeration representing a task execution status.
//    /// </summary>
//    public enum TaskState
//    {
//        /// <summary>State value representing that task is running </summary>
//        RUNNING = 1,
//        /// <summary>State value representing that task ran </summary>
//        COMPLETE = 2,
//        /// <summary>State value representing that task was cancelled </summary>
//        CANCELLED = 4,
//        /// <summary>State value representing that the task should be stopped.</summary>
//        STOP = 8
//    }

//    /// <summary> 
//    /// A cancellable asynchronous computation.  
//    /// </summary>	
//    /// <remarks> 
//    /// This class provides a base
//    /// implementation of <see cref="Spring.Threading.Future.IFuture{T}"/> , with methods to start and cancel
//    /// a computation, query to see if the computation is complete, and
//    /// retrieve the result of the computation.  The result can only be
//    /// retrieved when the computation has completed; the <see cref="Spring.Threading.Future.IFuture{T}.GetResult()"/>
//    /// method will block if the computation has not yet completed.  Once
//    /// the computation has completed, the computation cannot be restarted
//    /// or cancelled.
//    /// 
//    /// <p/>
//    /// A <see cref="Spring.Threading.Future.FutureTask{T}"/> can be used to wrap a <see cref="Spring.Threading.ICallable{T}"/> or
//    /// <see cref="Spring.Threading.IRunnable"/> object.  Because <see cref="Spring.Threading.Future.FutureTask{T}"/>
//    /// implements <see cref="Spring.Threading.IRunnable"/>, a <see cref="Spring.Threading.Future.FutureTask{T}"/> can be
//    /// submitted to an <see cref="Spring.Threading.IExecutor"/> for execution.
//    /// 
//    /// <p/>
//    /// In addition to serving as a standalone class, this class provides
//    /// protected functionality that may be useful when creating
//    /// customized task classes.
//    /// </remarks>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio (.NET)</author>
//    public class FutureTask<T> : IRunnableFuture<T>
//    {
//        private ICallable<T> _callable;
//        private T _result;
//        private Exception _exception;
//        private TaskState _taskState;
//        /// <summary> 
//        /// The thread running task. When nulled after set/cancel, this
//        /// indicates that the results are accessible.  Must be
//        /// volatile, to ensure visibility upon completion.
//        /// </summary>
//        private volatile Thread _runningThread;

//        #region Constructors
//        /// <summary> 
//        /// Creates a <see cref="Spring.Threading.Future.FutureTask{T}"/> that will, upon running, execute the
//        /// given <see cref="Spring.Threading.ICallable{T}"/>.
//        /// </summary>
//        /// <param name="callable">the callable task</param>
//        /// <exception cref="System.ArgumentNullException">if the <paramref name="callable"/> is null.</exception>
//        public FutureTask(ICallable<T> callable)
//        {
//            if (callable == null)
//                throw new ArgumentNullException("callable", "Callable cannot be null.");
//            _callable = callable;
//        }

//        /// <summary> Creates a <see cref="Spring.Threading.Future.FutureTask{T}"/> that will, upon running, execute the
//        /// given <see cref="Spring.Threading.IRunnable"/>, and arrange that <see cref="Spring.Threading.Future.IFuture{T}.GetResult"/> will return the
//        /// given <paramref name="result"/> upon successful completion.
//        /// </summary>
//        /// <param name="runnable">the runnable task</param>
//        /// <param name="result">the result to return on successful completion. If
//        /// you don't need a particular result, consider using
//        /// constructions of the form:
//        /// <code>
//        ///		Future f = new FutureTask(runnable, null)
//        ///	</code>	
//        /// </param>
//        /// <exception cref="System.ArgumentNullException">if the <paramref name="runnable"/> is null.</exception>
//        public FutureTask(IRunnable runnable, T result) : this(Executors.CreateCallable(runnable, result))
//        {
//        }
//        #endregion

//        #region IRunnableFuture Implementation
//        /// <summary>
//        /// Determines if this task was cancelled.
//        /// </summary>
//        /// <remarks> 
//        /// Returns <see lang="true"/> if this task was cancelled before it completed
//        /// normally.
//        /// </remarks>
//        /// <returns> <see lang="true"/>if task was cancelled before it completed
//        /// </returns>
//        public virtual bool IsCancelled
//        {
//            get
//            {
//                lock (this)
//                {
//                    return _taskState == TaskState.CANCELLED;
//                }
//            }
//        }
//        /// <summary> 
//        /// Returns <see lang="true"/> if this task completed.
//        /// </summary>
//        /// <remarks> 
//        /// Completion may be due to normal termination, an exception, or
//        /// cancellation -- in all of these cases, this method will return
//        /// <see lang="true"/> if this task completed.
//        /// </remarks>
//        /// <returns> <see lang="true"/>if this task completed.</returns>
//        public virtual bool IsDone
//        {
//            get
//            {
//                lock (this)
//                {
//                    return ranOrCancelled() && _runningThread == null;
//                }
//            }
//        }
//        /// <summary> 
//        /// Attempts to cancel execution of this task.  
//        /// </summary>
//        /// <remarks> 
//        /// This attempt will fail if the task has already completed, already been cancelled,
//        /// or could not be cancelled for some other reason. If successful,
//        /// and this task has not started when <see cref="Spring.Threading.Future.IFuture{T}.Cancel()"/> is called,
//        /// this task should never run.  If the task has already started, the in-progress tasks are allowed
//        /// to complete
//        /// </remarks>
//        /// <returns> <see lang="false"/> if the task could not be cancelled,
//        /// typically because it has already completed normally;
//        /// <see lang="true"/> otherwise
//        /// </returns>
//        public virtual bool Cancel()
//        {
//            return Cancel(false);
//        }
//        /// <summary> 
//        /// Attempts to cancel execution of this task.  
//        /// </summary>
//        /// <remarks> 
//        /// This attempt will fail if the task has already completed, already been cancelled,
//        /// or could not be cancelled for some other reason. If successful,
//        /// and this task has not started when <see cref="Spring.Threading.Future.IFuture{T}.Cancel()"/> is called,
//        /// this task should never run.  If the task has already started,
//        /// then the <paramref name="mayInterruptIfRunning"/> parameter determines
//        /// whether the thread executing this task should be interrupted in
//        /// an attempt to stop the task.
//        /// </remarks>
//        /// <param name="mayInterruptIfRunning"><see lang="true"/> if the thread executing this
//        /// task should be interrupted; otherwise, in-progress tasks are allowed
//        /// to complete
//        /// </param>
//        /// <returns> <see lang="false"/> if the task could not be cancelled,
//        /// typically because it has already completed normally;
//        /// <see lang="true"/> otherwise
//        /// </returns>
//        public virtual bool Cancel(bool mayInterruptIfRunning)
//        {
//            lock (this)
//            {
//                if (ranOrCancelled())
//                    return false;
//                _taskState = TaskState.CANCELLED;
//                if (mayInterruptIfRunning)
//                {
//                    Thread r = _runningThread;
//                    if (r != null)
//                    {
//                        try
//                        {
//                            r.Interrupt();
//                        } catch ( ThreadInterruptedException ){}
//                    }
//                }
//                _runningThread = null;
//                Monitor.PulseAll(this);
//            }
//            done();
//            return true;
//        }
//        /// <summary>
//        /// Waits for computation to complete, then returns its result. 
//        /// </summary>
//        /// <remarks> 
//        /// Waits if necessary for the computation to complete, and then
//        /// retrieves its result.
//        /// </remarks>
//        /// <returns>the computed result</returns>
//        /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
//        /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
//        /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
//        public virtual T GetResult()
//        {
//            lock (this)
//            {
//                waitFor();
//                return Result;
//            }
//        }
//        /// <summary>
//        /// Waits for the given time span, then returns its result.
//        /// </summary>
//        /// <remarks> 
//        /// Waits, if necessary, for at most the <paramref name="durationToWait"/> for the computation
//        /// to complete, and then retrieves its result, if available.
//        /// </remarks>
//        /// <param name="durationToWait">the <see cref="System.TimeSpan"/> to wait.</param>
//        /// <returns>the computed result</returns>
//        /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
//        /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
//        /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
//        /// <exception cref="Spring.Threading.TimeoutException">if the computation threw an exception.</exception>
//        public virtual T GetResult(TimeSpan durationToWait)
//        {
//            lock (this)
//            {
//                waitFor(durationToWait);
//                return Result;
//            }
//        }
//        /// <summary>
//        /// The entry point
//        /// </summary>
//        public void Run()
//        {
//            lock (this)
//            {
//                if (_taskState != 0)
//                    return;
//                _taskState = TaskState.RUNNING;
//                _runningThread = Thread.CurrentThread;
//            }
//            try
//            {
//                Completed = _callable.Call();
//            }
//            catch (Exception ex)
//            {
//                Failed = ex;
//            }
//        }
//        #endregion

//        #region Private Methods
//        /// <summary>
//        /// Sets the result of the task, and marks the task as completed
//        /// </summary>
//        private T Completed
//        {
//            set
//            {
//                lock (this)
//                {
//                    if (ranOrCancelled())
//                        return;
//                    _taskState = TaskState.COMPLETE;
//                    _result = value;
//                    _runningThread = null;
//                    Monitor.PulseAll(this);
//                }

//                // invoking callbacks *after* setting future as completed and
//                // outside the synchronization block makes it safe to call
//                // interrupt() from within callback code (in which case it will be
//                // ignored rather than cause deadlock / illegal state exception)
//                done();
//            }

//        }

//        /// <summary>
//        /// Sets the exception result of the task, and marks the tasks as completed.
//        /// </summary>
//        private Exception Failed
//        {
//            set
//            {
//                lock (this)
//                {
//                    if (ranOrCancelled())
//                        return;
//                    _taskState = TaskState.COMPLETE;
//                    _exception = value;
//                    _runningThread = null;
//                    Monitor.PulseAll(this);
//                }

//                // invoking callbacks *after* setting future as completed and
//                // outside the synchronization block makes it safe to call
//                // interrupt() from within callback code (in which case it will be
//                // ignored rather than cause deadlock / illegal state exception)
//                done();
//            }

//        }

//        /// <summary> 
//        /// Gets the result of the task.
//        /// </summary>
//        private T Result
//        {
//            get
//            {
//                if (_taskState == TaskState.CANCELLED)
//                {
//                    throw new CancellationException();
//                }
//                if (_exception != null)
//                {
//                    throw new ExecutionException(_exception);
//                }
//                return _result;
//            }

//        }

//        /// <summary> Waits for the task to complete.</summary>
//        private void waitFor()
//        {
//            while (!IsDone)
//            {
//                Monitor.Wait(this);
//            }
//        }

//        /// <summary> 
//        /// Waits for the task to complete for <paramref name="durationToWait"/> or throws a
//        /// <see cref="Spring.Threading.TimeoutException"/>
//        /// if still not completed after that
//        /// </summary>
//        private void waitFor(TimeSpan durationToWait)
//        {
//            if (durationToWait.TotalMilliseconds <= 0 )
//                throw new ArgumentException("Duration must be positive value.");
//            if (IsDone)
//            {
//                return;
//            }
//            else
//            {
//                Monitor.Wait(this, durationToWait);
//                if (IsDone)
//                    return;
//            }
//            throw new TimeoutException();
//        }
		
//        #endregion

//        #region Protected Methods
//        /// <summary> 
//        /// Protected method invoked when this task transitions to state
//        /// <see cref="Spring.Threading.Future.IFuture{T}.IsDone"/> (whether normally or via cancellation). 
//        /// </summary>
//        /// <remarks> 
//        /// The default implementation does nothing.  Subclasses may override
//        /// this method to invoke completion callbacks or perform
//        /// bookkeeping. Note that you can query status inside the
//        /// implementation of this method to determine whether this task
//        /// has been cancelled.
//        /// </remarks>
//        protected internal virtual void done()
//        {
//        }

//        /// <summary> 
//        /// Sets the result of this <see cref="Spring.Threading.Future.IFuture{T}"/> to the given <paramref name="result"/> value unless
//        /// this future has already been set or has been cancelled.
//        /// </summary>
//        /// <remarks>
//        /// This method is invoked internally by the <tt>run</tt> method
//        /// upon successful completion of the computation.
//        /// </remarks>
//        /// <param name="result">the value</param>
//        protected virtual void setResult(T result)
//        {
//            Completed = result;
//        }

//        /// <summary> 
//        /// Causes this future to report an <see cref="Spring.Threading.Execution.ExecutionException"/> 
//        /// with the given <see cref="System.Exception"/> as its cause, unless this <see cref="Spring.Threading.Future.IFuture{T}"/> has
//        /// already been set or has been cancelled.
//        /// </summary>
//        /// <remarks>
//        /// This method is invoked internally by the <see cref="Spring.Threading.IRunnable"/> method
//        /// upon failure of the computation.
//        /// </remarks>
//        /// <param name="t">the cause of failure</param>
//        protected virtual void setException(Exception t)
//        {
//            Failed = t;
//        }
//        /// <summary> 
//        /// Executes the computation without setting its result, and then
//        /// resets this Future to initial state, failing to do so if the
//        /// computation encounters an exception or is cancelled.  
//        /// </summary>
//        /// <remarks>
//        /// This is designed for use with tasks that intrinsically execute more
//        /// than once.
//        /// </remarks>
//        /// <returns> <see lang="true"/> if successfully run and reset</returns>
//        protected virtual bool runAndReset()
//        {
//            lock (this)
//            {
//                if (_taskState != 0)
//                    return false;
//                _taskState = TaskState.RUNNING;
//                _runningThread = Thread.CurrentThread;
//            }
//            try
//            {
//                _callable.Call();
//                lock (this)
//                {
//                    _runningThread = null;
//                    if (_taskState == TaskState.RUNNING)
//                    {
//                        _taskState = 0;
//                        return true;
//                    }
//                    else
//                    {
//                        return false;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Failed = ex;
//                return false;
//            }
//        }

//        private bool ranOrCancelled()
//        {
//            return _taskState == TaskState.COMPLETE || _taskState == TaskState.CANCELLED;
//        }
//        #endregion
//    }
//}