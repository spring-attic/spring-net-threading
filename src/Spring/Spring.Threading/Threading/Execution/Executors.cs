//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Spring.Threading.AtomicTypes;
//using Spring.Threading.Collections;
//using Spring.Threading.Future;

//namespace Spring.Threading.Execution
//{
//    /// <summary> 
//    /// Factory and utility methods for <see cref="Spring.Threading.IExecutor"/>, 
//    /// <see cref="Spring.Threading.Execution.IExecutorService"/>,
//    /// <see cref="Spring.Threading.Execution.IScheduledExecutorService"/>,
//    /// <see cref="Spring.Threading.IThreadFactory"/>,
//    /// and <see cref="Spring.Threading.ICallable{T}"/> classes defined in this
//    /// package. This class supports the following kinds of methods:
//    /// 
//    /// <ul>
//    /// <li> Methods that create and return an <see cref="Spring.Threading.Execution.IExecutorService"/>
//    /// set up with commonly useful configuration settings.</li>
//    /// <li> Methods that create and return a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/>
//    /// set up with commonly useful configuration settings.</li>
//    /// <li> Methods that create and return a "wrapped" ExecutorService, that
//    /// disables reconfiguration by making implementation-specific methods
//    /// inaccessible.</li>
//    /// <li> Methods that create and return a <see cref="Spring.Threading.IThreadFactory"/>
//    /// that sets newly created threads to a known state.</li>
//    /// <li> Methods that create and return a <see cref="Spring.Threading.ICallable{T}"/>
//    /// out of other closure-like forms, so they can be used
//    /// in execution methods requiring <see cref="Spring.Threading.ICallable{T}"/>.</li>
//    /// </ul>
//    /// </summary>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio (.NET)</author>
//    public class Executors
//    {
//        /// <summary>Private Default Constructor.</summary>
//        private Executors()
//        {
//        }

//        #region Public Static Methods
//        /// <summary> 
//        /// Creates a thread pool that reuses a fixed number of threads
//        /// operating off a shared unbounded queue. 
//        /// </summary>
//        /// <remarks>
//        /// At any point, at most
//        /// <paramref name="threadPoolSize"/> threads will be active processing tasks. If
//        /// additional tasks are submitted when all threads are active,
//        /// they will wait in the queue until a thread is available.  If
//        /// any thread terminates due to a failure during execution prior
//        /// to shutdown, a new one will take its place if needed to execute
//        /// subsequent tasks.
//        /// </remarks>
//        /// <param name="threadPoolSize">the number of threads in the pool</param>
//        /// <returns> the newly created thread pool</returns>
//        public static IExecutorService NewFixedThreadPool<T>(int threadPoolSize)
//        {
//            return new ThreadPoolExecutor(threadPoolSize, threadPoolSize, new TimeSpan(0), new LinkedBlockingQueue<T>());
//        }

//        /// <summary> 
//        /// Creates a thread pool that reuses a fixed number of threads
//        /// operating off a shared unbounded queue, using the provided
//        /// <see cref="Spring.Threading.IThreadFactory"/> to create new threads when needed.  
//        /// </summary>
//        /// <remarks>
//        /// At any point, at most <paramref name="threadPoolSize"/> threads will be active processing
//        /// tasks. If additional tasks are submitted when all threads are
//        /// active, they will wait in the queue until a thread is
//        /// available. If any thread terminates due to a failure during
//        /// execution prior to shutdown, a new one will take its place if
//        /// needed to execute subsequent tasks.
//        /// </remarks>
//        /// <param name="threadPoolSize">the number of threads in the pool</param>
//        /// <param name="threadFactory">the factory to use when creating new threads</param>
//        /// <returns> the newly created thread pool
//        /// </returns>
//        public static IExecutorService NewFixedThreadPool(int threadPoolSize, IThreadFactory threadFactory)
//        {
//            return new ThreadPoolExecutor(threadPoolSize, threadPoolSize, new TimeSpan(0), new LinkedBlockingQueue(), threadFactory);
//        }

//        /// <summary> 
//        /// Creates an <see cref="Spring.Threading.IExecutor"/> that uses a single worker thread operating
//        /// off an unbounded queue.
//        /// </summary>
//        /// <remarks>
//        /// <b>Note:</b> however that if this single
//        /// thread terminates due to a failure during execution prior to
//        /// shutdown, a new one will take its place if needed to execute
//        /// subsequent tasks.  Tasks are guaranteed to execute
//        /// sequentially, and no more than one task will be active at any
//        /// given time. Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewFixedThreadPool(int)"/>,
//        /// the returned executor is guaranteed not to be reconfigurable to use additional threads.
//        /// </remarks>
//        /// <returns> the newly created single-threaded <see cref="Spring.Threading.IExecutor"/> </returns>
//        public static IExecutorService NewSingleThreadExecutor()
//        {
//            return new DelegatedExecutorService(new ThreadPoolExecutor(1, 1, new TimeSpan(0), new LinkedBlockingQueue()));
//        }

//        /// <summary> 
//        /// Creates an <see cref="Spring.Threading.IExecutor"/> that uses a single worker thread operating
//        /// off an unbounded queue, and uses the provided <see cref="Spring.Threading.IThreadFactory"/> to
//        /// create a new thread when needed. 
//        /// </summary>
//        /// <remarks>
//        /// Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewFixedThreadPool(int, IThreadFactory)"/>, the
//        /// returned executor is guaranteed not to be reconfigurable to use
//        /// additional threads.
//        /// </remarks>
//        /// <param name="threadFactory">the factory to use when creating new threads</param>
//        /// <returns> the newly created single-threaded <see cref="Spring.Threading.IExecutor"/></returns>
//        public static IExecutorService NewSingleThreadExecutor(IThreadFactory threadFactory)
//        {
//            return new DelegatedExecutorService(new ThreadPoolExecutor(1, 1, new TimeSpan(0), new LinkedBlockingQueue(), threadFactory));
//        }

//        /// <summary> 
//        /// Creates a thread pool that creates new threads as needed, but
//        /// will reuse previously constructed threads when they are
//        /// available.  
//        /// </summary>
//        /// <remarks>
//        /// These pools will typically improve the performance
//        /// of programs that execute many short-lived asynchronous tasks.
//        /// Calls to <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/> will reuse previously constructed
//        /// threads if available. If no existing thread is available, a new
//        /// thread will be created and added to the pool. Threads that have
//        /// not been used for sixty seconds are terminated and removed from`
//        /// the cache. Thus, a pool that remains idle for long enough will
//        /// not consume any resources. <b>Note:</b> pools with similar
//        /// properties but different details (for example, timeout parameters)
//        /// may be created using <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> constructors.
//        /// </remarks>
//        /// <returns>the newly created thread pool</returns>
//        public static IExecutorService NewCachedThreadPool()
//        {
//            return new ThreadPoolExecutor(0, Int32.MaxValue, new TimeSpan(0, 0, 60), new SynchronousQueue());
//        }

//        /// <summary> 
//        /// Creates a thread pool that creates new threads as needed, but
//        /// will reuse previously constructed threads when they are
//        /// available, and uses the provided <see cref="Spring.Threading.IThreadFactory"/>  to create new threads when needed.
//        /// </summary>
//        /// <param name="threadFactory">the factory to use when creating new threads</param>
//        /// <returns> the newly created thread pool</returns>
//        public static IExecutorService NewCachedThreadPool(IThreadFactory threadFactory)
//        {
//            return new ThreadPoolExecutor(0, Int32.MaxValue, new TimeSpan(0, 0, 60), new SynchronousQueue(), threadFactory);
//        }

//        /// <summary> 
//        /// Creates a single-threaded executor that can schedule commands
//        /// to run after a given delay, or to execute periodically.
//        /// </summary>
//        /// <remarks>
//        /// <b>Note:</b> if this single thread terminates due to a failure during execution prior to
//        /// shutdown, a new one will take its place if needed to execute
//        /// subsequent tasks.  Tasks are guaranteed to execute
//        /// sequentially, and no more than one task will be active at any
//        /// given time. Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewScheduledThreadPool(int)"/>
//        /// the returned executor is guaranteed not to be reconfigurable to use additional threads.
//        /// </remarks>
//        /// <returns> the newly created scheduled executor</returns>
//        public static IScheduledExecutorService NewSingleThreadScheduledExecutor()
//        {
//            return new DelegatedScheduledExecutorService(new ScheduledThreadPoolExecutor(1));
//        }

//        /// <summary> 
//        /// Creates a single-threaded executor that can schedule commands
//        /// to run after a given delay, or to execute periodically.
//        /// </summary>
//        /// <remarks>
//        /// Note however that if this single thread terminates due to a failure
//        /// during execution prior to shutdown, a new one will take its
//        /// place if needed to execute subsequent tasks.)  Tasks are
//        /// guaranteed to execute sequentially, and no more than one task
//        /// will be active at any given time. Unlike the otherwise
//        /// equivalent <see cref="Spring.Threading.Execution.Executors.NewScheduledThreadPool(int, IThreadFactory)"/>
//        /// the returned executor is guaranteed not to be reconfigurable to
//        /// use additional threads.
//        /// </remarks>
//        /// <param name="threadFactory">the factory to use when creating new threads</param>
//        /// <returns> a newly created scheduled executor</returns>
//        public static IScheduledExecutorService NewSingleThreadScheduledExecutor(IThreadFactory threadFactory)
//        {
//            return new DelegatedScheduledExecutorService(new ScheduledThreadPoolExecutor(1, threadFactory));
//        }

//        /// <summary> 
//        /// Creates a thread pool that can schedule commands to run after a
//        /// given delay, or to execute periodically.
//        /// </summary>
//        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.
//        /// </param>
//        /// <returns> a newly created scheduled thread pool</returns>
//        public static IScheduledExecutorService NewScheduledThreadPool(int corePoolSize)
//        {
//            return new ScheduledThreadPoolExecutor(corePoolSize);
//        }

//        /// <summary> 
//        /// Creates a thread pool that can schedule commands to run after a
//        /// given delay, or to execute periodically and uses the provided <see cref="Spring.Threading.IThreadFactory"/> to
//        /// create a new thread when needed. 
//        /// </summary>
//        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
//        /// <param name="threadFactory">the factory to use when the executor creates a new thread.</param>
//        /// <returns> a newly created scheduled thread pool</returns>
//        public static IScheduledExecutorService NewScheduledThreadPool(int corePoolSize, IThreadFactory threadFactory)
//        {
//            return new ScheduledThreadPoolExecutor(corePoolSize, threadFactory);
//        }


//        /// <summary> 
//        /// Returns an object that delegates all defined 
//        /// <see cref="Spring.Threading.Execution.IExecutorService"/> 
//        /// methods to the given executor, but not any
//        /// other methods that might otherwise be accessible using
//        /// casts. 
//        /// </summary>
//        /// <remarks>
//        /// This provides a way to safely "freeze" configuration and
//        /// disallow tuning of a given concrete implementation.
//        /// </remarks>
//        /// <param name="executor">the underlying implementation</param>
//        /// <returns> an <see cref="Spring.Threading.Execution.IExecutorService"/> instance</returns>
//        /// <exception cref="System.ArgumentNullException">if <paramref name="executor"/> is null</exception>
//        public static IExecutorService UnconfigurableExecutorService(IExecutorService executor)
//        {
//            if (executor == null)
//                throw new ArgumentNullException("executor");
//            return new DelegatedExecutorService(executor);
//        }

//        /// <summary> 
//        /// Returns an object that delegates all defined <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> 
//        /// methods to the given executor, but not any other methods that might otherwise be accessible using
//        /// casts. This provides a way to safely "freeze" configuration and
//        /// disallow tuning of a given concrete implementation.
//        /// </summary>
//        /// <param name="executor">the underlying implementation</param>
//        /// <returns> a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> instance</returns>
//        /// <exception cref="System.ArgumentNullException">if <paramref name="executor"/> is null</exception>
//        public static IScheduledExecutorService UnconfigurableScheduledExecutorService(IScheduledExecutorService executor)
//        {
//            if (executor == null)
//                throw new ArgumentNullException("executor");
//            return new DelegatedScheduledExecutorService(executor);
//        }

//        /// <summary> 
//        /// Returns a default thread factory used to create new threads.
//        /// </summary>
//        /// <remarks>
//        /// This factory creates all new threads used by an <see cref="Spring.Threading.IExecutor"/>.
//        /// invoking this <see cref="Spring.Threading.Execution.Executors.GetDefaultThreadFactory()"/> method.
//        /// New threads have names accessible via <see cref="System.Threading.Thread.Name"/> of
//        /// <i>pool-N-thread-M</i>, where <i>N</i> is the sequence
//        /// number of this factory, and <i>M</i> is the sequence number
//        /// of the thread created by this factory.
//        /// </remarks>
//        /// <returns>a thread factory</returns>
//        public static IThreadFactory GetDefaultThreadFactory()
//        {
//            return new DefaultThreadFactory();
//        }

//        /// <summary> 
//        /// Returns a <see cref="Spring.Threading.ICallable"/>  object that, when
//        /// called, runs the given task and returns the given result.  
//        /// </summary>
//        /// <remarks>
//        /// This can be useful when applying methods requiring a
//        /// <see cref="Spring.Threading.ICallable"/> to an otherwise resultless action.
//        /// </remarks>
//        /// <param name="task">the task to run</param>
//        /// <param name="result">the result to return</param>
//        /// <returns>a callable object</returns>
//        /// <exception cref="System.ArgumentNullException">if the task is <see lang="null"/></exception>
//        public static ICallable<T> CreateCallable<T>(IRunnable task, T result)
//        {
//            if (task == null)
//                throw new ArgumentNullException("task");
//            return new RunnableAdapter<T>(task, result);
//        }

//        /// <summary> 
//        /// Returns a <see cref="Spring.Threading.ICallable"/> object that, when
//        /// called, runs the given task and returns <see lang="null"/>.
//        /// </summary>
//        /// <param name="task">the task to run</param>
//        /// <returns> a callable object</returns>
//        /// <exception cref="System.ArgumentNullException">if the task is <see lang="null"/></exception>
//        public static ICallable<T> CreateCallable<T>(IRunnable task)
//        {
//            if (task == null)
//                throw new ArgumentNullException("task");
//            return CreateCallable<T>(task, default(T));
//        }
//        #endregion

//        #region Non-public classes supporting the public methods

//        internal sealed class RunnableAdapter<T> : ICallable<T>
//        {
//            internal IRunnable task;
//            internal T result;

//            internal RunnableAdapter(IRunnable task, T result)
//            {
//                this.task = task;
//                this.result = result;
//            }

//            public T Call()
//            {
//                task.Run();
//                return result;
//            }
//        }

//        internal class DefaultThreadFactory : IThreadFactory
//        {
//            internal static readonly AtomicInteger poolNumber = new AtomicInteger(1);
//            internal AtomicInteger threadNumber = new AtomicInteger(1);
//            internal String namePrefix;

//            internal DefaultThreadFactory()
//            {
//                namePrefix = "pool-" + poolNumber.IncrementValueAndReturn() + "-thread-";
//            }

//            public virtual Thread NewThread(IRunnable r)
//            {
//                Thread t = new Thread(new ThreadStart(r.Run));
//                t.Name = namePrefix + threadNumber.IncrementValueAndReturn();
//                if (t.IsBackground)
//                    t.IsBackground = false;
//                if (t.Priority != ThreadPriority.Normal)
//                {
//                    t.Priority = ThreadPriority.Normal;
//                }
//                return t;
//            }
//        }

//        internal class DelegatedExecutorService : AbstractExecutorService
//        {
//            private IExecutorService _executorService;

//            public override bool IsShutdown
//            {
//                get { return _executorService.IsShutdown; }

//            }

//            public override bool IsTerminated
//            {
//                get { return _executorService.IsTerminated; }

//            }

//            internal DelegatedExecutorService(IExecutorService executor)
//            {
//                _executorService = executor;
//            }

//            public override void Execute(IRunnable command)
//            {
//                _executorService.Execute(command);
//            }

//            public override void Shutdown()
//            {
//                _executorService.Shutdown();
//            }

//            public override IList<IRunnable> ShutdownNow()
//            {
//                return _executorService.ShutdownNow();
//            }

//            public override bool AwaitTermination(TimeSpan duration)
//            {
//                return _executorService.AwaitTermination(duration);
//            }

//            public override IFuture<T> Submit<T>(IRunnable task)
//            {
//                return _executorService.Submit<T>(task);
//            }

//            public override IFuture<T> Submit<T>(ICallable<T> task)
//            {
//                return _executorService.Submit(task);
//            }

//            public override IFuture<T> Submit<T>(IRunnable task, T result)
//            {
//                return _executorService.Submit(task, result);
//            }

//            public override IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks)
//            {
//                return _executorService.InvokeAll(tasks);
//            }

//            public override IList<IFuture<T>> InvokeAll<T>(ICollection<ICallable<T>> tasks, TimeSpan duration)
//            {
//                return _executorService.InvokeAll(tasks, duration);
//            }

//            public override T InvokeAny<T>(ICollection<ICallable<T>> tasks)
//            {
//                return _executorService.InvokeAny(tasks);
//            }

//            public override T InvokeAny<T>(ICollection<ICallable<T>> tasks, TimeSpan duration)
//            {
//                return _executorService.InvokeAny(tasks, duration);
//            }
//        }

//        /// <summary> A wrapper class that exposes only the <see cref="Spring.Threading.Execution.IExecutorService"/> and
//        /// <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> methods of a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> implementation.
//        /// </summary>
//        internal class DelegatedScheduledExecutorService : DelegatedExecutorService, IScheduledExecutorService
//        {
//            private IScheduledExecutorService e;

//            internal DelegatedScheduledExecutorService(IScheduledExecutorService executor) : base(executor)
//            {
//                e = executor;
//            }

//            public virtual IScheduledFuture<T> Schedule<T>(IRunnable command, TimeSpan delay)
//            {
//                return e.Schedule<T>(command, delay);
//            }

//            public virtual IScheduledFuture<T> Schedule<T>(ICallable<T> callable, TimeSpan delay)
//            {
//                return e.Schedule(callable, delay);
//            }

//            public virtual IScheduledFuture<T> ScheduleAtFixedRate<T>(IRunnable command, TimeSpan initialDelay, TimeSpan period)
//            {
//                return e.ScheduleAtFixedRate<T>(command, initialDelay, period);
//            }

//            public virtual IScheduledFuture<T> ScheduleWithFixedDelay<T>(IRunnable command, TimeSpan initialDelay, TimeSpan delay)
//            {
//                return e.ScheduleWithFixedDelay<T>(command, initialDelay, delay);
//            }
//        }
//        #endregion
//    }
//}