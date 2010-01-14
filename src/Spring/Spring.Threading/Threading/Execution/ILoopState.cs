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

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Enables iterations of <see cref="IExecutorService"/> loops to interact 
    /// with other iterations. A different <see cref="ILoopState"/> instance 
    /// is provided to each thread involved in a loop.
    /// </summary>
    public interface ILoopState // NET_ONLY
    {
        /// <summary>
        /// Gets whether the current iteration of the loop should exit based 
        /// on requests made by this or other iterations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an iteration of a loop calls <see cref="Break"/> or 
        /// <see cref="Stop"/>, or when one throws an exception, the 
        /// <see cref="IExecutorService"/> class will proactively attempt to 
        /// prohibit additional iterations of the loop from starting execution.
        /// </para>
        /// <para>
        /// However, there may be cases where it is unable to prevent additional 
        /// iterations from starting.
        /// </para>
        /// <para>
        /// It may also be the case that a long-running iteration has already 
        /// begun execution. In such cases, iterations may explicitly check the 
        /// <see cref="ShouldExitCurrentIteration"/> property and cease 
        /// execution if the property returns true.
        /// </para>
        /// </remarks>
        /// <value>
        /// True if the current iteration should exit, otherwise false.
        /// </value>
        bool ShouldExitCurrentIteration { get; }

        /// <summary>
        /// Gets the lowest iteration of the loop from which <see cref="Break"/>
        /// was called.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If no iteration of the loop called <see cref="Break"/>, this 
        /// property will return null.
        /// </para>
        /// </remarks>
        /// <value>
        /// A <see cref="int"/> that represents the lowest iteration of the 
        /// loop from which <see cref="Break"/> was called.
        /// </value>
        long? LowestBreakIteration { get; }

        /// <summary>
        /// Gets whether any iteration of the loop has called <see cref="Stop"/>.
        /// </summary>
        /// <value>
        /// True if any iteration has called <see cref="Stop"/>, otherwise false.
        /// </value>
        bool IsStopped { get; }

        /// <summary>
        /// Gets whether any iteration of the loop has thrown an exception that 
        /// went unhandled by that iteration.
        /// </summary>
        /// <value>
        /// True if any iteration has thrown an exception that went unhandled, 
        /// otherwise false.
        /// </value>
        bool IsExceptional { get; }

        /// <summary>
        /// Gets the index of current iteration of the loop.
        /// </summary>
        /// <value>
        /// The zero indexed position of current iteration source in the source
        /// enumerable.
        /// </value>
        long CurrentIndex { get; }

        /// <summary>
        /// Communicates that the <see cref="IExecutorService"/> loop should 
        /// cease execution at the system's earliest convenience.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Stop"/> may be used to communicate to the loop that no 
        /// other iterations need be run.
        /// </para>
        /// <para>
        /// For long-running iterations that may already be executing, 
        /// <see cref="Stop"/> causes <see cref="IsStopped"/> to return true 
        /// for all other iterations of the loop, such that another iteration 
        /// may check <see cref="IsStopped"/> and exit early if it's observed 
        /// to be true.
        /// </para>
        /// <para>
        /// <see cref="Stop"/> is typically employed in search-based algorithms, 
        /// where once a result is found, no other iterations need be executed.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Break"/> method was previously called. <see cref="Break"/> 
        /// and <see cref="Stop"/> may not be used in combination by iterations 
        /// of the same loop.
        /// </exception>
        void Stop();

        /// <summary>
        /// Communicates that the <see cref="IExecutorService"/> loop should 
        /// cease execution at the system's earliest convenience of iterations 
        /// beyond the current iteration. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Break"/> may be used to communicate to the loop that no 
        /// other iterations after the current iteration need be run. For 
        /// example, if <see cref="Break"/> is called from the 100th iteration 
        /// of a for loop iterating in parallel from 0 to 1000, all iterations 
        /// less than 100 should still be run, but the iterations from 101 
        /// through to 1000 are not necessary.
        /// </para>
        /// <para>
        /// For long-running iterations that may already be executing,
        /// <see cref="Break"/> causes <see cref="LowestBreakIteration"/> to be 
        /// set to the current iteration's index if the current index is less 
        /// than the current value of <see cref="LowestBreakIteration"/>.
        /// </para>
        /// <para>
        /// <see cref="Break"/> is typically employed in search-based algorithms
        ///  where an ordering is present in the data source.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Stop"/> method was previously called. <see cref="Break"/> 
        /// and <see cref="Stop"/> may not be used in combination by iterations 
        /// of the same loop.
        /// </exception>
        void Break();
    }
}