#region Licence

/*
 * Copyright 2002-2004 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */ 
#endregion

using System;
using System.Threading;
using NUnit.Framework;
using Spring.Threading;

namespace Spring.Threading
{
    [TestFixture]
    public class ThreadInterruptionTest
    {
        /// <summary>
        /// Needed to test .Net Thread behaviour when interruped
        /// </summary>
        /// <seealso>http://opensource.atlassian.com/projects/spring/browse/SPRNET-9</seealso>
        private interface IAfterInterruptionStrategy
        {
            void AfterInterruption ();
            Runner Runner { set; }
        }

        private class EnterLockStrategy : IAfterInterruptionStrategy
        {
            protected Runner runner;

            public Runner Runner
            {
                get { return runner; }
                set { runner = value; }
            }

            public virtual void AfterInterruption ()
            {
                try
                {
                    MyStrategy ();
                }
                catch (ThreadInterruptedException e)
                {
                    Log (e.Message);
                    runner.gotException = e;
                }
                catch (Exception e)
                {
                    Log ("unexpected exception: " + e.Message);
                }
            }

            protected virtual void MyStrategy ()
            {
                runner.performingStrategy = true;
                lock (this)
                {
                    Log ("inside lock");
                }
            }
        }

        private class WaitForLockStrategy : EnterLockStrategy
        {
            // we should block on lock
            protected override void MyStrategy ()
            {
                runner.performingStrategy = true;
                Log ("getting runner lock");
                lock (runner)
                {
                    runner.lockEntered = true;
                    Log ("inside lock");
                    Assert.Fail ("should not enter lock block");
                }
            }
        }

        private class WaitInLockStrategy : EnterLockStrategy
        {
            protected override void MyStrategy ()
            {
                runner.performingStrategy = true;
                lock (this)
                {
                    Log ("going to wait");
                    Monitor.Wait (this);
                }
            }
        }

        private class FailIfInterruptedLockStrategy : EnterLockStrategy
        {
            protected override void MyStrategy ()
            {
                runner.performingStrategy = true;
                Thread.Sleep (0);
                lock (this)
                {
                    Log ("inside lock");
                }
            }
        }

        private class InterruptAgainStrategy : EnterLockStrategy
        {
            private readonly ISync goSync;
            private readonly ISync stopSync;
            public bool interrupted;
            public bool reInterrupted;

            public InterruptAgainStrategy (ISync stop, ISync go)
            {
                this.stopSync = stop;
                this.goSync = go;
            }

            public override void AfterInterruption ()
            {
                runner.performingStrategy = true;
                interrupted = false;
                try
                {
                    lock (this)
                    {
                        Log ("going to wait after interruption");
                        stopSync.Release ();
                        Monitor.Wait(this);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Log ("interrupted again");
                    interrupted = true;
                }
                catch (Exception e)
                {
                    Log ("unexpected exception: " + e.Message);
                }

                if (interrupted)
                {
                    Log ("interrupting my own thread");
                    Thread.CurrentThread.Interrupt ();
                    reInterrupted = true;
                }
                goSync.Release();
            }
        }

        private class Runner
        {
            private bool run = true;

            public bool running = false;
            public ThreadInterruptedException gotException = null;
            public bool performingStrategy;
            private IAfterInterruptionStrategy strategy;
            public bool lockEntered = false;

            public Runner (IAfterInterruptionStrategy strategy)
            {
                this.strategy = strategy;
                this.strategy.Runner = this;
            }

            public void Run ()
            {
                running = true;
                Log ("in sec. thread");
                bool flag = true;
                while (run)
                {
                    if (flag)
                    {
                        Log ("Doing work");
                        flag = false;
                    }
                }
                Log ("this thread was interrupted");
                strategy.AfterInterruption ();
            }


            public void Interrupted ()
            {
                run = false;
            }
        }

        private static void Log (string msg)
        {
            //Console.Out.WriteLine ("Thread #" + Thread.CurrentThread.GetHashCode () + ": " + msg);
        }

        [Test]
        public void LockingDoesNotRaiseThreadInterruptedException ()
        {
            Runner runner = new Runner (new EnterLockStrategy ());
            Do (runner);
            Assert.IsNull (runner.gotException, "runner got exception: " + runner.gotException);
        }

        [Test]
        public void WaitingDoesRaiseThreadInterruptedException ()
        {
            Runner runner = new Runner (new WaitInLockStrategy ());
            Do (runner);
            Assert.IsNotNull (runner.gotException, "runner did not get exception");
        }

        [Test]
        public void InterruptionCanBeEarlyDetectedWaitingZeroMilliseconds ()
        {
            Runner runner = new Runner (new FailIfInterruptedLockStrategy ());
            Do (runner);
            Assert.IsNotNull (runner.gotException, "runner did not got exception");
        }

        [Test]
        public void CanBeInterruptedAgainAfterInterruption ()
        {
            Latch stop = new Latch ();
            Latch go = new Latch ();
            InterruptAgainStrategy strategy = new InterruptAgainStrategy (go, stop);
            Runner runner = new Runner (strategy);
            Thread t = StartedThread (runner);
            Log ("Thread #" + t.GetHashCode () + " started: interrupting it");
            t.Interrupt ();
            Log ("Thread #" + t.GetHashCode () + " interrupted");
            runner.Interrupted ();
            Log ("artificially signalled interruption");
            go.Acquire ();
            t.Interrupt ();
            stop.Release ();
            Log ("Joining thread #" + t.GetHashCode ());
            t.Join ();
            Assert.IsTrue (strategy.interrupted, "thread could not take any action after interruption");
            Assert.IsTrue (strategy.reInterrupted, "thread could not interrupt itself after interruption");
        }

        // test different behaviour from java threads upon interruption and 
        [Test]
        public void InterruptionCausesThrowingExceptionsWhenWaitingForLock ()
        {
            Runner runner = new Runner (new WaitForLockStrategy ());
            Thread t = StartedThread (runner);
            Log ("Thread #" + t.GetHashCode () + " started: interrupting it");
            t.Interrupt ();
            Log ("Thread #" + t.GetHashCode () + " interrupted");
            lock (runner)
            {
                runner.Interrupted ();
                Log ("artificially signalled interruption");
                Log ("Joining thread #" + t.GetHashCode ());
                t.Join ();
            }
            AssertIsPerformingStrategy (runner);
            Assert.IsNotNull (runner.gotException, "runner did not got exception");
            Assert.IsFalse (runner.lockEntered, "runner entered lock block");
        }

        private void Do (Runner runner)
        {
            Thread t = StartedThread (runner);
            Log ("Thread #" + t.GetHashCode () + " started: interrupting it");
            t.Interrupt ();
            Log ("Thread #" + t.GetHashCode () + " interrupted");
            runner.Interrupted ();
            Log ("artificially signalled interruption");
            Log ("Joining thread #" + t.GetHashCode ());
            t.Join ();
            AssertIsPerformingStrategy (runner);
        }

        private void AssertIsPerformingStrategy (Runner runner)
        {
            Assert.IsTrue (runner.performingStrategy, "runner is not performing strategy");
        }

        private Thread StartedThread (Runner runner)
        {
            ThreadStart start = new ThreadStart (runner.Run);
            Thread t = new Thread (start);
            t.Name = "runner";
            t.Start ();
            while (!runner.running)
                Thread.SpinWait (1);
            return t;
        }

    }
}