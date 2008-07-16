using System;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
	
		/// <summary> 
		/// A service that decouples the production of new asynchronous tasks
		/// from the consumption of the results of completed tasks.  
		/// </summary>
		/// <remarks> 
		/// Producers
		/// submit tasks for execution. Consumers take
		/// completed tasks and process their results in the order they
		/// complete.  A <see cref="Spring.Threading.Execution.ICompletionService"/> can for example be used to
		/// manage asynchronous IO, in which tasks that perform reads are
		/// submitted in one part of a program or system, and then acted upon
		/// in a different part of the program when the reads complete,
		/// possibly in a different order than they were requested.
		/// <p/>
		/// 
		/// Typically, a <see cref="Spring.Threading.Execution.ICompletionService"/> relies on a separate 
		/// <see cref="Spring.Threading.IExecutor"/> to actually execute the tasks, in which case the
		/// <see cref="Spring.Threading.Execution.ICompletionService"/> only manages an internal completion
		/// queue. The {@link ExecutorCompletionService} class provides an
		/// <seealso cref="Spring.Threading.Execution.ExecutorCompletionService"/>
		/// </remarks>
		public interface ICompletionService
		{
			/// <summary> 
			///	Submits a value-returning task for execution and returns a <see cref="Spring.Threading.Future.IFuture"/>
			/// representing the pending results of the task. Upon completion,
			/// this task may be taken or polled.
			/// </summary>
			/// <param name="task">the task to submit</param>
			/// <returns> a <see cref="Spring.Threading.Future.IFuture"/> representing pending completion of the task</returns>
			/// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
			/// <exception cref="System.ArgumentNullException">if the command is null</exception>
			IFuture Submit(ICallable task);
		
		
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
			IFuture Submit(IRunnable task, object result);
		
			/// <summary> 
			/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
			/// completed task, waiting if none are yet present.
			/// </summary>
			/// <returns> the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task
			/// </returns>
			IFuture Take();
		
			/// <summary> 
			/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
			/// completed task or <see lang="null"/> if none are present.
			/// </summary>
			/// <returns> the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task, or
			/// <see lang="null"/> if none are present.
			/// </returns>
			IFuture Poll();
		
			/// <summary> 
			/// Retrieves and removes the <see cref="Spring.Threading.Future.IFuture"/> representing the next
			/// completed task, waiting, if necessary, up to the specified duration
			/// if none are yet present.
			/// </summary>
			/// <param name="duration">duration to wait if no completed task is present yet.</param>
			/// <returns> 
			/// the <see cref="Spring.Threading.Future.IFuture"/> representing the next completed task or
			/// <see lang="null"/> if the specified waiting time elapses before one
			/// is present.
			/// </returns>
			IFuture Poll(TimeSpan duration);
		}
}
