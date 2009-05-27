using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution.ExecutionPolicy;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    [TestFixture]
    public class ThreadPoolExecutorTests : BaseThreadingTestCase
    {
        private class ExtendedTPE : ThreadPoolExecutor
        {
            public volatile bool afterCalled;
            public volatile bool beforeCalled;
            public volatile bool terminatedCalled;

            public ExtendedTPE()
                : base(1, 1, LONG_DELAY_MS, new SynchronousQueue<IRunnable>())
            {
            }

            protected override void beforeExecute(Thread t, IRunnable r)
            {
                beforeCalled = true;
            }

            protected void afterExecute(IRunnable r)
            {
                afterCalled = true;
            }

            protected override void terminated()
            {
                terminatedCalled = true;
            }
        }

        private class FailingIThreadFactory : IThreadFactory
        {
            private int calls;

            #region IIThreadFactory Members

            public Thread NewThread(IRunnable runnable)
            {
                if (++calls > 1) return null;
                return new Thread((runnable.Run));
            }

            #endregion
        }

        public class AnonymousClassRunnable : IRunnable
        {
            private readonly ThreadPoolExecutor p;

            public AnonymousClassRunnable(ThreadPoolExecutor p)
            {
                this.p = p;
            }

            #region IRunnable Members

            public void Run()
            {
                try
                {
                    Thread.Sleep(SHORT_DELAY_MS);
                }
                catch (ThreadInterruptedException)
                {
                    Assert.Fail("Unexpected exception");
                }
            }

            #endregion
        }

        [Test] public void Execute()
        {
            var p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            try
            {
                p1.Execute(new AnonymousClassRunnable(p1));
                Thread.Sleep(SMALL_DELAY_MS);
            }
            catch (ThreadInterruptedException)
            {
                Assert.Fail("Unexpected exception");
            }
            JoinPool(p1);
        }
        [Test] public void GetActiveCount()
        {
            ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            Assert.AreEqual(0, p2.ActiveCount);
            p2.Execute(new MediumRunnable());
            try
            {
                Thread.Sleep(SHORT_DELAY_MS);
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected exception");
            }
            Assert.AreEqual(1, p2.ActiveCount);
            JoinPool(p2);
        }


    [Test] public void testPreStartCoreThread() {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(0, p2.PoolSize);
        Assert.IsTrue(p2.PreStartCoreThread());
        Assert.AreEqual(1, p2.PoolSize);
        Assert.IsTrue(p2.PreStartCoreThread());
        Assert.AreEqual(2, p2.PoolSize);
        Assert.IsFalse(p2.PreStartCoreThread());
        Assert.AreEqual(2, p2.PoolSize);
        JoinPool(p2);
    }

   
    [Test] public void testPreStartAllCoreThreads() {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(0, p2.PoolSize);
        p2.PreStartAllCoreThreads();
        Assert.AreEqual(2, p2.PoolSize);
        p2.PreStartAllCoreThreads();
        Assert.AreEqual(2, p2.PoolSize);
        JoinPool(p2);
    }

   
    [Test] public void testGetCompletedTaskCount()
    {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(0, p2.CompletedTaskCount);
        p2.Execute(new ShortRunnable());
        try
        {
            Thread.Sleep(SMALL_DELAY_MS);
        }
        catch (Exception e)
        {
            Assert.Fail("Unexpected Exception");
        }
        Assert.AreEqual(1, p2.CompletedTaskCount);
        try
        {
            p2.Shutdown();
        }
        catch (SecurityException ok)
        {
            return;
        }
        JoinPool(p2);
    }
    
    [Test] public void testGetCorePoolSize() {
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(1, p1.CorePoolSize);
        JoinPool(p1);
        }
    [Test] public void testGetKeepAliveTime()
    {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, new TimeSpan(0, 0, 1), new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(1, p2.KeepAliveTime.TotalSeconds);
        JoinPool(p2);
    }


      
    [Test] public void testGetIThreadFactory() {
        IThreadFactory tf = new SimpleThreadFactory();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10), tf, new NoOpREHandler());
        Assert.AreEqual(tf, p.ThreadFactory);
        JoinPool(p);
    }

   
    [Test] public void testSetIThreadFactory() {
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        IThreadFactory tf = new SimpleThreadFactory();
        p.ThreadFactory=tf;
        Assert.AreEqual(tf, p.ThreadFactory);
        JoinPool(p);
    }


    /**
     * setIThreadFactory(null) throws NPE
     */
    [Test] public void testSetIThreadFactoryNull() {
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            p.ThreadFactory = null;
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) {
        } finally {
            JoinPool(p);
        }
    }

    /**
     * getIRejectedExecutionHandler returns handler in constructor if not set
     */
    [Test] public void testGetIRejectedExecutionHandler() {
        IRejectedExecutionHandler h = new NoOpREHandler();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10), h);
        Assert.AreEqual(h, p.RejectedExecutionHandler);
        JoinPool(p);
    }

    /**
     * setIRejectedExecutionHandler sets the handler returned by
     * getIRejectedExecutionHandler
     */
    [Test] public void testSetIRejectedExecutionHandler() {
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        IRejectedExecutionHandler h = new NoOpREHandler();
        p.RejectedExecutionHandler=h;
        Assert.AreEqual(h, p.RejectedExecutionHandler);
        JoinPool(p);
    }


    /**
     * setIRejectedExecutionHandler(null) throws NPE
     */
    [Test] public void testSetIRejectedExecutionHandlerNull() {
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,2,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            p.RejectedExecutionHandler=null;
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException) {
        } finally {
            JoinPool(p);
        }
    }


    /**
     *   getLargestPoolSize increases, but doesn't overestimate, when
     *   multiple threads active
     */
    [Test] public void testGetLargestPoolSize() {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            Assert.AreEqual(0, p2.LargestPoolSize);
            p2.Execute(new MediumRunnable());
            p2.Execute(new MediumRunnable());
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(2, p2.LargestPoolSize);
        } catch(Exception e){
            Assert.Fail("Unexpected Exception");
        }
        JoinPool(p2);
    }

    /**
     *   getMaximumPoolSize returns value given in constructor if not
     *   otherwise set
     */
    [Test] public void testGetMaximumPoolSize() {
        ThreadPoolExecutor p2 = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(2, p2.MaximumPoolSize);
        JoinPool(p2);
    }

    /**
     *   PoolSize increases, but doesn't overestimate, when threads
     *   become active
     */
    [Test] public void testGetPoolSize() {
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.AreEqual(0, p1.PoolSize);
        p1.Execute(new MediumRunnable());
        Assert.AreEqual(1, p1.PoolSize);
        JoinPool(p1);
    }

    /**
     *  getTaskCount increases, but doesn't overestimate, when tasks Submitted
     */
    [Test] public void testGetTaskCount() {
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            Assert.AreEqual(0, p1.TaskCount);
            p1.Execute(new MediumRunnable());
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.AreEqual(1, p1.TaskCount);
        } catch(Exception e){
            Assert.Fail("Unexpected Exception");
        }
        JoinPool(p1);
    }

    /**
     *   isShutdown is false before shutdown, true after
     */
    [Test] public void testIsShutdown() {

	ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.IsFalse(p1.IsShutdown);
        try { p1.Shutdown(); } catch(SecurityException ok) { return; }
	Assert.IsTrue(p1.IsShutdown);
        JoinPool(p1);
    }


    /**
     *  isTerminated is false before termination, true after
     */
    [Test] public void testIsTerminated() {
	ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.IsFalse(p1.IsTerminated);
        try {
            p1.Execute(new MediumRunnable());
        } finally {
            try { p1.Shutdown(); } catch(SecurityException ok) { }
        }
	try {
	    Assert.IsTrue(p1.AwaitTermination(LONG_DELAY_MS));
            Assert.IsTrue(p1.IsTerminated);
	} catch(Exception e){
            Assert.Fail("Unexpected Exception");
        }
    }

    /**
     *  isTerminating is not true when running or when terminated
     */
    [Test] public void testIsTerminating() {
	ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        Assert.IsFalse(p1.IsTerminating);
        try {
            p1.Execute(new SmallRunnable());
            Assert.IsFalse(p1.IsTerminating);
        } finally {
            try { p1.Shutdown(); } catch(SecurityException ok) { }
        }
        try {
	    Assert.IsTrue(p1.AwaitTermination(LONG_DELAY_MS));
            Assert.IsTrue(p1.IsTerminated);
            Assert.IsFalse(p1.IsTerminating);
	} catch(Exception e){
            Assert.Fail("Unexpected Exception");
        }
    }

    /**
     * getQueue returns the work queue, which Contains queued tasks
     */
    [Test] public void testGetQueue() {
        IBlockingQueue<IRunnable> q = new ArrayBlockingQueue<IRunnable>(10);
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, q);
        FutureTask[] tasks = new FutureTask[5];
        for(int i = 0; i < 5; i++){
            tasks[i] = new FutureTask(new MediumPossiblyInterruptedRunnable(), true);
            p1.Execute(tasks[i]);
        }
        try {
            Thread.Sleep(SHORT_DELAY_MS);
            IBlockingQueue<IRunnable> wq = p1.Queue;
            Assert.AreEqual(q, wq);
            Assert.IsFalse(wq.Contains(tasks[0]));
            Assert.IsTrue(wq.Contains(tasks[4]));
            for (int i = 1; i < 5; ++i)
                tasks[i].Cancel(true);
            p1.ShutdownNow();
        } catch(Exception e) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p1);
        }
    }

    /**
     * Remove(task) removes queued task, and fails to remove active task
     */
    [Test] public void testRemove() {
        IBlockingQueue<IRunnable> q = new ArrayBlockingQueue<IRunnable>(10);
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, q);
        FutureTask[] tasks = new FutureTask[5];
        for(int i = 0; i < 5; i++){
            tasks[i] = new FutureTask(new MediumPossiblyInterruptedRunnable(), true);
            p1.Execute(tasks[i]);
        }
        try {
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsFalse(p1.Remove(tasks[0]));
            Assert.IsTrue(q.Contains(tasks[4]));
            Assert.IsTrue(q.Contains(tasks[3]));
            Assert.IsTrue(p1.Remove(tasks[4]));
            Assert.IsFalse(p1.Remove(tasks[4]));
            Assert.IsFalse(q.Contains(tasks[4]));
            Assert.IsTrue(q.Contains(tasks[3]));
            Assert.IsTrue(p1.Remove(tasks[3]));
            Assert.IsFalse(q.Contains(tasks[3]));
        } catch(Exception e) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p1);
        }
    }

    /**
     *   Purge Removes Cancelled tasks from the queue
     */
    [Test] public void testPurge() {
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        FutureTask[] tasks = new FutureTask[5];
        for(int i = 0; i < 5; i++){
            tasks[i] = new FutureTask(new MediumPossiblyInterruptedRunnable(), true);
            p1.Execute(tasks[i]);
        }
        tasks[4].Cancel(true);
        tasks[3].Cancel(true);
        p1.Purge();
        long count = p1.TaskCount;
        Assert.IsTrue(count >= 2 && count < 5);
        JoinPool(p1);
    }

    /**
     *  ShutdownNow returns a list containing tasks that were not run
     */
    [Test] public void testShutdownNow()
    {
        ThreadPoolExecutor p1 = new ThreadPoolExecutor(1, 1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        IList<IRunnable> l = null;
        try
        {
            for (int i = 0; i < 5; i++)
                p1.Execute(new MediumPossiblyInterruptedRunnable());
        }
        finally
        {
            try
            {
                l = p1.ShutdownNow();
            }
            catch (SecurityException ok)
            {
            }

        }
        Assert.IsTrue(p1.IsShutdown);
        Assert.IsNotNull(l);
        Assert.IsTrue(l.Count <= 4);
    }

        // Exception Tests


    /**
     * Constructor throws if corePoolSize argument is less than zero
     */
    [Test] public void testConstructor1() {
        try {
            new ThreadPoolExecutor(-1,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is less than zero
     */
    [Test] public void testConstructor2() {
        try {
            new ThreadPoolExecutor(1,-1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is equal to zero
     */
    [Test] public void testConstructor3() {
        try {
            new ThreadPoolExecutor(1,0,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if keepAliveTime is less than zero
     */
    [Test] public void testConstructor4() {
        try {
            new ThreadPoolExecutor(1,2, new TimeSpan(0,0,0,0,-1), new ArrayBlockingQueue<IRunnable>(10));
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if corePoolSize is greater than the maximumPoolSize
     */
    [Test] public void testConstructor5() {
        try {
            new ThreadPoolExecutor(2,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if workQueue is set to null
     */
    [Test] public void testConstructorNullReferenceException() {
        try {
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,null);
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }



    /**
     * Constructor throws if corePoolSize argument is less than zero
     */
    [Test] public void testConstructor6() {
        try {
            new ThreadPoolExecutor(-1,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        } catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is less than zero
     */
    [Test] public void testConstructor7() {
        try {
            new ThreadPoolExecutor(1,-1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is equal to zero
     */
    [Test] public void testConstructor8() {
        try {
            new ThreadPoolExecutor(1,0,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if keepAliveTime is less than zero
     */
    [Test] public void testConstructor9() {
        try {
            new ThreadPoolExecutor(1,2,new TimeSpan(0,0,0,0,-1),  new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if corePoolSize is greater than the maximumPoolSize
     */
    [Test] public void testConstructor10() {
        try {
            new ThreadPoolExecutor(2,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if workQueue is set to null
     */
    [Test] public void testConstructorNullReferenceException2() {
        try {
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,null,new SimpleThreadFactory());
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }

    /**
     * Constructor throws if threadFactory is set to null
     */
    [Test] public void testConstructorNullReferenceException3() {
        try {
            IThreadFactory f = null;
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10),f);
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }


    /**
     * Constructor throws if corePoolSize argument is less than zero
     */
    [Test] public void testConstructor11() {
        try {
            new ThreadPoolExecutor(-1,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is less than zero
     */
    [Test] public void testConstructor12() {
        try {
            new ThreadPoolExecutor(1,-1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is equal to zero
     */
    [Test] public void testConstructor13() {
        try {
            new ThreadPoolExecutor(1,0,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if keepAliveTime is less than zero
     */
    [Test] public void testConstructor14() {
        try {
            new ThreadPoolExecutor(1,2,new TimeSpan(0,0,0,0,-1), new ArrayBlockingQueue<IRunnable>(10),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if corePoolSize is greater than the maximumPoolSize
     */
    [Test] public void testConstructor15() {
        try {
            new ThreadPoolExecutor(2,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if workQueue is set to null
     */
    [Test] public void testConstructorNullReferenceException4() {
        try {
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,null,new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }

    /**
     * Constructor throws if handler is set to null
     */
    [Test] public void testConstructorNullReferenceException5() {
        try {
            IRejectedExecutionHandler r = null;
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10),r);
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }


    /**
     * Constructor throws if corePoolSize argument is less than zero
     */
    [Test] public void testConstructor16() {
        try {
            new ThreadPoolExecutor(-1,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is less than zero
     */
    [Test] public void testConstructor17() {
        try {
            new ThreadPoolExecutor(1,-1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if maximumPoolSize is equal to zero
     */
    [Test] public void testConstructor18() {
        try {
            new ThreadPoolExecutor(1,0,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if keepAliveTime is less than zero
     */
    [Test] public void testConstructor19() {
        try {
            new ThreadPoolExecutor(1,2,new TimeSpan(0,0,0,0,-1), new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if corePoolSize is greater than the maximumPoolSize
     */
    [Test] public void testConstructor20() {
        try {
            new ThreadPoolExecutor(2,1,LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch (ArgumentException success){}
    }

    /**
     * Constructor throws if workQueue is set to null
     */
    [Test] public void testConstructorNullReferenceException6() {
        try {
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,null,new SimpleThreadFactory(),new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }

    /**
     * Constructor throws if handler is set to null
     */
    [Test] public void testConstructorNullReferenceException7() {
        try {
            IRejectedExecutionHandler r = null;
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10),new SimpleThreadFactory(),r);
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException success) { }
    }

    /**
     * Constructor throws if IThreadFactory is set top null
     */
    [Test] public void testConstructorNullReferenceException8() {
        try {
            IThreadFactory f = null;
            new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10),f,new NoOpREHandler());
            Assert.Fail("Should throw exception");
        }
        catch(ArgumentNullException successdn8) { }
    }


    /**
     *  Execute throws RejectedExecutionException
     *  if saturated.
     */
    [Test] public void testSaturatedExecute() {
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1));
        try {

            for(int i = 0; i < 5; ++i){
                p.Execute(new MediumRunnable());
            }
            Assert.Fail("Should throw exception");
        } catch(RejectedExecutionException success){}
        JoinPool(p);
    }

    /**
     *  executor using CallerRunsPolicy runs task if saturated.
     */
    [Test] public void testSaturatedExecute2() {
        IRejectedExecutionHandler h = new CallerRunsPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);
        try {

            TrackedNoOpRunnable[] tasks = new TrackedNoOpRunnable[5];
            for(int i = 0; i < 5; ++i){
                tasks[i] = new TrackedNoOpRunnable();
            }
            TrackedLongRunnable mr = new TrackedLongRunnable();
            p.Execute(mr);
            for(int i = 0; i < 5; ++i){
                p.Execute(tasks[i]);
            }
            for(int i = 1; i < 5; ++i) {
                Assert.IsTrue(tasks[i].IsDone);
            }
            try { p.ShutdownNow(); } catch(SecurityException ok) { return; }
        } catch(RejectedExecutionException ex){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }

    /**
     *  executor using DiscardPolicy drops task if saturated.
     */
    [Test] public void testSaturatedExecute3() {
        IRejectedExecutionHandler h = new DiscardPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);
        try {

            TrackedNoOpRunnable[] tasks = new TrackedNoOpRunnable[5];
            for(int i = 0; i < 5; ++i){
                tasks[i] = new TrackedNoOpRunnable();
            }
            p.Execute(new TrackedLongRunnable());
            for(int i = 0; i < 5; ++i){
                p.Execute(tasks[i]);
            }
            for(int i = 0; i < 5; ++i){
                Assert.IsFalse(tasks[i].IsDone);
            }
            try { p.ShutdownNow(); } catch(SecurityException ok) { return; }
        } catch(RejectedExecutionException ex){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }

    /**
     *  executor using DiscardOldestPolicy drops oldest task if saturated.
     */
    [Test,Ignore("ReentrantLock runs into a stack overflow")] public void testSaturatedExecute4() {
        IRejectedExecutionHandler h = new DiscardOldestPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);
        try {
            p.Execute(new TrackedLongRunnable());
            TrackedLongRunnable r2 = new TrackedLongRunnable();
            p.Execute(r2);
            Assert.IsTrue(p.Queue.Contains(r2));
            TrackedNoOpRunnable r3 = new TrackedNoOpRunnable();
            p.Execute(r3);
            Assert.IsFalse(p.Queue.Contains(r2));
            Assert.IsTrue(p.Queue.Contains(r3));
            try { p.ShutdownNow(); } catch(SecurityException ok) { return; }
        } catch(RejectedExecutionException ex){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }

    /**
     *  Execute throws RejectedExecutionException if Shutdown
     */
    [Test] public void testRejectedExecutionExceptionOnShutdown() {
        ThreadPoolExecutor tpe =
            new ThreadPoolExecutor(1,1,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(1));
        try { tpe.Shutdown(); } catch(SecurityException ok) { return; }
	try {
	    tpe.Execute(new NoOpRunnable());
	    Assert.Fail("Should throw exception");
	} catch(RejectedExecutionException success){}

	JoinPool(tpe);
    }

    /**
     *  Execute using CallerRunsPolicy drops task on Shutdown
     */
    [Test] public void testCallerRunsOnShutdown() {
        IRejectedExecutionHandler h = new CallerRunsPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);

        try { p.Shutdown(); } catch(SecurityException ok) { return; }
	try {
            TrackedNoOpRunnable r = new TrackedNoOpRunnable();
	    p.Execute(r);
            Assert.IsFalse(r.IsDone);
	} catch(RejectedExecutionException success){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }

    /**
     *  Execute using DiscardPolicy drops task on Shutdown
     */
    [Test] public void testDiscardOnShutdown() {
        IRejectedExecutionHandler h = new DiscardPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);

        try { p.Shutdown(); } catch(SecurityException ok) { return; }
	try {
            TrackedNoOpRunnable r = new TrackedNoOpRunnable();
	    p.Execute(r);
            Assert.IsFalse(r.IsDone);
	} catch(RejectedExecutionException success){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }


    /**
     *  Execute using DiscardOldestPolicy drops task on Shutdown
     */
    [Test] public void testDiscardOldestOnShutdown() {
        IRejectedExecutionHandler h = new DiscardOldestPolicy();
        ThreadPoolExecutor p = new ThreadPoolExecutor(1,1, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(1), h);

        try { p.Shutdown(); } catch(SecurityException ok) { return; }
	try {
            TrackedNoOpRunnable r = new TrackedNoOpRunnable();
	    p.Execute(r);
            Assert.IsFalse(r.IsDone);
	} catch(RejectedExecutionException success){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(p);
        }
    }


    /**
     *  Execute (null) throws NPE
     */
    [Test] public void testExecuteNull() {
        ThreadPoolExecutor tpe = null;
        try {
	    tpe = new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10));
	    tpe.Execute((IRunnable)null);
            Assert.Fail("Should throw exception");
	} catch(ArgumentNullException success){}

	JoinPool(tpe);
    }

    /**
     *  setCorePoolSize of negative value throws ArgumentException
     */
    [Test] public void testCorePoolSizeArgumentException() {
	ThreadPoolExecutor tpe = null;
	try {
	    tpe = new ThreadPoolExecutor(1,2,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10));
	} catch(Exception e){}
	try {
	    tpe.CorePoolSize=-1;
	    Assert.Fail("Should throw exception");
	} catch(ArgumentException success){
        } finally {
            try { tpe.Shutdown(); } catch(SecurityException ok) { }
        }
        JoinPool(tpe);
    }

    /**
     *  setMaximumPoolSize(int) throws ArgumentException if
     *  given a value less the core pool size
     */
    [Test] public void testMaximumPoolSizeArgumentException() {
        ThreadPoolExecutor tpe = null;
        try {
            tpe = new ThreadPoolExecutor(2,3,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10));
        } catch(Exception e){}
        try {
            tpe.MaximumPoolSize=1;
            Assert.Fail("Should throw exception");
        } catch(ArgumentException success){
        } finally {
            try { tpe.Shutdown(); } catch(SecurityException ok) { }
        }
        JoinPool(tpe);
    }

    /**
     *  setMaximumPoolSize throws ArgumentException
     *  if given a negative value
     */
    [Test] public void testMaximumPoolSizeArgumentException2() {
        ThreadPoolExecutor tpe = null;
        try {
            tpe = new ThreadPoolExecutor(2,3,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10));
        } catch(Exception e){}
        try {
            tpe.MaximumPoolSize=-1;
            Assert.Fail("Should throw exception");
        } catch(ArgumentException success){
        } finally {
            try { tpe.Shutdown(); } catch(SecurityException ok) {  }
        }
        JoinPool(tpe);
    }


    /**
     *  setKeepAliveTime  throws ArgumentException
     *  when given a negative value
     */
    [Test] public void testKeepAliveTimeArgumentException() {
	ThreadPoolExecutor tpe = null;
        try {
            tpe = new ThreadPoolExecutor(2,3,LONG_DELAY_MS,new ArrayBlockingQueue<IRunnable>(10));
        } catch(Exception e){}

	try {
            tpe.KeepAliveTime= new TimeSpan(0,0,0,0,-1);
            Assert.Fail("Should throw exception");
        } catch(ArgumentException success){
        } finally {
            try { tpe.Shutdown(); } catch(SecurityException ok) {  }
        }
        JoinPool(tpe);
    }

    /**
     * terminated() is called on termination
     */
    [Test] public void testTerminated() {
        ExtendedTPE tpe = new ExtendedTPE();
        try { tpe.Shutdown(); } catch(SecurityException ok) { return; }
        Assert.IsTrue(tpe.terminatedCalled);
        JoinPool(tpe);
    }

    /**
     * beforeExecute and afterExecute are called when executing task
     */
    [Test] public void testBeforeAfter() {
        ExtendedTPE tpe = new ExtendedTPE();
        try {
            TrackedNoOpRunnable r = new TrackedNoOpRunnable();
            tpe.Execute(r);
            Thread.Sleep(SHORT_DELAY_MS);
            Assert.IsTrue(r.IsDone);
            Assert.IsTrue(tpe.beforeCalled);
            Assert.IsTrue(tpe.afterCalled);
            try { tpe.Shutdown(); } catch(SecurityException ok) { return; }
        }
        //catch(Exception ex) {
        //    Assert.Fail("Unexpected Exception");
        //} 
        finally {
            JoinPool(tpe);
        }
    }

    /**
     * completed Submit of callable returns result
     */
    [Test] public void testSubmitCallable() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            IFuture future = e.Submit(new StringTask());
            String result = (String)future.GetResult();
            Assert.AreEqual(TEST_STRING, result);
        }
        catch (ExecutionException ex) {
            Assert.Fail("Unexpected Exception");
        }
        catch (ThreadInterruptedException ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * completed Submit of runnable returns successfully
     */
    [Test] public void testSubmitRunnable() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            IFuture future = e.Submit(new NoOpRunnable());
            future.GetResult();
            Assert.IsTrue(future.IsDone);
        }
        catch (ExecutionException ex) {
            Assert.Fail("Unexpected Exception");
        }
        catch (ThreadInterruptedException ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * completed Submit of (runnable, result) returns result
     */
    [Test] public void testSubmitRunnable2() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            IFuture future = e.Submit(new NoOpRunnable(), TEST_STRING);
            String result = (String)future.GetResult();
            Assert.AreEqual(TEST_STRING, result);
        }
        catch (ExecutionException ex) {
            Assert.Fail("Unexpected Exception");
        }
        catch (ThreadInterruptedException ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }





    /**
     * InvokeAny(null) throws NPE
     */
    [Test] public void testInvokeAny1() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAny(null);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAny(empty collection) throws IAE
     */
    [Test] public void testInvokeAny2() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAny(new ArrayList());
        } catch (ArgumentException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAny(c) throws NPE if c has null elements
     */
    [Test] public void testInvokeAny3() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(null);
            e.InvokeAny(l);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAny(c) throws ExecutionException if no task completes
     */
    [Test] public void testInvokeAny4() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new NPETask());
            e.InvokeAny(l);
        } catch (ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAny(c) returns result of some task
     */
    [Test] public void testInvokeAny5() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(new StringTask());
            String result = (String)e.InvokeAny(l);
            Assert.AreEqual(TEST_STRING, result);
        } catch (ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAll(null) throws NPE
     */
    [Test] public void testInvokeAll1() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAll(null);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAll(empty collection) returns empty collection
     */
    [Test] public void testInvokeAll2() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            IList r = e.InvokeAll(new ArrayList());
            Assert.IsTrue(r.Count == 0);
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * InvokeAll(c) throws NPE if c has null elements
     */
    [Test] public void testInvokeAll3() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(null);
            e.InvokeAll(l);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * get of element of InvokeAll(c) throws exception on failed task
     */
    [Test] public void testInvokeAll4()
    {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try
        {
            ArrayList l = new ArrayList();
            l.Add(new NPETask());
            IList result = e.InvokeAll(l);
            Assert.AreEqual(1, result.Count);
            IEnumerator it = result.GetEnumerator();
            while (it.MoveNext())
                ((IFuture) it.Current).GetResult();
        }
        catch (ExecutionException success)
        {
        }
        catch (Exception ex)
        {
            Assert.Fail("Unexpected Exception");
        }
        finally
        {
            JoinPool(e);
        }
    }

        /**
     * InvokeAll(c) returns results of all completed tasks
     */
    [Test] public void testInvokeAll5() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(new StringTask());
            IList result = e.InvokeAll(l);
            Assert.AreEqual(2, result.Count);
            IEnumerator it = result.GetEnumerator();
            while (it.MoveNext())
                Assert.AreEqual(TEST_STRING, ((IFuture)it.Current).GetResult());
        } catch (ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }



    /**
     * timed InvokeAny(null) throws NPE
     */
    [Test] public void testTimedInvokeAny1() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAny(null, MEDIUM_DELAY_MS);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAny(empty collection) throws IAE
     */
    [Test] public void testTimedInvokeAny2() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAny(new ArrayList(), MEDIUM_DELAY_MS);
        } catch (ArgumentException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAny(c) throws NPE if c has null elements
     */
    [Test] public void testTimedInvokeAny3() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(null);
            e.InvokeAny(l, MEDIUM_DELAY_MS);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAny(c) throws ExecutionException if no task completes
     */
    [Test] public void testTimedInvokeAny4() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new NPETask());
            e.InvokeAny(l, MEDIUM_DELAY_MS);
        } catch(ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAny(c) returns result of some task
     */
    [Test] public void testTimedInvokeAny5() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(new StringTask());
            String result = (String)e.InvokeAny(l, MEDIUM_DELAY_MS);
            Assert.AreEqual(TEST_STRING, result);
        } catch (ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAll(null) throws NPE
     */
    [Test] public void testTimedInvokeAll1() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            e.InvokeAll(null, MEDIUM_DELAY_MS);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }
    /**
     * timed InvokeAll(empty collection) returns empty collection
     */
    [Test] public void testTimedInvokeAll2() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            IList r = e.InvokeAll(new ArrayList(), MEDIUM_DELAY_MS);
            Assert.IsTrue(r.Count== 0);
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAll(c) throws NPE if c has null elements
     */
    [Test] public void testTimedInvokeAll3() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(null);
            e.InvokeAll(l, MEDIUM_DELAY_MS);
        } catch (ArgumentNullException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * get of element of InvokeAll(c) throws exception on failed task
     */
    [Test] public void testTimedInvokeAll4() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new NPETask());
            IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
            Assert.AreEqual(1, result.Count);
            IEnumerator it = result.GetEnumerator();
            while ( it.MoveNext())
                ((IFuture)it.Current).GetResult();
        } catch(ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAll(c) returns results of all completed tasks
     */
    [Test] public void testTimedInvokeAll5() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(new StringTask());
            IList result = e.InvokeAll(l, MEDIUM_DELAY_MS);
            Assert.AreEqual(2, result.Count);
            foreach (var next in result)
            {
                Assert.AreEqual(TEST_STRING, ((IFuture)next).GetResult());
            }
        } catch (ExecutionException success) {
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * timed InvokeAll(c) Cancels tasks not completed by timeout
     */
    [Test] public void testTimedInvokeAll6() {
        IExecutorService e = new ThreadPoolExecutor(2, 2, LONG_DELAY_MS, new ArrayBlockingQueue<IRunnable>(10));
        try {
            ArrayList l = new ArrayList();
            l.Add(new StringTask());
            l.Add(Executors.CreateCallable(new MediumPossiblyInterruptedRunnable(), TEST_STRING));
            l.Add(new StringTask());
            IList result = e.InvokeAll(l, SHORT_DELAY_MS);
            Assert.AreEqual(3, result.Count);
            IEnumerator it = result.GetEnumerator();
            IFuture f1 = null;
            IFuture f2 = null;
            IFuture f3 = null; 
           
            if ( it.MoveNext() ) f1 = (IFuture)it.Current;
            if ( it.MoveNext() ) f2 = (IFuture)it.Current;
            if ( it.MoveNext() ) f3 = (IFuture)it.Current;
            Assert.IsNotNull(f1);
            Assert.IsNotNull(f2);
            Assert.IsNotNull(f3);
            Assert.IsTrue(f1.IsDone);
            Assert.IsTrue(f2.IsDone);
            Assert.IsTrue(f3.IsDone);
            Assert.IsFalse(f1.IsCancelled);
            Assert.IsTrue(f2.IsCancelled);
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * Execution continues if there is at least one thread even if
     * thread factory fails to create more
     */
    [Test] public void testFailingIThreadFactory() {
        IExecutorService e = new ThreadPoolExecutor(100, 100, LONG_DELAY_MS, new LinkedBlockingQueue<IRunnable>(), new FailingIThreadFactory());
        try {
            ArrayList l = new ArrayList();
            for (int k = 0; k < 100; ++k) {
                e.Execute(new NoOpRunnable());
            }
            Thread.Sleep(LONG_DELAY_MS);
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(e);
        }
    }

    /**
     * allowsCoreThreadTimeOut is by default false.
     */
    [Test] public void testAllowsCoreThreadTimeOut() {
        ThreadPoolExecutor tpe = new ThreadPoolExecutor(2, 2, new TimeSpan(0,0,0,0,1000),  new ArrayBlockingQueue<IRunnable>(10));
        Assert.IsFalse(tpe.AllowsCoreThreadsToTimeOut);
        JoinPool(tpe);
    }

    /**
     * allowCoreThreadTimeOut(true) causes idle threads to time out
     */
    [Test] public void testAllowCoreThreadTimeOut_true() {
        ThreadPoolExecutor tpe = new ThreadPoolExecutor(2, 10, new TimeSpan(0,0,0,0,10),  new ArrayBlockingQueue<IRunnable>(10));
        tpe.AllowsCoreThreadsToTimeOut = true;
        tpe.Execute(new NoOpRunnable());
        try {
            Thread.Sleep(MEDIUM_DELAY_MS);
            Assert.AreEqual(0, tpe.PoolSize);
        } catch(ThreadInterruptedException e){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(tpe);
        }
    }

    /**
     * allowCoreThreadTimeOut(false) causes idle threads not to time out
     */
    [Test] public void testAllowCoreThreadTimeOut_false() {
        ThreadPoolExecutor tpe = new ThreadPoolExecutor(2, 10, new TimeSpan(0,0,0,0,10), new ArrayBlockingQueue<IRunnable>(10));
        tpe.AllowsCoreThreadsToTimeOut = false;
        tpe.Execute(new NoOpRunnable());
        try {
            Thread.Sleep(MEDIUM_DELAY_MS);
            Assert.IsTrue(tpe.PoolSize >= 1);
        } catch(ThreadInterruptedException e){
            Assert.Fail("Unexpected Exception");
        } finally {
            JoinPool(tpe);
        }
    }

        public class RecycledRunnable : IRunnable
        {
            private AtomicInteger _ai;
            public RecycledRunnable( AtomicInteger ai )
            {
                _ai = ai;
            }

                public void Run() {
                    _ai.IncrementValueAndReturn();
                }
        }
    /**
     * Execute allows the same task to be Submitted multiple times, even
     * if rejected
     */
    [Test] public void testRejectedRecycledTask() {
        int nTasks = 1000;
        AtomicInteger nRun = new AtomicInteger(0);
        IRunnable recycledTask = new RecycledRunnable(nRun);
        ThreadPoolExecutor p =
            new ThreadPoolExecutor(1, 30, new TimeSpan(0, 1, 0),
                                   new ArrayBlockingQueue<IRunnable>(30));
        try {
            for (int i = 0; i < nTasks; ++i) {
                for (;;) {
                    try {
                        p.Execute(recycledTask);
                        break;
                    }
                    catch (RejectedExecutionException ignore) {
                    }
                }
            }
            Thread.Sleep(5000); // enough time to run all tasks
            Assert.AreEqual(nRun.IntegerValue, nTasks);
        } catch(Exception ex) {
            Assert.Fail("Unexpected Exception");
        } finally {
            p.Shutdown();
        }
    }
    }
}