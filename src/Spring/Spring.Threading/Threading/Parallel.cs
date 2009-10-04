using System;
using System.Collections.Generic;
using System.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading
{
    /// <summary>
    /// Provides support for parallel loops and regions.
    /// </summary>
    /// <remarks>
    /// The <see cref="Parallel"/> class provides library-based data parallel 
    /// replacements for common operations such as for loops, for each loops, 
    /// and execution of a set of statements.
    /// </remarks>
    public static class Parallel
    {
        private static readonly IExecutor _executor = new SystemPoolExecutor();

        private class SystemPoolExecutor : IExecutor
        {
            public void Execute(IRunnable command)
            {
                ThreadPool.QueueUserWorkItem(a => command.Run());
            }

            public void Execute(Action action)
            {
                ThreadPool.QueueUserWorkItem(a => action());
            }
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource> body)
        {
            new Parallel<TSource>(_executor).ForEach(source, int.MaxValue, body);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptins"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static void ForEach<TSource>(
            IEnumerable<TSource> source, 
            ParallelOptions parallelOptions, 
            Action<TSource> body)
        {
            new Parallel<TSource>(_executor).ForEach(source, parallelOptions, body);
        }
    }

    internal class Parallel<T>
    {
        private readonly IExecutor _executor;
        private int _maxDegreeOfParallelism;
        private Action<T> _body;

        //TODO: use ArrayBlockingQueue after it's fully tested.
        private LinkedBlockingQueue<T> _itemQueue;
        private List<IFuture<object>> _futures;
        private Exception _exception;
        private int _taskCount;

        public Parallel(IExecutor executor)
        {
            if(executor==null) throw new ArgumentNullException("executor");
            _executor = executor;
        }

        public void ForEach(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> body)
        {
            if (parallelOptions == null) throw new ArgumentNullException("parallelOptions");
            ForEach(source, parallelOptions.MaxDegreeOfParallelism, body);
        }

        public void ForEach(IEnumerable<T> source, int maxDegreeOfParallelism, Action<T> body)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (body == null) throw new ArgumentNullException("body");
            if (maxDegreeOfParallelism < 0) maxDegreeOfParallelism = int.MinValue;

            var c = source as ICollection<T>;
            _maxDegreeOfParallelism =
                c != null && c.Count < maxDegreeOfParallelism ?
                c.Count : maxDegreeOfParallelism;

            _body = body;

            var iterator = source.GetEnumerator();
            if (!iterator.MoveNext()) return;
            if (_maxDegreeOfParallelism == 1)
            {
                _body(iterator.Current);
                return;
            }

            _itemQueue = new LinkedBlockingQueue<T>(_maxDegreeOfParallelism);
            _futures = new List<IFuture<object>>(_maxDegreeOfParallelism);

            Submit(StartParallel);
            do _itemQueue.Put(iterator.Current); while (iterator.MoveNext());
            _itemQueue.Close();
            WaitForAllTaskToComplete();
        }

        private void Submit(Action action)
        {
            Action task =
                delegate
                {
                    try
                    {
                        lock (this) _taskCount++;
                        action();
                    }
                    catch (Exception e)
                    {
                        lock (this)
                        {
                            if (_exception == null) _exception = e;
                        }
                    }
                    finally
                    {
                        lock (this)
                        {
                            _taskCount--;
                            Monitor.Pulse(this);
                        }
                    }
                };
            var f = new FutureTask<object>(task, null);
            _executor.Execute(f);
            _futures.Add(f);
        }

        private void StartParallel()
        {
            T x;
            while (_itemQueue.TryTake(out x))
            {
                if (_taskCount < _maxDegreeOfParallelism)
                {
                    T source = x;
                    Submit(() => Process(source));
                }
                else
                {
                    Process(x);
                    break;
                }
            }
        }

        private void WaitForAllTaskToComplete()
        {
            lock (this)
            {
                while (true)
                {
                    if (_exception != null)
                    {
                        foreach (var future in _futures)
                        {
                            future.Cancel(true);
                        }
                        throw new AggregateException(_exception.Message, _exception);
                    }
                    if (_taskCount == 0) return;
                    Monitor.Wait(this);
                }
            }
        }

        private void Process(T source)
        {
            _body(source);
            T x;
            while (_itemQueue.TryTake(out x))
            {
                _body(x);
            }
        }
    }
}
