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

using Spring.Threading.Execution;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides completion status on the execution of a <see cref="Parallel"/> loop.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public struct ParallelLoopResult :ILoopResult
    {
        private readonly ILoopResult _result;
        internal ParallelLoopResult(ILoopResult result)
        {
            _result = result;
        }

        /// <summary>
        /// Gets whether the loop ran to completion, such that all iterations 
        /// of the loop were executed.
        /// </summary>
        /// <value>
        /// True if the loop ran to completion, otherwise false.
        /// </value>
        public bool IsCompleted
        {
            get { return _result.IsCompleted; }
        }

        /// <summary>
        /// Gets the index of the lowest iteration from which 
        /// <see cref="ParallelLoopState.Break"/> was called.
        /// </summary>
        /// <remarks>
        /// If Break  was not employed, this property will return null.
        /// </remarks>
        /// <value>
        /// An <see cref="int"/> that represents the lowest iteration from which
        /// <see cref="ParallelLoopState.Break"/> was called.
        /// </value>
        public long? LowestBreakIteration
        {
            get { return _result.LowestBreakIteration; }
        }
    }
}