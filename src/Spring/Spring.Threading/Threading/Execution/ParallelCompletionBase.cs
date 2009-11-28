#region License

/*
 * Copyright (C) 2002-2009 the original author or authors.
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
using System.Collections.Generic;
using System.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Implementation of core logic for parallel functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Kenneth Xu</author>
    internal abstract class ParallelCompletionBase<T> : ILoopResult
    {
        private readonly IExecutor _executor;
        private int _maxDegreeOfParallelism;
        //TODO: use ArrayBlockingQueue after it's fully tested.
        private LinkedBlockingQueue<KeyValuePair<long, T>> _itemQueue;
        private readonly AtomicLong _lowestBreakIteration = new AtomicLong(-1);
        private List<IFuture<object>> _futures;
        private volatile Exception _exception;
        private int _taskCount;
        private int _maxCount;
        private volatile bool _isStopped;

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

        protected abstract void Process(ILoopState state, IEnumerable<T> sources);

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
            long lowestBreakIteration = _lowestBreakIteration;
            var isBroken = (lowestBreakIteration >= 0 && iteration >= lowestBreakIteration);
            if (isBroken) _itemQueue.Break();
            return isBroken || _isStopped || _exception != null;
        }

        internal bool IsStopped
        {
            get
            {
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
                return _exception != null;
            }
        }

        internal int ActualDegreeOfParallelism { get { return _maxCount; } }

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

            var recommend = _executor as IRecommendParallelism;
            if (recommend != null)
                maxDegreeOfParallelism = Math.Min(recommend.MaxParallelism, maxDegreeOfParallelism);

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
                _taskCount++; // Must increase before submit for accurate counting.
                _executor.Execute(f);
                _maxCount = Math.Max(_taskCount,_maxCount);
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
                        lock(this) _taskCount--;
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

        private void Process(IEnumerator<T> iterator)
        {
            var state = new LoopState(this);
            Process(state, MakeSources(state, iterator));
        }

        private void Process(KeyValuePair<long, T> source)
        {
            var state = new LoopState(this);
            Process(state, MakeSources(state, source));
        }

        private IEnumerable<T> MakeSources(LoopState state, IEnumerator<T> iterator)
        {
            long count = 0;
            do
            {
                state.CurrentIndex = count++;
                yield return iterator.Current;
            }
            while (!ShouldExitCurrentIteration(count) && iterator.MoveNext());
        }

        private IEnumerable<T> MakeSources(LoopState state, KeyValuePair<long, T> source)
        {
            for (bool hasMore = true;
                 hasMore && !ShouldExitCurrentIteration(source.Key);
                 hasMore = _itemQueue.TryTake(out source))
            {
                state.CurrentIndex = source.Key;
                yield return source.Value;
            }
        }

        private class LoopState : ILoopState
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