using Spring.Threading.Execution;

namespace Spring.Threading.Execution
{
	/// <summary> 
	/// A handler for tasks that cannot be executed by a 
	/// <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface IRejectedExecutionHandler
	{
		/// <summary> 
		/// Method that may be invoked by a <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> when
		/// <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/> cannot accept a task. 
		/// </summary>
		/// <remarks> 
		/// This may occur when no more threads or queue slots are available because their bounds
		/// would be exceeded, or upon shutdown of the Executor.
		/// <p/>
		/// In the absence other alternatives, the method may throw an
		/// <see cref="Spring.Threading.Execution.RejectedExecutionException"/>, which will be
		/// propagated to the caller of <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/>.
		/// </remarks>
		/// <param name="runnable">the runnable task requested to be executed
		/// </param>
		/// <param name="executor">the executor attempting to execute this task
		/// </param>
		/// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if there is no remedy for the rejection.</exception>
		void RejectedExecution(IRunnable runnable, IExecutorService executor);
	}
}