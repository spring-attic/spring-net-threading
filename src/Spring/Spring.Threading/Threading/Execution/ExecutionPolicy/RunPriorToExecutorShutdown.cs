
namespace Spring.Threading.Execution.ExecutionPolicy
{
	/// <summary> 
	/// A <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/> for rejected tasks that runs the rejected task
	/// directly in the calling thread of the <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/> method,
	/// unless the executor has been shut down, in which case the task
	/// is discarded.
	/// </summary>
	public class RunPriorToExecutorShutdown : IRejectedExecutionHandler
	{
		/// <summary> 
		/// Creates a <see cref="RunPriorToExecutorShutdown"/>.
		/// </summary>
		public RunPriorToExecutorShutdown(){}

		/// <summary> 
		/// Executes <paramref name="runnable"/> in the caller's thread, 
		/// unless the <paramref name="executor"/> has been shut down, in which case the task is discarded.
		/// </summary>
		/// <param name="runnable">the <see cref="Spring.Threading.IRunnable"/> task requested to be executed</param>
		/// <param name="executor">the <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> attempting to execute this task</param>
		public virtual void RejectedExecution(IRunnable runnable, IExecutorService executor)
		{
			if (!executor.IsShutdown)
			{
				runnable.Run();
			}
		}
	}
}
