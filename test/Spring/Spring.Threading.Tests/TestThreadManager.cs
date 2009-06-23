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
        private Exception _threadException;

        private readonly ICollection<Thread> _runningThreads = new List<Thread>();

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
        /// Assert that the <see cref="expected"/> amount of threads has 
        /// registered by calling <see cref="RegisterCurrentThread"/>.
        /// It waits for amount of time specified by <see cref="timeToWait"/>
        /// for the thread to register.
        /// </summary>
        /// <param name="expected">
        /// The expected amount of threads.
        /// </param>
        /// <param name="timeToWait">
        /// Amount of time to wait for the threads to register.
        /// </param>
        public void AssertThreadCount(int expected, TimeSpan timeToWait)
        {
            lock (_runningThreads)
            {
                DateTime deadline = DateTime.UtcNow + timeToWait;
                while (timeToWait.Ticks > 0 && expected != _runningThreads.Count)
                {
                    Monitor.Wait(_runningThreads, timeToWait);
                    timeToWait = deadline - DateTime.UtcNow;
                }
                Assert.That(_runningThreads.Count, Is.EqualTo(expected), 
                            "Thread count is not as expected.");
            }
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
            if (aliveThread != null) Assert.Fail("Thread {0} is still alive.");
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
            Thread t = new Thread(NewVerifiableAction(action));
            t.Name = name;
            return t;
        }

        /// <summary>
        /// Verify there is not exception throw by any registered thread. It
        /// throws the first exception recorded by the registered thread if
        /// there was exception occurred.
        /// </summary>
        public void VerifyNoException()
        {
            if (_threadException != null)
            {
                Exception e = _threadException;
                _threadException = null;
                throw ExceptionExtensions.PreserveStackTrace(e);
            }
        }

        public void StartAndVerifyThread(int count, TimeSpan timeToWait, params Thread[] threads)
        {
            foreach (Thread thread in threads)
            {
                thread.Start();
            }
            AssertThreadCount(count, timeToWait);
        }

        public void StartAndVerifyThread(TimeSpan timeToWait, params Thread[] threads)
        {
            StartAndVerifyThread(threads.Length, timeToWait, threads);
        }

        public void JoinAndVerifyThreads(TimeSpan timeToWait)
        {
            JoinAndVerifyThreads(timeToWait, _runningThreads);
        }

        public void JoinAndVerifyThreads(TimeSpan timeToWait, params Thread[] threads)
        {
            JoinAndVerifyThreads(timeToWait, (IEnumerable<Thread>)threads);
        }

        public void JoinAndVerifyThreads(TimeSpan timeToWait, IEnumerable<Thread> threads)
        {
            foreach (Thread thread in threads)
            {
                thread.Join(timeToWait);
                Assert.IsFalse(
                    thread.IsAlive, 
                    "Thread {0} is expected to be terminated but still alive.", 
                    thread.Name);
            }
            VerifyNoException();
        }

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
    }
}