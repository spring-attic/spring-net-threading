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

namespace Spring.Threading.Future
{
	/// <summary>
	/// A <see cref="Spring.Threading.Future.IFuture"/> represents the result of an asynchronous
	/// computation.  
	/// </summary>
	/// <remarks> 
	/// Methods are provided to check if the computation is
	/// complete, to wait for its completion, and to retrieve the result of
	/// the computation.  The result can only be retrieved using method
	/// <see cref="Spring.Threading.Future.IFuture.GetResult()"/> when the computation has completed, blocking if
	/// necessary until it is ready.  Cancellation is performed by the
	/// <see cref="Spring.Threading.Future.IFuture.Cancel()"/> method.  Additional methods are provided to
	/// determine if the task completed normally or was cancelled. Once a
	/// computation has completed, the computation cannot be cancelled.
	/// 
	/// <p/>
	/// <b>Sample Usage</b> (Note that the following classes are all
	/// made-up.) 
	/// <p/>
	/// TODO: Provide code sample
	/// <p/>
	/// The <see cref="Spring.Threading.Future.FutureTask"/> class is an implementation of <see cref="Spring.Threading.Future.IFuture"/> that
	/// implements <see cref="Spring.Threading.IRunnable"/>, and so may be executed by an <see cref="Spring.Threading.IExecutor"/>.
	/// </remarks>
	/// <seealso cref="Spring.Threading.Future.FutureTask"/>
	/// <seealso cref="Spring.Threading.IExecutor"/>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <changes>
	/// <ol>
	/// <li>Added Cancel() method, with no bool parameter, which delegates to the Cancel(bool) method with a value of false.</li>
	/// </ol>
	/// </changes>
	public interface IFuture
	{
		/// <summary> 
		/// Attempts to cancel execution of this task.  
		/// </summary>
		/// <remarks> 
		/// This attempt will fail if the task has already completed, already been cancelled,
		/// or could not be cancelled for some other reason. If successful,
		/// and this task has not started when <see cref="Spring.Threading.Future.IFuture.Cancel()"/> is called,
		/// this task should never run.  If the task has already started, the in-progress tasks are allowed
		/// to complete
		/// </remarks>
		/// <returns> <see lang="false"/> if the task could not be cancelled,
		/// typically because it has already completed normally;
		/// <see lang="true"/> otherwise
		/// </returns>
		bool Cancel();
		/// <summary> 
		/// Attempts to cancel execution of this task.  
		/// </summary>
		/// <remarks> 
		/// This attempt will fail if the task has already completed, already been cancelled,
		/// or could not be cancelled for some other reason. If successful,
		/// and this task has not started when <see cref="Spring.Threading.Future.IFuture.Cancel()"/> is called,
		/// this task should never run.  If the task has already started,
		/// then the <paramref name="mayInterruptIfRunning"/> parameter determines
		/// whether the thread executing this task should be interrupted in
		/// an attempt to stop the task.
		/// </remarks>
		/// <param name="mayInterruptIfRunning"><see lang="true"/> if the thread executing this
		/// task should be interrupted; otherwise, in-progress tasks are allowed
		/// to complete
		/// </param>
		/// <returns> <see lang="false"/> if the task could not be cancelled,
		/// typically because it has already completed normally;
		/// <see lang="true"/> otherwise
		/// </returns>
		bool Cancel(bool mayInterruptIfRunning);

		/// <summary>
		/// Determines if this task was cancelled.
		/// </summary>
		/// <remarks> 
		/// Returns <see lang="true"/> if this task was cancelled before it completed
		/// normally.
		/// </remarks>
		/// <returns> <see lang="true"/>if task was cancelled before it completed
		/// </returns>
		bool IsCancelled {get;}

		/// <summary> 
		/// Returns <see lang="true"/> if this task completed.
		/// </summary>
		/// <remarks> 
		/// Completion may be due to normal termination, an exception, or
		/// cancellation -- in all of these cases, this method will return
		/// <see lang="true"/> if this task completed.
		/// </remarks>
		/// <returns> <see lang="true"/>if this task completed.</returns>
		bool IsDone {get;}

		/// <summary>
		/// Waits for computation to complete, then returns its result. 
		/// </summary>
		/// <remarks> 
		/// Waits if necessary for the computation to complete, and then
		/// retrieves its result.
		/// </remarks>
		/// <returns>the computed result</returns>
		/// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
		/// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
		/// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
		object GetResult();

		/// <summary>
		/// Waits for the given time span, then returns its result.
		/// </summary>
		/// <remarks> 
		/// Waits, if necessary, for at most the <paramref name="durationToWait"/> for the computation
		/// to complete, and then retrieves its result, if available.
		/// </remarks>
		/// <param name="durationToWait">the <see cref="System.TimeSpan"/> to wait.</param>
		/// <returns>the computed result</returns>
		/// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
		/// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
		/// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
		/// <exception cref="Spring.Threading.TimeoutException">if the computation threw an exception.</exception>
		object GetResult(TimeSpan durationToWait);
	}
//#if NET_2_0
//    /// <summary>
//    /// A <see cref="Spring.Threading.Future.IFuture{T}"/> represents the result of an asynchronous
//    /// computation.  
//    /// </summary>
//    /// <remarks> 
//    /// Methods are provided to check if the computation is
//    /// complete, to wait for its completion, and to retrieve the result of
//    /// the computation.  The result can only be retrieved using method
//    /// <see cref="Spring.Threading.Future.IFuture{T}.GetResult()"/> when the computation has completed, blocking if
//    /// necessary until it is ready.  Cancellation is performed by the
//    /// <see cref="Spring.Threading.Future.IFuture.Cancel()"/> method.  Additional methods are provided to
//    /// determine if the task completed normally or was cancelled. Once a
//    /// computation has completed, the computation cannot be cancelled.
//    /// 
//    /// <p/>
//    /// <b>Sample Usage</b> (Note that the following classes are all
//    /// made-up.) 
//    /// <p/>
//    /// TODO: Provide code sample
//    /// <p/>
//    /// The <see cref="Spring.Threading.Future.FutureTask{T}"/> class is an implementation of <see cref="Spring.Threading.Future.IFuture{T}"/> that
//    /// implements <see cref="Spring.Threading.IRunnable"/>, and so may be executed by an <see cref="Spring.Threading.IExecutor"/>.
//    /// </remarks>
//    /// <seealso cref="Spring.Threading.Future.FutureTask{T}"/>
//    /// <seealso cref="Spring.Threading.IExecutor"/>
//    /// <author>Doug Lea</author>
//    /// <author>Griffin Caprio (.NET)</author>
//    /// <author>Kenneth Xu</author>
//    public interface IFuture<T> : IFuture
//    {
//        /// <summary>
//        /// Waits for computation to complete, then returns its result. 
//        /// </summary>
//        /// <remarks> 
//        /// Waits if necessary for the computation to complete, and then
//        /// retrieves its result.
//        /// </remarks>
//        /// <returns>the computed result</returns>
//        /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
//        /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
//        /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
//        new T GetResult();
//
//        /// <summary>
//        /// Waits for the given time span, then returns its result.
//        /// </summary>
//        /// <remarks> 
//        /// Waits, if necessary, for at most the <paramref name="durationToWait"/> for the computation
//        /// to complete, and then retrieves its result, if available.
//        /// </remarks>
//        /// <param name="durationToWait">the <see cref="System.TimeSpan"/> to wait.</param>
//        /// <returns>the computed result</returns>
//        /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
//        /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
//        /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
//        /// <exception cref="Spring.Threading.TimeoutException">if the computation threw an exception.</exception>
//        new T GetResult(TimeSpan durationToWait);
//    }
//#endif
}