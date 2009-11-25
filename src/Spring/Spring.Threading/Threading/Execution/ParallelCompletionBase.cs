using System;
using System.Collections.Generic;
using System.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    internal abstract class ParallelCompletionBase<T> : ILoopResult
    {
        private readonly IExecutor _executor;
        private int _maxDegreeOfParallelism;
        //TODO: use ArrayBlockingQueue after it's fully tested.
        protected LinkedBlockingQueue<KeyValuePair<long, T>> _itemQueue;
        private List<IFuture<object>> _futures;
        private Exception _exception;
        private int _taskCount;
        private int _maxCount;

        internal ParallelCompletionBase(IExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            _executor = executor;
        }

        public bool IsCompleted
        {
            get { return !ShouldExitCurrentIteration; }
        }

        public long? LowestBreakIteration
        {
            get;
            internal set;
        }

        internal bool ShouldExitCurrentIteration
        {
            get { return LowestBreakIteration.HasValue || IsStopped || IsExceptional; }
        }

        internal bool IsStopped
        {
            get;
            set;
        }

        internal bool IsExceptional
        {
            get { return _exception != null; }
        }

        internal int ActualDegreeOfParallelism { get { return _maxCount; } }
        protected abstract void Process(IEnumerator<T> iterator);
        protected abstract void Process(KeyValuePair<long, T> source);

        internal ILoopResult ForEach(IEnumerable<T> source, ParallelOptions parallelOptions)
        {
            if (parallelOptions == null) throw new ArgumentNullException("parallelOptions");
            return ForEach(source, parallelOptions.MaxDegreeOfParallelism);
        }

        internal ILoopResult ForEach(IEnumerable<T> source, int maxDegreeOfParallelism)
        {
            if (source == null) throw new ArgumentNullException("source");

            _maxDegreeOfParallelism = OptimizeMaxDegreeOfParallelism(source, maxDegreeOfParallelism);

            var iterator = source.GetEnumerator();
            if (!iterator.MoveNext()) return this;
            if (_maxDegreeOfParallelism == 1)
            {
                Process(iterator);
                return this;
            }

            _itemQueue = new LinkedBlockingQueue<KeyValuePair<long, T>>(_maxDegreeOfParallelism);
            _futures = new List<IFuture<object>>(_maxDegreeOfParallelism);

            try
            {
                Submit(StartParallel);
            }
            catch(RejectedExecutionException)
            {
                Process(iterator);
                return this;
            }

            bool success;
            long count = 0;
            do success = _itemQueue.TryPut(new KeyValuePair<long, T>(count++, iterator.Current));
            while (success && iterator.MoveNext());

            _itemQueue.Close();

            WaitForAllTaskToComplete();
            return this;
        }

        private int OptimizeMaxDegreeOfParallelism(IEnumerable<T> source, int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 0) maxDegreeOfParallelism = int.MaxValue;

            var tpe = _executor as ThreadPoolExecutor;
            if (tpe != null)
                maxDegreeOfParallelism = Math.Min(tpe.CorePoolSize, maxDegreeOfParallelism);

            var c = source as ICollection<T>;
            if (c != null) maxDegreeOfParallelism = Math.Min(c.Count, maxDegreeOfParallelism);
            return maxDegreeOfParallelism;
        }

        private void Submit(Action action)
        {
            Action task =
                delegate
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            lock (this)
                            {
                                if (_exception == null) _exception = e;
                                _itemQueue.Break();
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
            lock (this)
            {
                _executor.Execute(f);
                _maxCount = Math.Max(++_taskCount,_maxCount);
            }
            _futures.Add(f);
        }

        private void StartParallel()
        {
            KeyValuePair<long, T> x;
            while (_itemQueue.TryTake(out x))
            {
                if (_taskCount < _maxDegreeOfParallelism)
                {
                    KeyValuePair<long, T> source = x;
                    try
                    {
                        Submit(() => Process(source));
                        continue;
                    }
                    catch (RejectedExecutionException)
                    {
                        // fine we'll just run with less parallelism
                    }
                }
                Process(x);
                break;
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

        protected class LoopState : ILoopState
        {
            private readonly ParallelCompletionBase<T> _parent;
            public LoopState(ParallelCompletionBase<T> parent)
            {
                _parent = parent;
            }

            public bool ShouldExitCurrentIteration
            {
                get { return _parent.ShouldExitCurrentIteration; }
            }

            public long? LowestBreakIteration
            {
                get { return _parent.LowestBreakIteration; }
            }

            public bool IsStopped
            {
                get { return _parent.IsStopped; }
            }

            public bool IsExceptional
            {
                get { return _parent.IsExceptional; }
            }

            public void Stop()
            {
                _parent.IsStopped = true;
            }

            public void Break()
            {
                _parent.LowestBreakIteration = CurrentIndex;
            }

            public long CurrentIndex { get; internal set; }
        }
    }
}