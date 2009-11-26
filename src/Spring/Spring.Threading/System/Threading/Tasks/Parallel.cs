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
namespace System.Threading.Tasks
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

        #region ForEach<TSource> Methods

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
            return new ParallelLoopResult(_executor.ForEach(source, body));
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, and a
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely.
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
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState> body)
        {
            var result = _executor.ForEach(source, 
                (s, pls) => body(s, new ParallelLoopState(pls)));
            return new ParallelLoopResult(result);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely, and the current elements's index (an
        /// <see cref="long"/>).
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
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState, long> body)
        {
            var result = _executor.ForEach(source, 
                (s, pls) => body(s, new ParallelLoopState(pls), pls.CurrentIndex));
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
        /// The exception that is thrown when the <paramref name="parallelOptions"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource> body)
        {
            return new ParallelLoopResult(_executor.ForEach(source, parallelOptions, body));
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, and a
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely.
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
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptions"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState> body)
        {
            var result = _executor.ForEach(source, parallelOptions, 
                (s, pls) => body(s, new ParallelLoopState(pls)));
            return new ParallelLoopResult(result);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely, and the current elements's index (an
        /// <see cref="long"/>).
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
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptions"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState, long> body)
        {
            var result = _executor.ForEach(source, parallelOptions, 
                (s, pls) => body(s, new ParallelLoopState(pls), pls.CurrentIndex));
            return new ParallelLoopResult(result);
        }

        #endregion

        #region ForEach<TSource, TLocal> Methods

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of  the loop prematurely, and some local state that may be 
        /// shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent body invocation returns a possibly modified state 
        /// value that is passed to the next body invocation. Finally, the last 
        /// body invocation on each thread returns a state value that is passed 
        /// to the <paramref name="localFinally"/> delegate. 
        /// The <paramref name="localFinally"/> delegate is invoked once per 
        /// thread to perform a final action on each thread's local state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            var result = _executor.ForEach(
                source,
                localInit,
                (s, pls, l) => body(s, new ParallelLoopState(pls), l),
                localFinally);
            return new ParallelLoopResult(result);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of  the loop prematurely, the current element's index (an 
        /// <see cref="long"/>), and some local state that may be shared amongst
        /// iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent body invocation returns a possibly modified state 
        /// value that is passed to the next body invocation. Finally, the last 
        /// body invocation on each thread returns a state value that is passed 
        /// to the <paramref name="localFinally"/> delegate. 
        /// The <paramref name="localFinally"/> delegate is invoked once per 
        /// thread to perform a final action on each thread's local state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            var result = _executor.ForEach(
                source,
                localInit,
                (s, pls, l) => body(s, new ParallelLoopState(pls), pls.CurrentIndex, l),
                localFinally);
            return new ParallelLoopResult(result);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of  the loop prematurely, and some local state that may be 
        /// shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent body invocation returns a possibly modified state 
        /// value that is passed to the next body invocation. Finally, the last 
        /// body invocation on each thread returns a state value that is passed 
        /// to the <paramref name="localFinally"/> delegate. 
        /// The <paramref name="localFinally"/> delegate is invoked once per 
        /// thread to perform a final action on each thread's local state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            var result = _executor.ForEach(
                source,
                parallelOptions,
                localInit,
                (s, pls, l) => body(s, new ParallelLoopState(pls), l),
                localFinally);
            return new ParallelLoopResult(result);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the following parameters: the current element, a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of  the loop prematurely, the current element's index (an 
        /// <see cref="long"/>), and some local state that may be shared amongst
        /// iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent body invocation returns a possibly modified state 
        /// value that is passed to the next body invocation. Finally, the last 
        /// body invocation on each thread returns a state value that is passed 
        /// to the <paramref name="localFinally"/> delegate. 
        /// The <paramref name="localFinally"/> delegate is invoked once per 
        /// thread to perform a final action on each thread's local state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            var result = _executor.ForEach(
                source,
                parallelOptions, 
                localInit,
                (s, pls, l) => body(s, new ParallelLoopState(pls), pls.CurrentIndex, l),
                localFinally);
            return new ParallelLoopResult(result);
        }

        #endregion

        #region For Methods

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an <see cref="int"/>).
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            int fromInclusive,
            int toExclusive,
            Action<int> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="int"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            int fromInclusive, 
            int toExclusive, 
            Action<int, ParallelLoopState> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an <see cref="long"/>).
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            long fromInclusive, 
            long toExclusive,
            Action<long> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="long"/>), and a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            long fromInclusive,
            long toExclusive,
            Action<long, ParallelLoopState> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an <see cref="int"/>).
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            int fromInclusive, 
            int toExclusive, 
            ParallelOptions parallelOptions, 
            Action<int> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="int"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Action<int, ParallelLoopState> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an <see cref="long"/>).
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Action<long> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="long"/>), and a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break
        /// out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For(
            long fromInclusive, 
            long toExclusive, 
            ParallelOptions parallelOptions,
            Action<long, ParallelLoopState> body)
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        #endregion

        #region For<TLocal> Methods

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="int"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent <paramref name="body"/> invocation returns a 
        /// possibly modified state value that is passed to the next body 
        /// invocation. Finally, the last body invocation on each thread 
        /// returns a state value that is passed to the <paramref name="localFinally"/>
        /// delegate. The <paramref name="localFinally"/> delegate is invoked 
        /// once per thread to perform a final action on each thread's local 
        /// state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TLocal">
        /// The type of the thread-local data.
        /// </typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For<TLocal>(
            int fromInclusive,
            int toExclusive,
            Func<TLocal> localInit,
            Func<int, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            return ForEach(fromInclusive.Loop(toExclusive), localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="long"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent <paramref name="body"/> invocation returns a 
        /// possibly modified state value that is passed to the next body 
        /// invocation. Finally, the last body invocation on each thread 
        /// returns a state value that is passed to the <paramref name="localFinally"/>
        /// delegate. The <paramref name="localFinally"/> delegate is invoked 
        /// once per thread to perform a final action on each thread's local 
        /// state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TLocal">
        /// The type of the thread-local data.
        /// </typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For<TLocal>(
            long fromInclusive,
            long toExclusive,
            Func<TLocal> localInit,
            Func<long, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            return ForEach(fromInclusive.Loop(toExclusive), localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="int"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent <paramref name="body"/> invocation returns a 
        /// possibly modified state value that is passed to the next body 
        /// invocation. Finally, the last body invocation on each thread 
        /// returns a state value that is passed to the <paramref name="localFinally"/>
        /// delegate. The <paramref name="localFinally"/> delegate is invoked 
        /// once per thread to perform a final action on each thread's local 
        /// state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TLocal">
        /// The type of the thread-local data.
        /// </typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For<TLocal>(
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<int, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an <see cref="long"/>), a 
        /// <see cref="ParallelLoopState"/> instance that may be used to break 
        /// out of the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each 
        /// thread that participates in the loop's execution and returns the 
        /// initial local state for each of those threads. These initial states 
        /// are passed to the first body invocations on each thread. Then, 
        /// every subsequent <paramref name="body"/> invocation returns a 
        /// possibly modified state value that is passed to the next body 
        /// invocation. Finally, the last body invocation on each thread 
        /// returns a state value that is passed to the <paramref name="localFinally"/>
        /// delegate. The <paramref name="localFinally"/> delegate is invoked 
        /// once per thread to perform a final action on each thread's local 
        /// state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TLocal">
        /// The type of the thread-local data.
        /// </typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">
        /// A <see cref="ParallelOptions"/> instance that configures the 
        /// behavior of this operation.
        /// </param>
        /// <param name="localInit">
        /// The function delegate that returns the initial state of the local 
        /// data for each thread.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <param name="localFinally">
        /// The delegate that performs a final action on the local state of 
        /// each thread.
        /// </param>
        /// <returns>
        /// An <see cref="ParallelLoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptions"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localInit"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="localFinally"/> 
        /// argument is null.
        /// </exception>
        public static ParallelLoopResult For<TLocal>(
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<long, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            return ForEach(fromInclusive.Loop(toExclusive), parallelOptions, localInit, body, localFinally);
        }

        #endregion

        #region Invoke Methods

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

        #endregion
    }
}
#endif
