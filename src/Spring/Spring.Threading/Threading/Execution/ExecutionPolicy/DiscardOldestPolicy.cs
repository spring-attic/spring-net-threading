namespace Spring.Threading.Execution.ExecutionPolicy
{
	/// <summary> 
	/// A <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/> for rejected tasks that discards the oldest unhandled
	/// request and then retries <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/>, unless the executor
	/// is shut down, in which case the task is discarded.
	/// </summary>
	public class DiscardOldestPolicy : IRejectedExecutionHandler
	{
		/// <summary> Creates a <see cref="Spring.Threading.Execution.ExecutionPolicy.DiscardOldestPolicy"/>.</summary>
		public DiscardOldestPolicy()
		{
		}

		/// <summary> 
		/// Retries execution of <paramref name="runnable"/>. If the the <paramref name="executor"/>
		/// is shut down, the <paramref name="runnable"/> is instead discarded.
		/// If <paramref name="executor"/> is a <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> instance,
		/// the next task that the <paramref name="executor"/>
		/// would otherwise execute is obtained and ignored, if one is immediately available.
		/// </summary>
		/// <param name="runnable">the <see cref="Spring.Threading.IRunnable"/> task requested to be executed</param>
		/// <param name="executor">the <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> attempting to execute this task</param>
		public virtual void RejectedExecution(IRunnable runnable, IExecutorService executor)
		{
			if (!executor.IsShutdown)
			{
				if ( executor is ThreadedExecutor )
				{
					ThreadPoolExecutor threadPoolExecutor = (ThreadPoolExecutor) executor;
				    IRunnable head;
					threadPoolExecutor.Queue.Poll(out head);
				}
				executor.Execute(runnable);
			}
		}
	}
}