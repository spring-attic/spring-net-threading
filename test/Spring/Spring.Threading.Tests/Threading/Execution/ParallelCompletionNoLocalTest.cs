using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.Framework;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="ParallelCompletion{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))] 
    [TestFixture(typeof(string))] 
    public class ParallelCompletionNoLocalTest<T> : ParallelCompletionTestBase
    {

        private ParallelCompletion<T> _sut;

        [Test] public void ConstructorChokesOnNullExecutor()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T>(null, (t, s) => { }));
            Assert.That(e.ParamName, Is.EqualTo("executor"));
        }

        [Test] public void ForEachChokesOnNullSource()
        {
            _sut = new ParallelCompletion<T>(_executor, (t, s) => { });
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(null, 2));
            Assert.That(e.ParamName, Is.EqualTo("source"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(null, new ParallelOptions()));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ForEachChokesOnNullBody()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T>(_executor, null));
            Assert.That(e.ParamName, Is.EqualTo("body"));
        }

        [Test] public void ForEachChokesOnNullParallelOptions()
        {
            _sut = new ParallelCompletion<T>(_executor, (t, s) => { });
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(new T[0], null));
            Assert.That(e.ParamName, Is.EqualTo("parallelOptions"));
        }

        [Test] public void ForEachReturnsImmediatelyOnEmptySource()
        {
            List<T> results = new List<T>();
            _sut = new ParallelCompletion<T>(_executor, (t, s) => { lock (results) results.Add(t); });
            _sut.ForEach(new T[0], new ParallelOptions());
            Assert.That(results, Is.Empty);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(0));
        }

        [Test] public void ForEachUseCurrentThreadWhenParallelistIsOne()
        {
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) => { Thread.Sleep(10); lock (results) results.Add(t); });
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            _sut.ForEach(sources, 1);
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(0));
        }

        [Test] public void ForEachCompletesAllFaskTasks()
        {
            _sampleSize = 200;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor, (t, s) => { lock (results) results.Add(t); });
            _sut.ForEach(sources, new ParallelOptions { MaxDegreeOfParallelism = Parallelism });
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.LessThanOrEqualTo(Parallelism));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCompletesAllSlowTasks([Values(int.MaxValue, 3, 1, 0)] int maxThread)
        {
            _executor.Threshold = maxThread;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) => { Thread.Sleep(10); lock (results) results.Add(t); });
            _sut.ForEach(sources, Parallelism);
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(Math.Min(Parallelism, maxThread)));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachDoesNotSumitMoreThenMaxDegreeOfParallelism()
        {
            _executor.Delay = Delays.Short;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) => { Thread.Sleep(10); lock (results) results.Add(t); });
            _sut.ForEach(sources, Parallelism);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(Parallelism));
            Assert.That(results, Is.EquivalentTo(sources));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachLimitsParallismToThreadPoolExecutorCoreSize(
            [Values(Parallelism - 2, Parallelism + 2)] int coreSize)
        {
            var tf = new ManagedThreadFactory(ThreadManager);
            var executor = new ThreadPoolExecutor(coreSize, Parallelism + 2, 
                TimeSpan.MaxValue, new LinkedBlockingQueue<IRunnable>(1), tf);
            try
            {
                T[] sources = TestData<T>.MakeTestArray(_sampleSize);
                List<T> results = new List<T>(_sampleSize);
                var parallel = new ParallelCompletion<T>(executor,
                    (t, s) => { Thread.Sleep(10); lock (results) results.Add(t); });
                parallel.ForEach(sources, Parallelism);
                Assert.That(results, Is.EquivalentTo(sources));
                Assert.That(parallel.ActualDegreeOfParallelism, Is.EqualTo(Math.Min(Parallelism, coreSize)));
            }
            finally
            {
                executor.ShutdownNow();
            }
            
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachChokesOnAnyExceptionFromBody()
        {
            var exception = new Exception("exception message");
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                            (t, s) =>
                            {
                                if (Equals(t, sources[0]))
                                {
                                    Thread.Sleep(Delays.Short);
                                    throw exception;
                                }
                            });
            ThreadManager.StartAndAssertRegistered("Driver",
                () => {
                    var e = Assert.Throws<AggregateException>(() =>
                        _sut.ForEach(sources, Parallelism));
                    Assert.That(e.InnerException, Is.SameAs(exception));
                    Assert.That(_executor.ThreadCount.Value, Is.GreaterThanOrEqualTo(2));
                });
            Thread.Sleep(Delays.Short);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachStopsAsSoonAsStopCalledAt(
            [Values(Parallelism / 2, Parallelism, Parallelism*2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? Delays.ShortMillis : 10);
                    if (s.CurrentIndex == cancelAt) s.Stop();
                    else
                    {
                        if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                        else
                        {
                            Assert.That(s.LowestBreakIteration, Is.Null);
                            Assert.That(s.IsExceptional, Is.False);
                            Assert.That(s.IsStopped, Is.True);
                        }
                    }
                });
            var result = _sut.ForEach(sources, Parallelism);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(completed, Has.No.Member(sources[0]));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachStopsAsSoonAsExceptionThrownAt(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? Delays.ShortMillis : 10);
                    if (s.CurrentIndex == cancelAt) throw new Exception();
                    if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                    else
                    {
                        Assert.That(s.LowestBreakIteration, Is.Null);
                        Assert.That(s.IsExceptional, Is.True);
                        Assert.That(s.IsStopped, Is.False);
                    }
                });
            Assert.Catch(()=>_sut.ForEach(sources, Parallelism));
            Assert.That(completed, Has.No.Member(sources[0]));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCompletesIterationsLessThenBreakIndexOf(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? Delays.ShortMillis : 10);
                    if (s.CurrentIndex == cancelAt) s.Break();
                    else
                    {
                        if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                        else
                        {
                            Assert.That(s.LowestBreakIteration, Is.EqualTo(cancelAt));
                            Assert.That(s.IsExceptional, Is.False);
                            Assert.That(s.IsStopped, Is.False);
                        }
                    }
                });
            var result = _sut.ForEach(sources, Parallelism);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(cancelAt));
            Assert.That(completed.Count, Is.GreaterThanOrEqualTo(cancelAt));
            Assert.That(completed, Has.Member(sources[0]));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachRecordsTheLowestOfMultipleBreaks(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int breakAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            _sut = new ParallelCompletion<T>(_executor,
                (t, s) => BreakAt(s, breakAt));
            var result = _sut.ForEach(sources, Parallelism);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(breakAt));
            ThreadManager.JoinAndVerify();
        }
    }
}
