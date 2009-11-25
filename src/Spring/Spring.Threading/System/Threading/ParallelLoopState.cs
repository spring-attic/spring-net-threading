using Spring.Threading.Execution;

namespace System.Threading
{
    /// <summary>
    /// Enables iterations of <see cref="Parallel"/> loops to interact with 
    /// other iterations. A different <see cref="ParallelLoopState"/> instance 
    /// is provided to each thread involved in a loop.
    /// </summary>
    public class ParallelLoopState
    {
        private readonly ILoopState _state;

        internal ParallelLoopState(ILoopState state)
        {
            _state = state;
        }
        /// <summary>
        /// Gets whether the current iteration of the loop should exit based 
        /// on requests made by this or other iterations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an iteration of a loop calls <see cref="Break"/> or 
        /// <see cref="Stop"/>, or when one throws an exception, the 
        /// <see cref="Parallel"/> class will proactively attempt to prohibit 
        /// additional iterations of the loop from starting execution.
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
        public bool ShouldExitCurrentIteration
        {
            get { return _state.ShouldExitCurrentIteration; }
        }

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
        public long? LowestBreakIteration
        {
            get { return _state.LowestBreakIteration; }
        }

        /// <summary>
        /// Gets whether any iteration of the loop has called <see cref="Stop"/>.
        /// </summary>
        /// <value>
        /// True if any iteration has called <see cref="Stop"/>, otherwise false.
        /// </value>
        public bool IsStopped
        {
            get{ return _state.IsStopped; }
        }

        /// <summary>
        /// Gets whether any iteration of the loop has thrown an exception that 
        /// went unhandled by that iteration.
        /// </summary>
        /// <value>
        /// True if any iteration has thrown an exception that went unhandled, 
        /// otherwise false.
        /// </value>
        public bool IsExceptional
        {
            get { return _state.IsExceptional; }
        }

        /// <summary>
        /// Communicates that the <see cref="Parallel"/> loop should cease 
        /// execution at the system's earliest convenience.
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
        public void Stop()
        {
            _state.Stop();
        }

        /// <summary>
        /// Communicates that the <see cref="Parallel"/> loop should cease 
        /// execution at the system's earliest convenience of iterations beyond 
        /// the current iteration. 
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
        public void Break()
        {
            _state.Break();
        }
    }
}