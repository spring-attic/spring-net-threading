using System;
using System.Collections.Generic;
using System.Threading;
using Spring.Threading.AtomicTypes;
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
        private readonly AtomicLong _lowestBreakIteration = new AtomicLong(-1);
        private List<IFuture<object>> _futures;
        private Exception _exception;
        private int _taskCount;
        private int _maxCount;
        private bool _isStopped;

        internal ParallelCompletionBase(IExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            _executor = executor;
        }

        public bool IsCompleted
        {
            get { return !ShouldExitCurrentIteration(long.MaxValue); }
        }

        public long? LowestBreakIteration
        {
            get
            {
                long lowestBreakIteration = _lowestBreakIteration;
                return lowestBreakIteration == -1 ? (long?) null : lowestBreakIteration;
            }
        }

        internal void Break(long iteration)
        {
            long lowestBreakIteration;
            do
            {
                lowestBreakIteration = _lowestBreakIteration;
                if (lowestBreakIteration >= 0 && lowestBreakIteration <= iteration) break;
            } while (!_lowestBreakIteration.CompareAndSet(lowestBreakIteration, iteration));
        }

        internal bool ShouldExitCurrentIteration(long iteration)
        {
            long lowestBreakIteration = _lowestBreakIteration; //this causes memory barrier.
            var isBroken = (lowestBreakIteration >= 0 && iteration >= lowestBreakIteration);
            if (isBroken) _itemQueue.Break();
            return isBroken || _isStopped || _exception != null;
        }

        internal bool IsStopped
        {
            get
            {
                Thread.MemoryBarrier();
                return _isStopped;
            }
        }

        internal void Stop()
        {
            _isStopped = true;
            _itemQueue.Stop();
        }

        internal bool IsExceptional
        {
            get
            {
                Thread.MemoryBarrier();
                return _exception != null;
            }
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
            // avoid running out of memory.
            var capacity = Math.Min(512, _maxDegreeOfParallelism);
            _itemQueue = new LinkedBlockingQueue<KeyValuePair<long, T>>(capacity);
            _futures = new List<IFuture<object>>(capacity);

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

            _itemQueue.Break();

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
                                _itemQueue.Stop();
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
                get { return _parent.ShouldExitCurrentIteration(CurrentIndex); }
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
                _parent.Stop();
            }

            public void Break()
            {
                _parent.Break(CurrentIndex);
            }

            public long CurrentIndex { get; internal set; }
        }
    }
}