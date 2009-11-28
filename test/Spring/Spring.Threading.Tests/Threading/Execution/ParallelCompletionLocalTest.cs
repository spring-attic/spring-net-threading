using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="ParallelCompletion{T,L}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class ParallelCompletionLocalTest<T> : ParallelCompletionTestBase
    {
        private ParallelCompletion<T, int> _sut;
        private Func<int> _localInit;
        private Action<int> _localFinally;
        private Func<T, ILoopState, int, int> _body;

        [SetUp] public void SetUpDelegates()
        {
            _localInit = MockRepository.GenerateMock<Func<int>>();
            _localInit.Stub(x => x()).Return(0);
            _localFinally = MockRepository.GenerateMock<Action<int>>();
            _localFinally.Stub(x => x(Arg<int>.Is.Anything));
            _body = MockRepository.GenerateMock<Func<T, ILoopState, int, int>>();
            _body.Stub(x => x(Arg<T>.Is.Anything, Arg<ILoopState>.Is.Anything, Arg<int>.Is.Anything)).Return(0);
        }

        [Test] public void ConstructorChokesOnNullExecutor()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T, int>(
                    null, _localInit, _body, _localFinally));
            Assert.That(e.ParamName, Is.EqualTo("executor"));
        }

        [Test] public void ForEachChokesOnNullSource()
        {
            _sut = new ParallelCompletion<T, int>(_executor, 
                _localInit, _body, _localFinally);
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(null, 2));
            Assert.That(e.ParamName, Is.EqualTo("source"));
            e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(null, new ParallelOptions()));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ForEachChokesOnNullLocalInit()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T, int>(_executor,
                    null, _body, _localFinally));
            Assert.That(e.ParamName, Is.EqualTo("localInit"));
        }


        [Test] public void ForEachChokesOnNullBody()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T, int>(_executor,
                    _localInit, null, _localFinally));
            Assert.That(e.ParamName, Is.EqualTo("body"));
        }

        [Test] public void ForEachChokesOnNullLocalFinally()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => new ParallelCompletion<T, int>(_executor,
                    _localInit, _body, null));
            Assert.That(e.ParamName, Is.EqualTo("localFinally"));
        }

        [Test] public void ForEachChokesOnNullParallelOptions()
        {
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit, _body, _localFinally);
            var e = Assert.Throws<ArgumentNullException>(
                () => _sut.ForEach(new T[0], null));
            Assert.That(e.ParamName, Is.EqualTo("parallelOptions"));
        }

        [Test] public void ForEachReturnsImmediatelyOnEmptySource()
        {
            List<T> results = new List<T>();
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                    {
                        lock (results) results.Add(t);
                        return 0;
                    },
                _localFinally);
            _sut.ForEach(new T[0], new ParallelOptions());
            Assert.That(results, Is.Empty);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(0));
        }

        [Test] public void ForEachUseCurrentThreadWhenParallelistIsOne()
        {
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                    {
                        Thread.Sleep(10); lock (results) results.Add(t);
                        return 0;
                    },
                _localFinally);
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
            _sut = new ParallelCompletion<T, int>(_executor, 
                _localInit,
                (t, s, l) =>
                    {
                        lock (results) results.Add(t);
                        return 0;
                    },
                _localFinally);
            _sut.ForEach(sources, new ParallelOptions { MaxDegreeOfParallelism = Parallelism });
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.LessThanOrEqualTo(Parallelism));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCompletesAllSlowTasksWithMaxThreadOf(
            [Values(int.MaxValue, 3, 1, 0)] int maxThread)
        {
            _executor.Threshold = maxThread;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                    {
                        Thread.Sleep(10); lock (results) results.Add(t);
                        return 0;
                    },
                _localFinally);
            _sut.ForEach(sources, Parallelism);
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(Math.Min(Parallelism, maxThread)));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachDoesNotSumitMoreThenMaxDegreeOfParallelism()
        {
            _executor.Delay = SHORT_DELAY;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                    {
                        Thread.Sleep(10); lock (results) results.Add(t);
                        return 0;
                    },
                _localFinally);
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
                var parallel = new ParallelCompletion<T, int>(executor,
                    _localInit,
                    (t, s, l) =>
                        {
                            Thread.Sleep(10); lock (results) results.Add(t);
                            return 0;
                        },
                    _localFinally);
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
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                {
                    if (Equals(t, sources[0]))
                    {
                        Thread.Sleep(SHORT_DELAY);
                        throw exception;
                    }
                    var e2 = Assert.Throws<ThreadInterruptedException>(
                        () => Thread.Sleep(LONG_DELAY_MS));
                    if (!Equals(t, sources[_sampleSize - 1])) throw e2;
                    return 0;
                },
                _localFinally);
            ThreadManager.StartAndAssertRegistered("Driver",
                () =>
                {
                    var e = Assert.Throws<AggregateException>(() =>
                        _sut.ForEach(sources, Parallelism));
                    Assert.That(e.InnerException, Is.SameAs(exception));
                    Assert.That(_executor.ThreadCount.Value, Is.EqualTo(Parallelism));
                });
            Thread.Sleep(SHORT_DELAY);
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachStopsAsSoonAsStopCalledAt(
            [Values(Parallelism / 2, Parallelism, Parallelism*2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? SHORT_DELAY_MS : 10);
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
                    return 0;
                },
                _localFinally);
            var result = _sut.ForEach(sources, Parallelism);
            _localFinally.AssertWasCalled(x => x(0));
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(completed, Has.No.Member(sources[0]));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachStopsAsSoonAsExceptionThrownAt(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? SHORT_DELAY_MS : 10);
                    if (s.CurrentIndex == cancelAt) throw new Exception();
                    if (!s.ShouldExitCurrentIteration) lock (completed) completed.Add(t);
                    else
                    {
                        Assert.That(s.LowestBreakIteration, Is.Null);
                        Assert.That(s.IsExceptional, Is.True);
                        Assert.That(s.IsStopped, Is.False);
                    }
                    return 0;
                },
                _localFinally);
            Assert.Catch(()=>_sut.ForEach(sources, Parallelism));
            Assert.That(completed, Has.No.Member(sources[0]));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCompletesIterationsLessThenBreakIndexOf(
            [Values(Parallelism / 2, Parallelism, Parallelism * 2)] int cancelAt)
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> completed = new List<T>(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                {
                    Thread.Sleep(s.CurrentIndex == 0 ? SHORT_DELAY_MS : 10);
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
                    return 0;
                },
                _localFinally);
            var result = _sut.ForEach(sources, Parallelism);
            _localFinally.AssertWasCalled(x => x(0));
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
            _sut = new ParallelCompletion<T, int>(_executor,
                _localInit,
                (t, s, l) =>
                {
                    BreakAt(s, breakAt);
                    return 0;
                },
                _localFinally);
            var result = _sut.ForEach(sources, Parallelism);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.LowestBreakIteration, Is.EqualTo(breakAt));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCallsLocalInitBodyLocalFinallyInOrder()
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor, _localInit, _body, _localFinally);
            _sut.ForEach(sources, Parallelism);

            (
                _localFinally.ActivityOf(x => x(Arg<int>.Is.Anything)).First
                >
                _body.ActivityOf(x => x(Arg<T>.Is.Anything, Arg<ILoopState>.Is.Anything, Arg<int>.Is.Anything)).First
                >
                _localInit.ActivityOf(x => x()).First
            )
            .AssertOccured();

            (
                _localFinally.ActivityOf(x => x(Arg<int>.Is.Anything)).Last
                >
                _body.ActivityOf(x => x(Arg<T>.Is.Anything, Arg<ILoopState>.Is.Anything, Arg<int>.Is.Anything)).Last
                >
                _localInit.ActivityOf(x => x()).Last
            )
            .AssertOccured();

            ThreadManager.JoinAndVerify();
        }

        [Test] public void UseLocalToCount()
        {
            object @lock = new object();
            int count = 0;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            _sut = new ParallelCompletion<T, int>(_executor,
                () => 0,
                (t, s, l) => l + 1,
                l => { lock (@lock) count += l; });

            _sut.ForEach(sources, Parallelism);
            Assert.That(count, Is.EqualTo(_sampleSize));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void UseLocalToCollect()
        {
            List<T> collected = new List<T>();
            T[] sources = TestData<T>.MakeTestArray(100);
            var sut = new ParallelCompletion<T, IList<T>>(_executor,
                () => new List<T>(),
                (t, s, l) => { l.Add(t); return l; },
                l => { lock (collected) collected.AddRange(l); });

            sut.ForEach(sources, Parallelism);

            Assert.That(collected, Is.EquivalentTo(sources));
            ThreadManager.JoinAndVerify();
        }
    }
}
