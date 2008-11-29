//using System;
//using System.Collections.Generic;
//using Spring.Threading.Future;

//namespace Spring.Threading.Execution
//{
//    /// <summary> 
//    /// An <see cref="Spring.Threading.IExecutor"/> that provides methods to manage termination and
//    /// methods that can produce a <see cref="Spring.Threading.Future.IFuture{T}"/> for tracking progress of
//    /// one or more asynchronous tasks.
//    /// </summary>
//    /// <remarks>
//    /// An <see cref="Spring.Threading.Execution.IExecutorService"/> can be shut down, which will cause it
//    /// to stop accepting new tasks.  After being shut down, the executor
//    /// will eventually terminate, at which point no tasks are actively
//    /// executing, no tasks are awaiting execution, and no new tasks can be
//    /// submitted.
//    /// 
//    /// <p/> 
//    /// Method <see cref="Spring.Threading.Execution.IExecutorService.Submit{T}(ICallable{T})"/> extends base method 
//    /// <see cref="Spring.Threading.IExecutor.Execute"/> by creating and returning a <see cref="Spring.Threading.Future.IFuture{T}"/> that
//    /// can be used to cancel execution and/or wait for completion.
//    /// Methods <see cref="Spring.Threading.Execution.IExecutorService.InvokeAny{T}(ICollection{ICallable{T}})"/> and <see cref="Spring.Threading.Execution.IExecutorService.InvokeAll{T}(ICollection{ICallable{T}})"/>
//    /// perform the most commonly useful forms of bulk execution, executing a collection of
//    /// tasks and then waiting for at least one, or all, to
//    /// complete. (Class <see cref="Spring.Threading.Execution.ExecutorCompletionService{T}"/> can be used to
//    /// write customized variants of these methods.)
//    /// 
//    /// <p/>
//    /// The <see cref="Spring.Threading.Execution.Executors"/> class provides factory methods for the
//    /// executor services provided in this package.
//    /// </remarks>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio (.NET)</author>
//    public interface IExecutorService : IExecutor
//    {
//        /// <summary> 
//        /// Returns <see lang="true"/> if this executor has been shut down.
//        /// </summary>
//        /// <returns> 
//        /// Returns <see lang="true"/> if this executor has been shut down.
//        /// </returns>
//        bool IsShutdown { get; }

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
//        bool IsTerminated { get; }

//        /// <summary> 
//        /// Initiates an orderly shutdown in which previously submitted
//        /// tasks are executed, but no new tasks will be
//        /// accepted. Invocation has no additional effect if already shut
//        /// down.
//        /// </summary>
//        void Shutdown();

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
//        IList<IRunnable> ShutdownNow();

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
//        bool AwaitTermination(TimeSpan timeSpan);

//        /// <summary> 
//        /// Submits a value-returning task for execution and returns a
//        /// Future representing the pending results of the task. The
//        /// <see cref="Spring.Threading.Future.IFuture{T}.GetResult()"/> method will return the task's result upon
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
//        /// <see cref="Spring.Threading.ICallable{T}"/> form so they can be submitted.
//        /// </remarks>
//        /// <param name="task">the task to submit</param>
//        /// <returns> a <see cref="Spring.Threading.Future.IFuture{T}"/> representing pending completion of the task</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        IFuture<T> Submit<T>(ICallable<T> task);

//        /// <summary> 
//        /// Submits a <see cref="Spring.Threading.IRunnable"/> task for execution and returns a
//        /// <see cref="Spring.Threading.Future.IFuture{T}"/> 
//        /// representing that task. The <see cref="Spring.Threading.Future.IFuture{T}.GetResult()"/> method will
//        /// return the given result upon successful completion.
//        /// </summary>
//        /// <param name="task">the task to submit</param>
//        /// <param name="result">the result to return</param>
//        /// <returns> a <see cref="Spring.Threading.Future.IFuture{T}"/> representing pending completion of the task</returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        IFuture<T> Submit<T>(IRunnable task, T result);

//        /// <summary> Submits a Runnable task for execution and returns a Future
//        /// representing that task. The Future's <see cref="Spring.Threading.Future.IFuture{T}.GetResult"/> method will
//        /// return <see lang="null"/> upon successful completion.
//        /// </summary>
//        /// <param name="task">the task to submit
//        /// </param>
//        /// <returns> a Future representing pending completion of the task
//        /// </returns>
//        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
//        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
//        IFuture<T> Submit<T>(IRunnable task);

//        /// <summary> 
//        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture{T}"/>s holding
//        /// their status and results when all complete.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="Spring.Threading.Future.IFuture{T}.IsDone"/>
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
//        IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks);

//        /// <summary> 
//        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture{T}"/>s holding
//        /// their status and results when all complete or the <paramref name="durationToWait"/> expires, whichever happens first.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="Spring.Threading.Future.IFuture{T}.IsDone"/>
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
//        IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait);

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
//        T InvokeAny<T>(ICollection<ICallable<T>> tasks);

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
//        T InvokeAny<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait);
//    }
//}