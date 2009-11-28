using System.Collections.Generic;
using NUnit.Framework;
using Spring;
using Spring.Threading.AtomicTypes;

#if !NET_4_0
namespace System.Threading.Tasks
{
    [TestFixture] public class ParallelTest : Spring.Threading.Execution.ParallelCompletionTestBase
    {
        [SetUp]
        public void SetUpThreshold()
        {
            _sampleSize = Environment.ProcessorCount * 50 + 2;
        }

        [TestCase(  0, 50)]
        [TestCase(-10, 10)]
        [TestCase( 20, 99)]
        public void ForInt32Count(int from, int to)
        {
            var completed = new List<int>(to - from);
            var result = Parallel.For(from, to, i => { lock (completed) completed.Add(i); });

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(result.LowestBreakIteration, Is.Null);
            Assert.That(completed.Count, Is.EqualTo(to - from));
            for (int i = from; i < to; i++)
            {
                Assert.That(completed, Has.Member(i));
            }
        }

        [TestCase(  0, 50)]
        [TestCase(-10, 10)]
        [TestCase( 20, 99)]
        public void ForInt64Count(long from, long to)
        {
            var completed = new List<long>((int)(to - from));
            var result = Parallel.For(from, to, i => { lock (completed) completed.Add(i); });

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(result.LowestBreakIteration, Is.Null);
            Assert.That(completed.Count, Is.EqualTo(to - from));
            for (long i = from; i < to; i++)
            {
                Assert.That(completed, Has.Member(i));
            }
        }

        [TestCase(Parallelism)]
        [TestCase(Parallelism * 2 - 1)]
        [TestCase(Parallelism * 2 + 1)]
        public void ForInt32Break(int breakAt)
        {
            var completed = new List<int>(_sampleSize);
            var result = Parallel.For(
                0, _sampleSize,
                (i, s) =>
                {
                    if(DelayForBreak(i, breakAt))s.Break();
                    if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(i);
                });

            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(breakAt));
            Assert.That(completed.Count, Is.GreaterThanOrEqualTo(breakAt));
            Assert.That(completed.Count, Is.LessThan(_sampleSize));
            for (int i = 0; i < breakAt; i++)
            {
                Assert.That(completed, Has.Member(i));
            }
        }

        [TestCase(Parallelism)]
        [TestCase(Parallelism * 2 - 1)]
        [TestCase(Parallelism * 2 + 1)]
        public void ForInt64Break(long breakAt)
        {
            var completed = new List<long>(_sampleSize);
            var result = Parallel.For(
                0, (long)_sampleSize,
                (i, s) =>
                {
                    if (DelayForBreak(i, breakAt)) s.Break();
                    if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(i);
                });

            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(breakAt));
            Assert.That(completed.Count, Is.GreaterThanOrEqualTo(breakAt));
            Assert.That(completed.Count, Is.LessThan(_sampleSize));
            for (long i = 0; i < breakAt; i++)
            {
                Assert.That(completed, Has.Member(i));
            }
        }

        [Test]
        public void ForInt32MaxParallelism(
            [Values(Parallelism - 1, Parallelism, Parallelism + 1)] int parallelism,
            [Values("WithState", "WithoutState")] string state)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var threads = new Dictionary<Thread, Thread>();
            Action a = () => {
                Thread.Sleep(10);
                var currentThread = Thread.CurrentThread;
                lock(threads) threads[currentThread] = currentThread;
            };
            var result = state == "WithState" ?
                Parallel.For(0, _sampleSize, options, (i, s) => a()) :
                Parallel.For(0, _sampleSize, options, i => a());

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(threads.Count, Is.LessThanOrEqualTo(parallelism));
        }

        [Test]
        public void ForInt64MaxParallelism(
            [Values(Parallelism - 1, Parallelism, Parallelism + 1)] int parallelism)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var threads = new Dictionary<Thread, Thread>();
            Func<int> localInit = () => 0;
            Func<long, ParallelLoopState, int, int> body = (i, s, l) =>
            {
                Thread.Sleep(10);
                return 0;
            };
            Action<int> localFinally = i =>
            {
                var currentThread = Thread.CurrentThread;
                lock(threads)threads[currentThread] = currentThread;
            };

            var result = Parallel.For(0, _sampleSize, options, localInit, body, localFinally);

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(threads.Count, Is.LessThanOrEqualTo(parallelism));
        }

        [TestCase(  0, 50)]
        [TestCase(-10, 10)]
        [TestCase( 20, 100000)]
        public void ForInt32LocalSum(int from, int to)
        {
            var total = new AtomicInteger(0);
            var result = Parallel.For(
                from, to,
                () => 0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [TestCase(  0, 50)]
        [TestCase(-10, 10)]
        [TestCase( 20, 2000000)]
        public void ForInt64LocalSum(long from, long to)
        {
            var total = new AtomicLong(0);
            var result = Parallel.For(
                from, to,
                () => (long)0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [TestCase(   0, 300, Parallelism - 1)]
        [TestCase(-100,  10, Parallelism)]
        [TestCase( 100, 200, Parallelism + 1)]
        public void ForInt32LocalSumWithParallelism(int from, int to, int parallelism)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var total = new AtomicInteger(0);
            var result = Parallel.For(
                from, to, options,
                () => 0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [TestCase(   0, 300, Parallelism - 1)]
        [TestCase(-100,  10, Parallelism)]
        [TestCase( 100, 200, Parallelism + 1)]
        public void ForInt64LocalSumWithParallelism(long from, long to, int parallelism)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var total = new AtomicLong(0);
            var result = Parallel.For(
                from, to, options,
                () => (long)0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [Test] public void ForEachPassingIndex([Values(-1, Parallelism)] int parallelism)
        {
            var sources = TestData<string>.MakeTestArray(_sampleSize);
            bool badIndex = false;
            Action<string, ParallelLoopState, long> body = 
                (s, pls, i) => { if (s != i.ToString()) badIndex = true; };
            if (parallelism == -1) Parallel.ForEach(sources, body);
            else  Parallel.ForEach(sources, new ParallelOptions{MaxDegreeOfParallelism = parallelism}, body);
            if(badIndex) Assert.Fail("Index doesn't match with item.");
        }

        [Test] public void ForEachUsingLocalCollects([Values(-1, Parallelism)] int parallelism)
        {
            var sources = TestData<string>.MakeTestArray(_sampleSize);
            var collected = new List<string>();
            Func<List<string>> localInit = ()=> new List<string>();
            Func<string, ParallelLoopState, long, List<string>, List<string>> body =
                (s, pls, i, l) => 
                { 
                    if (s == i.ToString()) l.Add(s);
                    return l;
                };
            Action<List<string>> localFinally =
                l =>
                {
                    lock(collected) collected.AddRange(l);
                };
            if (parallelism == -1)
            {
                Parallel.ForEach(sources, localInit, body, localFinally);
            }
            else
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
                Parallel.ForEach(sources, options, localInit, body, localFinally);
            }
            Assert.That(collected, Is.EquivalentTo(sources));
        }

        [Test] public void InvokeExecutesAllActions([Values(-1, Parallelism)] int parallelism)
        {
            var sources = TestData<string>.MakeTestArray(_sampleSize);
            var actions = new Action[_sampleSize];
            var collected = new List<string>(_sampleSize);
            for (int i = sources.Length - 1; i >= 0; i--)
            {
                string source = sources[i];
                actions[i] = () => {
                    lock (collected)
                        collected.Add(source);
                };
            }
            if (parallelism == -1)
            {
                Parallel.Invoke(actions);
            }
            else
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
                Parallel.Invoke(options, actions);
            }
            Assert.That(collected, Is.EquivalentTo(sources));
        }

        [Test] public void ForEachStopsAsSoonAsStopCalledAt(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int stopAt)
        {
            string[] sources = TestData<string>.MakeTestArray(_sampleSize);
            List<string> completed = new List<string>(_sampleSize);
            Exception e = null;
            long badIndex = 0;
            var stopCalled = new AtomicBoolean();
            Action<string, ParallelLoopState, long> body =
                (t, s, i) =>
                    {
                        if (i == stopAt)
                        {
                            s.Stop();
                            stopCalled.Value = true;
                        }
                        else
                        {
                            try
                            {
                                do { Thread.Sleep(10); } 
                                while (i == 0 && !stopCalled);
                            }
                            catch(ThreadInterruptedException){}
                            if (!s.ShouldExitCurrentIteration)
                                lock (completed) completed.Add(t);
                            else
                            {
                                try
                                {
                                    Assert.That(s.LowestBreakIteration, Is.Null);
                                    Assert.That(s.IsExceptional, Is.False);
                                    Assert.That(s.IsStopped, Is.True);
                                }
                                catch (Exception ex)
                                {
                                    lock (completed)
                                    {
                                        e = ex;
                                        badIndex = i;
                                    }
                                }
                            }
                        }
                    };
            var result = Parallel.ForEach(sources, body);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.Null);
            lock (completed)
            {
                if (e != null) throw new AssertionException("Error processing " + badIndex, e);
                Assert.That(completed, Has.No.Member(sources[0]));
            }
        }

        [Test]
        public void ForEachStopsWhenExceptionThrownAt(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int errorAt)
        {
            string[] sources = TestData<string>.MakeTestArray(_sampleSize);
            List<string> completed = new List<string>(_sampleSize);
            Exception e = null;
            long badIndex = 0;
            Action<string, ParallelLoopState, long> body =
                (t, s, i) =>
                {
                    if (i == errorAt) throw new Exception();
                    try { Thread.Sleep(10); }
                    catch (ThreadInterruptedException) { }
                    if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                    else
                    {
                        try
                        {
                            Assert.That(s.LowestBreakIteration, Is.Null);
                            Assert.That(s.IsExceptional, Is.True);
                            Assert.That(s.IsStopped, Is.False);
                        }
                        catch(Exception ex)
                        {
                            lock (completed)
                            {
                                e = ex;
                                badIndex = i;
                            }
                        }
                    }
                };
            Assert.Catch(() => Parallel.ForEach(sources, body));
            lock (completed)
            {
                if (e != null) throw new AssertionException("Error processing " + badIndex, e);
                Assert.That(completed.Count, Is.LessThan(_sampleSize));
            }
        }

        [Test]
        public void ForEachCompletesIterationsLessThenBreakIndexOf(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int breakAt)
        {
            string[] sources = TestData<string>.MakeTestArray(_sampleSize);
            List<string> completed = new List<string>(_sampleSize);
            Exception e = null;
            long badIndex = 0;
            Action<string, ParallelLoopState, long> body =
                (t, s, i) =>
                {
                    if (i == breakAt) s.Break();
                    else
                    {
                        try { Thread.Sleep(i == 0 ? SMALL_DELAY_MS : 10); }
                        catch (ThreadInterruptedException) { }
                        if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                        else
                        {
                            try
                            {
                                Assert.That(s.LowestBreakIteration, Is.EqualTo(breakAt));
                                Assert.That(s.IsExceptional, Is.False);
                                Assert.That(s.IsStopped, Is.False);
                            }
                            catch (Exception ex)
                            {
                                lock (completed)
                                {
                                    e = ex;
                                    badIndex = i;
                                }
                            }
                        }
                    }
                };
            var result = Parallel.ForEach(sources, body);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(breakAt));
            lock (completed)
            {
                if (e != null) throw new AssertionException("Error processing " + badIndex, e);
                Assert.That(completed.Count, Is.GreaterThanOrEqualTo(breakAt));
                Assert.That(completed, Has.Member(sources[0]));
            }
        }
    }
}
#endif