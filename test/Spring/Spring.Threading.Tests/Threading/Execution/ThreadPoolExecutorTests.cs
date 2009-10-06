using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution.ExecutionPolicy;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    [TestFixture]
    public class ThreadPoolExecutorTests : ExecutorServiceTestFixture
    {
        protected new ThreadPoolExecutor ExecutorService
        {
            get
            {
                return (ThreadPoolExecutor)base.ExecutorService;
            }
            set
            {
                base.ExecutorService = value;
            }
        }

        protected override IExecutorService NewExecutorService()
        {
            return NewThreadPoolExecutor(2, 2);
        }

        protected static ThreadPoolExecutor NewThreadPoolExecutor(int corePoolSize, int maxPoolSize)
        {
            return new ThreadPoolExecutor(corePoolSize, maxPoolSize, LONG_DELAY, 
                new ArrayBlockingQueue<IRunnable>(10));
        }

        private static ThreadPoolExecutor NewThreadPoolExecutorWithThreadFactory(int corePoolSize, int maxPoolSize)
        {
            return new ThreadPoolExecutor(corePoolSize, maxPoolSize, LONG_DELAY, 
                new ArrayBlockingQueue<IRunnable>(10), new SimpleThreadFactory());
        }

        private static ThreadPoolExecutor NewThreadPoolExecutorWithRejectHandler(int corePoolSize, int maxPoolSize)
        {
            return new ThreadPoolExecutor(corePoolSize, maxPoolSize, LONG_DELAY,
                new ArrayBlockingQueue<IRunnable>(10), new NoOpREHandler());
        }

        private static ThreadPoolExecutor NewThreadPoolExecutorWithBoth(int corePoolSize, int maxPoolSize)
        {
            return new ThreadPoolExecutor(corePoolSize, maxPoolSize, LONG_DELAY,
                new ArrayBlockingQueue<IRunnable>(10), new SimpleThreadFactory(), new NoOpREHandler());
        }

        [Test] public void ActiveCountIncreasesWhenThreadBecomeActiveButDoesNotOverestimate()
        {
            var es = ExecutorService;
            Assert.AreEqual(0, es.ActiveCount);

            es.Execute(_mediumInterruptableAction);
            Thread.Sleep(SHORT_DELAY);

            Assert.AreEqual(1, es.ActiveCount);
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }


        [Test] public void PreStartCoreThreadStartsOneThreadWhenUnderCorePoolSize() {
            var es = ExecutorService;
            Assert.AreEqual(0, es.PoolSize);
            Assert.IsTrue(es.PreStartCoreThread());
            Assert.AreEqual(1, es.PoolSize);
            Assert.IsTrue(es.PreStartCoreThread());
            Assert.AreEqual(2, es.PoolSize);
            Assert.IsFalse(es.PreStartCoreThread());
            Assert.AreEqual(2, es.PoolSize);
            JoinPool(es);
        }

        [Test] public void PreStartAllCoreThreadsStartsAllCorePoolSizeThreads() {
            var es = ExecutorService;
            Assert.AreEqual(0, es.PoolSize);
            es.PreStartAllCoreThreads();
            Assert.AreEqual(2, es.PoolSize);
            es.PreStartAllCoreThreads();
            Assert.AreEqual(2, es.PoolSize);
            JoinPool(es);
        }
       
        [Test] public void CompletedTaskCountIncreasesWhenTasksCompleteButDoesNotOverestimate()
        {
            var es = ExecutorService;
            Assert.AreEqual(0, es.CompletedTaskCount);

            es.Execute(ThreadManager.NewVerifiableTask(() => { }));
            Thread.Sleep(SHORT_DELAY);

            Assert.AreEqual(1, es.CompletedTaskCount);
            JoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void CorePoolSizeReturnsSizeGivenInConstructor([Values(1,3)] int corePoolSize) 
        {
            ExecutorService = NewThreadPoolExecutor(corePoolSize, 3);
            Assert.AreEqual(corePoolSize, ExecutorService.CorePoolSize);
            JoinPool(ExecutorService);
        }

        [Test] public void KeepAliveTimeRetunsValueGevinInConstructor([Values(1,3)] int keepAliveSeconds)
        {
            var keepAlive = TimeSpan.FromSeconds(keepAliveSeconds);
            ExecutorService = new ThreadPoolExecutor(2, 2, keepAlive, new ArrayBlockingQueue<IRunnable>(10));
            Assert.AreEqual(keepAlive, ExecutorService.KeepAliveTime);
            JoinPool(ExecutorService);
        }

        [Test] public void ThreadFactoryReturnsFactoryGivenInConstructor() {
            IThreadFactory tf = MockRepository.GenerateStub<IThreadFactory>();

            ExecutorService = new ThreadPoolExecutor(1,2,LONG_DELAY, new ArrayBlockingQueue<IRunnable>(10), tf, new NoOpREHandler());

            Assert.AreSame(tf, ExecutorService.ThreadFactory);
            JoinPool(ExecutorService);
        }

   
        [Test] public void ThreadFactorySetterSetsTheThreadFactory() 
        {
            IThreadFactory tf = MockRepository.GenerateStub<IThreadFactory>();
            tf.Stub(f => f.NewThread(Arg<IRunnable>.Is.NotNull)).Do(
                new Delegates.Function<Thread, IRunnable>(r => ThreadManager.NewVerifiableThread(r.Run)));

            var es = ExecutorService;
            es.ThreadFactory=tf;
            es.PreStartCoreThread();

            Assert.AreEqual(tf, es.ThreadFactory);
            tf.AssertWasCalled(f => f.NewThread(Arg<IRunnable>.Is.NotNull));
            JoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ThreadFactoryChokesOnNullValue() 
        {
            Assert.Throws<ArgumentNullException>(()=>ExecutorService.ThreadFactory = null);
        }

        [Test] public void RejectedExecutionHandlerReturnsHandlerGivenInConstructor() {
            var h = MockRepository.GenerateStub<IRejectedExecutionHandler>();

            ExecutorService = new ThreadPoolExecutor(1,2,LONG_DELAY, new ArrayBlockingQueue<IRunnable>(10), h);

            Assert.AreEqual(h, ExecutorService.RejectedExecutionHandler);
            JoinPool(ExecutorService);
        }

        [Test] public void RejectedExecutionHandlerSetterSetsTheHandler() {
            ExecutorService = new ThreadPoolExecutor(1,1,LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1));
            var h = MockRepository.GenerateStub<IRejectedExecutionHandler>();
            var es = ExecutorService;

            es.RejectedExecutionHandler = h;
            es.Execute(_mediumInterruptableAction);
            es.Execute(_mediumInterruptableAction);
            es.Execute(_mediumInterruptableAction); //this should be rejected

            Assert.AreEqual(h, es.RejectedExecutionHandler);
            h.AssertWasCalled(a => a.RejectedExecution(Arg<IRunnable>.Is.NotNull, Arg<ThreadPoolExecutor>.Is.Equal(es)));
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void RejectedExecutionHandlerSetterChokesOnNullArgument() 
        {
            Assert.Throws<ArgumentNullException>(() => ExecutorService.RejectedExecutionHandler = null);
            JoinPool(ExecutorService);
        }

        [Test] public void LargestPoolSizIncreasesWhenMultipleThreadsActiveButDoesNotOverestimate() 
        {
            var es = ExecutorService;
            Assert.AreEqual(0, es.LargestPoolSize);

            es.Execute(_mediumInterruptableAction);
            es.Execute(_mediumInterruptableAction);

            Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(2, es.LargestPoolSize);
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void MaximumPoolSizeReturnsValueGivenInConstructor() 
        {
            ExecutorService = NewThreadPoolExecutor(2, 2);
            Assert.AreEqual(2, ExecutorService.MaximumPoolSize);
            JoinPool(ExecutorService);
        }

        [Test] public void PoolSizeIncreasesWhenThreadsBecomeActiveButDoesNotOverestimate([Values(1,3)] int poolSize) 
        {
            var es = NewThreadPoolExecutor(poolSize, poolSize);
            ExecutorService = es;
            Assert.AreEqual(0, es.PoolSize);

            // submit one more them max size.
            for (int i = 0; i < poolSize + 1; i++) es.Execute(_mediumInterruptableAction);

            Assert.AreEqual(poolSize, es.PoolSize);
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void TaskCountIncreasesWhenTaskSubmittedButDoesNotOverestimate(
            [Values(1,2)] int poolSize, [Values(1,3)] int taskCount) 
        {
            var es = NewThreadPoolExecutor(poolSize, poolSize);
            ExecutorService = es;
            Assert.AreEqual(0, es.TaskCount);

            for (int i = 0; i < taskCount; i++) es.Execute(ThreadManager.NewVerifiableTask(() => { }));
            for (int i = 0; i < taskCount; i++) es.Execute(_mediumInterruptableAction);

            Assert.That(es.TaskCount, Is.LessThanOrEqualTo(taskCount * 2));
            Assert.That(es.TaskCount, Is.GreaterThanOrEqualTo(Math.Min(taskCount * 2, poolSize)));
            Thread.Sleep(SHORT_DELAY); //TODO: why do we need to wait?
            Assert.AreEqual(taskCount * 2, es.TaskCount);
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }


        [Test, Description("Transition to terminating, then terminated upon shutdown")]
        public void TransitionToTerminatingThenTerminiatedUponShutdown() 
        {
            var es = ExecutorService;
            Assert.IsFalse(es.IsTerminated);
            Assert.IsFalse(es.IsTerminating);
            es.Execute(ThreadManager.NewVerifiableTask(()=>Thread.Sleep(SHORT_DELAY)));
            es.Shutdown();
            Assert.IsFalse(es.IsTerminated);
            Assert.IsTrue(es.IsTerminating);
            Assert.IsTrue(es.AwaitTermination(LONG_DELAY));
            Assert.IsFalse(es.IsTerminating);
            Assert.IsTrue(es.IsTerminated);
        }

        [Test, Description("Queue returns the work queue, which Contains queued tasks")]
        public void QueueRetursWorkQueuContainsQueuedTasks()
        {
            IBlockingQueue<IRunnable> q = new ArrayBlockingQueue<IRunnable>(10);
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, q);
            ExecutorService = es;
            FutureTask<bool>[] tasks = new FutureTask<bool>[_size];
            for (int i = 0; i < _size; i++)
            {
                tasks[i] = new FutureTask<bool>(_mediumInterruptableAction, true);
                es.Execute(tasks[i]);
            }
            Thread.Sleep(SHORT_DELAY);
            IBlockingQueue<IRunnable> wq = es.Queue;
            Assert.AreEqual(q, wq);
            Assert.IsFalse(wq.Contains(tasks[0]));
            Assert.IsTrue(wq.Contains(tasks[_size-1]));
            for (int i = 1; i < _size; ++i)
                tasks[i].Cancel(true);
        }

        [Test, Description("Remove(runnable) removes queued task, and fails to remove active task")]
        public void RemoveRunnableRemovesQueuedTaskAndFailesToRemoveActiveTask()
        {
            IBlockingQueue<IRunnable> q = new ArrayBlockingQueue<IRunnable>(10);
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, q);
            ExecutorService = es;
            FutureTask<bool>[] tasks = new FutureTask<bool>[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = new FutureTask<bool>(_mediumInterruptableAction, true);
                es.Execute(tasks[i]);
            }
            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(es.Remove(tasks[0]));
            Assert.IsTrue(q.Contains(tasks[4]));
            Assert.IsTrue(q.Contains(tasks[3]));
            Assert.IsTrue(es.Remove(tasks[4]));
            Assert.IsFalse(es.Remove(tasks[4]));
            Assert.IsFalse(q.Contains(tasks[4]));
            Assert.IsTrue(q.Contains(tasks[3]));
            Assert.IsTrue(es.Remove(tasks[3]));
            Assert.IsFalse(q.Contains(tasks[3]));
            InterruptAndJoinPool(es);
        }

        [Test, Description("Remove(action) removes queued task, and fails to remove active task")]
        public void RemoveActionRemovesQueuedTaskAndFailesToRemoveActiveTask()
        {
            IBlockingQueue<IRunnable> q = new ArrayBlockingQueue<IRunnable>(10);
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, q);
            ExecutorService = es;
            var tasks = new Action[5];
            for (int i = 0; i < 5; i++)
            {
                int timeout = i;
                tasks[i] = NewInterruptableAction(MEDIUM_DELAY + TimeSpan.FromMilliseconds(timeout));
                es.Execute(tasks[i]);
            }
            Thread.Sleep(SHORT_DELAY);
            Assert.IsFalse(es.Remove(tasks[0]));
            Assert.That(q.Count, Is.EqualTo(4));
            Assert.IsTrue(es.Remove(tasks[4]));
            Assert.IsFalse(es.Remove(tasks[4]));
            Assert.That(q.Count, Is.EqualTo(3));
            Assert.IsTrue(es.Remove(tasks[3]));
            Assert.That(q.Count, Is.EqualTo(2));
            InterruptAndJoinPool(es);
        }

        [Test, Description("Purge Removes Cancelled tasks from the queue")]
        public void PurgeRemmovesCancelledTasksFromTheQueue()
        {
            var es = NewThreadPoolExecutor(1, 1);
            ExecutorService = es;
            FutureTask<bool>[] tasks = new FutureTask<bool>[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = new FutureTask<bool>(_mediumInterruptableAction, true);
                es.Execute(tasks[i]);
            }
            tasks[4].Cancel(true);
            tasks[3].Cancel(true);
            es.Purge();
            Assert.That(es.TaskCount, Is.InRange(2, 4));
            Assert.That(es.Queue.Count, Is.EqualTo(2));
            InterruptAndJoinPool(es);
        }

        [Test, Description("Constructor throws if corePoolSize argument is less than zero")]
        public void ConstructorChokesOnNegativeCorePoolSize()
        {
            const string expected = "corePoolSize";
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutor(-1, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithThreadFactory(-1, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithRejectHandler(-1, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithBoth(-1, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if maximumPoolSize is non positive")]
        public void ConstructorChokesOnNonPositiveMaximumPoolSize([Values(0, -1)] int maxPoolSize)
        {
            const string expected = "maximumPoolSize";
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutor(1, maxPoolSize));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithThreadFactory(1, maxPoolSize));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithRejectHandler(1, maxPoolSize));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => NewThreadPoolExecutorWithBoth(1, maxPoolSize));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if keepAliveTime is less than zero")]
        public void ConstructorChokesOnNegativeKeepAliveTime()
        {
            const string expected = "keepAliveTime";
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new ThreadPoolExecutor(1,2, TimeSpan.FromTicks(-1), 
                    new ArrayBlockingQueue<IRunnable>(10)));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new ThreadPoolExecutor(1, 2, TimeSpan.FromTicks(-1), 
                    new ArrayBlockingQueue<IRunnable>(10), new SimpleThreadFactory()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new ThreadPoolExecutor(1, 2, TimeSpan.FromTicks(-1),
                    new ArrayBlockingQueue<IRunnable>(10), new NoOpREHandler()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentOutOfRangeException>(
                () => new ThreadPoolExecutor(1, 2, TimeSpan.FromTicks(-1),
                    new ArrayBlockingQueue<IRunnable>(10), new SimpleThreadFactory(), new NoOpREHandler()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if maximumPoolSize is less than corePoolSize")]
        public void ConstructorChokesWhenMaximumPoolSizeIsLessThenCorePoolSize()
        {
            const string expected = "maximumPoolSize";
            var e = Assert.Throws<ArgumentException>(
                () => NewThreadPoolExecutor(2, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentException>(
                () => NewThreadPoolExecutorWithThreadFactory(2, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentException>(
                () => NewThreadPoolExecutorWithRejectHandler(2, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentException>(
                () => NewThreadPoolExecutorWithBoth(2, 1));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if workQueue is set to null")]
        public void ConstructorChokesOnNullWorkQueue()
        {
            const string expected = "workQueue";
            var e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, null));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, null, 
                    new SimpleThreadFactory()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, null, 
                    new NoOpREHandler()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, null, 
                    new SimpleThreadFactory(), new NoOpREHandler()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if threadFactory is set to null")]
        public void ConstructorChokesOnNullThreadFactory()
        {
            const string expected = "threadFactory";
            const IThreadFactory f = null;
            var e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, 
                    new ArrayBlockingQueue<IRunnable>(10), f));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, 
                    new ArrayBlockingQueue<IRunnable>(10), f, new NoOpREHandler()));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Constructor throws if handler is set to null")]
        public void ConstructorChokesOnNullHandler()
        {
            const string expected = "handler";
            const IRejectedExecutionHandler r = null;
            var e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, 
                    new ArrayBlockingQueue<IRunnable>(10), r));
            Assert.That(e.ParamName, Is.EqualTo(expected));
            e = Assert.Throws<ArgumentNullException>(
                () => new ThreadPoolExecutor(1, 2, LONG_DELAY, 
                    new ArrayBlockingQueue<IRunnable>(10), new SimpleThreadFactory(), r));
            Assert.That(e.ParamName, Is.EqualTo(expected));
        }

        [Test, Description("Executor using CallerRunsPolicy runs task in caller thread if saturated")]
        public void ExecuteRunsTaskInCurrentThreadWhenSaturatedWithCallerRunsPolicy()
        {
            IRejectedExecutionHandler h = new CallerRunsPolicy();
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            ExecutorService = es;
            IRunnable[] tasks = new IRunnable[_size];
            var caller = Thread.CurrentThread;
            for (int i = 0; i < _size; ++i)
            {
                tasks[i] = MockRepository.GenerateStub<IRunnable>();
                tasks[i].Stub(r => r.Run()).Do(
                    ThreadManager.NewVerifiableTask(
                        () => Assert.That(Thread.CurrentThread, Is.EqualTo(caller))));
            }

            es.Execute(_mediumInterruptableAction);
            for (int i = 0; i < _size; ++i) es.Execute(tasks[i]);

            for (int i = 1; i < _size; ++i)
                tasks[i].AssertWasCalled(r => r.Run());
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("Executor using DiscardPolicy drops task if saturated")]
        public void ExecuteDropsTaskWhenSaturatedWithDiscardPolicy()
        {
            IRejectedExecutionHandler h = new DiscardPolicy();
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            ExecutorService = es;
            IRunnable[] tasks = new IRunnable[_size];
            for (int i = 0; i < _size; ++i)
                tasks[i] = MockRepository.GenerateStub<IRunnable>();

            es.Execute(_mediumInterruptableAction);
            for (int i = 0; i < _size; ++i) es.Execute(tasks[i]);

            for (int i = 1; i < _size; ++i)
                tasks[i].AssertWasNotCalled(r => r.Run());
            InterruptAndJoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("Executor using DiscardOldestPolicy drops oldest task if saturated")]
        public void ExecuteDropsOldestTaskWhenSaturatedWithDiscardOldestPolicy()
        {
            IRejectedExecutionHandler h = new DiscardOldestPolicy();
            var es = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            ExecutorService = es;
            es.Execute(_mediumInterruptableAction);
            var r2 = MockRepository.GenerateStub<IRunnable>();
            es.Execute(r2);
            Assert.IsTrue(es.Queue.Contains(r2));
            var r3 = MockRepository.GenerateStub<IRunnable>();
            es.Execute(r3);
            Assert.IsFalse(es.Queue.Contains(r2));
            Assert.IsTrue(es.Queue.Contains(r3));
            InterruptAndJoinPool(es);
        }

        private void AssertExecutorDropsTaskOnShutdown(IExecutorService executor)
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            executor.Shutdown();

            executor.Execute(runnable);

            JoinPool(executor);
            runnable.AssertWasNotCalled(r => r.Run());
        }

        [Test, Description("Executor using CallerRunsPolicy drops task on shutdown")]
        public void ExecuteDropsTaskOnShutdownWithCallerRunsPolicy()
        {
            IRejectedExecutionHandler h = new CallerRunsPolicy();
            ExecutorService = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            AssertExecutorDropsTaskOnShutdown(ExecutorService);
        }

        [Test, Description("Executor using DiscardPolicy drops task on shutdown")]
        public void ExecuteDropsTaskOnShutdownWithDiscardPolicy()
        {
            IRejectedExecutionHandler h = new DiscardPolicy();
            ExecutorService = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            AssertExecutorDropsTaskOnShutdown(ExecutorService);
        }

        [Test, Description("Executor using DiscardOldestPolicy drops task on shutdown")]
        public void ExecuteDropsOldestTaskOnShutdownWithDiscardOldestPolicy()
        {
            IRejectedExecutionHandler h = new DiscardOldestPolicy();
            ExecutorService = new ThreadPoolExecutor(1, 1, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(1), h);
            AssertExecutorDropsTaskOnShutdown(ExecutorService);
        }

        [Test, Description("CorePoolSize setter throws if given a negative value")]
        public void CorePoolSizeSetterChokesOnNegativeValue()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => ExecutorService.CorePoolSize = -1);
            Assert.That(e.ParamName, Is.EqualTo("value"));
            JoinPool(ExecutorService);
        }

        [Test, Description("MaximumPoolSize setter throws if given a value less the core pool size")]
        public void MaximumPoolSizeSetterChokesIfLessThenCorePoolSize()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => ExecutorService.MaximumPoolSize = 1);
            Assert.That(e.ParamName, Is.EqualTo("value"));
            JoinPool(ExecutorService);
        }

        [Test, Description("MaximumPoolSize setter throws if given a negative value")]
        public void MaximumPoolSizeSetterChokesOnNegativeValue()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => ExecutorService.MaximumPoolSize = -1);
            Assert.That(e.ParamName, Is.EqualTo("value"));
            JoinPool(ExecutorService);
        }

        [Test, Description("KeepAliveTime setter throws if given a negative value")]
        public void KeepAliveTimeSetterChokesOnNegativeValue()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(
                () => ExecutorService.KeepAliveTime = TimeSpan.FromTicks(-1));
            Assert.That(e.ParamName, Is.EqualTo("value"));
            JoinPool(ExecutorService);
        }

        [Test, Description("Terminated is called on termination")]
        public void TerminatedIsCalledOnTermination()
        {
            var es = Mockery.GeneratePartialMock<ThreadPoolExecutor>(1, 1, LONG_DELAY, new SynchronousQueue<IRunnable>());
            ExecutorService = es;
            es.Shutdown();
            es.AssertWasCalled(e => e.Terminated());
            JoinPool(es);
        }

        [Test, Description("BeforeExecute and AfterExecute are called when executing task")]
        public void BeforeAfterExecuteAreCalledWhenExecutingTask()
        {
            var runnable = MockRepository.GenerateStub<IRunnable>();
            var es = Mockery.GeneratePartialMock<ThreadPoolExecutor>(1, 1, LONG_DELAY, new SynchronousQueue<IRunnable>());
            ExecutorService = es;
            try
            {
                es.Execute(runnable);
                Thread.Sleep(SHORT_DELAY);
                Mockery.Assert(
                    es.ActivityOf(e => e.AfterExecute(runnable, null))
                    > runnable.ActivityOf(r => r.Run())
                    > es.ActivityOf(e => e.BeforeExecute(Arg<Thread>.Is.NotNull, Arg<IRunnable>.Is.Equal(runnable))));
            }
            finally // workaround a bug in RhinoMocks.
            {
                es.Shutdown(); 
                Thread.Sleep(SHORT_DELAY); 
            }
            JoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("BeforeExecute and AfterExecute are called when executing failed task")]
        public void BeforeAfterExecuteAreCalledWhenExecutingFailedTaks()
        {
            var ex = new NullReferenceException();
            var runnable = MockRepository.GenerateStub<IRunnable>();
            runnable.Stub(r => r.Run()).Throw(ex);
            var noExceptionFactory = MockRepository.GenerateStub<IThreadFactory>();
            noExceptionFactory.Stub(f => f.NewThread(Arg<IRunnable>.Is.Anything)).Do(
                new Delegates.Function<Thread, IRunnable>(
                    r => new Thread(() => { try { r.Run(); } catch(NullReferenceException) { } })));
            var es = Mockery.GeneratePartialMock<ThreadPoolExecutor>(1, 1, LONG_DELAY,
                new SynchronousQueue<IRunnable>(), noExceptionFactory);
            ExecutorService = es;

            try
            {
                es.Execute(runnable);
                Thread.Sleep(SHORT_DELAY);
                es.AssertWasCalled(e => e.AfterExecute(runnable, ex));
            }
            finally // workaround a bug in RhinoMocks.
            {
                es.Shutdown();
                Thread.Sleep(SHORT_DELAY);
            }
            JoinPool(es);
            ThreadManager.JoinAndVerify();
        }

        [Test, Description("Execution continues with one thread even if thread factory fails to create more")]
        public void ExecutionContinuesWithOneThreadWhenFactoryFailesToCreateMore()
        {
            var nRun = new AtomicInteger(0);
            const int nTasks = 100;

            var failingThreadFactory = MockRepository.GenerateStub<IThreadFactory>();
            failingThreadFactory.Stub(f => f.NewThread(Arg<IRunnable>.Is.NotNull))
                .Do(new Function<IRunnable, Thread>(r => new Thread(r.Run))).Repeat.Once();

            var es = new ThreadPoolExecutor(nTasks, nTasks, LONG_DELAY,
                new LinkedBlockingQueue<IRunnable>(), failingThreadFactory);
            ExecutorService = es;

            for (int k = 0; k < nTasks; ++k)
            {
                es.Execute(() => nRun.IncrementValueAndReturn());
            }
            for (int i = 0; i < 20 && nRun.Value < nTasks; i++) Thread.Sleep(SHORT_DELAY);
            Assert.That(es.PoolSize, Is.EqualTo(1));
            Assert.AreEqual(nTasks, nRun.Value);
        }

        [Test, Description("AllowsCoreThreadTimeOut is by default false")]
        public void AllowsCoreThreadTimeOutIsFalseByDefault()
        {
            ExecutorService = new ThreadPoolExecutor(2, 2, new TimeSpan(0, 0, 0, 0, 1000), new ArrayBlockingQueue<IRunnable>(10));
            Assert.IsFalse(ExecutorService.AllowsCoreThreadsToTimeOut);
        }

        [Test, Description("AllowsCoreThreadTimeOut value controls if idle threads timeout")]
        public void AllowsCoreThreadTimeOutControlsIdleThreadTimeout([Values(true, false)] bool isTimeout)
        {
            var es = new ThreadPoolExecutor(2, 10, TimeSpan.FromMilliseconds(10), new ArrayBlockingQueue<IRunnable>(10));
            ExecutorService = es;
            es.AllowsCoreThreadsToTimeOut = isTimeout;
            es.Execute(ThreadManager.NewVerifiableTask(() => { }));
            Thread.Sleep(SHORT_DELAY);
            if (isTimeout)
                Assert.That(es.PoolSize, Is.EqualTo(0));
            else
                Assert.That(es.PoolSize, Is.GreaterThan(0));
        }


        [Test, Description("Execute allows the same task to be Submitted multiple times, even if rejected")]
        public void ExecuteAllowsResubmitSameTask()
        {
            const int nTasks = 1000;
            AtomicInteger nRun = new AtomicInteger(0);
            IRunnable runnable = new Runnable(() => nRun.IncrementValueAndReturn());
            var es = new ThreadPoolExecutor(1, 30, LONG_DELAY, new ArrayBlockingQueue<IRunnable>(30));
            ExecutorService = es;
            for (int i = 0; i < nTasks; ++i)
            {
                for (; ; )
                {
                    try
                    {
                        es.Execute(runnable);
                        break;
                    }
                    catch (RejectedExecutionException)
                    {
                    }
                }
            }
            // enough time to run all tasks
            for (int i = 0; i < 20 && nRun.Value < nTasks; i++) Thread.Sleep(SHORT_DELAY);
            Assert.AreEqual(nRun.Value, nTasks);
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ThreadPoolExecutorLargePoolTests<T> : ExecutorServiceTestFixture<T>
    {
        protected override IExecutorService NewExecutorService()
        {
            return new ThreadPoolExecutor(50, 50, LONG_DELAY, new SynchronousQueue<IRunnable>());
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ThreadPoolExecutorSmallPoolTests<T> : ExecutorServiceTestFixture<T>
    {
        protected override IExecutorService NewExecutorService()
        {
            return new ThreadPoolExecutor(_size, _size, LONG_DELAY, new SynchronousQueue<IRunnable>());
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ThreadPoolExecutoTinyPoolTests<T> : ExecutorServiceTestFixture<T>
    {
        protected override IExecutorService NewExecutorService()
        {
            return new ThreadPoolExecutor(1, 1, LONG_DELAY, new LinkedBlockingQueue<IRunnable>(10));
        }
    }

    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ThreadPoolExecutoMediumPoolTests<T> : ExecutorServiceTestFixture<T>
    {
        protected override IExecutorService NewExecutorService()
        {
            return new ThreadPoolExecutor(1, 20, LONG_DELAY, new LinkedBlockingQueue<IRunnable>(10));
        }
    }

}