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

#if !NET_4_0

namespace System
{
    /// <summary>
    /// Represents one or more errors that occur during application execution.
    /// </summary>
    /// <remarks>
    /// <see cref="AggregateException"/> is used to consolidate multiple 
    /// failures into a single, throwable exception object.
    /// </remarks>
    /// <author>Kenneth Xu</author>
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

#endif