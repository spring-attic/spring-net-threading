using System;

namespace Spring.Threading
{
    /// <summary>
    /// Stores options that configure the operation of methods on the 
    /// <see cref="Parallel"/> class.
    /// </summary>
    public class ParallelOptions
    {
        private int _maxDegreeOfParallelism = -1;

        /// <summary>
        /// Gets or sets the maximum degree of parallelism enabled by this 
        /// <see cref="ParallelOptions"/> instance.
        /// </summary>
        /// <remarks>
        /// The <see cref="MaxDegreeOfParallelism"/> limits the number of 
        /// concurrent operations run by <see cref="Parallel"/> method calls 
        /// that are passed this <see cref="ParallelOptions"/> instance to 
        /// the set value, if it is positive. If <see cref="MaxDegreeOfParallelism"/>
        /// is -1, then there is no limit placed on the number of concurrently 
        /// running operations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The exception that is thrown when this <see cref="MaxDegreeOfParallelism"/> 
        /// is set to 0 or some value less than -1.
        /// </exception>
        public int 	MaxDegreeOfParallelism
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