using System;

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

/*
File: Callable.java

Originally written by Doug Lea and released into the public domain.
This may be used for any purposes whatsoever without acknowledgment.
Thanks for the assistance and support of Sun Microsystems Labs,
and everyone contributing, testing, and using this code.

History:
Date       Who                What
30Jun1998  dl               Create public version
5Jan1999  dl               Change Exception to Throwable in call signature
27Jan1999  dl               Undo last change*/

namespace Spring.Threading
{
	/// <summary> 
	/// <p>Interface for runnable actions that bear results and/or throw Exceptions.
	/// This interface is designed to provide a common protocol for
	/// result-bearing actions that can be run independently in threads, 
	/// in which case
	/// they are ordinarily used as the bases of Runnables that set
	/// FutureResults
	/// </p>
	/// </summary>
	/// <seealso cref="FutureResult">
	/// </seealso>
	public interface ICallable
	{
		/// <summary>Perform some action that returns a result or throws an exception *</summary>
		object Call();
	}

#if NET_2_0
    /// <summary> 
    /// <p>Interface for runnable actions that bear results and/or throw Exceptions.
    /// This interface is designed to provide a common protocol for
    /// result-bearing actions that can be run independently in threads, 
    /// in which case
    /// they are ordinarily used as the bases of Runnables that set
    /// FutureResults
    /// </p>
    /// </summary>
    /// <typeparam name="T">Data type of the result to be returned.</typeparam>
    /// <seealso cref="FutureResult{T}"/>
    /// <seealso cref="ICallable"/>
    /// <seealso cref="Spring.Threading.Call{T}"/>
    /// <author>Kenneth Xu</author>
    public interface ICallable<T> : ICallable
    {
        /// <summary>
        /// Perform some action that returns a result or throws an exception.
        /// </summary>
        ///<returns>The result of the action.</returns>
        new T Call();
    }
#endif
}