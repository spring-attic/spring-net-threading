using System;

namespace Spring.Threading
{
    /// <summary>
    /// Represents one or more errors that occur during application execution.
    /// </summary>
    /// <remarks>
    /// <see cref="AggregateException"/> is used to consolidate multiple 
    /// failures into a single, throwable exception object.
    /// </remarks>
    [Serializable]
    public class AggregateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/>
        /// class with a specified error message and a reference to the inner 
        /// exception that is the cause of this exception.
        /// </summary>
        /// <remarks>
        /// An exception that is thrown as a direct result of a previous 
        /// exception should include a reference to the previous exception 
        /// in the <see cref="Exception.InnerException"/> property. The
        /// <see cref="Exception.InnerException"/> property returns the same 
        /// value that is passed into the constructor, or null reference if 
        /// the <see cref="Exception.InnerException"/> property does not 
        /// supply the inner exception value to the constructor.
        /// </remarks>
        /// <param name="message">
        /// The message that describes the exception. The caller of this 
        /// constructor is required to ensure that this string has been 
        /// localized for the current system culture. 
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the 
        /// <paramref name="innerException"/> parameter is not null reference, 
        /// the current exception is raised in a catch block that handles the 
        /// inner exception. 
        /// </param>
        public AggregateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}