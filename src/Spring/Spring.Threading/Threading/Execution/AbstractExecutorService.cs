//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using Spring.Threading.Future;
//using ArrayList=System.Collections.ArrayList;

//namespace Spring.Threading.Execution
//{
//    /// <summary> 
//    /// Provides default implementations of <see cref="Spring.Threading.Execution.IExecutorService"/>
//    /// execution methods. 
//    /// </summary>
//    /// <remarks> 
//    /// This class implements the <see cref="Spring.Threading.Execution.IExecutorService.Submit{T}(IRunnable, T)"/> and 
//    /// the <see cref="Spring.Threading.Execution.IExecutorService.Submit{T}(ICallable{T})"
//    /// methods,
//    /// <see cref="Spring.Threading.Execution.IExecutorService.InvokeAny{T}"/> and <see cref="Spring.Threading.Execution.IExecutorService.InvokeAll{T}"/> methods using a
//    /// <see cref="Spring.Threading.Future.IRunnableFuture{T}"/> returned by <see cref="Spring.Threading.Execution.AbstractExecutorService.NewTaskFor{T}"/>
//    /// , which defaults to the <see cref="Spring.Threading.Future.FutureTask{T}"/> class provided in this package.  
//    /// <p/>
//    /// For example, the implementation of <see cref="Spring.Threading.Execution.IExecutorService.Submit(IRunnable)"/> creates an
//    /// associated <see cref="Spring.Threading.Future.IRunnableFuture{T}"/> that is executed and
//    /// returned. Subclasses may override the <see cref="Spring.Threading.Execution.AbstractExecutorService.NewTaskFor{T}"/> methods
//    /// to return <see cref="Spring.Threading.Future.IRunnableFuture{T}"/> implementations other than
//    /// <see cref="Spring.Threading.Future.FutureTask{T}"/>.
//    /// 
//    /// <p/> 
//    /// <b>Extension example</b>. 
//    /// Here is a sketch of a class
//    /// that customizes <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> to use
//    /// a custom Task class instead of the default <see cref="Spring.Threading.Future.FutureTask{T}"/>:
//    /// <code>
//    /// public class CustomThreadPoolExecutor : ThreadPoolExecutor {
//    ///		static class CustomTask : IRunnableFuture {...}
//    /// 
//    ///		protected IRunnableFuture newTaskFor(ICallable c) {
//    ///			return new CustomTask(c);
//    /// 	}
//    ///		protected IRunnableFuture newTaskFor(IRunnable r) {
//    /// 		return new CustomTask(r);
//    /// 	}
//    /// 	// ... add constructors, etc.
//    /// }
//    /// </code>
//    /// </remarks>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio(.NET)</author>
//    public abstract class AbstractExecutorService : IExecutorService
//    {
//        #region Abstract Methods
//        /// <summary> 
//        /// Initiates an orderly shutdown in which previously submitted
//        /// tasks are executed, but no new tasks will be
//        /// accepted. Invocation has no additional effect if already shut
//        /// down.
//        /// </summary>
//        public abstract void Shutdown();
//        /// <summary> 
//        /// Attempts to stop all actively executing tasks, halts the
//        /// processing of waiting tasks, and returns a list of the tasks that were
//        /// awaiting execution.
//        /// </summary>
//        /// <remarks> 
//        /// There are no guarantees beyond best-effort attempts to stop
//        /// processing actively executing tasks.  For example, typical
//        /// implementations will cancel via <see cref="System.Threading.Thread.Interrupt()"/>, so if any
//        /// tasks mask or fail to respond to interrupts, they may never terminate.
//        /// </remarks>
//        /// <returns> list of tasks that never commenced execution</returns>
//        public abstract IList<IRunnable> ShutdownNow();
//        /// <summary> 
//        /// Executes the given command at some time in the future.
//        /// </summary>
//        /// <remarks>
//        /// The command may execute in a new thread, in a pooled thread, or in the calling
//        /// thread, at the discretion of the <see cref="Spring.Threading.IExecutor"/> implementation.
//        /// </remarks>
//        /// <param name="runnable">the runnable task</param>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>	
//        public abstract void Execute(IRunnable runnable);
//        /// <summary> 
//        /// Blocks until all tasks have completed execution after a shutdown
//        /// request, or the timeout occurs, or the current thread is
//        /// interrupted, whichever happens first. 
//        /// </summary>
//        /// <param name="timeSpan">the time span to wait.
//        /// </param>
//        /// <returns> <see lang="true"/> if this executor terminated and <see lang="false"/>
//        /// if the timeout elapsed before termination
//        /// </returns>
//        public abstract bool AwaitTermination(TimeSpan timeSpan);
//        /// <summary> 
//        /// Returns <see lang="true"/> if all tasks have completed following shut down.
//        /// </summary>
//        /// <remarks>
//        /// Note that this will never return <see lang="true"/> unless
//        /// either <see cref="Spring.Threading.Execution.IExecutorService.Shutdown()"/> or 
//        /// <see cref="Spring.Threading.Execution.IExecutorService.ShutdownNow()"/> was called first.
//        /// </remarks>
//        /// <returns> <see lang="true"/> if all tasks have completed following shut down
//        /// </returns>
//        public abstract bool IsTerminated { get; }
//        /// <summary> 
//        /// Returns <see lang="true"/> if this executor has been shut down.
//        /// </summary>
//        /// <returns> 
//        /// Returns <see lang="true"/> if this executor has been shut down.
//        /// </returns>
//        public abstract bool IsShutdown { get; }

//        #endregion

//        /// <summary> 
//        /// Returns a <see cref="Spring.Threading.Future.IRunnableFuture{T}"/> for the given runnable and default
//        /// value.
//        /// </summary>
//        /// <param name="runnable">the runnable task being wrapped
//        /// </param>
//        /// <param name="defaultValue">the default value for the returned future
//        /// </param>
//        /// <returns>
//        /// A <see cref="Spring.Threading.Future.IRunnableFuture"/> which, when run, will run the
//        /// underlying runnable and which, as a <see cref="Spring.Threading.Future.IFuture"/>, will yield
//        /// the given value as its result and provide for cancellation of
//        /// the underlying task.
//        /// </returns>
//        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(IRunnable runnable, T defaultValue)
//        {
//            return new FutureTask<T>(runnable, defaultValue);
//        }

//        /// <summary> 
//        /// Returns a <see cref="Spring.Threading.Future.IRunnableFuture"/> for the given callable task.
//        /// </summary>
//        /// <param name="callable">the callable task being wrapped</param>
//        /// <returns> a <see cref="Spring.Threading.Future.IRunnableFuture"/> which when run will call the
//        /// underlying callable and which, as a <see cref="Spring.Threading.Future.IFuture"/>, will yield
//        /// the callable's result as its result and provide for
//        /// cancellation of the underlying task.
//        /// </returns>
//        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(ICallable<T> callable)
//        {
//            return new FutureTask<T>(callable);
//        }
		
//        #region IExecutorService Implementation
//        /// <summary> 
//        /// Submits a <see cref="Spring.Threading.IRunnable"/> task for execution and returns a
//        /// <see cref="Spring.Threading.Future.IFuture"/> 
//        /// representing that task. The <see cref="Spring.Threading.Future.IFuture.GetResult()"/> method will
//        /// return the given result upon successful completion.
//        /// </summary>
//        /// <param name="task">the task to submit</param>
//        /// <param name="result">the result to return</param>
//        /// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual IFuture<T> Submit<T>(IRunnable task, T result)
//        {
//            if (task == null)
//                throw new ArgumentNullException("task");
//            IRunnableFuture<T> runnableFuture = NewTaskFor(task, result);
//            Execute(runnableFuture);
//            return runnableFuture;
//        }
//        /// <summary> 
//        /// Submits a value-returning task for execution and returns a
//        /// Future representing the pending results of the task. The
//        /// <see cref="Spring.Threading.Future.IFuture.GetResult()"/> method will return the task's result upon
//        /// <b>successful</b> completion.
//        /// </summary>
//        /// <remarks> 
//        /// If you would like to immediately block waiting
//        /// for a task, you can use constructions of the form
//        /// <code>
//        ///		result = exec.Submit(aCallable).GetResult();
//        /// </code> 
//        /// <p/> 
//        /// Note: The <see cref="Spring.Threading.Execution.Executors"/> class includes a set of methods
//        /// that can convert some other common closure-like objects,
//        /// for example, <see cref="Spring.Threading.IRunnable"/> to
//        /// <see cref="Spring.Threading.ICallable"/> form so they can be submitted.
//        /// </remarks>
//        /// <param name="callable">the task to submit</param>
//        /// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual IFuture<T> Submit<T>(ICallable<T> callable)
//        {
//            if (callable == null)
//                throw new ArgumentNullException("callable");
//            IRunnableFuture<T> runnableFuture = NewTaskFor(callable);
//            Execute(runnableFuture);
//            return runnableFuture;
//        }
//        /// <summary> Submits a Runnable task for execution and returns a Future
//        /// representing that task. The Future's <see cref="M:Spring.Threading.Future.IFuture.GetResult"/> method will
//        /// return <see lang="null"/> upon successful completion.
//        /// </summary>
//        /// <param name="runnable">the task to submit</param>
//        /// <returns> a Future representing pending completion of the task</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual IFuture<T> Submit<T>(IRunnable runnable)
//        {
//            return Submit(runnable, default(T));
//        }
//        /// <summary> 
//        /// Executes the given tasks, returning the result
//        /// of one that has completed successfully (i.e., without throwing
//        /// an exception), if any do. 
//        /// </summary>
//        /// <remarks>
//        /// Upon normal or exceptional return, tasks that have not completed are cancelled.
//        /// The results of this method are undefined if the given
//        /// collection is modified while this operation is in progress.
//        /// </remarks>
//        /// <param name="tasks">the collection of tasks</param>
//        /// <returns> The result returned by one of the tasks.</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual T InvokeAny<T>(ICollection<ICallable<T>> tasks)
//        {
//            try
//            {
//                return doInvokeAny<T>(tasks, false, new TimeSpan(0));
//            }
//            catch (TimeoutException)
//            {
//                Debug.Assert(false);
//                return default(T);
//            }
//        }
//        /// <summary> Executes the given tasks, returning the result
//        /// of one that has completed successfully (i.e., without throwing
//        /// an exception), if any do before the given timeout elapses.
//        /// </summary>
//        /// <remarks>
//        /// Upon normal or exceptional return, tasks that have not
//        /// completed are cancelled.
//        /// The results of this method are undefined if the given
//        /// collection is modified while this operation is in progress.
//        /// </remarks>
//        /// <param name="tasks">the collection of tasks</param>
//        /// <param name="durationToWait">the time span to wait.</param> 
//        /// <returns> The result returned by one of the tasks.
//        /// </returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual T InvokeAny<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            return doInvokeAny(tasks, true, durationToWait);
//        }
//        /// <summary> 
//        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture"/>s holding
//        /// their status and results when all complete.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="Spring.Threading.Future.IFuture.IsDone"/>
//        /// is <see lang="true"/> for each element of the returned list.
//        /// Note that a <b>completed</b> task could have
//        /// terminated either normally or by throwing an exception.
//        /// The results of this method are undefined if the given
//        /// collection is modified while this operation is in progress.
//        /// </remarks>
//        /// <param name="tasks">the collection of tasks</param>
//        /// <returns> A list of Futures representing the tasks, in the same
//        /// sequential order as produced by the iterator for the given task
//        /// list, each of which has completed.
//        /// </returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        //TODO: Replace "ICollection" with ICallableCollection
//        public virtual IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks)
//        {
//            if (tasks == null)
//                throw new ArgumentNullException("tasks");
//            IList<IFuture<T>> futures = new List<IFuture<T>>(tasks.Count);
//            bool done = false;
//            try
//            {
//                foreach(ICallable<T> callable in tasks)
//                {
//                    IRunnableFuture<T> runnableFuture = NewTaskFor(callable);
//                    futures.Add(runnableFuture);
//                    Execute(runnableFuture);
//                }
//                foreach(IFuture<T> future in futures)
//                {
//                    if (!future.IsDone)
//                    {
//                        try
//                        {
//                            future.GetResult();
//                        }
//                        catch (CancellationException)
//                        {
//                        }
//                        catch (ExecutionException)
//                        {
//                        }
//                    }
//                }
//                done = true;
//                return futures;
//            }
//            finally
//            {
//                if (!done)
//                {
//                    foreach(IFuture<T> future in futures)
//                    {
//                        future.Cancel(true);
//                    }
//                }
//            }
//        }
//        /// <summary> 
//        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture"/>s holding
//        /// their status and results when all complete or the <paramref name="durationToWait"/> expires, whichever happens first.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="Spring.Threading.Future.IFuture.IsDone"/>
//        /// is <see lang="true"/> for each element of the returned list.
//        /// Note that a <b>completed</b> task could have
//        /// terminated either normally or by throwing an exception.
//        /// The results of this method are undefined if the given
//        /// collection is modified while this operation is in progress.
//        /// </remarks>
//        /// <param name="tasks">the collection of tasks</param>
//        /// <param name="durationToWait">the time span to wait.</param> 
//        /// <returns> A list of Futures representing the tasks, in the same
//        /// sequential order as produced by the iterator for the given
//        /// task list. If the operation did not time out, each task will
//        /// have completed. If it did time out, some of these tasks will
//        /// not have completed.
//        /// </returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        public virtual IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            if (tasks == null )
//                throw new ArgumentNullException("Tasks");
//            TimeSpan duration = durationToWait;
//            IList<IFuture<T>> futures = new List<IFuture<T>>(tasks.Count);
//            bool done = false;
//            try
//            {
//                foreach ( ICallable<T> callable in tasks ) 
//                {
//                    futures.Add(NewTaskFor(callable));
//                }

//                DateTime lastTime = DateTime.Now;

//                // Interleave time checks and calls to execute in case
//                // executor doesn't have any/much parallelism.
//                foreach ( IRunnable runnable in futures )
//                {
//                    Execute(runnable);

//                    duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
//                    lastTime = DateTime.Now;
//                    if (duration.Ticks <= 0)
//                        return futures;
//                }

//                foreach(IFuture<T> future in futures)
//                {
//                    if (!future.IsDone)
//                    {
//                        if (duration.Ticks <= 0)
//                            return futures;
//                        try
//                        {
//                            future.GetResult(duration);
//                        }
//                        catch (CancellationException)
//                        {
//                        }
//                        catch (ExecutionException)
//                        {
//                        }
//                        catch (TimeoutException)
//                        {
//                            return futures;
//                        }

//                        duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
//                        lastTime = DateTime.Now;
//                    }
//                }
//                done = true;
//                return futures;
//            }
//            finally
//            {
//                if (!done)
//                {

//                    foreach(IFuture<T> future in futures)
//                    {
//                        future.Cancel(true);
//                    }
//                }
//            }
//        }

//        #endregion

//        private T doInvokeAny<T>(ICollection<ICallable<T>> tasks, bool timed, TimeSpan durationToWait)
//        {
//            if (tasks == null)
//                throw new ArgumentNullException("tasks");
//            int numberOfTasks = tasks.Count;
//            if (numberOfTasks == 0)
//                throw new ArgumentException("No tasks passed in.");
//            IList<IFuture<T>> futures = new List<IFuture<T>>(numberOfTasks);
//            ExecutorCompletionService<T> ecs = new ExecutorCompletionService<T>(this);
//            TimeSpan duration = durationToWait;

//            // For efficiency, especially in executors with limited
//            // parallelism, check to see if previously submitted tasks are
//            // done before submitting more of them. This interleaving
//            // plus the exception mechanics account for messiness of main
//            // loop.

//            try
//            {
//                // Record exceptions so that if we fail to obtain any
//                // result, we can throw the last exception we got.
//                ExecutionException ee = null;
//                DateTime lastTime = (timed) ? DateTime.Now : new DateTime(0);
//                IEnumerator<ICallable<T>> it = tasks.GetEnumerator();
//                it.MoveNext();
//                futures.Add(ecs.Submit(it.Current ));
//                --numberOfTasks;
//                int active = 1;

//                for (;; )
//                {
//                    IFuture<T> f = ecs.Poll();
//                    if (f == null)
//                    {
//                        if (numberOfTasks > 0)
//                        {
//                            --numberOfTasks;
//                            it.MoveNext();
//                            futures.Add(ecs.Submit((ICallable<T>) it.Current));
//                            ++active;
//                        }
//                        else if (active == 0)
//                            break;
//                        else if (timed)
//                        {
//                            f = ecs.Poll(duration);
//                            if (f == null)
//                                throw new TimeoutException();
//                            duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
//                            lastTime = DateTime.Now;
//                        }
//                        else
//                            f = ecs.Take();
//                    }
//                    if (f != null)
//                    {
//                        --active;
//                        try
//                        {
//                            return f.GetResult();
//                        }
//                        catch (ThreadInterruptedException ie)
//                        {
//                            throw ie;
//                        }
//                        catch (ExecutionException eex)
//                        {
//                            ee = eex;
//                        }
//                        catch (SystemException rex)
//                        {
//                            ee = new ExecutionException(rex);
//                        }
//                    }
//                }

//                if (ee == null)
//                    ee = new ExecutionException();
//                throw ee;
//            }
//            finally
//            {
//                foreach ( IFuture<T> future in futures)
//                {
//                    future.Cancel(true);
//                }
//            }
//        }

//    }
//}