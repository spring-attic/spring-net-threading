using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution.ExecutionPolicy;
using Spring.Threading.Locks;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Enumeration representing the state of the ThreadPool.
    /// </summary>
    public enum ThreadPoolState
    {
        /// <summary>
        /// Normal, not-shutdown mode
        /// </summary>
        RUNNING = 0,
        /// <summary>
        /// Controlled shutdown mode
        /// </summary>
        SHUTDOWN = 1,
        /// <summary>
        /// Immediate shutdown mode
        /// </summary>
        STOP = 2,
        /// <summary>
        /// Final State
        /// </summary>
        TERMINATED = 3
    }

    /// <summary> An <see cref="Spring.Threading.Execution.IExecutorService"/> that executes each submitted task using
    /// one of possibly several pooled threads, normally configured
    /// using <see cref="Spring.Threading.Execution.Executors"/> factory methods.
    /// </summary> 
    /// <remarks>
    /// Thread pools address two different problems: they usually
    /// provide improved performance when executing large numbers of
    /// asynchronous tasks, due to reduced per-task invocation overhead,
    /// and they provide a means of bounding and managing the resources,
    /// including threads, consumed when executing a collection of tasks.
    /// Each <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> also maintains some basic
    /// statistics, such as the number of completed tasks.
    /// 
    /// <p/>
    /// To be useful across a wide range of contexts, this class
    /// provides many adjustable parameters and extensibility
    /// hooks. However, programmers are urged to use the more convenient
    /// <see cref="Spring.Threading.Execution.Executors"/> factory methods 
    /// <see cref="M:Spring.Threading.Execution.Executors.NewCachedThreadPool"/> ( unbounded thread pool, with
    /// automatic thread reclamation), <see cref="M:Spring.Threading.Execution.Executors.NewFixedThreadPool"/>
    /// (fixed size thread pool) and <see cref="M:Spring.Threading.Execution.Executors.NewSingleThreadExecutor"/>
    /// single background thread), that preconfigure settings for the most common usage
    /// scenarios. Otherwise, use the following guide when manually
    /// configuring and tuning this class:
    /// 
    /// <dl>
    ///		<dt>Core and maximum pool sizes</dt>
    /// 	<dd>
    /// 		A <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> will automatically adjust the
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.CurrentPoolSize"/>
    ///			according to the bounds set by <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    ///			and <see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>
    ///			When a new task is submitted in method
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute"/>, 
    /// 		and fewer than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> threads
    /// 		are running, a new thread is created to handle the request, even if
    /// 		other worker threads are idle.  If there are more than
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    /// 		but less than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>
    /// 		threads running, a new thread will be created only if the queue is full.  By setting
    /// 		core pool size and maximum pool size the same, you create a fixed-size
    /// 		thread pool. By setting maximum pool size to an essentially unbounded
    /// 		value such as <see cref="System.Int32.MaxValue"/>, you allow the pool to
    /// 		accommodate an arbitrary number of concurrent tasks. Most typically,
    /// 		core and maximum pool sizes are set only upon construction, but they
    /// 		may also be changed dynamically using 
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> and
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>.
    /// 	</dd>
    /// 
    ///		<dt>On-demand construction</dt>
    ///		<dd> 
    ///			By default, even core threads are initially created and
    /// 		started only when new tasks arrive, but this can be overridden
    /// 		dynamically using method
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.PreStartCoreThread()"/> or
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.PreStartAllCoreThreads()"/>.
    /// 		You probably want to prestart threads if you construct the
    /// 		pool with a non-empty queue. 
    /// 	</dd>
    /// 
    ///		<dt>Creating new threads</dt>
    /// 	<dd>
    /// 		New threads are created using a <see cref="Spring.Threading.IThreadFactory"/>.
    /// 		If not otherwise specified, a  <see cref="Spring.Threading.Execution.Executors.DefaultThreadFactory"/> is used, 
    /// 		that creates threads to all with the same <see cref="System.Threading.ThreadPriority"/> set to 
    /// 		<see cref="System.Threading.ThreadPriority.Normal"/>
    /// 		priority and non-daemon status. By supplying
    /// 		a different <see cref="Spring.Threading.IThreadFactory"/>, you can alter the thread's name,
    /// 		priority, daemon status, etc. If a <see cref="Spring.Threading.IThreadFactory"/> fails to create
    /// 		a thread when asked by returning null from <see cref="Spring.Threading.IThreadFactory.NewThread(IRunnable)"/>,
    /// 		the executor will continue, but might not be able to execute any tasks. 
    /// 	</dd>
    /// 
    ///		<dt>Keep-alive times</dt>
    /// 	<dd>
    /// 		If the pool currently has more than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> threads,
    /// 		excess threads will be terminated if they have been idle for more
    /// 		than the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.KeepAliveTime"/>.
    /// 		This provides a means of reducing resource consumption when the pool is not being actively
    /// 		used. If the pool becomes more active later, new threads will be
    /// 		constructed. This parameter can also be changed dynamically using
    /// 		method <see cref="Spring.Threading.Execution.ThreadPoolExecutor.KeepAliveTime"/>. Using a value
    /// 		of <see cref="System.Int32.MaxValue"/> effectively
    /// 		disables idle threads from ever terminating prior to shut down. By
    /// 		default, the keep-alive policy applies only when there are more
    /// 		than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> Threads. But method {@link
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.AllowsCoreThreadsToTimeOut"/> can be used to apply
    /// 		this time-out policy to core threads as well, so long as
    /// 		the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.KeepAliveTime"/> value is non-zero. 
    /// 	</dd>
    /// 
    ///		<dt>Queuing</dt>
    ///		<dd>
    ///			Any <see cref="IBlockingQueue{T}"/> may be used to transfer and hold
    ///			submitted tasks.  The use of this queue interacts with pool sizing:
    /// 
    ///			<ul>
    /// 			<li> 
    /// 				If fewer than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    /// 				threads are running, the Executor always prefers adding a new thread
    /// 				rather than queuing.
    /// 			</li>
    ///				<li> 
    ///					If <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    ///					or more threads are running, the Executor always prefers queuing a request rather than adding a new
    ///					thread.
    ///				</li>
    ///				<li> 
    ///					If a request cannot be queued, a new thread is created unless
    ///					this would exceed <see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>, 
    ///					in which case, the task will be rejected.
    /// 			</li>
    ///			</ul>
    /// 
    ///			There are three general strategies for queuing:
    /// 
    ///			<ol>
    ///				<li> 
    ///					<i> Direct handoffs.</i> A good default choice for a work
    ///					queue is a <see cref="SynchronousQueue{T}"/> 
    ///					that hands off tasks to threads without otherwise holding them. Here, an attempt to queue a task
    /// 				will fail if no threads are immediately available to run it, so a
    /// 				new thread will be constructed. This policy avoids lockups when
    /// 				handling sets of requests that might have internal dependencies.
    /// 				Direct handoffs generally require unbounded 
    /// 				<see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>
    /// 				to avoid rejection of new submitted tasks. This in turn admits the
    /// 				possibility of unbounded thread growth when commands continue to
    /// 				arrive on average faster than they can be processed.  
    /// 			</li>
    ///				<li>
    ///					<i>Unbounded queues.</i> Using an unbounded queue (for
    ///					example a <see cref="LinkedBlockingQueue{T}"/> without a predefined
    /// 				capacity) will cause new tasks to wait in the queue when all
    /// 				<see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    /// 				threads are busy. Thus, no more than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>
    /// 				threads will ever be created. (And the value of the 
    /// 				<see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>
    /// 				therefore doesn't have any effect.)  This may be appropriate when
    /// 				each task is completely independent of others, so tasks cannot
    /// 				affect each others execution; for example, in a web page server.
    /// 				While this style of queuing can be useful in smoothing out
    /// 				transient bursts of requests, it admits the possibility of
    /// 				unbounded work queue growth when commands continue to arrive on
    /// 				average faster than they can be processed.  
    /// 			</li>
    ///				<li>
    ///					<i>Bounded queues.</i> A bounded queue (for example, an
    ///					<see cref="ArrayBlockingQueue{T}"/>) helps prevent resource exhaustion when
    /// 				used with finite <see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>, 
    /// 				but can be more difficult to tune and control.  Queue sizes and maximum pool sizes may be traded
    /// 				off for each other: Using large queues and small pools minimizes
    /// 				CPU usage, OS resources, and context-switching overhead, but can
    /// 				lead to artificially low throughput.  If tasks frequently block (for
    /// 				example if they are I/O bound), a system may be able to schedule
    /// 				time for more threads than you otherwise allow. Use of small queues
    /// 				generally requires larger pool sizes, which keeps CPUs busier but
    /// 				may encounter unacceptable scheduling overhead, which also
    /// 				decreases throughput.  
    /// 			</li>
    ///			</ol>
    ///		</dd>
    /// 
    ///		<dt>Rejected tasks</dt>
    ///		<dd> 
    ///			New tasks submitted in method <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute"/>
    ///			will be <i>rejected</i> when the Executor has been shut down, and also when the Executor uses finite
    /// 		bounds for both maximum threads and work queue capacity, and is
    /// 		saturated.  In either case, the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute"/> method invokes the
    /// 		<see cref="IRejectedExecutionHandler.RejectedExecution"/> method of its
    /// 		<see cref="IRejectedExecutionHandler"/>.  Four predefined handler policies
    /// 		are provided:
    /// 		
    ///			<ol>
    ///				<li> 
    ///					In the default <see cref="Spring.Threading.Execution.ExecutionPolicy.AbortPolicy"/>, the handler throws a
    ///					runtime <see cref="Spring.Threading.Execution.RejectedExecutionException"/> upon rejection. 
    ///				</li>
    ///				<li> 
    ///					In <see cref="Spring.Threading.Execution.ExecutionPolicy.RunPriorToExecutorShutdown"/>, the thread that invokes
    ///					<see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute"/> itself runs the task. This provides a simple
    /// 				feedback control mechanism that will slow down the rate that new tasks are submitted. 
    /// 			</li>
    ///				<li> 
    ///					In <see cref="Spring.Threading.Execution.ExecutionPolicy.DiscardPolicy"/>,
    ///					a task that cannot be executed is simply dropped.
    ///				</li>
    ///				<li>
    ///					In <see cref="Spring.Threading.Execution.ExecutionPolicy.DiscardOldestPolicy"/>, if the executor is not
    ///					shut down, the task at the head of the work queue is dropped, and
    /// 				then execution is retried (which can fail again, causing this to be
    /// 				repeated.) 
    /// 			</li>
    ///			</ol>
    ///			It is possible to define and use other kinds of
    /// 		<see cref="IRejectedExecutionHandler"/> classes. Doing so requires some care
    /// 		especially when policies are designed to work only under particular
    /// 		capacity or queuing policies. 
    ///		</dd>
    /// 
    ///		<dt>Hook methods</dt>
    /// 	<dd>
    /// 		This class provides <i>protected</i> overridable <see cref="Spring.Threading.Execution.ThreadPoolExecutor.beforeExecute(Thread, IRunnable)"/>
    /// 		and <see cref="Spring.Threading.Execution.ThreadPoolExecutor.afterExecute(IRunnable, Exception)"/> methods that are called before and
    ///			after execution of each task.  These can be used to manipulate the
    /// 		execution environment; for example, reinitializing ThreadLocals,
    /// 		gathering statistics, or adding log entries. Additionally, method
    /// 		<see cref="Spring.Threading.Execution.ThreadPoolExecutor.terminated()"/> can be overridden to perform
    /// 		any special processing that needs to be done once the Executor has
    /// 		fully terminated.
    /// 
    ///			<p/>
    ///			If hook or callback methods throw exceptions, internal worker threads may in turn fail and
    ///			abruptly terminate.
    ///		</dd>
    /// 
    ///		<dt>Queue maintenance</dt>
    ///		<dd> 
    ///			Method <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Queue"/> allows access to
    /// 		the work queue for purposes of monitoring and debugging.  Use of
    ///			this method for any other purpose is strongly discouraged. 
    /// 	</dd> 
    /// </dl>
    /// 
    /// <p/>
    /// <b>Extension example</b>. Most extensions of this class
    /// override one or more of the protected hook methods. For example,
    /// here is a subclass that adds a simple pause/resume feature:
    /// 
    /// <code>
    ///		public class PausableThreadPoolExecutor : ThreadPoolExecutor {
    ///			private boolean isPaused;
    /// 		private ReentrantLock pauseLock = new ReentrantLock();
    /// 		private ICondition unpaused = pauseLock.NewCondition();
    /// 
    /// 		public PausableThreadPoolExecutor(...) { super(...); }
    /// 
    /// 		protected override void beforeExecute(Thread t, IRunnable r) {
    /// 				super.beforeExecute(t, r);
    /// 				pauseLock.Lock();
    /// 				try {
    /// 					while (isPaused) unpaused.Await();
    /// 				} catch (ThreadInterruptedException ie) {
    ///						t.Interrupt();
    /// 				} finally {
    ///						pauseLock.Unlock();
    ///					}
    ///			}
    /// 
    ///			public void Pause() {
    /// 			pauseLock.Lock();
    /// 			try {
    /// 				isPaused = true;
    ///				} finally {
    ///					pauseLock.Unlock();
    ///				}
    ///			}
    /// 
    ///			public void Resume() {
    ///				pauseLock.Lock();
    ///				try {
    ///					isPaused = false;
    /// 				unpaused.SignalAll();
    /// 			} finally {
    /// 				pauseLock.Unlock();
    /// 			}
    /// 		}
    /// 	}
    /// </code>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    public class ThreadPoolExecutor : AbstractExecutorService, IDisposable
    {
        #region Worker Class

        /// <summary>
        /// Class Worker mainly maintains interrupt control state for
        /// threads running tasks, along with other minor bookkeeping. This
        /// class opportunistically extends ReentrantLock to simplify
        /// acquiring and releasing a lock surrounding each task execution.
        /// This protects against interrupts that are intended to wake up a
        /// worker thread waiting for a task from instead interrupting a
        /// task being run.
        /// </summary>
        protected internal class Worker : ReentrantLock, IRunnable
        {
            private readonly ThreadPoolExecutor _parentThreadPoolExecutor;

            /// <summary> 
            /// Per thread completed task counter; accumulated
            /// into completedTaskCount upon termination.
            /// </summary>
            protected internal volatile int _completedTasks;

            /// <summary> 
            /// Initial task to run before entering run loop
            /// </summary>
            protected internal IRunnable _firstTask;

            /// <summary> 
            /// Thread this worker is running in.  Acts as a final field,
            /// but cannot be set until thread is created.
            /// </summary>
            internal Thread _thread;

            /// <summary>
            /// Default Constructor
            /// </summary>
            /// <param name="firstTask">Task to run before entering run loop.</param>
            /// <param name="parentThreadPoolExecutor"><see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> that controls this worker</param>
            internal Worker(ThreadPoolExecutor parentThreadPoolExecutor, IRunnable firstTask)
            {
                _firstTask = firstTask;
                _parentThreadPoolExecutor = parentThreadPoolExecutor;
                _thread = parentThreadPoolExecutor.ThreadFactory.NewThread(this);
            }

            #region IRunnable Members

            /// <summary>
            /// Runs the associated task, signalling the <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> when exiting.
            /// </summary>
            public void Run()
            {
                // TODO: No ideal.  
                _parentThreadPoolExecutor.runWorker(this);
            }

            #endregion
        }

        #endregion

        #region Private Fields

// TODO: Fix comments

        /**
     * The main pool control state, controlState, is an atomic integer packing
     * two conceptual fields
     *   workerCount, indicating the effective number of threads
     *   runState,    indicating whether running, shutting down etc
     *
     * In order to pack them into one int, we limit workerCount to
     * (2^29)-1 (about 500 million) threads rather than (2^31)-1 (2
     * billion) otherwise representable. If this is ever an issue in
     * the future, the variable can be changed to be an AtomicLong,
     * and the shift/mask constants below adjusted. But until the need
     * arises, this code is a bit faster and simpler using an int.
     *
     * The workerCount is the number of workers that have been
     * permitted to start and not permitted to stop.  The value may be
     * transiently different from the actual number of live threads,
     * for example when a ThreadFactory fails to create a thread when
     * asked, and when exiting threads are still performing
     * bookkeeping before terminating. The user-visible pool size is
     * reported as the current size of the workers set.
     *
     * The runState provides the main lifecyle control, taking on values:
     *
     *   RUNNING:  Accept new tasks and process queued tasks
     *   SHUTDOWN: Don't accept new tasks, but process queued tasks
     *   STOP:     Don't accept new tasks, don't process queued tasks,
     *             and interrupt in-progress tasks
     *   TIDYING:  All tasks have terminated, workerCount is zero,
     *             the thread transitioning to state TIDYING
     *             will run the terminated() hook method
     *   TERMINATED: terminated() has completed
     *
     * The numerical order among these values matters, to allow
     * ordered comparisons. The runState monotonically increases over
     * time, but need not hit each state. The transitions are:
     *
     * RUNNING -> SHUTDOWN
     *    On invocation of shutdown(), perhaps implicitly in finalize()
     * (RUNNING or SHUTDOWN) -> STOP
     *    On invocation of shutdownNow()
     * SHUTDOWN -> TIDYING
     *    When both queue and pool are empty
     * STOP -> TIDYING
     *    When pool is empty
     * TIDYING -> TERMINATED
     *    When the terminated() hook method has completed
     *
     * Threads waiting in awaitTermination() will return when the
     * state reaches TERMINATED.
     *
     * Detecting the transition from SHUTDOWN to TIDYING is less
     * straightforward than you'd like because the queue may become
     * empty after non-empty and vice versa during SHUTDOWN state, but
     * we can only terminate if, after seeing that it is empty, we see
     * that workerCount is 0 (which sometimes entails a recheck -- see
     * below).
     */
        private const int CAPACITY = (1 << COUNT_BITS) - 1;
        private const int COUNT_BITS = 29; // Integer.SIZE - 3;

        // runState is stored in the high-order bits
        private const int RUNNING = -1 << COUNT_BITS;
        private const int SHUTDOWN = 0 << COUNT_BITS;

        /// <summary> 
        /// The default <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/>
        /// </summary>
        private static readonly IRejectedExecutionHandler _defaultRejectedExecutionHandler = new AbortPolicy();

        private const int STOP = 1 << COUNT_BITS;
        private const int TERMINATED = 3 << COUNT_BITS;
        private const int TIDYING = 2 << COUNT_BITS;
        private readonly AtomicInteger _controlState = new AtomicInteger(ctlOf(RUNNING, 0));

        // Packing and unpacking controlState

        /// <summary> 
        /// Set containing all worker threads in pool.
        /// </summary>
        private readonly IList<Worker> _currentWorkerThreads = new List<Worker>();

        /// <summary> 
        /// Lock held on updates to poolSize, corePoolSize, maximumPoolSize, and workers set.
        /// </summary>
        private readonly ReentrantLock _mainLock = new ReentrantLock();

        /// <summary> 
        /// Queue used for holding tasks and handing off to worker threads.
        /// </summary>
        private readonly IBlockingQueue<IRunnable> _workQueue;

        /// <summary>
        /// Wait condition to support AwaitTermination
        /// </summary>
        private readonly ICondition termination;

        /// <summary> 
        /// If <see lang="false"/> ( the default), core threads stay alive even when idle.
        /// If <see lang="true"/>, core threads use <see cref="Spring.Threading.Execution.ThreadPoolExecutor.KeepAliveTime"/> 
        /// to time out waiting for work.
        /// </summary>
        private bool _allowCoreThreadsToTimeOut;

        /// <summary>
        ///If false (^default), core threads stay alive even when idle.
        /// If true, core threads use keepAliveTime to time out waiting
        /// for work.
        /// </summary>
        private volatile bool _allowCoreThreadTimeOut;

        /// <summary> 
        /// Counter for completed tasks. Updated only on termination of
        /// worker threads.
        /// </summary>
        private long _completedTaskCount;

        /// <summary> 
        /// Core pool size, updated only while holding a lock,
        /// but volatile to allow concurrent readability even
        /// during updates.
        /// </summary>
        private volatile int _corePoolSize;

        /// <summary> 
        /// The <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>'s current Lifecycle state.
        /// </summary>
        internal volatile ThreadPoolState _currentLifecycleState;

        /// <summary> 
        /// Current pool size, updated only while holding a lock,
        /// but volatile to allow concurrent readability even
        /// during updates.
        /// </summary>
        private volatile int _currentPoolSize;

        /// <summary> 
        ///	Timeout <see cref="System.TimeSpan"/> for idle threads waiting for work.
        /// Threads use this timeout only when there are more than
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> present. Otherwise they wait forever for new work.
        /// </summary>
        private TimeSpan _keepAliveTime;

        /// <summary> 
        /// Tracks largest attained pool size.
        /// </summary>
        private int _largestPoolSize;

        /// <summary> 
        /// Maximum pool size, updated only while holding a lock,
        /// but volatile to allow concurrent readability even
        /// during updates.
        /// </summary>
        private volatile int _maximumPoolSize;

        /// <summary> 
        /// <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/> called when
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> is saturated or  
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Shutdown()"/> in executed.
        /// </summary>
        private volatile IRejectedExecutionHandler _rejectedExecutionHandler;

        /// <summary> 
        /// <see cref="Spring.Threading.IThreadFactory"/> for creating new threads.
        /// </summary>
        private volatile IThreadFactory _threadFactory;

        private static int runStateOf(int c)
        {
            return c & ~CAPACITY;
        }

        private static int workerCountOf(int c)
        {
            return c & CAPACITY;
        }

        private static int ctlOf(int rs, int wc)
        {
            return rs | wc;
        }

        /*
     * Bit field accessors that don't require unpacking controlState.
     * These depend on the bit layout and on workerCount being never negative.
     */

        private static bool runStateLessThan(int c, int s)
        {
            return c < s;
        }

        private static bool runStateAtLeast(int c, int s)
        {
            return c >= s;
        }

        private static bool isRunning(int c)
        {
            return c < SHUTDOWN;
        }

        #endregion

        #region Public Properties

        /// <summary> 
        /// Gets / Sets the time limit for which threads may remain idle before
        /// being terminated.  
        /// </summary>
        /// <remarks>
        /// If there are more than the core number of
        /// threads currently in the pool, after waiting this amount of
        /// time without processing a task, excess threads will be
        /// terminated.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// if <i>value</i> is less than 0 or if <i>value</i> equals 0 and 
        /// <see cref="AllowsCoreThreadsToTimeOut"/> is 
        /// <see lang="true"/>
        /// </exception>
        public TimeSpan KeepAliveTime
        {
            set
            {
                if (value.Ticks < 0)
                {
                    throw new ArgumentException("Keep alive time must be greater than 0.");
                }
                if (value.Ticks == 0 && AllowsCoreThreadsToTimeOut)
                {
                    throw new ArgumentException("Core threads must have nonzero keep alive times");
                }
                _keepAliveTime = value;
            }
            get { return _keepAliveTime; }
        }

        /// <summary>
        /// Returns <see lang="true"/> if this pool allows core threads to time out and
        /// terminate if no tasks arrive within the keepAlive time, being
        /// replaced if needed when new tasks arrive. 
        /// </summary>
        /// <remarks>
        /// When true, the same keep-alive policy applying to non-core threads applies also to
        /// core threads. When false (the default), core threads are never
        /// terminated due to lack of incoming tasks.
        /// </remarks>
        /// <returns> <see lang="true"/> if core threads are allowed to time out,
        /// else <see lang="false"/>
        /// </returns>
        /// <exception cref="System.ArgumentException">if <see lang="true"/> and keep alive time is less than or equal to 0</exception>
        public bool AllowsCoreThreadsToTimeOut
        {
            get { return _allowCoreThreadsToTimeOut; }
            set
            {
                if (value && _keepAliveTime.Ticks <= 0)
                {
                    throw new ArgumentException("Core threads must have nonzero keep alive times");
                }

                _allowCoreThreadsToTimeOut = value;
            }
        }

        /// <summary> 
        /// Returns <see lang="true"/> if this executor is in the process of terminating
        /// after <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Shutdown()"/> or
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.ShutdownNow()"/> but has not
        /// completely terminated.  
        /// </summary>
        /// <remarks>
        /// This method may be useful for debugging. A return of <see lang="true"/> reported a sufficient
        /// period after shutdown may indicate that submitted tasks have
        /// ignored or suppressed interruption, causing this executor not
        /// to properly terminate.
        /// </remarks>
        /// <returns><see lang="true"/>if terminating but not yet terminated.</returns>
        public bool IsTerminating
        {
            get { return _currentLifecycleState == ThreadPoolState.STOP; }
        }

        /// <summary>
        /// Gets / Sets the thread factory used to create new threads.
        /// </summary>
        /// <returns>the current thread factory</returns>
        /// <exception cref="System.ArgumentNullException">if the threadfactory is null</exception>
        public IThreadFactory ThreadFactory
        {
            get { return _threadFactory; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("threadfactory");
                }
                _threadFactory = value;
            }
        }

        /// <summary> 
        /// Gets / Sets the current handler for unexecutable tasks.
        /// </summary>
        /// <returns>the current handler</returns>
        /// <exception cref="System.ArgumentNullException">if the execution handler is null.</exception>
        public IRejectedExecutionHandler RejectedExecutionHandler
        {
            get { return _rejectedExecutionHandler; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("rejectedExecutionHandler");
                }
                _rejectedExecutionHandler = value;
            }
        }

        /// <summary> 
        /// Returns the task queue used by this executor. Access to the
        /// task queue is intended primarily for debugging and monitoring.
        /// This queue may be in active use.  Retrieving the task queue
        /// does not prevent queued tasks from executing.
        /// </summary>
        /// <returns>the task queue</returns>
        public IBlockingQueue<IRunnable> Queue
        {
            get { return _workQueue; }
        }

        /// <summary> 
        /// Sets the core number of threads.  This overrides any value set
        /// in the constructor.  If the new value is smaller than the
        /// current value, excess existing threads will be terminated when
        /// they next become idle.  If larger, new threads will, if needed,
        /// be started to execute any queued tasks.
        ///
        /// @param corePoolSize the new core size
        /// @throws IllegalArgumentException if {@code corePoolSize < 0}
        /// @see #getCorePoolSize
        /// </summary>
        public int CorePoolSize
        {
            get { return _corePoolSize; }

            set
            {
                if (value < 0)
                    throw new ArgumentException("CorePoolSize cannot be less than 0");
                int delta = value - _corePoolSize;
                _corePoolSize = value;
                if (workerCountOf(_controlState.IntegerValue) > value)
                    interruptIdleWorkers();
                else if (delta > 0)
                {
                    // We don't really know how many new threads are "needed".
                    // As a heuristic, prestart enough new workers (up to new
                    // core size) to handle the current number of tasks in
                    // queue, but stop if queue becomes empty while doing so.
                    int k = Math.Min(delta, _workQueue.Count);
                    while (k-- > 0 && addWorker(null, true))
                    {
                        if (_workQueue.Count < 1)
                            break;
                    }
                }
            }
        }

        /// <summary> 
        /// Gets / Sets the maximum allowed number of threads. 
        /// </summary>
        /// <remarks>
        /// This overrides any
        /// value set in the constructor. If the new value is smaller than
        /// the current value, excess existing threads will be
        /// terminated when they next become idle.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">If value is less than zero or less than 
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>. 
        /// </exception>
        public int MaximumPoolSize
        {
            get { return _maximumPoolSize; }

            set
            {
                if (value <= 0 || value < _corePoolSize)
                    throw new ArgumentException(String.Format("Maximum pool size cannont be less than 1 and cannot be less than Core Pool Size {0}", _corePoolSize));
                _maximumPoolSize = value;
                if (workerCountOf(_controlState.IntegerValue) > value)
                    interruptIdleWorkers();
            }
        }

        /// <summary> 
        /// Returns the current number of threads in the pool.
        /// </summary>
        /// <returns>the number of threads</returns>
        public int CurrentPoolSize
        {
            get { return _currentPoolSize; }
        }

        /// <summary> 
        /// Returns the largest number of threads that have ever
        /// simultaneously been in the pool.
        /// </summary>
        /// <returns> the number of threads</returns>
        public int LargestPoolSize
        {
            get
            {
                lock (_mainLock)
                {
                    return _largestPoolSize;
                }
            }
        }

        /// <summary> 
        /// Returns the approximate total number of tasks that have been
        /// scheduled for execution. 
        /// </summary>
        /// <remarks>
        /// Because the states of tasks and
        /// threads may change dynamically during computation, the returned
        /// value is only an approximation, but one that does not ever
        /// decrease across successive calls.
        /// </remarks>
        /// <returns>the number of tasks</returns>
        public long TaskCount
        {
            get
            {
                _mainLock.Lock();
                try
                {
                    long n = _completedTaskCount;
                    foreach (var w in _currentWorkerThreads)
                    {
                        n += w._completedTasks;
                        if (w.IsLocked)
                            ++n;
                    }
                    return n + _workQueue.Count;
                }
                finally
                {
                    _mainLock.Unlock();
                }
            }
        }

        /// <summary> 
        /// Returns the approximate total number of tasks that have
        /// completed execution. 
        /// </summary>
        /// <remarks>
        /// Because the states of tasks and threads
        /// may change dynamically during computation, the returned value
        /// is only an approximation, but one that does not ever decrease
        /// across successive calls.
        /// </remarks>
        /// <returns>the number of tasks</returns>
        public long CompletedTaskCount
        {
            get
            {
                lock (_mainLock)
                {
                    long n = _completedTaskCount;
                    foreach (Worker worker in _currentWorkerThreads)
                    {
                        n += worker._completedTasks;
                    }
                    return n;
                }
            }
        }

        #endregion

        #region Private Methods

        private static bool ONLY_ONE = true;

        /// <summary>
        /// Main worker run loop.  Repeatedly gets tasks from queue and
        /// executes them, while coping with a number of issues:
        ///
        /// 1. We may start out with an initial task, in which case we
        /// don't need to get the first one. Otherwise, as long as pool is
        /// running, we get tasks from getTask. If it returns null then the
        /// worker exits due to changed pool state or configuration
        /// parameters.  Other exits result from exception throws in
        /// external code, in which case completedAbruptly holds, which
        /// usually leads processWorkerExit to replace this thread.
        ///
        /// 2. Before running any task, the lock is acquired to prevent
        /// other pool interrupts while the task is executing, and
        /// clearInterruptsForTaskRun called to ensure that unless pool is
        /// stopping, this thread does not have its interrupt set.
        ///
        /// 3. Each task run is preceded by a call to beforeExecute, which
        /// might throw an exception, in which case we cause thread to die
        /// (breaking loop with completedAbruptly true) without processing
        /// the task.
        ///
        /// 4. Assuming beforeExecute completes normally, we run the task,
        /// gathering any of its thrown exceptions to send to
        /// afterExecute. We separately handle RuntimeException, Error
        /// (both of which the specs guarantee that we trap) and arbitrary
        /// Throwables.  Because we cannot rethrow Throwables within
        /// Runnable.run, we wrap them within Errors on the way out (to the
        /// thread's UncaughtExceptionHandler).  Any thrown exception also
        /// conservatively causes thread to die.
        ///
        /// 5. After task.run completes, we call afterExecute, which may
        /// also throw an exception, which will also cause thread to
        /// die. According to JLS Sec 14.20, this exception is the one that
        /// will be in effect even if task.run throws.
        ///
        /// The net effect of the exception mechanics is that afterExecute
        /// and the thread's UncaughtExceptionHandler have as accurate
        /// information as we can provide about any problems encountered by
        /// user code.
        ///
        /// @param w the worker
        /// </summary>
        protected void runWorker(Worker w)
        {
            IRunnable task = w._firstTask;
            w._firstTask = null;
            bool completedAbruptly = true;
            try
            {
                while (task != null || (task = getTask()) != null)
                {
                    w.Lock();
                    clearInterruptsForTaskRun();
                    try
                    {
                        beforeExecute(w._thread, task);
                        try
                        {
                            task.Run();
                        }
                        finally
                        {
                            afterExecute(task);
                        }
                    }
                    finally
                    {
                        task = null;
                        w._completedTasks++;
                        w.Unlock();
                    }
                }
                completedAbruptly = false;
            }
            finally
            {
                processWorkerExit(w, completedAbruptly);
            }
        }

        /// <summary>
        /// Performs cleanup and bookkeeping for a dying worker. Called
        /// only from worker threads. Unless completedAbruptly is set,
        /// assumes that workerCount has already been adjusted to account
        /// for exit.  This method removes thread from worker set, and
        /// possibly terminates the pool or replaces the worker if either
        /// it exited due to user task exception or if fewer than
        /// corePoolSize workers are running or queue is non-empty but
        /// there are no workers.
        ///
        /// @param w the worker
        /// @param completedAbruptly if the worker died due to user exception
        /// </summary>
        private void processWorkerExit(Worker w, bool completedAbruptly)
        {
            if (completedAbruptly) // If abrupt, then workerCount wasn't adjusted
                decrementWorkerCount();

            _mainLock.Lock();
            try
            {
                _completedTaskCount += w._completedTasks;
                _currentWorkerThreads.Remove(w);
            }
            finally
            {
                _mainLock.Unlock();
            }

            tryTerminate();

            int c = _controlState.IntegerValue;
            if (runStateLessThan(c, STOP))
            {
                if (!completedAbruptly)
                {
                    int min = _allowCoreThreadTimeOut ? 0 : _corePoolSize;
                    if (min == 0 && _workQueue.Count > 0)
                        min = 1;
                    if (workerCountOf(c) >= min)
                        return; // replacement not needed
                }
                addWorker(null, false);
            }
        }

/**
     * Checks if a new worker can be added with respect to current
     * pool state and the given bound (either core or maximum). If so,
     * the worker count is adjusted accordingly, and, if possible, a
     * new worker is created and started running firstTask as its
     * first task. This method returns false if the pool is stopped or
     * eligible to shut down. It also returns false if the thread
     * factory fails to create a thread when asked, which requires a
     * backout of workerCount, and a recheck for termination, in case
     * the existence of this worker was holding up termination.
     *
     * @param firstTask the task the new thread should run first (or
     * null if none). Workers are created with an initial first task
     * (in method execute()) to bypass queuing when there are fewer
     * than corePoolSize threads (in which case we always start one),
     * or when the queue is full (in which case we must bypass queue).
     * Initially idle threads are usually created via
     * prestartCoreThread or to replace other dying workers.
     *
     * @param core if true use corePoolSize as bound, else
     * maximumPoolSize. (A boolean indicator is used here rather than a
     * value to ensure reads of fresh values after checking other pool
     * state).
     * @return true if successful
     */

        private bool addWorker(IRunnable firstTask, bool core)
        {
            retry:
            for (;;)
            {
                int c = _controlState.IntegerValue;
                int rs = runStateOf(c);

                // Check if queue empty only if necessary.
                if (rs >= SHUTDOWN && !(rs == SHUTDOWN & firstTask == null && _workQueue.Count > 0))
                    return false;

                for (;;)
                {
                    int wc = workerCountOf(c);
                    if (wc >= CAPACITY || wc >= (core ? _corePoolSize : _maximumPoolSize))
                        return false;
                    if (compareAndIncrementWorkerCount(c))
                        //		    break retry;
                        goto proceed;
                    c = _controlState.IntegerValue; // Re-read ctl
                    if (runStateOf(c) != rs)
                        //                    continue retry;
                        goto retry;
                    // else CAS failed due to workerCount change; retry inner loop
                }
            }
            proceed:

            Worker w = new Worker(this, firstTask);
            Thread t = w._thread;

            _mainLock.Lock();
            try
            {
                // Recheck while holding lock.
                // Back out on ThreadFactory failure or if
                // shut down before lock acquired.
                int c = _controlState.IntegerValue;
                int rs = runStateOf(c);

                if (t == null ||
                    (rs >= SHUTDOWN &&
                     !(rs == SHUTDOWN &&
                       firstTask == null)))
                {
                    decrementWorkerCount();
                    tryTerminate();
                    return false;
                }

                _currentWorkerThreads.Add(w);

                int s = _currentWorkerThreads.Count;
                if (s > _largestPoolSize)
                    _largestPoolSize = s;
            }
            finally
            {
                _mainLock.Unlock();
            }

            t.Start();
            // It is possible (but unlikely) for a thread to have been
            // added to workers, but not yet started, during transition to
            // STOP, which could result in a rare missed interrupt,
            // because Thread.interrupt is not guaranteed to have any effect
            // on a non-yet-started Thread (see Thread#interrupt).
            if (runStateOf(_controlState.IntegerValue) == STOP && t.IsAlive)
                t.Interrupt();

            return true;
        }

        /// <summary>
        /// Attempt to CAS-increment the workerCount field of ctl.
        /// </summary>
        private bool compareAndIncrementWorkerCount(int expect)
        {
            return _controlState.CompareAndSet(expect, expect + 1);
        }

        /// <summary>
        ///Attempt to CAS-decrement the workerCount field of ctl.
        /// </summary>
        private bool compareAndDecrementWorkerCount(int expect)
        {
            return _controlState.CompareAndSet(expect, expect - 1);
        }

        /// <summary>
        /// Decrements the workerCount field of ctl. This is called only on
        /// abrupt termination of a thread (see processWorkerExit). Other
        /// decrements are performed within getTask.
        /// </summary>
        private void decrementWorkerCount()
        {
            do
            {
            } while (! compareAndDecrementWorkerCount(_controlState.IntegerValue));
        }

        /// <summary>
        /// Method invoked upon completion of execution of the given Runnable.
        /// This method is invoked by the thread that executed the task. If
        /// non-null, the Throwable is the uncaught {@code RuntimeException}
        /// or {@code Error} that caused execution to terminate abruptly.
        ///
        /// <p>This implementation does nothing, but may be customized in
        /// subclasses. Note: To properly nest multiple overridings, subclasses
        /// should generally invoke {@code super.afterExecute} at the
        /// beginning of this method.
        ///
        /// <p><b>Note:</b> When actions are enclosed in tasks (such as
        /// {@link FutureTask}) either explicitly or via methods such as
        /// {@code submit}, these task objects catch and maintain
        /// computational exceptions, and so they do not cause abrupt
        /// termination, and the internal exceptions are <em>not</em>
        /// passed to this method. If you would like to trap both kinds of
        /// failures in this method, you can further probe for such cases,
        /// as in this sample subclass that prints either the direct cause
        /// or the underlying exception if a task has been aborted:
        ///
        ///  <pre> {@code
        /// class ExtendedExecutor extends ThreadPoolExecutor {
        ///   // ...
        ///   protected void afterExecute(Runnable r, Throwable t) {
        ///     super.afterExecute(r, t);
        ///     if (t == null && r instanceof Future<?>) {
        ///       try {
        ///         Object result = ((Future<?>) r).get();
        ///       } catch (CancellationException ce) {
        ///           t = ce;
        ///       } catch (ExecutionException ee) {
        ///           t = ee.getCause();
        ///       } catch (InterruptedException ie) {
        ///           Thread.currentThread().interrupt(); // ignore/reset
        ///       }
        ///     }
        ///     if (t != null)
        ///       System.out.println(t);
        ///   }
        /// }}</pre>
        ///
        /// @param r the runnable that has completed
        /// @param t the exception that caused termination, or null if
        /// execution completed normally
        ///
        protected void afterExecute(IRunnable r)
        {
        }

        /// <summary> 
        /// Ensures that unless the pool is stopping, the current thread
        /// does not have its interrupt set. This requires a double-check
        /// of state in case the interrupt was cleared concurrently with a
        /// shutdownNow -- if so, the interrupt is re-enabled.
        /// </summary>
        private void clearInterruptsForTaskRun()
        {
            if (runStateLessThan(_controlState.IntegerValue, STOP) &&
                !Thread.CurrentThread.IsAlive &&
                runStateAtLeast(_controlState.IntegerValue, STOP))
                Thread.CurrentThread.Interrupt();
        }

        /// <summary> 
        /// Gets the next task for a worker thread to run.
        /// </summary>
        /// <returns> the task</returns>
        private IRunnable getTask()
        {
            for (;;)
            {
                try
                {
                    switch (_currentLifecycleState)
                    {
                        case ThreadPoolState.RUNNING:
                            {
                                if (_currentPoolSize <= _corePoolSize && !_allowCoreThreadsToTimeOut)
                                {
                                    return _workQueue.Take();
                                }
                                TimeSpan timeout = _keepAliveTime;
                                if (timeout.Ticks <= 0)
                                {
                                    return null;
                                }
                                IRunnable r;
                                if (_workQueue.Poll(timeout, out r))
                                {
                                    return r;
                                }
                                if (_currentPoolSize > _corePoolSize || _allowCoreThreadsToTimeOut)
                                {
                                    return null;
                                }
                                break;
                            }
                        case ThreadPoolState.SHUTDOWN:
                            {
                                IRunnable r;
                                if (_workQueue.Poll(out r))
                                {
                                    return r;
                                }
                                if ((_workQueue.Count == 0))
                                {
                                    interruptIdleWorkers();
                                    return null;
                                }
                                return _workQueue.Take();
                            }
                        case ThreadPoolState.STOP:
                            return null;
                        default:
                            Debug.Fail("Thread poll in illegal state.");
                            break;
                    }
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        /// <summary> 
        /// Invokes the rejected execution handler for the given command.
        /// </summary>
        private void reject(IRunnable command)
        {
            _rejectedExecutionHandler.RejectedExecution(command, this);
        }

        /// <summary>
        /// Creates and returns a new <see cref="System.Threading.Thread"/>, with no first task assigned.
        /// Call only while holding the main lock.
        /// </summary>
        /// <returns>the new thread, or <see lang="null"/> if the thread factory fails to create a new thread</returns>
        private Thread addThread()
        {
            return addThread(null);
        }

        /// <summary> 
        /// Creates and returns a new thread running <paramref name="firstTask"/> as its first
        /// task. Call only while holding mainLock.
        /// </summary>
        /// <param name="firstTask">the task the new thread should run first (or <see lang="null"/> if none)</param>
        /// <returns> the new thread, or <see lang="null"/> if thread factory fails to create thread</returns>
        private Thread addThread(IRunnable firstTask)
        {
            Worker worker = new Worker(this, firstTask);
            Thread newThread = _threadFactory.NewThread(worker);
            if (newThread != null)
            {
                worker._thread = newThread;
                _currentWorkerThreads.Add(worker);
                int newThreadPoolSize = ++_currentPoolSize;
                if (newThreadPoolSize > _largestPoolSize)
                {
                    _largestPoolSize = newThreadPoolSize;
                }
            }
            return newThread;
        }

        /// <summary> 
        /// Creates and starts a new thread running <paramref name="firstTask"/> as its first
        /// task, only if fewer than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> threads are running.
        /// </summary>
        /// <param name="firstTask">the task the new thread should run first (or <see lang="null"/> if none)</param>
        /// <returns> <see lang="true"/> if successful, <see lang="false"/> otherwise.</returns>
        private bool addIfUnderCorePoolSize(IRunnable firstTask)
        {
            Thread newThread = null;
            lock (_mainLock)
            {
                if (_currentPoolSize < _corePoolSize)
                {
                    newThread = addThread(firstTask);
                }
            }
            if (newThread == null)
            {
                return false;
            }
            newThread.Start();
            return true;
        }

        /// <summary> 
        /// Creates and starts a new thread only if fewer than <see cref="Spring.Threading.Execution.ThreadPoolExecutor.MaximumPoolSize"/>
        /// threads are running.  The new thread runs as its first task the
        /// next task in queue, or if there is none, the given task.
        /// </summary>
        /// <param name="firstTask">the task the new thread should run first (or <see lang="null"/> if none)</param>
        /// <returns> <see lang="null"/> on failure, else the first task to be run by new thread.</returns>
        private IRunnable addIfUnderMaximumPoolSize(IRunnable firstTask)
        {
            Thread newThread = null;
            IRunnable nextTask = null;
            lock (_mainLock)
            {
                if (_currentPoolSize < _maximumPoolSize)
                {
                    if (!_workQueue.Poll(out nextTask))
                    {
                        nextTask = firstTask;
                    }
                    newThread = addThread(nextTask);
                }
            }
            if (newThread == null)
            {
                return null;
            }
            newThread.Start();
            return nextTask;
        }

        /// <summary> 
        /// Interrupts all threads that might be waiting for tasks.
        /// </summary>
        private void interruptIdleWorkers()
        {
            interruptIdleWorkers(false);
        }

        /// <summary> 
        /// Interrupts threads that might be waiting for tasks (as
        /// indicated by not being locked) so they can check for
        /// termination or configuration changes. Ignores
        /// SecurityExceptions (in which case some threads may remain
        /// uninterrupted).
        ///
        /// @param onlyOne If true, interrupt at most one worker. This is
        /// called only from tryTerminate when termination is otherwise
        /// enabled but there are still other workers.  In this case, at
        /// most one waiting worker is interrupted to propagate shutdown
        /// signals in case all threads are currently waiting.
        /// Interrupting any arbitrary thread ensures that newly arriving
        /// workers since shutdown began will also eventually exit.
        /// To guarantee eventual termination, it suffices to always
        /// interrupt only one idle worker, but shutdown() interrupts all
        /// idle workers so that redundant workers exit promptly, not
        /// waiting for a straggler task to finish.
        /// </summary>
        private void interruptIdleWorkers(bool onlyOne)
        {
            _mainLock.Lock();
            try
            {
                foreach (var workerThread in _currentWorkerThreads)
                {
                    var t = workerThread._thread;
                    if (t.IsAlive && workerThread.TryLock())
                    {
                        try
                        {
                            t.Interrupt();
                        }
                        finally
                        {
                            workerThread.Unlock();
                        }
                    }
                    if (onlyOne)
                        break;
                }
            }
            finally
            {
                _mainLock.Unlock();
            }
        }

        #endregion

        #region Default Constructors

        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> with the given initial
        /// parameters and default thread factory and rejected execution handler.
        /// </summary>
        /// <remarks>>
        /// It may be more convenient to use one of the <see cref="Spring.Threading.Execution.Executors"/> factory
        /// methods instead of this general purpose constructor.
        /// </remarks>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="Spring.Threading.IRunnable"/>
        /// tasks submitted by the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute(IRunnable)"/> method.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> is null</exception>
        /// <throws>  NullPointerException if <tt>workQueue</tt> is null </throws>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue)
            : this(
                corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.GetDefaultThreadFactory(),
                _defaultRejectedExecutionHandler)
        {
        }

        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> with the given initial
        /// parameters and default <see cref="Spring.Threading.Execution.RejectedExecutionException"/>.
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="Spring.Threading.IRunnable"/>
        /// tasks submitted by the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="threadFactory">
        /// <see cref="Spring.Threading.IThreadFactory"/> to use for new thread creation.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> or <paramref name="threadFactory"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IThreadFactory threadFactory)
            : this(corePoolSize, maximumPoolSize, keepAliveTime, workQueue, threadFactory, _defaultRejectedExecutionHandler)
        {
        }

        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> with the given initial
        /// parameters and <see cref="Spring.Threading.IThreadFactory"/>.
        /// </summary>
        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> with the given initial
        /// parameters and default <see cref="Spring.Threading.Execution.RejectedExecutionException"/>.
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="Spring.Threading.IRunnable"/>
        /// tasks submitted by the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="handler">
        /// The <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/> to use when execution is blocked
        /// because the thread bounds and queue capacities are reached.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> or <paramref name="handler"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IRejectedExecutionHandler handler)
            : this(corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.GetDefaultThreadFactory(), handler)
        {
        }

        /// <summary> Creates a new <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> with the given initial
        /// parameters.
        /// 
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="Spring.Threading.IRunnable"/>
        /// tasks submitted by the <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="threadFactory">
        /// <see cref="Spring.Threading.IThreadFactory"/> to use for new thread creation.
        /// </param>
        /// <param name="handler">
        /// The <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/> to use when execution is blocked
        /// because the thread bounds and queue capacities are reached.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/>, <paramref name="handler"/>, or <paramref name="threadFactory"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IThreadFactory threadFactory, IRejectedExecutionHandler handler)
        {
            if (corePoolSize < 0)
            {
                throw new ArgumentException("core pool size must be greater than or equal to zero: " + corePoolSize);
            }
            if (maximumPoolSize <= 0)
            {
                throw new ArgumentException("maximum pool size cannot be less than or equal to zero: " + maximumPoolSize);
            }
            if (maximumPoolSize < corePoolSize)
            {
                throw new ArgumentException("maximum pool size, " + maximumPoolSize + " cannot be less than core pool size, " +
                                            corePoolSize + ".");
            }
            if (keepAliveTime.Ticks < 0)
            {
                throw new ArgumentException("keep alive time must be greater than or equal to zero.");
            }
            if (workQueue == null)
            {
                throw new ArgumentNullException("workQueue");
            }
            if (threadFactory == null)
            {
                throw new ArgumentNullException("threadFactory");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _corePoolSize = corePoolSize;
            _maximumPoolSize = maximumPoolSize;
            _workQueue = workQueue;
            _keepAliveTime = keepAliveTime;
            _threadFactory = threadFactory;
            _rejectedExecutionHandler = handler;
            termination = _mainLock.NewCondition();
        }

        #endregion

        #region AbstractExecutorService Implementations

        /// <summary> 
        /// Returns <see lang="true"/> if this executor has been shut down.
        /// </summary>
        /// <returns> 
        /// Returns <see lang="true"/> if this executor has been shut down.
        /// </returns>
        public override bool IsShutdown
        {
            get { return _currentLifecycleState != ThreadPoolState.RUNNING; }
        }

        /// <summary> 
        /// Returns <see lang="true"/> if all tasks have completed following shut down.
        /// </summary>
        /// <remarks>
        /// Note that this will never return <see lang="true"/> unless
        /// either <see cref="Spring.Threading.Execution.IExecutorService.Shutdown()"/> or 
        /// <see cref="Spring.Threading.Execution.IExecutorService.ShutdownNow()"/> was called first.
        /// </remarks>
        /// <returns> <see lang="true"/> if all tasks have completed following shut down</returns>
        public override bool IsTerminated
        {
            get { return _currentLifecycleState == ThreadPoolState.TERMINATED; }
        }

        /// <summary> 
        /// Executes the given task sometime in the future.  The task
        /// may execute in a new thread or in an existing pooled thread.
        /// </summary>
        /// <remarks>
        /// If the task cannot be submitted for execution, either because this
        /// executor has been shutdown or because its capacity has been reached,
        /// the task is handled by the current <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/>
        /// for this <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>.
        /// </remarks>
        /// <param name="command">the task to execute</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">
        /// if the task cannot be accepted. 
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="command"/> is <see lang="null"/></exception>
        public override void Execute(IRunnable command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            for (;;)
            {
                if (_currentLifecycleState != ThreadPoolState.RUNNING)
                {
                    reject(command);
                    return;
                }
                if (_currentPoolSize < _corePoolSize && addIfUnderCorePoolSize(command))
                {
                    return;
                }
                if (_workQueue.Offer(command))
                {
                    return;
                }
                IRunnable r = addIfUnderMaximumPoolSize(command);
                if (r == command)
                {
                    return;
                }
                if (r == null)
                {
                    reject(command);
                    return;
                }
            }
        }

        /// <summary>
        /// Transitions control state to given target or leaves if alone if
        /// already at least the given target.
        /// </summary>
        /// <param name="targetState">the desired state, either SHUTDOWN or STOP ( but 
        /// not TIDYING or TERMINATED -- use TryTerminate for that )</param>
        private void advanceRunState(int targetState)
        {
            for (;;)
            {
                var state = _controlState.IntegerValue;
                if (runStateAtLeast(state, targetState) ||
                    _controlState.CompareAndSet(state, ctlOf(targetState, workerCountOf(state))))
                    break;
            }
        }

        /// <summary> 
        ///Performs any further cleanup following run state transition on
        /// invocation of shutdown.  A no-op here, but used by
        /// ScheduledThreadPoolExecutor to cancel delayed tasks.
        /// </summary>
        protected void onShutdown()
        {
        }

        /// <summary> 
        /// Transitions to TERMINATED state if either (SHUTDOWN and pool
        /// and queue empty) or (STOP and pool empty).  If otherwise
        /// eligible to terminate but workerCount is nonzero, interrupts an
        /// idle worker to ensure that shutdown signals propagate. This
        /// method must be called following any action that might make
        /// termination possible -- reducing worker count or removing tasks
        /// from the queue during shutdown. The method is non-private to
        /// allow access from ScheduledThreadPoolExecutor.
        /// </summary>
        private void tryTerminate()
        {
            for (;;)
            {
                int c = _controlState.IntegerValue;
                if (isRunning(c) ||
                    runStateAtLeast(c, TIDYING) ||
                    (runStateOf(c) == SHUTDOWN && _workQueue.Count > 0))
                    return;
                if (workerCountOf(c) != 0)
                {
                    // Eligible to terminate
                    interruptIdleWorkers(ONLY_ONE);
                    return;
                }

                _mainLock.Lock();
                try
                {
                    if (_controlState.CompareAndSet(c, ctlOf(TIDYING, 0)))
                    {
                        try
                        {
                            terminated();
                        }
                        finally
                        {
                            _controlState.SetNewAtomicValue((ctlOf(TERMINATED, 0)));
                            termination.SignalAll();
                        }
                        return;
                    }
                }
                finally
                {
                    _mainLock.Unlock();
                }
            }
        }

        /// <summary> 
        /// Initiates an orderly shutdown in which previously submitted
        /// tasks are executed, but no new tasks will be
        /// accepted. Invocation has no additional effect if already shut
        /// down.
        /// </summary>
        public override void Shutdown()
        {
            _mainLock.Lock();
            try
            {
                advanceRunState(SHUTDOWN);
                interruptIdleWorkers();
                onShutdown(); // hook for ScheduledThreadPoolExecutor
            }
            finally
            {
                _mainLock.Unlock();
            }
            tryTerminate();
        }

        /// <summary> 
        /// Attempts to stop all actively executing tasks, halts the
        /// processing of waiting tasks, and returns a list of the tasks
        /// that were awaiting execution. These tasks are drained (removed)
        /// from the task queue upon return from this method.
        ///
        /// <p>There are no guarantees beyond best-effort attempts to stop
        /// processing actively executing tasks.  This implementation
        /// cancels tasks via {@link Thread#interrupt}, so any task that
        /// fails to respond to interrupts may never terminate.
        ///
        /// @throws SecurityException {@inheritDoc}
        /// </summary> 
        public override IList<IRunnable> ShutdownNow()
        {
            IList<IRunnable> tasks;
            _mainLock.Lock();
            try
            {
                advanceRunState(STOP);
                interruptWorkers();
                tasks = drainQueue();
            }
            finally
            {
                _mainLock.Unlock();
            }
            tryTerminate();
            return tasks;
        }


        /// <summary> 
        /// Interrupts all threads, even if active. Ignores SecurityExceptions
        /// (in which case some threads may remain uninterrupted).
        /// </summary> 
        private void interruptWorkers()
        {
            _mainLock.Lock();
            try
            {
                foreach (var worker in _currentWorkerThreads)
                {
                    try
                    {
                        worker._thread.Interrupt();
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
            finally
            {
                _mainLock.Unlock();
            }
        }

        /// <summary> 
        /// Drains the task queue into a new list, normally using
        /// drainTo. But if the queue is a DelayQueue or any other kind of
        /// queue for which poll or drainTo may fail to remove some
        /// elements, it deletes them one by one.
        /// </summary> 
        private IList<IRunnable> drainQueue()
        {
            IBlockingQueue<IRunnable> q = _workQueue;
            IList<IRunnable> taskList = new List<IRunnable>();
            q.DrainTo(taskList);
            if (q.Count > 0)
            {
                foreach (var runnable in q)
                {
                    if (q.Remove(runnable))
                        taskList.Add(runnable);
                }
            }
            return taskList;
        }

        /// <summary> 
        /// Blocks until all tasks have completed execution after a shutdown
        /// request, or the timeout occurs, or the current thread is
        /// interrupted, whichever happens first. 
        /// </summary>
        /// <param name="duration">the time span to wait.
        /// </param>
        /// <returns> <see lang="true"/> if this executor terminated and <see lang="false"/>
        /// if the timeout elapsed before termination
        /// </returns>
        public override bool AwaitTermination(TimeSpan duration)
        {
            var durationToWait = duration;
            var deadline = DateTime.Now.Add(durationToWait);
            _mainLock.Lock();
            try
            {
                if (_currentLifecycleState == ThreadPoolState.TERMINATED)
                {
                    return true;
                }
                while (durationToWait.Ticks > 0)
                {
                    termination.Await(durationToWait);
                    if (_currentLifecycleState == ThreadPoolState.TERMINATED)
                    {
                        return true;
                    }
                    durationToWait = deadline.Subtract(DateTime.Now);
                }
                return false;
            }
            finally
            {
                _mainLock.Unlock();
            }
        }

        #endregion

        #region Public Methods

        /// <summary> 
        /// Starts a core thread, causing it to idly wait for work. 
        /// </summary>
        /// <remarks> 
        /// This overrides the default policy of starting core threads only when
        /// new tasks are executed. This method will return <see lang="false"/>
        /// if all core threads have already been started.
        /// </remarks>
        /// <returns><see lang="true"/> if a thread was started</returns>
        public bool PreStartCoreThread()
        {
            return addIfUnderCorePoolSize(null);
        }

        /// <summary> 
        /// Starts all core threads, causing them to idly wait for work. 
        /// </summary>
        /// <remarks>
        /// This overrides the default policy of starting core threads only when
        /// new tasks are executed.
        /// </remarks>
        /// <returns>the number of threads started.</returns>
        public int PreStartAllCoreThreads()
        {
            int n = 0;
            while (addIfUnderCorePoolSize(null))
            {
                ++n;
            }
            return n;
        }

        #endregion

        #region Overriddable Methods

        /// <summary> 
        /// Method invoked prior to executing the given <see cref="Spring.Threading.IRunnable"/> in the
        /// given thread.  
        /// </summary>
        /// <remarks>
        /// This method is invoked by <paramref name="thread"/> that
        /// will execute <paramref name="runnable"/>, and may be used to re-initialize
        /// ThreadLocals, or to perform logging. This implementation does
        /// nothing, but may be customized in subclasses. <b>Note:</b> To properly
        /// nest multiple overridings, subclasses should generally invoke
        /// <i>base.beforeExecute</i> at the end of this method.
        /// </remarks>
        /// <param name="thread">the thread that will run <paramref name="runnable"/>.</param>
        /// <param name="runnable">the task that will be executed.</param>
        protected internal virtual void beforeExecute(Thread thread, IRunnable runnable)
        {
        }

        /// <summary> 
        /// Method invoked upon completion of execution of the given <paramref name="runnable"/>.
        /// </summary>
        /// <remarks>
        /// This method is invoked by the thread that executed the <paramref name="runnable"/>.
        /// 
        /// <p/>
        /// <b>Note:</b> When actions are enclosed in tasks (such as
        /// <see cref="Spring.Threading.Future.FutureTask"/>) either explicitly or via methods such as
        /// <see cref="M:Spring.Threading.Execution.ExecutorCompletionService.Submit"/> these task objects catch and maintain
        /// computational exceptions, and so they do not cause abrupt
        /// termination, and the internal exceptions are <b>not</b>
        /// passed to this method.
        /// 
        /// <p/>
        /// This implementation does nothing, but may be customized in
        /// subclasses. <b>Note:</b> To properly nest multiple overridings, subclasses
        /// should generally invoke <i>base.afterExecute</i> at the
        /// beginning of this method.
        /// </remarks>
        /// <param name="runnable">the runnable that has completed.</param>
        /// <param name="exception">the exception that caused termination, or <see lang="null"/> if execution completed normally.</param>
        protected internal virtual void afterExecute(IRunnable runnable, Exception exception)
        {
        }

        /// <summary> 
        /// Method invoked when the <see cref="Spring.Threading.IExecutor"/> has terminated.  
        /// Default implementation does nothing. 
        /// <p/>
        /// <b>Note:</b> To properly nest multiple
        /// overridings, subclasses should generally invoke
        /// <i>base.terminated</i> within this method.
        /// </summary>
        protected internal virtual void terminated()
        {
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Shutsdown and disposes of this <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Helper method to dispose of this <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>
        /// </summary>
        /// <param name="disposing"><see lang="true"/> if being called from <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Dispose()"/>,
        /// <see lang="false"/> if being called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown();
            }
        }

        #region Finalizer

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ThreadPoolExecutor()
        {
            Dispose(false);
        }

        #endregion
    }
}