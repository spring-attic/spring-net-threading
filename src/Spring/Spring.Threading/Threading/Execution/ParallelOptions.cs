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
    /// Stores options that configure the parallel operation of methods on the 
    /// <see cref="IExecutorService"/> class.
    /// </summary>
    public class ParallelOptions // NET_ONLY
    {
        private int _maxDegreeOfParallelism = -1;

        /// <summary>
        /// Gets or sets the maximum degree of parallelism enabled by this 
        /// <see cref="ParallelOptions"/> instance.
        /// </summary>
        /// <remarks>
        /// The <see cref="MaxDegreeOfParallelism"/> limits the number of 
        /// concurrent operations run by parallal method calls 
        /// that are passed this <see cref="ParallelOptions"/> instance to 
        /// the set value, if it is positive. If <see cref="MaxDegreeOfParallelism"/>
        /// is -1, then there is no limit placed on the number of concurrently 
        /// running operations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The exception that is thrown when this <see cref="MaxDegreeOfParallelism"/> 
        /// is set to 0 or some value less than -1.
        /// </exception>
        public int MaxDegreeOfParallelism
        {
            get { return _maxDegreeOfParallelism; }
            set
            {
                if (value == 0 || value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "Value cannot be zero or less then -1.");
                }
                _maxDegreeOfParallelism = value;
            }
        }
    }
}