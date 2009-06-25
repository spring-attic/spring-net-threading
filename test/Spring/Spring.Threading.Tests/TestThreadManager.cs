using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace Spring
{
    /// <summary>
    /// An important utility class to help test class with multiple concurrent threads.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class TestThreadManager
    {
        /// <summary>
        /// Default time to wait for thread(s) to register is 1.5 seconds.
        /// </summary>
        public static readonly TimeSpan DefaultRegisterWait = TimeSpan.FromMilliseconds(1500);

        /// <summary>
        /// Default time to wait for thead(s) to join is 3 seconds.
        /// </summary>
        public static readonly TimeSpan DefaultJoinWait = TimeSpan.FromMilliseconds(3000);

        private Exception _threadException;

        private readonly ICollection<Thread> _runningThreads = new List<Thread>();

        /// <summary>
        /// Check if current thread is interrupted without changing its
        /// interrupted state.
        /// </summary>
        /// <returns><c>true</c> if interrupted, otherwise false.</returns>
        public static bool IsCurrentThreadInterrupted()
        {
            try
            {
                Thread.Sleep(0); // get exception if interrupted.
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt(); // keep interrupted state.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Register current thread to be managed by this 
        /// <see cref="TestThreadManager"/>.
        /// </summary>
        public void RegisterCurrentThread()
        {
            Thread t = Thread.CurrentThread;
            lock (_runningThreads)
            {
                if (!_runningThreads.Contains(t))
                {
                    _runningThreads.Add(t);
                    Monitor.PulseAll(_runningThreads);
                }
            }
        }

        /// <summary>
        /// Assert all given <paramref name="threads"/> are registered to this
        /// <see cref="TestThreadManager"/> within the given amount of 
        /// <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for threads to register.</param>
        /// <param name="threads">Threads that are expected to register</param>
        public void AssertRegistered(TimeSpan timeToWait, params Thread[] threads)
        {
            lock (_runningThreads)
            {
                DateTime deadline = DateTime.UtcNow + timeToWait;

                Thread t = FindUnregisteredThread(threads);
                while (timeToWait.Ticks > 0 && t != null)
                {
                    Monitor.Wait(_runningThreads, timeToWait);
                    t = FindUnregisteredThread(threads);
                    timeToWait = deadline - DateTime.UtcNow;
                }
                if (t!=null) Assert.Fail("Thread {0} is not registered.", t.Name);
            }
        }

        /// <summary>
        /// Assert all given <paramref name="threads"/> are registered to this
        /// <see cref="TestThreadManager"/> within the given amount of time
        /// specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="threads">Threads that are expected to register</param>
        public void AssertRegistered(params Thread[] threads)
        {
            AssertRegistered(DefaultRegisterWait, threads);
        }

        /// <summary>
        /// Assert the given <paramref name="thread"/> is registered to this
        /// <see cref="TestThreadManager"/> within the given amount of 
        /// <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to register.</param>
        /// <param name="thread">Thread that are expected to register</param>
        public void AssertRegistered(TimeSpan timeToWait, Thread thread)
        {
            lock (_runningThreads)
            {
                DateTime deadline = DateTime.UtcNow + timeToWait;
                bool isRegistered = _runningThreads.Contains(thread);
                while (timeToWait.Ticks > 0 && !isRegistered)
                {
                    Monitor.Wait(_runningThreads, timeToWait);
                    isRegistered = _runningThreads.Contains(thread);
                    timeToWait = deadline - DateTime.UtcNow;
                }
                if (!isRegistered) Assert.Fail("Thread {0} is not registered.", thread.Name);
            }
        }

        /// <summary>
        /// Assert the given <paramref name="thread"/> is registered to this
        /// <see cref="TestThreadManager"/> within the given amount of time
        /// specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="thread">Thread that are expected to register.</param>
        public void AssertRegistered(Thread thread)
        {
            AssertRegistered(DefaultRegisterWait, thread);
        }

        /// <summary>
        /// Abort every registered thread if it is still alive. Used in the 
        /// tear down method of the test fixture.
        /// </summary>
        public void TearDown()
        {
            Thread aliveThread = null;
            foreach (Thread thread in _runningThreads)
            {
                if (thread.IsAlive)
                {
                    aliveThread = thread;
                    thread.Abort();
                }
            }
            if (aliveThread != null) Assert.Fail("Thread {0} is still alive.", aliveThread.Name);
        }

        /// <summary>
        /// Create a new <see cref="ThreadStart"/> delegate that will register 
        /// itself to this <see cref="TestThreadManager"/> upon execution and
        /// record exceptions if any to this <see cref="TestThreadManager"/>
        /// during execution.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>
        /// The new <see cref="ThreadStart"/> delegate registers itself and
        /// record exception.
        /// </returns>
        public ThreadStart NewVerifiableAction(ThreadStart action)
        {
            return delegate
                       {
                           RegisterCurrentThread();
                           try
                           {
                               action();
                           }
                           catch (Exception e)
                           {
                               if (_threadException == null)
                               {
                                   while (e is TargetInvocationException) e = e.InnerException;
                                   e.Source = Thread.CurrentThread.Name;
                                   _threadException = e;
                               }
                           }
                       };
        }

        /// <summary>
        /// Create a new thread that will register itself to this
        /// <see cref="TestThreadManager"/> upon start and record exceptions
        /// if any to this <see cref="TestThreadManager"/> during execution.
        /// </summary>
        /// <param name="action">the action to execute.</param>
        /// <returns>
        /// A new thread that registers itself and record exception.
        /// </returns>
        public Thread NewVerifiableThread(ThreadStart action)
        {
            return NewVerifiableThread(action, "Test Thread");
        }

        /// <summary>
        /// Create a new thread that will register itself to this
        /// <see cref="TestThreadManager"/> upon start and record exceptions
        /// if any to this <see cref="TestThreadManager"/> during execution.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="name">Name of the thread, useful during debuging.</param>
        /// <returns>
        /// A new thread that registers itself and record exception.
        /// </returns>
        public Thread NewVerifiableThread(ThreadStart action, string name)
        {
            return new Thread(NewVerifiableAction(action)) {Name = name};
        }

        /// <summary>
        /// Verify there is not exception throw by any registered thread. It
        /// throws the first exception recorded by the registered thread if
        /// there was exception occurred.
        /// </summary>
        public void Verify()
        {
            if (_threadException != null)
            {
                Exception e = _threadException;
                _threadException = null;
                throw ExceptionExtensions.PreserveStackTrace(e);
            }
        }

        /// <summary>
        /// Start all given <paramref name="threads"/> and assert they are 
        /// registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for threads to register.</param>
        /// <param name="threads">Threads to start and be exptected to register.</param>
        public void StartAndAssertRegistered(TimeSpan timeToWait, params Thread[] threads)
        {
            foreach (Thread thread in threads) thread.Start();
            AssertRegistered(timeToWait, threads);
        }

        /// <summary>
        /// Start all given <paramref name="threads"/> and assert they are 
        /// registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of time specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="threads">Threads to start and be exptected to register.</param>
        public void StartAndAssertRegistered(params Thread[] threads)
        {
            StartAndAssertRegistered(DefaultRegisterWait, threads);
        }

        /// <summary>
        /// Start the given <paramref name="thread"/> and assert it is 
        /// registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to register.</param>
        /// <param name="thread">Thread to start and be exptected to register.</param>
        public void StartAndAssertRegistered(TimeSpan timeToWait, Thread thread)
        {
            thread.Start();
            AssertRegistered(timeToWait, thread);
        }

        /// <summary>
        /// Start the given <paramref name="thread"/> and assert it is 
        /// registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of time specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="thread">Thread to start and be exptected to register.</param>
        public void StartAndAssertRegistered(Thread thread)
        {
            StartAndAssertRegistered(DefaultRegisterWait, thread);
        }

        /// <summary>
        /// Execute all given <paramref name="actions"/> in new threads and assert 
        /// they are registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for threads to register.</param>
        /// <param name="namePrefix">Prefix to construct the name of threads.</param>
        /// <param name="actions">
        /// Delegates to execute and be exptected to register.
        /// </param>
        /// <returns>
        /// Threads that are created corresponding to the <paramref name="actions"/>.
        /// </returns>
        public Thread[] StartAndAssertRegistered(TimeSpan timeToWait, string namePrefix, params ThreadStart[] actions)
        {
            Thread[] threads = new Thread[actions.Length];
            for (int i = actions.Length - 1; i >= 0; i--)
            {
                threads[i] = NewVerifiableThread(actions[i], namePrefix + i);
            }
            StartAndAssertRegistered(timeToWait, threads);
            return threads;
        }

        /// <summary>
        /// Execute all given <paramref name="actions"/> in new threads and assert 
        /// they are registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of time specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="namePrefix">Prefix to construct the name of threads.</param>
        /// <param name="actions">
        /// Delegates to executed and be exptected to register.
        /// </param>
        /// <returns>
        /// Threads that are created corresponding to the <paramref name="actions"/>.
        /// </returns>
        public Thread[] StartAndAssertRegistered(string namePrefix, params ThreadStart[] actions)
        {
            return StartAndAssertRegistered(DefaultRegisterWait, namePrefix, actions);
        }

        /// <summary>
        /// Execute the given <paramref name="action"/> in new thread and assert 
        /// it is registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of <paramref name="timeToWait">time to wait</paramref>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to register.</param>
        /// <param name="name">Name of the created thread.</param>
        /// <param name="action">Delegate to execute and be exptected to register.</param>
        public Thread StartAndAssertRegistered(TimeSpan timeToWait, string name, ThreadStart action)
        {
            Thread thread = NewVerifiableThread(action);
            thread.Start();
            AssertRegistered(timeToWait, thread);
            return thread;
        }

        /// <summary>
        /// Execute the given <paramref name="action"/> in new thread and assert 
        /// it is registered to this <see cref="TestThreadManager"/> within the 
        /// given amount of time specified by <see cref="DefaultRegisterWait"/>.
        /// </summary>
        /// <param name="name">Name of the created thread.</param>
        /// <param name="action">Delegate to execute and be exptected to register.</param>
        public Thread StartAndAssertRegistered(string name, ThreadStart action)
        {
            return StartAndAssertRegistered(DefaultRegisterWait, name, action);
        }

        /// <summary>
        /// Assert all registered threads complete successfully within a given 
        /// time period specified by <paramref name="timeToWait"/>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to complete.</param>
        public void JoinAndVerify(TimeSpan timeToWait)
        {
            Thread[] threads;
            lock(_runningThreads)
            {
                threads = new Thread[_runningThreads.Count];
                _runningThreads.CopyTo(threads, 0);
            }
            JoinAndVerify(timeToWait, threads);
            lock (_runningThreads)
            {
                foreach (Thread thread in _runningThreads)
                {
                    Assert.IsFalse(
                        thread.IsAlive,
                        "Thread {0} is expected to be terminated but still alive.",
                        thread.Name);
                }
            }
        }

        /// <summary>
        /// Assert all registered threads complete successfully within a given 
        /// time period specified by <see cref="DefaultJoinWait"/>.
        /// </summary>
        public void JoinAndVerify()
        {
            JoinAndVerify(DefaultJoinWait);
        }

        /// <summary>
        /// Assert all specified <paramref name="threads"/> complete 
        /// successfully within a given time period specified by 
        /// <paramref name="timeToWait"/>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to complete.</param>
        /// <param name="threads">Threads expected to complete.</param>
        public void JoinAndVerify(TimeSpan timeToWait, params Thread[] threads)
        {
            JoinAndVerify(timeToWait, (IEnumerable<Thread>)threads);
        }

        /// <summary>
        /// Assert all specified <paramref name="threads"/> complete 
        /// successfully within a given time period specified by 
        /// <see cref="DefaultJoinWait"/>.
        /// </summary>
        /// <param name="threads">Threads expected to complete.</param>
        public void JoinAndVerify(params Thread[] threads)
        {
            JoinAndVerify(DefaultJoinWait, (IEnumerable<Thread>)threads);
        }

        /// <summary>
        /// Assert all specified <paramref name="threads"/> complete 
        /// successfully within a given time period specified by 
        /// <paramref name="timeToWait"/>.
        /// </summary>
        /// <param name="timeToWait">Time to wait for thread to complete.</param>
        /// <param name="threads">Threads expected to complete.</param>
        public void JoinAndVerify(TimeSpan timeToWait, IEnumerable<Thread> threads)
        {
            foreach (Thread thread in threads)
            {
                thread.Join(timeToWait);
                Assert.IsFalse(
                    thread.IsAlive, 
                    "Thread {0} is expected to be terminated but still alive.", 
                    thread.Name);
            }
            Verify();
        }

        /// <summary>
        /// Assert all specified <paramref name="threads"/> complete 
        /// successfully within a given time period specified by 
        /// <see cref="DefaultJoinWait"/>.
        /// </summary>
        /// <param name="threads">Threads expected to complete.</param>
        public void JoinAndVerify(IEnumerable<Thread> threads)
        {
            JoinAndVerify(DefaultJoinWait, threads);
        }

        private Thread FindUnregisteredThread(IEnumerable<Thread> threads)
        {
            foreach (Thread thread in threads)
            {
                if (!_runningThreads.Contains(thread)) return thread;
            }
            return null;
        }

    }
}