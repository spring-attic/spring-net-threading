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
    /// <summary>
    /// Extension methods to provide parallel functions.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public static class ParallelExtension
    {
        internal static IEnumerable<long> Loop(this long fromInclusive, long toExclusive)
        {
            for (long i = fromInclusive; i < toExclusive; i++) yield return i;
        }

        internal static IEnumerable<int> Loop(this int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++) yield return i;
        }


        #region ForEach<TSource> Methods

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ILoopResult ForEach<TSource>(this IExecutor executor, IEnumerable<TSource> source, Action<TSource> body)
        {
            return new ParallelCompletion<TSource>(executor, (s, ls) => body(s))
                .ForEach(source, int.MaxValue);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="source">
        /// An enumerable data source.
        /// </param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ILoopResult ForEach<TSource>(this IExecutor executor, IEnumerable<TSource> source, Action<TSource, ILoopState> body)
        {
            return new ParallelCompletion<TSource>(executor, body)
                .ForEach(source, int.MaxValue);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptins"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ILoopResult ForEach<TSource>(this IExecutor executor, IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            return new ParallelCompletion<TSource>(executor, (s, ls) => body(s)).ForEach(source, parallelOptions);
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="IEnumerable{TSource}"/> 
        /// in which iterations may run in parallel using this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each 
        /// element in the <paramref name="source"/> enumerable. It is provided
        /// with the current element as a parameter.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the data in the source.
        /// </typeparam>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptins"/> argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> argument is null.
        /// </exception>
        public static ILoopResult ForEach<TSource>(this IExecutor executor, IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, ILoopState> body)
        {
            return new ParallelCompletion<TSource>(executor, body).ForEach(source, parallelOptions);
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
        /// <see cref="ILoopState"/> instance that may be used to break out of 
        /// the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
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
        public static ILoopResult ForEach<TSource, TLocal>(
            this IExecutor executor, 
            IEnumerable<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return new ParallelCompletion<TSource, TLocal>(executor, localInit, body, localFinally)
                .ForEach(source, int.MaxValue);
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
        /// <see cref="ILoopState"/> instance that may be used to break out of 
        /// the loop prematurely, and some local state that may be shared 
        /// amongst iterations that execute on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="source"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
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
        public static ILoopResult ForEach<TSource, TLocal>(
            this IExecutor executor, 
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return new ParallelCompletion<TSource, TLocal>(executor, localInit, body, localFinally)
                .ForEach(source, parallelOptions);
        }

        #endregion

        #region For Methods

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an Int32).
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(this IExecutor executor, int fromInclusive, int toExclusive, Action<int> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int32), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(this IExecutor executor, int fromInclusive, int toExclusive, Action<int, ILoopState> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an Int64).
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(this IExecutor executor, long fromInclusive, long toExclusive, Action<long> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int64), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">
        /// The delegate that is invoked once per iteration.
        /// </param>
        /// <returns>
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(this IExecutor executor, long fromInclusive, long toExclusive, Action<long, ILoopState> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an Int32).
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(
            this IExecutor executor, 
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Action<int> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int32), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(
            this IExecutor executor, 
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Action<int, ILoopState> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the iteration 
        /// count (an Int64).
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(
            this IExecutor executor, 
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Action<long> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int64), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
        /// argument is null.<br/>
        /// -or-<br/>
        /// The exception that is thrown when the <paramref name="body"/> 
        /// argument is null.
        /// </exception>
        public static ILoopResult For(
            this IExecutor executor, 
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Action<long, ILoopState> body)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, body);
        }

        #endregion

        #region For<TLocal> Methods

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int32), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely, and 
        /// some local state that may be shared amongst iterations that execute 
        /// on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
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
        public static ILoopResult For<TLocal>(
            this IExecutor executor, 
            int fromInclusive,
            int toExclusive,
            Func<TLocal> localInit,
            Func<int, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int64), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely, and 
        /// some local state that may be shared amongst iterations that execute 
        /// on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
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
        public static ILoopResult For<TLocal>(
            this IExecutor executor, 
            long fromInclusive,
            long toExclusive,
            Func<TLocal> localInit,
            Func<long, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int32), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely, and 
        /// some local state that may be shared amongst iterations that execute 
        /// on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
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
        public static ILoopResult For<TLocal>(
            this IExecutor executor, 
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<int, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, localInit, body, localFinally);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel using 
        /// this <see cref="IExecutorService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value 
        /// in the iteration range: [<paramref name="fromInclusive"/>, 
        /// <paramref name="toExclusive"/>). It is provided with the following 
        /// parameters: the iteration count (an Int64), a <see cref="ILoopState"/>
        /// instance that may be used to break out of the loop prematurely, and 
        /// some local state that may be shared amongst iterations that execute 
        /// on the same thread.
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
        /// <param name="executor">
        /// An <see cref="IExecutor"/> to run the parallel tasks.
        /// </param>
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
        /// An <see cref="ILoopResult"/> instance that contains information 
        /// on what portion of the loop completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when the <paramref name="parallelOptins"/> 
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
        public static ILoopResult For<TLocal>(
            this IExecutor executor, 
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<long, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            return ForEach(executor, fromInclusive.Loop(toExclusive), parallelOptions, localInit, body, localFinally);
        }

        #endregion

    }
}