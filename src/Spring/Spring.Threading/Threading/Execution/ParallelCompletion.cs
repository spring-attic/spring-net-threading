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

namespace Spring.Threading.Execution
{
    public static class ParallelCompletion
    {
        internal static IEnumerable<long> Loop(long fromInclusive, long toExclusive)
        {
            for (long i = fromInclusive; i < toExclusive; i++) yield return i;
        }

        internal static IEnumerable<int> Loop(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++) yield return i;
        }

    }

    /// <summary>
    /// Implemetation of parallel functions.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <author>Kenneth Xu</author>
    internal class ParallelCompletion<TSource> : ParallelCompletionBase<TSource>
    {
        private readonly Action<TSource, ILoopState> _body;

        internal ParallelCompletion(IExecutor executor, Action<TSource, ILoopState> body)
            : base(executor)
        {
            if (body == null) throw new ArgumentNullException("body");
            _body = body;
        }

        protected override void Process(IEnumerator<TSource> iterator)
        {
            var state = new LoopState(this);
            long count = 0;
            do
            {
                state.CurrentIndex = count++;
                _body(iterator.Current, state);
            } 
            while (!state.ShouldExitCurrentIteration && iterator.MoveNext());
        }

        protected override void Process(KeyValuePair<long, TSource> source)
        {
            var state = new LoopState(this);
            for (bool hasMore = true;
                hasMore && !ShouldExitCurrentIteration; 
                hasMore = _itemQueue.TryTake(out source) )
            {
                state.CurrentIndex = source.Key;
                _body(source.Value, state);
            }
        }
    }

    internal class ParallelCompletion<TSource, TLocal> : ParallelCompletionBase<TSource>
    {
        private readonly Func<TLocal> _localInit;
        private readonly Func<TSource, ILoopState, TLocal, TLocal> _body;
        private readonly Action<TLocal> _localFinally;

        internal ParallelCompletion(
            IExecutor executor, 
            Func<TLocal> localInit, 
            Func<TSource, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
            : base(executor)
        {
            if (localInit == null) throw new ArgumentNullException("localInit");
            if (body == null) throw new ArgumentNullException("body");
            if (localFinally == null) throw new ArgumentNullException("localFinally");
            _localInit = localInit;
            _body = body;
            _localFinally = localFinally;
        }

        protected override void Process(IEnumerator<TSource> iterator)
        {
            var state = new LoopState(this);
            long count = 0;
            var local = _localInit();
            do
            {
                state.CurrentIndex = count++;
                local = _body(iterator.Current, state, local);
            }
            while (!state.ShouldExitCurrentIteration && iterator.MoveNext());
            _localFinally(local);
        }

        protected override void Process(KeyValuePair<long, TSource> source)
        {
            var state = new LoopState(this);
            var local = _localInit();
            for (bool hasMore = true;
                hasMore && !ShouldExitCurrentIteration;
                hasMore = _itemQueue.TryTake(out source))
            {
                state.CurrentIndex = source.Key;
                local = _body(source.Value, state, local);
            }
            _localFinally(local);
        }
    }
}