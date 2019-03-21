#region License

/*
 * Copyright (C) 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

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
    [Serializable, DebuggerDisplay("Count = {InnerExceptions.Count}")]
    public class AggregateException : Exception
    {
        private const string _defaultMessage = "One or more errors occurred.";

        private readonly ReadOnlyCollection<Exception> _innerExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with a 
        /// system-supplied message that describes the error.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor initializes the <see cref="Exception.Message"/> 
        /// property of the new instance to a system-supplied message that 
        /// describes the error.
        /// </para>
        /// <para>
        /// </para>
        /// </remarks>
        public AggregateException()
            : base(_defaultMessage)
        {
            _innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with references
        /// to the inner exceptions that are the cause of this exception.
        /// </summary>
        /// <param name="innerExceptions">
        /// The exceptions that are the cause of the current exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="innerExceptions"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element of <paramref name="innerExceptions"/> is null.
        /// </exception>
        public AggregateException(IEnumerable<Exception> innerExceptions)
            : this(_defaultMessage, innerExceptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with references
        /// to the inner exceptions that are the cause of this exception.
        /// </summary>
        /// <param name="innerExceptions">
        /// The exceptions that are the cause of the current exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="innerExceptions"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element of <paramref name="innerExceptions"/> is null.
        /// </exception>
        public AggregateException(params Exception[] innerExceptions)
            : this(_defaultMessage, innerExceptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with a
        /// specified message that describes the error.
        /// </summary>
        /// <param name="message">
        /// The message that describes the exception. The caller of this constructor 
        /// is required to ensure that this string has been localized for the current
        /// system culture.
        /// </param>
        public AggregateException(string message)
            : base(message)
        {
            _innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with serialized
        /// data.
        /// </summary>
        /// <param name="info">
        /// The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The contextual information about the source or destination.
        /// </param>
        [SecurityCritical]
        protected AggregateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Exception[] exceptions = info.GetValue("InnerExceptions", typeof(Exception[])) as Exception[];
            if (exceptions == null)
            {
                throw new SerializationException("Failed to deserialize the instance of AggregateException");
            }
            _innerExceptions = new ReadOnlyCollection<Exception>(exceptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with a
        /// specified error message and a reference to the inner exceptions that are the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerExceptions">
        /// The exceptions that are the cause of the current exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="innerExceptions"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element of <paramref name="innerExceptions"/> is null.
        /// </exception>
        public AggregateException(string message, IEnumerable<Exception> innerExceptions)
            : this(message, (innerExceptions == null) ? null : new List<Exception>(innerExceptions))
        {
        }

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
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }
            _innerExceptions = new ReadOnlyCollection<Exception>(new[] { innerException });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with a
        /// specified error message and a reference to the inner exceptions that are the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerExceptions">
        /// The exceptions that are the cause of the current exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="innerExceptions"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element of <paramref name="innerExceptions"/> is null.
        /// </exception>
        public AggregateException(string message, params Exception[] innerExceptions)
            : this(message, (IList<Exception>)innerExceptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/> class with a
        /// specified error message and a reference to the inner exceptions that are the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerExceptions">
        /// The exceptions that are the cause of the current exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="innerExceptions"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element of <paramref name="innerExceptions"/> is null.
        /// </exception>
        private AggregateException(string message, IList<Exception> innerExceptions)
            : base(message, ((innerExceptions != null) && (innerExceptions.Count > 0)) ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }
            var exceptions = new Exception[innerExceptions.Count];
            var i = 0;
            foreach (var exception in innerExceptions)
            {
                if (exception == null)
                {
                    throw new ArgumentException("Elements of inner exception list must not be null but encountered null at index " + i, "innerExceptions");
                }
                exceptions[i++] = exception;
            }
            _innerExceptions = new ReadOnlyCollection<Exception>(exceptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateException"/>
        /// class with references to the inner exceptions that are the cause of
        /// this exception.
        /// </summary>
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get { return _innerExceptions; }
        }

        /// <summary>
        /// Flattens an <see cref="AggregateException"/> instances into a single, new instance.
        /// </summary>
        /// <returns>
        /// A new, flattened <see cref="AggregateException"/>.
        /// </returns>
        public AggregateException Flatten()
        {
            var innerExceptions = new List<Exception>();
            var aggregateExceptions = new List<AggregateException> { this };
            for (var i = 0; i < aggregateExceptions.Count; i++)
            {
                foreach (var item in aggregateExceptions[i].InnerExceptions)
                {
                    var aggregateException = item as AggregateException;
                    if (aggregateException != null)
                    {
                        aggregateExceptions.Add(aggregateException);
                    }
                    else
                    {
                        innerExceptions.Add(item);
                    }
                }
            }
            return new AggregateException(Message, innerExceptions);
        }

        /// <summary>
        /// Returns the <see cref="AggregateException"/> that is the root cause of this exception.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="AggregateException"/> that is the root cause of this exception.
        /// </returns>
        public override Exception GetBaseException()
        {
            Exception innerException = this;
            for (var e = this; (e != null) && (e.InnerExceptions.Count == 1); e = innerException as AggregateException)
            {
                innerException = innerException.InnerException;
            }
            return innerException;
        }

        /// <summary>
        /// Initializes a new instance of the System.AggregateException class with serialized 
        /// data.
        /// </summary>
        /// <param name="info">
        /// The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The contextual information about the source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> argumentn is null.
        /// </exception>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            Exception[] array = new Exception[_innerExceptions.Count];
            _innerExceptions.CopyTo(array, 0);
            info.AddValue("InnerExceptions", array, typeof(Exception[]));
        }

        /// <summary>
        /// Invokes a handler on each System.Exception contained by this <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="predicate">
        /// The predicate to execute for each exception. The predicate accepts as an
        /// argument the System.Exception to be processed and returns a Boolean to indicate
        /// whether the exception was handled.
        /// </param>
        /// <see cref="ArgumentNullException">
        /// The <paramref name="predicate"/> argument is null.
        /// </see>
        public void Handle(Func<Exception, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            var innerExceptions = (from x in _innerExceptions where !predicate(x) select x).ToList();
            if (innerExceptions.Count > 0)
            {
                throw new AggregateException(Message, innerExceptions);
            }
        }

        /// <summary>
        /// Creates and returns a string representation of the current 
        /// <see cref="AggregateException"/>.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            for (int i = 0; i < _innerExceptions.Count; i++)
            {
                sb.Append(Environment.NewLine)
                    .Append("}---> (Inner Exception #").Append(i).Append(") ")
                    .Append(_innerExceptions[i]).Append("<---");
            }
            return sb.ToString();
        }
    }
}

#endif