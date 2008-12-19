#region License
/*
* Copyright © 2002-2005 the original author or authors.
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
Originally written by Doug Lea and released into the public domain.
This may be used for any purposes whatsoever without acknowledgment.
Thanks for the assistance and support of Sun Microsystems Labs,
and everyone contributing, testing, and using this code.
*/
using System.Threading;

namespace Spring.Threading
{
	
	/// <summary> 
	/// An object that creates new threads on demand.  
	/// </summary>
	/// <remarks> 
	/// Using thread factories removes hardwiring of calls to new Thread,
	/// enabling applications to use special thread subclasses, priorities, etc.
	/// <p/>
	/// The simplest implementation of this interface is just:
	/// <code>
	/// class SimpleThreadFactory : IThreadFactory {
	///		public Thread NewThread(IRunnable r) {
	///			return new Thread(new ThreadStart(r.Run));
	///		}
	/// }
	/// </code>
	/// 
	/// The <see cref="Spring.Threading.Execution.Executors.GetDefaultThreadFactory()"/> method provides a more
	/// useful simple implementation, that sets the created thread context
	/// to known values before returning it.
	/// </remarks>
	/// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    /// <author>Griffin Caprio (.Net)</author>
    public interface IThreadFactory
    {
		/// <summary> 
		/// Constructs a new <see cref="System.Threading.Thread"/>.  
		/// </summary>
		/// <remarks> 
		/// Implementations may also initialize
		/// priority, name, daemon status, thread state, etc.
		/// </remarks>
		/// <param name="runnable">
		/// a runnable to be executed by new thread instance
		/// </param>
		/// <returns>constructed thread</returns>
        Thread NewThread(IRunnable runnable);
    }
}