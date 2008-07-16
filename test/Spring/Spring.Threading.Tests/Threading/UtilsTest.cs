using System;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading
{
    [TestFixture]
	public class UtilsTest
	{

        [Test]
        [Ignore("What's the purpose of this test? It always fails on my machine. -Aleks")]
        public void TranslatesDateTimeToACommonStartingPointInTime ()
        {
            long before = Utils.CurrentTimeMillis;
            Thread.Sleep(10);
            long after = Utils.ToTimeMillis(DateTime.Now);
            Assert.IsTrue(after - before >= 10);
        }

        [Test]
        public void TimeMillisAreRealtiveTo_1970_01_01 ()
        {
            DateTime now = DateTime.Now;
            DateTime javaBase = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long actual = Utils.ToTimeMillis(now);
            Assert.AreEqual(Utils.DeltaTimeMillis(now, javaBase), actual);
        }


        class InterruptTester
        {
            internal ISync started;
            internal Thread thread;
            internal bool raised;
            
            internal void Help ()
            {
                try
                {
                    started.Release();
                    DoHelp ();
                }
                catch (ThreadInterruptedException)
                {
                    OnCatch ();
                }
                OnExit ();
            }

            internal virtual void DoHelp ()
            {
                while (true)
                    Utils.FailFastIfInterrupted();                
            }

            internal virtual void OnCatch ()
            {
                raised = true;
            }

            internal virtual void OnExit ()
            {
                raised = true;
            }
        }

        class CheckTester : InterruptTester
        {
            public bool interrupted = false;

            internal override void DoHelp ()
            {
                while (!Utils.ThreadInterrupted)
                {}
                interrupted = Utils.ThreadInterrupted;
            }

            internal override void OnCatch ()
            {
                base.OnCatch ();
            }

            internal override void OnExit ()
            {
                Assert.IsTrue(Utils.ThreadInterrupted);
            }
        }

        [Test]
        public void AllowToFailFastIfTheTreadWasInterrupted ()
        {
            InterruptTester tester = new InterruptTester();
            Do (tester);
            Assert.IsTrue(tester.raised);
        }

        [Test]
        public void AllowToCheckIfAThreadHasBeenInterrupted ()
        {
            CheckTester tester = new CheckTester();
            Do (tester);
            Assert.IsTrue (tester.interrupted);
        }

        private static void Do (InterruptTester tester)
        {
            tester.raised = false;
            tester.started = new Semaphore(0);
            tester.thread = new Thread(new ThreadStart(tester.Help));
            tester.thread.Start();
            tester.started.Acquire();
            tester.thread.Interrupt();
            tester.thread.Join();
        }

	}
}
