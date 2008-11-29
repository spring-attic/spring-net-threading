//namespace Spring.Threading.Future
//{
//    /// <summary> 
//    /// A <see cref="Spring.Threading.Future.IFuture{T}"/> that is a <see cref="Spring.Threading.IRunnable"/>. Successful execution of
//    /// the <see cref="Spring.Threading.IRunnable.Run()"/> method causes completion of the 
//    /// <see cref="Spring.Threading.Future.IFuture{T}"/> and allows access to its results.
//    /// </summary>
//    /// <seealso cref="Spring.Threading.Future.FutureTask{T}"/>
//    /// <seealso cref="Spring.Threading.IExecutor"/>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio (.NET)</author>
//    public interface IRunnableScheduledFuture<T> : IRunnableFuture<T>, IScheduledFuture<T>
//    {
//        /// <summary>
//        /// Returns <see lang="true"/> if this is a periodic task. 
//        /// </summary>
//        /// <remarks>
//        /// A periodic task may re-run according to some schedule. A non-periodic task can be
//        /// run only once.
//        /// </remarks>
//        /// <returns><see lang="true"/> if this task is periodic, <see lang="false"/> otherwise.</returns>
//        bool IsPeriodic { get; }
//    }
//}