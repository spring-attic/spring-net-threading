using System;
using System.Reflection;
using System.Threading;

namespace Spring
{
    /// <summary>
    /// Static class to provide extension methods to basic system function.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public static class SystemExtensions //NET_ONLY
    {
        /// <summary>
        /// Tests whether the current thread has been interrupted.  The
        /// <i>interrupted status</i> of the thread is cleared by this method.
        /// In other words, if this method were to be called twice in 
        /// succession, the second call would return false (unless the current 
        /// thread were interrupted again, after the first call had cleared 
        /// its interrupted status and before the second call had examined it).
        /// </summary>
        /// <remarks>
        /// A thread interruption ignored because a thread was not alive at the 
        /// time of the interrupt will be reflected by this method returning 
        /// false.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if the current thread has been interrupted; <c>false</c> 
        /// otherwise.
        /// </returns>
        public static bool IsCurrentThreadInterrupted()
        {
            try
            {
                Thread.Sleep(0); // get exception if interrupted.
            }
            catch (ThreadInterruptedException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lock the stack trace information of the given <paramref name="exception"/>
        /// so that it can be rethrow without losing the stack information.
        /// </summary>
        /// <remarks>
        /// <example>
        ///     <code>
        ///     try
        ///     {
        ///         //...
        ///     }
        ///     catch( Exception e )
        ///     {
        ///         //...
        ///         throw e.PreserveStackTrace(); //rethrow the exception - preserving the full call stack trace!
        ///     }
        ///     </code>
        /// </example>
        /// </remarks>
        /// <param name="exception">The exception to lock the statck trace.</param>
        /// <returns>The same <paramref name="exception"/> with stack traced locked.</returns>
        public static T PreserveStackTrace<T>(T exception) where T : Exception
        {
            _preserveStackTrace(exception);
            return exception;
        }

        private static readonly Action<Exception> _preserveStackTrace = (Action<Exception>)Delegate.CreateDelegate(typeof(Action<Exception>),
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic));
    }
}