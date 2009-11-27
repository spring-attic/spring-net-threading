using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.AtomicTypes;

namespace Spring.Threading.Execution
{
    [TestFixture]
    public class ParallelExtensionTest : ParallelCompletionTestBase
    {
        [SetUp]
        public void SetUpThreshold()
        {
            _sampleSize = 100;
            _executor.Threshold = Parallelism * 2;
        }

        [TestCase(0, 50)]
        [TestCase(-10, 10)]
        [TestCase(20, 99)]
        public void ForInt32Count(int from, int to)
        {
            var completed = new List<int>(to - from);
            var result = _executor.For(from, to, i => { lock (completed) completed.Add(i); });

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(result.LowestBreakIteration, Is.Null);
            Assert.That(completed.Count, Is.EqualTo(to - from));
            for (int i = from; i < to; i++)
            {
                Assert.That(completed, Has.Member(i));
            }
        }

        [TestCase(0, 50)]
        [TestCase(-10, 10)]
        [TestCase(20, 99)]
        public void ForInt64Count(long from, long to)
        {
            var completed = new List<long>((int)(to - from));
            var result = _executor.For(from, to, i => { lock (completed) completed.Add(i); });

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
            var result = _executor.For(
                0, _sampleSize,
                (i, s) =>
                {
                    BreakAt(s, breakAt);
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
            var result = _executor.For(
                0, (long)_sampleSize,
                (i, s) =>
                {
                    BreakAt(s, breakAt);
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

        [Test] public void ForInt32MaxParallelism(
            [Values(Parallelism - 1, Parallelism, Parallelism + 1)] int parallelism,
            [Values("WithState", "WithoutState")] string state)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var result = state == "WithState" ?
                _executor.For(0, _sampleSize, options, (i, s) => Thread.Sleep(10)) :
                _executor.For(0, _sampleSize, options, i => Thread.Sleep(10));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(parallelism));
        }

        [Test] public void ForInt64MaxParallelism(
            [Values(Parallelism - 1, Parallelism, Parallelism + 1)] int parallelism,
            [Values("WithState", "WithoutState")] string state)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var result = state == "WithState" ?
                _executor.For(0, (long)_sampleSize, options, (i, s) => Thread.Sleep(10)) :
                _executor.For(0, (long)_sampleSize, options, i => Thread.Sleep(10));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(parallelism));
        }

        [TestCase(0, 50)]
        [TestCase(-10, 10)]
        [TestCase(20, 100000)]
        public void ForInt32LocalSum(int from, int to)
        {
            var total = new AtomicInteger(0);
            var result = _executor.For(
                from, to,
                ()=>0,
                (i, s, l) => l+i,
                l=>total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from+to-1)*(to-from)/2));
        }

        [TestCase(0, 50)]
        [TestCase(-10, 10)]
        [TestCase(20, 2000000)]
        public void ForInt64LocalSum(long from, long to)
        {
            var total = new AtomicLong(0);
            var result = _executor.For(
                from, to,
                () => (long)0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [TestCase(0, 300, Parallelism-1)]
        [TestCase(-100, 10, Parallelism)]
        [TestCase(100, 200, Parallelism + 1)]
        public void ForInt32LocalSumWithParallelism(int from, int to, int parallelism)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var total = new AtomicInteger(0);
            var result = _executor.For(
                from, to, options,
                () => 0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }

        [TestCase(0, 300, Parallelism - 1)]
        [TestCase(-100, 10, Parallelism)]
        [TestCase(100, 200, Parallelism + 1)]
        public void ForInt64LocalSumWithParallelism(long from, long to, int parallelism)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
            var total = new AtomicLong(0);
            var result = _executor.For(
                from, to, options,
                () => (long)0,
                (i, s, l) => l + i,
                l => total.AddDeltaAndReturnNewValue(l));

            Assert.That(result.IsCompleted, Is.True);
            Assert.That(total.Value, Is.EqualTo((from + to - 1) * (to - from) / 2));
        }
    }
}
