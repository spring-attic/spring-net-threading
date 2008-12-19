namespace Spring.Threading.Future
{
    /// <summary> 
    /// A delayed result-bearing action that can be cancelled.
    /// </summary>
    /// <remarks>
    /// Usually a scheduled future is the result of scheduling
    /// a task with a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/>.
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio(.NET)</author>
    public interface IScheduledFuture : IDelayed, IFuture
    {
    }
}