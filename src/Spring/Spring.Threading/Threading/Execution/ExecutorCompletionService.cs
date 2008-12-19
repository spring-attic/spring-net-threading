using System;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
	/// <summary> 
	/// A <see cref="Spring.Threading.Execution.ICompletionService"/> 
	/// that uses a supplied <see cref="Spring.Threading.IExecutor"/>
	/// to execute tasks.  
	/// </summary>
	/// <remarks>
	/// This class arranges that submitted tasks are,
	/// upon completion, placed on a queue accessible using 
	/// <see cref="Spring.Threading.Execution.ICompletionService.Take()"/>.
	/// The class is lightweight enough to be suitable for transient use
	/// when processing groups of tasks.
	/// <p/>
	/// 
	/// <b>Usage Examples.</b>
	/// 
	/// Suppose you have a set of solvers for a certain problem, each
	/// returning a value of some type result, and would like to
	/// run them concurrently, processing the results of each of them that
	/// return a non-null value, in some method ( use(FutureResult r). 
	/// 
	/// Suppose instead that you would like to use the first non-null result
	/// of the set of tasks, ignoring any that encounter exceptions,
	/// and cancelling all other tasks when the first one is ready:
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class ExecutorCompletionService : ICompletionService
	{
		private IExecutor _executor;
		private IBlockingQueue<IFuture> _completionQueue;

		private class QueueingFuture : FutureTask
		{
			private ExecutorCompletionService _enclosingExecutionCompletionService;

			internal QueueingFuture(ExecutorCompletionService enclosingInstance, ICallable c) : base(c)
			{
				this._enclosingExecutionCompletionService = enclosingInstance;
			}

			internal QueueingFuture(ExecutorCompletionService enclosingInstance, IRunnable t, Object r) : base(t, r)
			{
				this._enclosingExecutionCompletionService = enclosingInstance;
			}

			protected internal override void done()
			{
				_enclosingExecutionCompletionService.addFuture(this);
			}
		}


		/// <summary> 
		/// Creates an <see cref="Spring.Threading.Execution.ExecutorCompletionService"/> using the supplied
		/// executor for base task execution and a
		/// <see cref="LinkedBlockingQueue{T}"/> as a completion queue.
		/// </summary>
		/// <param name="executor">the executor to use</param>
		/// <exception cref="System.ArgumentNullException">if the executor is null</exception>
		public ExecutorCompletionService(IExecutor executor) : this( executor, new LinkedBlockingQueue<IFuture>())
		{
		}

		/// <summary> 
		/// Creates an <see cref="Spring.Threading.Execution.ExecutorCompletionService"/> using the supplied
		/// executor for base task execution and the supplied queue as its
		/// completion queue.
		/// </summary>
		/// <param name="executor">the executor to use</param>
		/// <param name="completionQueue">the queue to use as the completion queue
		/// normally one dedicated for use by this service
		/// </param>
		/// <exception cref="System.ArgumentNullException">if the executor is null</exception>
		public ExecutorCompletionService(IExecutor executor, IBlockingQueue<IFuture> completionQueue)
		{
			if (executor == null)
				throw new ArgumentNullException("executor", "Executor cannot be null.");
			if (completionQueue == null)
				throw new ArgumentNullException("completionQueue", "Completion Queue cannot be null.");
			_executor = executor;
			_completionQueue = completionQueue;
		}
		/// <summary> 
		///	Submits a value-returning task for execution and returns a <see cref="Spring.Threading.Future.IFuture"/>
		/// representing the pending results of the task. Upon completion,
		/// this task may be taken or polled.
		/// </summary>
		/// <param name="task">the task to submit</param>
		/// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
		/// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
		/// <exception cref="System.ArgumentNullException">if the command is null</exception>
		public virtual IFuture Submit(ICallable task)
		{
			if (task == null)
				throw new ArgumentNullException("task", "Task cannot be null.");
			QueueingFuture f = new QueueingFuture(this, task);
			_executor.Execute(f);
			return f;
		}
		/// <summary> 
		/// Submits a <see cref="Spring.Threading.IRunnable"/> task for execution 
		/// and returns a <see cref="Spring.Threading.Future.IFuture"/>
		/// representing that task.  Upon completion, this task may be taken or polled.
		/// </summary>
		/// <param name="task">the task to submit</param>
		/// <param name="result">the result to return upon successful completion</param>
		/// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task,
		/// and whose <see cref="Spring.Threading.Future.IFuture.GetResult()"/> method will return the given result value
		/// upon completion
		/// </returns>
		/// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
		/// <exception cref="System.ArgumentNullException">if the command is null</exception>
		public virtual IFuture Submit(IRunnable task, object result)
		{
			if (task == null)
				throw new ArgumentNullException("task", "Task cannot be null.");
			QueueingFuture f = new QueueingFuture(this, task, result);
			_executor.Execute(f);
			return f;
		}
		/// <summary> 
		/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
		/// completed task, waiting if none are yet present.
		/// </summary>
		/// <returns> the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task
		/// </returns>
		public virtual IFuture Take()
		{
			return (IFuture) _completionQueue.Take();
		}
		/// <summary> 
		/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
		/// completed task or <see lang="null"/> if none are present.
		/// </summary>
		/// <returns> the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task, or
		/// <see lang="null"/> if none are present.
		/// </returns>
		public virtual IFuture Poll()
		{
		    IFuture next;
            if ( !_completionQueue.Poll(out next))
            {
                return null;
            }
		    return next; 
		}
		/// <summary> 
		/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
		/// completed task, waiting, if necessary, up to the specified duration
		/// if none are yet present.
		/// </summary>
		/// <param name="durationToWait">duration to wait if no completed task is present yet.</param>
		/// <returns> 
		/// the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task or
		/// <see lang="null"/> if the specified waiting time elapses before one
		/// is present.
		/// </returns>
		public virtual IFuture Poll(TimeSpan durationToWait)
		{
		    IFuture next;
            if ( !_completionQueue.Poll(durationToWait, out next))
            {
                return null;
            }
		    return next;
		}

		/// <summary>
		/// Adds the <paramref name="future"/> to this <see cref="Spring.Threading.Execution.ExecutorCompletionService"/>
		/// </summary>
		/// <param name="future"></param>
		protected void addFuture( IFuture future )
		{
			_completionQueue.Add(future);
		}
	}
}