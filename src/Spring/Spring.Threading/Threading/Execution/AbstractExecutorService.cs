#region License

/*
* Copyright (C) 2002-2009 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    /// <summary> 
    /// Provides default implementations of <see cref="Spring.Threading.Execution.IExecutorService"/>
    /// execution methods. 
    /// </summary>
    /// <remarks> 
    /// This class implements the <see cref="Spring.Threading.Execution.IExecutorService"/> methods using a
    /// <see cref="Spring.Threading.Future.IRunnableFuture"/> returned by NewTaskFor
    /// , which defaults to the <see cref="Spring.Threading.Future.FutureTask"/> class provided in this package.  
    /// <p/>
    /// For example, the implementation of <see cref="Spring.Threading.Execution.IExecutorService.Submit(IRunnable)"/> creates an
    /// associated <see cref="Spring.Threading.Future.IRunnableFuture"/> that is executed and
    /// returned. Subclasses may override the NewTaskFor methods
    /// to return <see cref="Spring.Threading.Future.IRunnableFuture"/> implementations other than
    /// <see cref="Spring.Threading.Future.FutureTask"/>.
    /// 
    /// <p/> 
    /// <b>Extension example</b>. 
    /// Here is a sketch of a class
    /// that customizes <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> to use
    /// a custom Task class instead of the default <see cref="Spring.Threading.Future.FutureTask"/>:
    /// <code>
    /// public class CustomThreadPoolExecutor : ThreadPoolExecutor {
    ///		class CustomTask : IRunnableFuture {...}
    /// 
    ///		protected IRunnableFuture newTaskFor(ICallable c) {
    ///			return new CustomTask(c);
    /// 	}
    ///		protected IRunnableFuture newTaskFor(IRunnable r) {
    /// 		return new CustomTask(r);
    /// 	}
    /// 	// ... add constructors, etc.
    /// }
    /// </code>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio(.NET)</author>
    /// <author>Kenneth Xu</author>
    public abstract class AbstractExecutorService : IExecutorService
    {
        #region Private Static Fields

        private static readonly TimeSpan NoTime = new TimeSpan(0);

        #endregion

        private readonly Converter<object, IRunnableFuture> Callable2Future;

        /// <summary>
        /// Default constructor for subclasses
        /// </summary>
        protected AbstractExecutorService()
        {
            Callable2Future = delegate(object callable) { return NewTaskFor((ICallable) callable); };
        }

        #region Abstract Methods

        /// <summary> 
        /// Initiates an orderly shutdown in which previously submitted
        /// tasks are executed, but no new tasks will be
        /// accepted. Invocation has no additional effect if already shut
        /// down.
        /// </summary>
        public abstract void Shutdown();

        /// <summary> 
        /// Attempts to stop all actively executing tasks, halts the
        /// processing of waiting tasks, and returns a list of the tasks that were
        /// awaiting execution.
        /// </summary>
        /// <remarks> 
        /// There are no guarantees beyond best-effort attempts to stop
        /// processing actively executing tasks.  For example, typical
        /// implementations will cancel via <see cref="System.Threading.Thread.Interrupt()"/>, so if any
        /// tasks mask or fail to respond to interrupts, they may never terminate.
        /// </remarks>
        /// <returns> list of tasks that never commenced execution</returns>
        public abstract IList<IRunnable> ShutdownNow();

        /// <summary> 
        /// Executes the given command at some time in the future.
        /// </summary>
        /// <remarks>
        /// The command may execute in a new thread, in a pooled thread, or in the calling
        /// thread, at the discretion of the <see cref="Spring.Threading.IExecutor"/> implementation.
        /// </remarks>
        /// <param name="runnable">the runnable task</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>	
        public abstract void Execute(IRunnable runnable);

        /// <summary> 
        /// Blocks until all tasks have completed execution after a shutdown
        /// request, or the timeout occurs, or the current thread is
        /// interrupted, whichever happens first. 
        /// </summary>
        /// <param name="timeSpan">the time span to wait.
        /// </param>
        /// <returns> <see lang="true"/> if this executor terminated and <see lang="false"/>
        /// if the timeout elapsed before termination
        /// </returns>
        public abstract bool AwaitTermination(TimeSpan timeSpan);

        /// <summary> 
        /// Returns <see lang="true"/> if all tasks have completed following shut down.
        /// </summary>
        /// <remarks>
        /// Note that this will never return <see lang="true"/> unless
        /// either <see cref="Spring.Threading.Execution.IExecutorService.Shutdown()"/> or 
        /// <see cref="Spring.Threading.Execution.IExecutorService.ShutdownNow()"/> was called first.
        /// </remarks>
        /// <returns> <see lang="true"/> if all tasks have completed following shut down
        /// </returns>
        public abstract bool IsTerminated { get; }

        /// <summary> 
        /// Returns <see lang="true"/> if this executor has been shut down.
        /// </summary>
        /// <returns> 
        /// Returns <see lang="true"/> if this executor has been shut down.
        /// </returns>
        public abstract bool IsShutdown { get; }

        #endregion

        /// <summary> 
        /// Returns a <see cref="Spring.Threading.Future.IRunnableFuture"/> for the given runnable and default
        /// value.
        /// </summary>
        /// <param name="runnable">the runnable task being wrapped
        /// </param>
        /// <param name="defaultValue">the default value for the returned future
        /// </param>
        /// <returns>
        /// A <see cref="Spring.Threading.Future.IRunnableFuture"/> which, when run, will run the
        /// underlying runnable and which, as a <see cref="Spring.Threading.Future.IFuture"/>, will yield
        /// the given value as its result and provide for cancellation of
        /// the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture NewTaskFor(IRunnable runnable, object defaultValue)
        {
            return new FutureTask(runnable, defaultValue);
        }

        /// <summary> 
        /// Returns a <see cref="Spring.Threading.Future.IRunnableFuture"/> for the given callable task.
        /// </summary>
        /// <param name="callable">the callable task being wrapped</param>
        /// <returns> a <see cref="Spring.Threading.Future.IRunnableFuture"/> which when run will call the
        /// underlying callable and which, as a <see cref="Spring.Threading.Future.IFuture"/>, will yield
        /// the callable's result as its result and provide for
        /// cancellation of the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture NewTaskFor(ICallable callable)
        {
            return new FutureTask(callable);
        }

        #region .NET 2.0

        //        /// <summary> 
//        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
//        /// <paramref name="call"/> delegate.
//        /// </summary>
//        /// <param name="call">
//        /// The <see cref="Call{T}"/> delegate being wrapped.
//        /// </param>
//        /// <returns>
//        /// An <see cref="IRunnableFuture{T}"/> which when run will call the
//        /// underlying <paramref name="call"/> delegate and which, as a 
//        /// <see cref="IFuture{T}"/>, will yield the result of <c>call</c>as 
//        /// its result and provide for cancellation of the underlying task.
//        /// </returns>
//        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(Call<T> call)
//        {
//            return new FutureTask<T>(call);
//        }
//
//        /// <summary> 
//        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
//        /// <paramref name="callable"/> task.
//        /// </summary>
//        /// <param name="callable">The callable task being wrapped.</param>
//        /// <returns>
//        /// An <see cref="IRunnableFuture{T}"/> which when run will call the
//        /// underlying <paramref name="callable"/> and which, as a 
//        /// <see cref="IFuture{T}"/>, will yield the callable's result as its 
//        /// result and provide for cancellation of the underlying task.
//        /// </returns>
//        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(ICallable<T> callable)
//        {
//            return new FutureTask<T>(callable);
//        }
//
//        private IFuture SubmitCall<T>(Call<T> call)
//        {
//            IRunnableFuture<T> runnableFuture = NewTaskFor(call);
//            Execute(runnableFuture);
//            return runnableFuture;
//        }
//
//        private Converter<object, IRunnableFuture> GenericCallable2Future<T>()
//        {
//            return delegate(object callable) { return NewTaskFor((ICallable<T>)callable); };
//        }
//
//        private Converter<object, IRunnableFuture> Call2Future<T>()
//        {
//            return delegate(object call) { return NewTaskFor((Call<T>)call); };
//        }
//
        //#endif

        #endregion

        #region IExecutorImplementation

        /// <summary> 
        /// Executes the given task at some time in the future.
        /// </summary>
        /// <remarks>
        /// The task may execute in a new thread, in a pooled thread, or in the calling
        /// thread, at the discretion of the <see cref="IExecutor"/> implementation.
        /// </remarks>
        /// <param name="task">The task to be executed.</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">
        /// If the task cannot be accepted for execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="task"/> is <c>null</c>
        /// </exception>
        public virtual void Execute(Task task)
        {
            Execute(Executors.CreateRunnable(task));
        }

        #endregion

        #region IExecutorService Implementation

        /// <summary> 
        /// Submits a <see cref="Spring.Threading.IRunnable"/> task for execution and returns a
        /// <see cref="Spring.Threading.Future.IFuture"/> 
        /// representing that task. The <see cref="Spring.Threading.Future.IFuture.GetResult()"/> method will
        /// return the given result upon successful completion.
        /// </summary>
        /// <param name="task">the task to submit</param>
        /// <param name="result">the result to return</param>
        /// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual IFuture Submit(IRunnable task, object result)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            var runnableFuture = NewTaskFor(task, result);
            Execute(runnableFuture);
            return runnableFuture;
        }

        /// <summary> 
        /// Submits a value-returning task for execution and returns a
        /// Future representing the pending results of the task. The
        /// <see cref="Spring.Threading.Future.IFuture.GetResult()"/> method will return the task's result upon
        /// <b>successful</b> completion.
        /// </summary>
        /// <remarks> 
        /// If you would like to immediately block waiting
        /// for a task, you can use constructions of the form
        /// <code>
        ///		result = exec.Submit(aCallable).GetResult();
        /// </code> 
        /// <p/> 
        /// Note: The <see cref="Spring.Threading.Execution.Executors"/> class includes a set of methods
        /// that can convert some other common closure-like objects,
        /// for example, <see cref="Spring.Threading.IRunnable"/> to
        /// <see cref="Spring.Threading.ICallable"/> form so they can be submitted.
        /// </remarks>
        /// <param name="callable">the task to submit</param>
        /// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual IFuture Submit(ICallable callable)
        {
            if (callable == null)
                throw new ArgumentNullException("callable");
            var runnableFuture = NewTaskFor(callable);
            Execute(runnableFuture);
            return runnableFuture;
        }

        /// <summary> Submits a Runnable task for execution and returns a Future
        /// representing that task. The Future's <see cref="M:Spring.Threading.Future.IFuture.GetResult"/> method will
        /// return <see lang="null"/> upon successful completion.
        /// </summary>
        /// <param name="runnable">the task to submit</param>
        /// <returns> a Future representing pending completion of the task</returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual IFuture Submit(IRunnable runnable)
        {
            return Submit(runnable, null);
        }

        /// <summary> 
        /// Submits a delegate <see cref="Task"/> for execution and returns an
        /// <see cref="IFuture"/> representing that <paramref name="task"/>. The 
        /// <see cref="IFuture.GetResult()"/> method will return the given 
        /// <paramref name="result"/> upon successful completion.
        /// </summary>
        /// <param name="task">The task to submit.</param>
        /// <param name="result">The result to return.</param>
        /// <returns>
        /// An <see cref="IFuture"/> representing pending completion of the 
        /// <paramref name="task"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="task"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="task"/> is <c>null</c>
        /// </exception>
        public virtual IFuture Submit(Task task, object result)
        {
            return Submit(Executors.CreateCallable(task, result));
        }

        /// <summary> 
        /// Submits a delegate <see cref="Task"/> for execution and returns an
        /// <see cref="IFuture"/> representing that <paramref name="task"/>. The 
        /// <see cref="IFuture.GetResult()"/> method will return <c>null</c>.
        /// </summary>
        /// <param name="task">The task to submit.</param>
        /// <returns>
        /// An <see cref="IFuture"/> representing pending completion of the 
        /// <paramref name="task"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="task"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="task"/> is <c>null</c>
        /// </exception>
        public virtual IFuture Submit(Task task)
        {
            return Submit(Executors.CreateCallable(task, null));
        }

//
//
//#if NET_2_0
//
//	    /// <summary> 
//	    /// Submits a delegate <see cref="Call{T}"/> for execution and returns a
//	    /// <see cref="IFuture{T}"/> representing that <paramref name="call"/>. 
//	    /// The <see cref="IFuture{T}.GetResult()"/> method will return the 
//	    /// result of <paramref name="call"/><c>()</c> upon successful completion.
//	    /// </summary>
//	    /// <param name="call">The task to submit.</param>
//	    /// <returns>
//	    /// An <see cref="IFuture{T}"/> representing pending completion of the
//	    /// <paramref name="call"/>.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the <paramref name="call"/> cannot be accepted for execution.
//	    /// </exception>
//	    /// <exception cref="ArgumentNullException">
//	    /// If the <paramref name="call"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IFuture Submit<T>(Call<T> call)
//        {
//            if (call == null) throw new ArgumentNullException("call");
//            return SubmitCall(call);
//        }
//
//
//	    /// <summary> 
//	    /// Submits a delegate <see cref="Task"/> for execution and returns a
//	    /// <see cref="IFuture{T}"/> representing that <paramref name="task"/>. 
//	    /// The <see cref="IFuture{T}.GetResult()"/> method will return the 
//	    /// given <paramref name="result"/> upon successful completion.
//	    /// </summary>
//	    /// <param name="task">The task to submit.</param>
//	    /// <param name="result">The result to return.</param>
//	    /// <returns>
//	    /// An <see cref="IFuture{T}"/> representing pending completion of the
//	    /// <paramref name="task"/>.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the <paramref name="task"/> cannot be accepted for execution.
//	    /// </exception>
//	    /// <exception cref="ArgumentNullException">
//	    /// If the <paramref name="task"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IFuture Submit<T>(Task task, T result)
//        {
//            return SubmitCall(Executors.CreateCall(task, result));
//        }
//
//	    /// <summary> 
//	    /// Submits a <see cref="ICallable{T}"/> for execution and returns a
//	    /// <see cref="IFuture{T}"/> representing that <paramref name="callable"/>. 
//	    /// The <see cref="IFuture{T}.GetResult()"/> method will return the 
//	    /// result of <see cref="ICallable{T}.Call"/> upon successful completion.
//	    /// </summary>
//	    /// <param name="callable">The task to submit.</param>
//	    /// <returns>
//	    /// An <see cref="IFuture{T}"/> representing pending completion of the
//	    /// <paramref name="callable"/>.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the <paramref name="callable"/> cannot be accepted for execution.
//	    /// </exception>
//	    /// <exception cref="ArgumentNullException">
//	    /// If the <paramref name="callable"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IFuture Submit<T>(ICallable<T> callable)
//        {
//            return SubmitCall(Executors.CreateCall(callable));
//        }
//
//	    /// <summary> 
//	    /// Submits a <see cref="IRunnable"/> task for execution and returns a
//	    /// <see cref="IFuture{T}"/> representing that <paramref name="task"/>. 
//	    /// The <see cref="IFuture{T}.GetResult()"/> method will return the 
//	    /// given <paramref name="result"/> upon successful completion.
//	    /// </summary>
//	    /// <param name="task">The task to submit.</param>
//	    /// <param name="result">The result to return.</param>
//	    /// <returns>
//	    /// An <see cref="IFuture{T}"/> representing pending completion of the
//	    /// <paramref name="task"/>.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the <paramref name="task"/> cannot be accepted for execution.
//	    /// </exception>
//	    /// <exception cref="ArgumentNullException">
//	    /// If the <paramref name="task"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IFuture Submit<T>(IRunnable task, T result)
//        {
//            return SubmitCall(Executors.CreateCall(task, result));
//        }
//
//#endif
        /// <summary> 
        /// Executes the given tasks, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do. 
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, tasks that have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// collection is modified while this operation is in progress.
        /// </remarks>
        /// <param name="tasks">the collection of tasks</param>
        /// <returns> The result returned by one of the tasks.</returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual Object InvokeAny(ICollection tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            try
            {
                return doInvokeAny(tasks, tasks.Count, false, NoTime);
            }
            catch (TimeoutException)
            {
                Debug.Assert(false);
                return null;
            }
        }

//#if NET_2_0
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do. 
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable"/> objects.
//	    /// </param>
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual object InvokeAny(ICollection<ICallable> tasks)
//        {
//            return doInvokeAny(tasks, tasks.Count, false, NoTime);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do. 
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(ICollection<ICallable<T>> tasks)
//        {
//            object result = doInvokeAny(tasks, tasks.Count, false, NoTime, GenericCallable2Future<T>());
//            return (T)result;
//        }

//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do. 
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(ICollection<Call<T>> tasks)
//        {
//            object result = doInvokeAny(tasks, tasks.Count, false, NoTime, Call2Future<T>());
//            return (T)result;
//        }

//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do. 
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(IEnumerable<ICallable<T>> tasks)
//        {
//            object result = doInvokeAny(tasks, 0, false, NoTime, GenericCallable2Future<T>());
//            return (T)result;
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do. 
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(IEnumerable<Call<T>> tasks)
//        {
//            object result = doInvokeAny(tasks, 0, false, NoTime, Call2Future<T>());
//            return (T)result;
//        }
//#endif

        /// <summary> Executes the given tasks, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do before the given timeout elapses.
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, tasks that have not
        /// completed are cancelled.
        /// The results of this method are undefined if the given
        /// collection is modified while this operation is in progress.
        /// </remarks>
        /// <param name="tasks">the collection of tasks</param>
        /// <param name="durationToWait">the time span to wait.</param> 
        /// <returns> The result returned by one of the tasks.
        /// </returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual Object InvokeAny(ICollection tasks, TimeSpan durationToWait)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }

            return doInvokeAny(tasks, tasks.Count, true, durationToWait);
        }

//
//#if NET_2_0
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do before the given 
//	    /// <paramref name="durationToWait"/> elapses.
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual object InvokeAny(ICollection<ICallable> tasks, TimeSpan durationToWait)
//        {
//            return doInvokeAny(tasks, tasks.Count, true, durationToWait);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do before the given 
//	    /// <paramref name="durationToWait"/> elapses.
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            object result = doInvokeAny(tasks, tasks.Count, true, durationToWait, GenericCallable2Future<T>());
//            return (T) result;
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do before the given 
//	    /// <paramref name="durationToWait"/> elapses.
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(ICollection<Call<T>> tasks, TimeSpan durationToWait)
//        {
//            object result = doInvokeAny(tasks, tasks.Count, true, durationToWait, Call2Future<T>());
//            return (T)result;
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do before the given 
//	    /// <paramref name="durationToWait"/> elapses.
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(IEnumerable<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            object result = doInvokeAny(tasks, 0, true, durationToWait, GenericCallable2Future<T>());
//            return (T)result;
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning the result
//	    /// of one that has completed successfully (i.e., without throwing
//	    /// an exception), if any do before the given 
//	    /// <paramref name="durationToWait"/> elapses.
//	    /// </summary>
//	    /// <remarks>
//	    /// Upon normal or exceptional return, <paramref name="tasks"/> that 
//	    /// have not completed are cancelled.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>The result returned by one of the tasks.</returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual T InvokeAny<T>(IEnumerable<Call<T>> tasks, TimeSpan durationToWait)
//        {
//            object result = doInvokeAny(tasks, 0, true, durationToWait, Call2Future<T>());
//            return (T)result;
//        }
//
//#endif
//
//#if NET_2_0
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable"/> objects.
//	    /// </param>
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list, each of which has completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll(ICollection<ICallable> tasks)
//        {
//            return InvokeAll(tasks, tasks.Count);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
//	    /// holding their status and results when all complete.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <returns>
//	    /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list, each of which has completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(ICollection<ICallable<T>> tasks)
//        {
//            List<IFuture> result = InvokeAll(tasks, tasks.Count, GenericCallable2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
//	    /// holding their status and results when all complete.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// </remarks>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <returns>
//	    /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list, each of which has completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(ICollection<Call<T>> tasks)
//        {
//            List<IFuture> result = InvokeAll(tasks, tasks.Count, Call2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
//	    /// holding their status and results when all complete.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="IEnumerable{T}">enumeration</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <returns>
//	    /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list, each of which has completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(IEnumerable<ICallable<T>> tasks)
//        {
//            List<IFuture> result = InvokeAll(tasks, 0, GenericCallable2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
//	    /// holding their status and results when all complete.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="IEnumerable{T}">enumeration</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <returns>
//	    /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list, each of which has completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(IEnumerable<Call<T>> tasks)
//        {
//            List<IFuture> result = InvokeAll(tasks, 0, Call2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//#endif

        /// <summary> 
        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture"/>s holding
        /// their status and results when all complete.
        /// </summary>
        /// <remarks>
        /// <see cref="Spring.Threading.Future.IFuture.IsDone"/>
        /// is <see lang="true"/> for each element of the returned list.
        /// Note that a <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// collection is modified while this operation is in progress.
        /// </remarks>
        /// <param name="tasks">the collection of tasks</param>
        /// <returns> A list of Futures representing the tasks, in the same
        /// sequential order as produced by the iterator for the given task
        /// list, each of which has completed.
        /// </returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        //TODO: Replace "ICollection" with ICallableCollection
        public virtual IList InvokeAll(ICollection tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            return InvokeAll(tasks, tasks.Count);
        }

//
//#if NET_2_0
//        private List<IFuture> InvokeAll(IEnumerable tasks, int count)
//        {
//            return InvokeAll(tasks, count, Callable2Future);
//        }
//
//        private List<IFuture> InvokeAll(IEnumerable tasks, int count, Converter<object, IRunnableFuture> converter)
//#else
        private IList InvokeAll(IEnumerable tasks, int count)
//#endif
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
//#if NET_2_0
//			List<IFuture> futures = count > 0 ?  new List<IFuture>(count) : new List<IFuture>();
//#else
            IList futures = new ArrayList(count);
//#endif
            bool done = false;
            try
            {
//#if NET_2_0
//                foreach (object task in tasks)
//                {
//                    IRunnableFuture runnableFuture = converter(task);
//#else
                foreach (ICallable callable in tasks)
                {
                    IRunnableFuture runnableFuture = NewTaskFor(callable);
//#endif
                    futures.Add(runnableFuture);
                    Execute(runnableFuture);
                }
                foreach (IFuture future in futures)
                {
                    if (!future.IsDone)
                    {
                        try
                        {
                            future.GetResult();
                        }
                        catch (CancellationException)
                        {
                        }
                        catch (ExecutionException)
                        {
                        }
                    }
                }
                done = true;
                return futures;
            }
            finally
            {
                if (!done)
                {
                    foreach (IFuture future in futures)
                    {
                        future.Cancel(true);
                    }
                }
            }
        }

//
//#if NET_2_0
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete or the
//	    /// <paramref name="durationToWait"/> expires, whichever happens
//	    /// first.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list. If the operation did not time out, each task will
//	    /// have completed. If it did time out, some of these tasks will
//	    /// not have completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll(ICollection<ICallable> tasks, TimeSpan durationToWait)
//        {
//            return InvokeAll(tasks, tasks.Count, durationToWait);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete or the
//	    /// <paramref name="durationToWait"/> expires, whichever happens
//	    /// first.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list. If the operation did not time out, each task will
//	    /// have completed. If it did time out, some of these tasks will
//	    /// not have completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(ICollection<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            List<IFuture> result = InvokeAll(tasks, tasks.Count, durationToWait, GenericCallable2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete or the
//	    /// <paramref name="durationToWait"/> expires, whichever happens
//	    /// first.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// collection is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="ICollection{T}">collection</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list. If the operation did not time out, each task will
//	    /// have completed. If it did time out, some of these tasks will
//	    /// not have completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(ICollection<Call<T>> tasks, TimeSpan durationToWait)
//        {
//            List<IFuture> result = InvokeAll(tasks, tasks.Count, durationToWait, Call2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete or the
//	    /// <paramref name="durationToWait"/> expires, whichever happens
//	    /// first.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="IEnumerable{T}">enumeration</see> of 
//	    /// <see cref="ICallable{T}"/> objects.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list. If the operation did not time out, each task will
//	    /// have completed. If it did time out, some of these tasks will
//	    /// not have completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(IEnumerable<ICallable<T>> tasks, TimeSpan durationToWait)
//        {
//            List<IFuture> result = InvokeAll(tasks, 0, durationToWait, GenericCallable2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//	    /// <summary> 
//	    /// Executes the given <paramref name="tasks"/>, returning a 
//	    /// <see cref="IList{T}">list</see> of <see cref="IFuture"/>s 
//	    /// holding their status and results when all complete or the
//	    /// <paramref name="durationToWait"/> expires, whichever happens
//	    /// first.
//	    /// </summary>
//	    /// <remarks>
//	    /// <para>
//	    /// <see cref="IFuture.IsDone"/> is <c>true</c> for each element of 
//	    /// the returned list.
//	    /// </para>
//	    /// <para>
//	    /// Note: 
//	    /// A <b>completed</b> task could have
//	    /// terminated either normally or by throwing an exception.
//	    /// The results of this method are undefined if the given
//	    /// enumerable is modified while this operation is in progress.
//	    /// </para>
//	    /// </remarks>
//	    /// <typeparam name="T">
//	    /// The type of the result to be returned by <see cref="IFuture{T}"/>.
//	    /// </typeparam>
//	    /// <param name="tasks">
//	    /// The <see cref="IEnumerable{T}">enumeration</see> of 
//	    /// <see cref="Call{T}"/> delegates.
//	    /// </param>
//	    /// <param name="durationToWait">The time span to wait.</param> 
//	    /// <returns>
//	    /// A list of <see cref="IFuture"/>s representing the tasks, in the 
//	    /// same sequential order as produced by the iterator for the given 
//	    /// task list. If the operation did not time out, each task will
//	    /// have completed. If it did time out, some of these tasks will
//	    /// not have completed.
//	    /// </returns>
//	    /// <exception cref="RejectedExecutionException">
//	    /// If the any of the <paramref name="tasks"/> cannot be accepted for 
//	    /// execution.
//	    /// </exception>
//	    /// <exception cref="System.ArgumentNullException">
//	    /// If the <paramref name="tasks"/> is <c>null</c>.
//	    /// </exception>
//	    public virtual IList<IFuture> InvokeAll<T>(IEnumerable<Call<T>> tasks, TimeSpan durationToWait)
//        {
//            List<IFuture> result = InvokeAll(tasks, 0, durationToWait, Call2Future<T>());
//            return new DownCastList<IFuture, IFuture>(result);
//        }
//
//
//#endif

        /// <summary> 
        /// Executes the given tasks, returning a list of <see cref="Spring.Threading.Future.IFuture"/>s holding
        /// their status and results when all complete or the <paramref name="durationToWait"/> expires, whichever happens first.
        /// </summary>
        /// <remarks>
        /// <see cref="Spring.Threading.Future.IFuture.IsDone"/>
        /// is <see lang="true"/> for each element of the returned list.
        /// Note that a <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// collection is modified while this operation is in progress.
        /// </remarks>
        /// <param name="tasks">the collection of tasks</param>
        /// <param name="durationToWait">the time span to wait.</param> 
        /// <returns> A list of Futures representing the tasks, in the same
        /// sequential order as produced by the iterator for the given
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual IList InvokeAll(ICollection tasks, TimeSpan durationToWait)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            return InvokeAll(tasks, tasks.Count, durationToWait);
        }

//#if NET_2_0
//        private List<IFuture> InvokeAll(IEnumerable tasks, int count, TimeSpan durationToWait)
//        {
//            return InvokeAll(tasks, count, durationToWait, Callable2Future);
//        }
//
//        private List<IFuture> InvokeAll(IEnumerable tasks, int count, TimeSpan durationToWait, Converter<object, IRunnableFuture> converter)
//#else
        private IList InvokeAll(IEnumerable tasks, int count, TimeSpan durationToWait)
//#endif
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            TimeSpan duration = durationToWait;
//#if NET_2_0
//            List<IFuture> futures = count > 0 ? new List<IFuture>(count) : new List<IFuture>();
//#else
			IList futures = new ArrayList(count);
//#endif
            bool done = false;
            try
            {
//#if NET_2_0
//                foreach (object task in tasks)
//                {
//                    futures.Add(converter(task));
//                }
//#else
				foreach ( ICallable callable in tasks)
				{
					futures.Add(NewTaskFor(callable));
                }
//#endif

                DateTime lastTime = DateTime.Now;

                // Interleave time checks and calls to execute in case
                // executor doesn't have any/much parallelism.
                foreach (IRunnable runnable in futures)
                {
                    Execute(runnable);

                    duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                    lastTime = DateTime.Now;
                    if (duration.Ticks <= 0)
                        return futures;
                }

                foreach (IFuture future in futures)
                {
                    if (!future.IsDone)
                    {
                        if (duration.Ticks <= 0)
                            return futures;
                        try
                        {
                            future.GetResult(duration);
                        }
                        catch (CancellationException)
                        {
                        }
                        catch (ExecutionException)
                        {
                        }
                        catch (TimeoutException)
                        {
                            return futures;
                        }

                        duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                        lastTime = DateTime.Now;
                    }
                }
                done = true;
                return futures;
            }
            finally
            {
                if (!done)
                {
                    foreach (IFuture future in futures)
                    {
                        future.Cancel(true);
                    }
                }
            }
        }

        #endregion

//#if NET_2_0
//        private object doInvokeAny(IEnumerable tasks, int count, bool timed, TimeSpan durationToWait)
//        {
//            return doInvokeAny(tasks, count, timed, durationToWait, Callable2Future);
//        }
//
//
//        private object doInvokeAny(IEnumerable tasks, int count, bool timed, TimeSpan durationToWait, Converter<object, IRunnableFuture> converter)
//#else
        private object doInvokeAny(IEnumerable tasks, int count, bool timed, TimeSpan durationToWait)
//#endif
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            IList futures = count == 0 ? new ArrayList() : new ArrayList(count);
            var ecs = new ExecutorCompletionService(this);
            var duration = durationToWait;

            // For efficiency, especially in executors with limited
            // parallelism, check to see if previously submitted tasks are
            // done before submitting more of them. This interleaving
            // plus the exception mechanics account for messiness of main
            // loop.

            try
            {
                // Record exceptions so that if we fail to obtain any
                // result, we can throw the last exception we got.
                ExecutionException ee = null;
                var lastTime = (timed) ? DateTime.Now : new DateTime(0);
                var it = tasks.GetEnumerator();
                var hasMoreTasks = it.MoveNext();
                if (!hasMoreTasks)
                    throw new ArgumentException("No tasks passed in.");
                futures.Add(ecs.Submit((ICallable) it.Current));
                var active = 1;

                for (;;)
                {
                    IFuture f = ecs.Poll();
                    if (f == null)
                    {
                        if (hasMoreTasks && (hasMoreTasks = it.MoveNext()))
                        {
                            futures.Add(ecs.Submit((ICallable) it.Current));
                            ++active;
                        }
                        else if (active == 0)
                            break;
                        else if (timed)
                        {
                            f = ecs.Poll(duration);
                            if (f == null)
                                throw new TimeoutException();
                            //TODO: done't understand what are we doing here. Useless!? -K.X.
                            duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                            lastTime = DateTime.Now;
                        }
                        else
                            f = ecs.Take();
                    }
                    if (f != null)
                    {
                        --active;
                        try
                        {
                            return f.GetResult();
                        }
                        catch (ThreadInterruptedException)
                        {
                            throw;
                        }
                        catch (ExecutionException eex)
                        {
                            ee = eex;
                        }
                        catch (SystemException rex)
                        {
                            ee = new ExecutionException(rex);
                        }
                    }
                }

                if (ee == null)
                    ee = new ExecutionException();
                throw ee;
            }
            finally
            {
                foreach (IFuture future in futures)
                {
                    future.Cancel(true);
                }
            }
        }
    }
}