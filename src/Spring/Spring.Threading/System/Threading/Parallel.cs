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

using System.Collections.Generic;
using Spring.Threading;
using Spring.Threading.Execution;

#if !NET_4_0
namespace System.Threading
{
    /// <summary>
    /// Provides support for parallel loops and regions.
    /// </summary>
    /// <remarks>
    /// The <see cref="Parallel"/> class provides library-based data parallel 
    /// replacements for common operations such as for loops, for each loops, 
    /// and execution of a set of statements.
    /// </remarks>
    /// <author>Kenneth Xu</author>
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

        #region ForEach Methods

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
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource> body)
        {
            var result = new ParallelCompletion<TSource>(_executor, (s, pls) => body(s))
                .ForEach(source, int.MaxValue);
            return new ParallelLoopResult(result);
        }

        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState> body)
        {
            var result = new ParallelCompletion<TSource>(_executor, (s, pls) => body(s, new ParallelLoopState(pls)))
                .ForEach(source, int.MaxValue);
            return new ParallelLoopResult(result);
        }

        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState, long> body
        )
        {
            var result = new ParallelCompletion<TSource>(_executor, (s, pls) => body(s, new ParallelLoopState(pls), pls.CurrentIndex))
                .ForEach(source, int.MaxValue);
            return new ParallelLoopResult(result);
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
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource> body)
        {
            var result = new ParallelCompletion<TSource>(_executor, (s, pls) => body(s))
                .ForEach(source, parallelOptions);
            return new ParallelLoopResult(result);
        }

        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState> body)
        {
            var result = new ParallelCompletion<TSource>(_executor, (s, pls) => body(s, new ParallelLoopState(pls)))
                .ForEach(source, parallelOptions);
            return new ParallelLoopResult(result);
        }

        #endregion

        #region For Methods

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), body);
        }

        public static ParallelLoopResult For(
            int fromInclusive, 
            int toExclusive, 
            Action<int, ParallelLoopState> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), body);
        }

        public static ParallelLoopResult For(
            long fromInclusive, 
            long toExclusive,
            Action<long> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), body);
        }

        public static ParallelLoopResult For(
            long fromInclusive,
            long toExclusive,
            Action<long, ParallelLoopState> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), body);
        }

        public static ParallelLoopResult For(
            int fromInclusive, 
            int toExclusive, 
            ParallelOptions parallelOptions, 
            Action<int> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), parallelOptions, body);
        }

        public static ParallelLoopResult For(
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Action<int, ParallelLoopState> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), parallelOptions, body);
        }

        public static ParallelLoopResult For(
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Action<long> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), parallelOptions, body);
        }

        public static ParallelLoopResult For(
            long fromInclusive, 
            long toExclusive, 
            ParallelOptions parallelOptions,
            Action<long, ParallelLoopState> body)
        {
            return ForEach(ParallelCompletion.Loop(fromInclusive, toExclusive), parallelOptions, body);
        }

        #endregion

        /// <summary>
        /// Executes each of the provided actions, possibly in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be used to execute a set of operations, 
        /// potentially in parallel. 
        /// </para>
        /// <para>
        /// No guarantees are made about the order in which the operations 
        /// execute or whether they execute in parallel. This method does 
        /// not return until each of the provided operations has completed, 
        /// regardless of whether completion occurs due to normal or 
        /// exceptional termination.
        /// </para>
        /// </remarks>
        /// <param name="actions">
        /// An array of <see cref="Action"/> to execute.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="actions"/> 
        /// argument is null.
        /// </exception>
        /// <exception cref="AggregateException">
        /// The exception that is thrown when any action in the actions array 
        /// throws an exception.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The exception that is thrown when the <paramref name="actions"/> 
        /// array contains a null element.
        /// </exception>
        public static void Invoke(Action[] actions)
        {
            ForEach(actions, b => b());
        }

        /// <summary>
        /// Executes each of the provided actions, possibly in parallel.
        /// </summary>
        /// 
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="actions">
        /// An array of <see cref="Action"/> to execute.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="actions"/> 
        /// argument is null.
        /// </exception>
        /// <exception cref="AggregateException">
        /// The exception that is thrown when any action in the actions array 
        /// throws an exception.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The exception that is thrown when the <paramref name="actions"/> 
        /// array contains a null element.
        /// </exception>
        public static void Invoke(ParallelOptions parallelOptions, Action[] actions)
        {
            ForEach(actions, parallelOptions, b => b());
        }
    }
}
#endif
