using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Threading;
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
		/// Worker threads
		/// </summary>
		private class Worker : IRunnable
		{
			/// <summary> 
			/// The runLock is acquired and released surrounding each task
			/// execution. It mainly protects against interrupts that are
			/// intended to cancel the worker thread from instead
			/// interrupting the task being run.
			/// </summary>
			private ReentrantLock _runLock = new ReentrantLock();

			/// <summary> 
			/// Initial task to run before entering run loop
			/// </summary>
			private IRunnable _firstTask;

			/// <summary> 
			/// Per thread completed task counter; accumulated
			/// into completedTaskCount upon termination.
			/// </summary>
			internal long _completedTasks;

			/// <summary> 
			/// Thread this worker is running in.  Acts as a final field,
			/// but cannot be set until thread is created.
			/// </summary>
			internal Thread _thread;

			/// <summary>
			/// <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> that holds this worker.
			/// </summary>
			private ThreadPoolExecutor _parentThreadPoolExecutor;

			/// <summary>
			/// Default Constructor
			/// </summary>
			/// <param name="firstTask">Task to run before entering run loop.</param>
			/// <param name="parentThreadPoolExecutor"><see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> that controls this worker</param>
			internal Worker( ThreadPoolExecutor parentThreadPoolExecutor, IRunnable firstTask )
			{
				_parentThreadPoolExecutor = parentThreadPoolExecutor;
				_firstTask = firstTask;
			}

			/// <summary>
			/// Gets a value indicating if the lock is active.
			/// </summary>
			internal bool IsLockActive
			{
				get { return _runLock.IsLocked; }
			}

			/// <summary> 
			/// Interrupt thread if not running a task.  Swallows resulting <see cref="System.Threading.ThreadInterruptedException"/>.
			/// </summary>
			internal void InterruptIfIdle()
			{
				ReentrantLock runLock = _runLock;
				if ( runLock.TryLock() )
				{
					try
					{
						_thread.Interrupt();
					}
					catch ( ThreadInterruptedException ) {}
					finally
					{
						runLock.Unlock();
					}
				}
			}

			/// <summary> 
			/// Interrupt thread even if running a task.  Swallows resulting <see cref="System.Threading.ThreadInterruptedException"/>.
			/// </summary>
			internal void InterruptNow()
			{
				try
				{
					_thread.Interrupt();
				}
				catch ( ThreadInterruptedException ) {}
			}

			/// <summary> 
			/// Run a single task between before/after methods.
			/// </summary>
			private void runTask( IRunnable task )
			{
				ReentrantLock runLock = _runLock;
				runLock.Lock();
				try
				{
					if ( _parentThreadPoolExecutor._currentLifecycleState == ThreadPoolState.STOP )
					{
						return;
					}

					bool ran = false;
					_parentThreadPoolExecutor.beforeExecute( _thread, task );
					try
					{
						task.Run();
						ran = true;
						_parentThreadPoolExecutor.afterExecute( task, null );
						++_completedTasks;
					}
					catch ( SystemException ex )
					{
						if ( !ran )
						{
							_parentThreadPoolExecutor.afterExecute( task, ex );
						}
						// Else the exception occurred within
						// afterExecute itself in which case we don't
						// want to call it again.
						throw ex;
					}
				}
				finally
				{
					runLock.Unlock();
				}
			}

			/// <summary>
			/// Runs the associated task, signalling the <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> when exiting.
			/// </summary>
			public void Run()
			{
				try
				{
					IRunnable task = _firstTask;
					_firstTask = null;
					while ( task != null || ( task = _parentThreadPoolExecutor.getTask() ) != null )
					{
						runTask( task );
					}
				}
				finally
				{
					_parentThreadPoolExecutor.workerDone( this );
				}
			}
		}

		#endregion

		#region Private Fields

		/// <summary> 
		/// Queue used for holding tasks and handing off to worker threads.
		/// </summary>
		private IBlockingQueue<IRunnable> _workQueue;

		/// <summary> 
		/// Lock held on updates to poolSize, corePoolSize, maximumPoolSize, and workers set.
		/// </summary>
		private object _mainLock = new object();

		/// <summary> 
		/// Set containing all worker threads in pool.
		/// </summary>
		private IList _currentWorkerThreads = new ArrayList();

		/// <summary> 
		///	Timeout <see cref="System.TimeSpan"/> for idle threads waiting for work.
		/// Threads use this timeout only when there are more than
		/// <see cref="Spring.Threading.Execution.ThreadPoolExecutor.CorePoolSize"/> present. Otherwise they wait forever for new work.
		/// </summary>
		private TimeSpan _keepAliveTime;

		/// <summary> 
		/// If <see lang="false"/> ( the default), core threads stay alive even when idle.
		/// If <see lang="true"/>, core threads use <see cref="Spring.Threading.Execution.ThreadPoolExecutor.KeepAliveTime"/> 
		/// to time out waiting for work.
		/// </summary>
		private bool _allowCoreThreadsToTimeOut;

		/// <summary> 
		/// Core pool size, updated only while holding a lock,
		/// but volatile to allow concurrent readability even
		/// during updates.
		/// </summary>
		private volatile int _corePoolSize;

		/// <summary> 
		/// Maximum pool size, updated only while holding a lock,
		/// but volatile to allow concurrent readability even
		/// during updates.
		/// </summary>
		private volatile int _maximumPoolSize;

		/// <summary> 
		/// Current pool size, updated only while holding a lock,
		/// but volatile to allow concurrent readability even
		/// during updates.
		/// </summary>
		private volatile int _currentPoolSize;

		/// <summary> 
		/// The <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>'s current Lifecycle state.
		/// </summary>
		internal volatile ThreadPoolState _currentLifecycleState;

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

		/// <summary> 
		/// Tracks largest attained pool size.
		/// </summary>
		private int _largestPoolSize;

		/// <summary> 
		/// Counter for completed tasks. Updated only on termination of
		/// worker threads.
		/// </summary>
		private long _completedTaskCount;

		/// <summary> 
		/// The default <see cref="Spring.Threading.Execution.IRejectedExecutionHandler"/>
		/// </summary>
		private static readonly IRejectedExecutionHandler _defaultRejectedExecutionHandler = new AbortPolicy();

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
				if ( value.Ticks < 0 )
				{
					throw new ArgumentException( "Keep alive time must be greater than 0." );
				}
				if ( value.Ticks == 0 && AllowsCoreThreadsToTimeOut )
				{
					throw new ArgumentException( "Core threads must have nonzero keep alive times" );
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
				if ( value && _keepAliveTime.Ticks <= 0 )
				{
					throw new ArgumentException( "Core threads must have nonzero keep alive times" );
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
				if ( value == null )
				{
					throw new ArgumentNullException( "threadfactory" );
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
				if ( value == null )
				{
					throw new ArgumentNullException( "rejectedExecutionHandler" );
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
		/// Gets / Sets the core number of threads.  
		/// </summary>
		/// <remarks>
		/// This overrides any value set
		/// in the constructor.  If the new value is smaller than the
		/// current value, excess existing threads will be terminated when
		/// they next become idle. If larger, new threads will, if needed,
		/// be started to execute any queued tasks.
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">if the value is less than zero.</exception>
		public int CorePoolSize
		{
			get { return _corePoolSize; }

			set
			{
				if ( value < 0 )
				{
					throw new ArgumentOutOfRangeException( "corePoolSize" );
				}
				lock ( _mainLock )
				{
					int extra = _corePoolSize - value;
					_corePoolSize = value;
					if ( extra < 0 )
					{
						int n = _workQueue.Count;
						// We have to create initially-idle threads here
						// because we otherwise have no recourse about
						// what to do with a dequeued task if addThread fails.
						while ( extra++ < 0 && n-- > 0 && _currentPoolSize < value )
						{
							Thread t = addThread();
							if ( t != null )
							{
								t.Start();
							}
							else
							{
								break;
							}
						}
					}
					else if ( extra > 0 && _currentPoolSize > value )
					{
						foreach ( Worker worker in _currentWorkerThreads )
						{
							if ( extra-- > 0 && _currentPoolSize > value && _workQueue.RemainingCapacity == 0 )
							{
								worker.InterruptIfIdle();
							}
						}
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
				if ( value <= 0 || value < _corePoolSize )
				{
					throw new ArgumentOutOfRangeException();
				}
				lock ( _mainLock )
				{
					int extra = _maximumPoolSize - value;
					_maximumPoolSize = value;
					if ( extra > 0 && _currentPoolSize > value )
					{
						foreach ( Worker worker in _currentWorkerThreads )
						{
							if ( extra > 0 && _currentPoolSize > value )
							{
								worker.InterruptIfIdle();
								--extra;
							}
						}
					}
				}
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
		/// Returns the approximate number of threads that are actively
		/// executing tasks.
		/// </summary>
		/// <returns>the active number of threads</returns>
		public int ActiveThreadCount
		{
			get
			{
				lock ( _mainLock )
				{
					int n = 0;
					foreach ( Worker worker in _currentWorkerThreads )
					{
						if ( worker.IsLockActive )
						{
							++n;
						}
					}
					return n;
				}
			}
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
				lock ( _mainLock )
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
				lock ( _mainLock )
				{
					long n = _completedTaskCount;
					foreach ( Worker worker in _currentWorkerThreads )
					{
						n += worker._completedTasks;
						if ( worker.IsLockActive )
						{
							++n;
						}
					}
					return n + _workQueue.Count;
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
				lock ( _mainLock )
				{
					long n = _completedTaskCount;
					foreach ( Worker worker in _currentWorkerThreads )
					{
						n += worker._completedTasks;
					}
					return n;
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary> 
		/// Gets the next task for a worker thread to run.
		/// </summary>
		/// <returns> the task</returns>
		private IRunnable getTask()
		{
			for ( ;; )
			{
				try
				{
					switch ( _currentLifecycleState )
					{
						case ThreadPoolState.RUNNING:
						{
							if ( _currentPoolSize <= _corePoolSize && !_allowCoreThreadsToTimeOut )
							{
								return (IRunnable) _workQueue.Take();
							}
							TimeSpan timeout = _keepAliveTime;
							if ( timeout.Ticks <= 0 )
							{
								return null;
							}
						    IRunnable r;
							if ( _workQueue.Poll(timeout, out r) )
							{
								return r;
							}
							if ( _currentPoolSize > _corePoolSize || _allowCoreThreadsToTimeOut )
							{
								return null;
							}
							break;
						}
						case ThreadPoolState.SHUTDOWN:
						{
							IRunnable r;
							if ( _workQueue.Poll(out r))
							{
								return r;
							}
							if ( ( _workQueue.Count == 0 ) )
							{
								interruptIdleWorkers();
								return null;
							}
							return (IRunnable) _workQueue.Take();
						}
						case ThreadPoolState.STOP:
							return null;
						default:
							Debug.Fail( "Thread poll in illegal state." );
							break;
					}
				}
				catch ( ThreadInterruptedException ) {}
			}
		}

		/// <summary> 
		/// Invokes the rejected execution handler for the given command.
		/// </summary>
		private void reject( IRunnable command )
		{
			_rejectedExecutionHandler.RejectedExecution( command, this );
		}

		/// <summary>
		/// Creates and returns a new <see cref="System.Threading.Thread"/>, with no first task assigned.
		/// Call only while holding the main lock.
		/// </summary>
		/// <returns>the new thread, or <see lang="null"/> if the thread factory fails to create a new thread</returns>
		private Thread addThread()
		{
			return addThread( null );
		}

		/// <summary> 
		/// Creates and returns a new thread running <paramref name="firstTask"/> as its first
		/// task. Call only while holding mainLock.
		/// </summary>
		/// <param name="firstTask">the task the new thread should run first (or <see lang="null"/> if none)</param>
		/// <returns> the new thread, or <see lang="null"/> if thread factory fails to create thread</returns>
		private Thread addThread( IRunnable firstTask )
		{
			Worker worker = new Worker( this, firstTask );
			Thread newThread = _threadFactory.NewThread( worker );
			if ( newThread != null )
			{
				worker._thread = newThread;
				_currentWorkerThreads.Add( worker );
				int newThreadPoolSize = ++_currentPoolSize;
				if ( newThreadPoolSize > _largestPoolSize )
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
		private bool addIfUnderCorePoolSize( IRunnable firstTask )
		{
			Thread newThread = null;
			lock ( _mainLock )
			{
				if ( _currentPoolSize < _corePoolSize )
				{
					newThread = addThread( firstTask );
				}
			}
			if ( newThread == null )
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
		private IRunnable addIfUnderMaximumPoolSize( IRunnable firstTask )
		{
			Thread newThread = null;
			IRunnable nextTask = null;
			lock ( _mainLock )
			{
				if ( _currentPoolSize < _maximumPoolSize )
				{
					if ( !_workQueue.Poll(out nextTask))
					{
						nextTask = firstTask;
					}
					newThread = addThread( nextTask );
				}
			}
			if ( newThread == null )
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
			lock ( _mainLock )
			{
				foreach ( Worker worker in _currentWorkerThreads )
				{
					worker.InterruptIfIdle();
				}
			}
		}

		/// <summary> 
		/// Performs bookkeeping for a terminated worker thread.</summary>
		/// <param name="workerThread">the worker</param>
		private void workerDone( Worker workerThread )
		{
			lock ( _mainLock )
			{
				_completedTaskCount += workerThread._completedTasks;
				_currentWorkerThreads.Remove( workerThread );
				if ( --_currentPoolSize > 0 )
				{
					return;
				}

				// Else, this is the last thread. Deal with potential shutdown.

				ThreadPoolState state = _currentLifecycleState;
				Debug.Assert( state != ThreadPoolState.TERMINATED );

				if ( state != ThreadPoolState.STOP )
				{
					if ( _workQueue.Count != 0 )
					{
						Thread t = addThread( null );
						if ( t != null )
						{
							t.Start();
						}
						return;
					}
					if ( state == ThreadPoolState.RUNNING )
					{
						return;
					}
				}

				Monitor.PulseAll( _mainLock );
				_currentLifecycleState = ThreadPoolState.TERMINATED;
			}
			Debug.Assert( _currentLifecycleState == ThreadPoolState.TERMINATED );
			terminated();
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
		public ThreadPoolExecutor( int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue )
			: this(
				corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.GetDefaultThreadFactory(),
				_defaultRejectedExecutionHandler ) {}

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
		public ThreadPoolExecutor( int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
		                           IThreadFactory threadFactory )
			: this( corePoolSize, maximumPoolSize, keepAliveTime, workQueue, threadFactory, _defaultRejectedExecutionHandler ) {}

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
		public ThreadPoolExecutor( int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
		                           IRejectedExecutionHandler handler )
			: this( corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.GetDefaultThreadFactory(), handler ) {}

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
		public ThreadPoolExecutor( int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
		                           IThreadFactory threadFactory, IRejectedExecutionHandler handler )
		{
			if ( corePoolSize < 0 )
			{
				throw new ArgumentException( "core pool size must be greater than or equal to zero: " + corePoolSize );
			}
			if ( maximumPoolSize <= 0 )
			{
				throw new ArgumentException( "maximum pool size cannot be less than or equal to zero: " + maximumPoolSize );
			}
			if ( maximumPoolSize < corePoolSize )
			{
				throw new ArgumentException( "maximum pool size, " + maximumPoolSize + " cannot be less than core pool size, " +
				                             corePoolSize + "." );
			}
			if ( keepAliveTime.Ticks < 0 )
			{
				throw new ArgumentException( "keep alive time must be greater than or equal to zero." );
			}
			if ( workQueue == null )
			{
				throw new ArgumentNullException( "workQueue" );
			}
			if ( threadFactory == null )
			{
				throw new ArgumentNullException( "threadFactory" );
			}
			if ( handler == null )
			{
				throw new ArgumentNullException( "handler" );
			}
			_corePoolSize = corePoolSize;
			_maximumPoolSize = maximumPoolSize;
			_workQueue = workQueue;
			_keepAliveTime = keepAliveTime;
			_threadFactory = threadFactory;
			_rejectedExecutionHandler = handler;
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
		public override void Execute( IRunnable command )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( "command" );
			}
			for ( ;; )
			{
				if ( _currentLifecycleState != ThreadPoolState.RUNNING )
				{
					reject( command );
					return;
				}
				if ( _currentPoolSize < _corePoolSize && addIfUnderCorePoolSize( command ) )
				{
					return;
				}
				if ( _workQueue.Offer( command ) )
				{
					return;
				}
				IRunnable r = addIfUnderMaximumPoolSize( command );
				if ( r == command )
				{
					return;
				}
				if ( r == null )
				{
					reject( command );
					return;
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
			bool fullyTerminated = false;
			lock ( _mainLock )
			{
				if ( _currentWorkerThreads.Count > 0 )
				{
					ThreadPoolState state = _currentLifecycleState;
					if ( state == ThreadPoolState.RUNNING )
					{
						_currentLifecycleState = ThreadPoolState.SHUTDOWN;
					}
					try
					{
						foreach ( Worker worker in _currentWorkerThreads )
						{
							worker.InterruptIfIdle();
						}
					}
					catch ( Exception )
					{
						_currentLifecycleState = state;
						throw;
					}
				}
				else
				{
					fullyTerminated = true;
					_currentLifecycleState = ThreadPoolState.TERMINATED;
					Monitor.PulseAll( _mainLock );
				}
			}
			if ( fullyTerminated )
			{
				terminated();
			}
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
		/// Attempts to stop all actively executing tasks, halts the
		/// processing of waiting tasks, and returns a list of the tasks that were
		/// awaiting execution.
		/// </summary>	
		/// <remarks> 
		/// This implementation cancels tasks via <see cref="System.Threading.Thread.Interrupt()"/>,
		/// so if any tasks mask or fail to respond to
		/// interrupts, they may never terminate.
		/// </remarks>
		/// <returns> list of tasks that never commenced execution</returns>
		public override IList<IRunnable> ShutdownNow()
		{
			bool fullyTerminated = false;
			lock ( _mainLock )
			{
				if ( _currentWorkerThreads.Count > 0 )
				{
					ThreadPoolState state = _currentLifecycleState;
					if ( state != ThreadPoolState.TERMINATED )
					{
						_currentLifecycleState = ThreadPoolState.STOP;
					}
					try
					{
						foreach ( Worker worker in _currentWorkerThreads )
						{
							worker.InterruptNow();
						}
					}
					catch ( SecurityException se )
					{
						_currentLifecycleState = state; // back out;
						throw se;
					}
				}
				else
				{
					fullyTerminated = true;
					_currentLifecycleState = ThreadPoolState.TERMINATED;
					Monitor.PulseAll( _mainLock );
				}
			}
			if ( fullyTerminated )
			{
				terminated();
			}
		    IRunnable[] remaining = new IRunnable[_workQueue.RemainingCapacity];
		    _workQueue.CopyTo(remaining, 0);
		    return remaining;
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
		public override bool AwaitTermination( TimeSpan duration )
		{
			TimeSpan durationToWait = duration;
			lock ( _mainLock )
			{
				DateTime deadline = DateTime.Now.Add( durationToWait );
				for ( ;; )
				{
					if ( _currentLifecycleState == ThreadPoolState.TERMINATED )
					{
						return true;
					}
					if ( durationToWait.Ticks <= 0 )
					{
						return false;
					}
					Monitor.Wait( _mainLock, durationToWait );
					durationToWait = deadline.Subtract( DateTime.Now );
				}
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
			return addIfUnderCorePoolSize( null );
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
			while ( addIfUnderCorePoolSize( null ) )
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
		protected internal virtual void beforeExecute( Thread thread, IRunnable runnable ) {}

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
		protected internal virtual void afterExecute( IRunnable runnable, Exception exception ) {}

		/// <summary> 
		/// Method invoked when the <see cref="Spring.Threading.IExecutor"/> has terminated.  
		/// Default implementation does nothing. 
		/// <p/>
		/// <b>Note:</b> To properly nest multiple
		/// overridings, subclasses should generally invoke
		/// <i>base.terminated</i> within this method.
		/// </summary>
		protected internal virtual void terminated() {}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Shutsdown and disposes of this <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Helper method to dispose of this <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/>
		/// </summary>
		/// <param name="disposing"><see lang="true"/> if being called from <see cref="Spring.Threading.Execution.ThreadPoolExecutor.Dispose()"/>,
		/// <see lang="false"/> if being called from finalizer.</param>
		protected virtual void Dispose( bool disposing )
		{
			if ( disposing )
			{
				Shutdown();
			}
		}

		#endregion

		#region Finalizer

		/// <summary>
		/// Finalizer
		/// </summary>
		~ThreadPoolExecutor()
		{
			Dispose( false );
		}

		#endregion
	}
}