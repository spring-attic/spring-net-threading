#region License

/*
 * Copyright (C) 2002-2005 the original author or authors.
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
using System.Threading;
using Spring.Threading.Locks;

namespace Spring.Threading
{
    /// <summary> 
    /// A synchronization aid that allows a set of threads to all wait for
    /// each other to reach a common barrier point.  
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="CyclicBarrier"/>s are useful in programs involving a fixed 
    /// sized party of threads that must occasionally wait for each other. 
    /// The barrier is called <i>cyclic</i> because it can be re-used after 
    /// the waiting threads are released.
    /// </para>
    /// <para>
    /// A <see cref="CyclicBarrier"/> supports an optional <see cref="Action"/>
    /// or <see cref="IRunnable"/> command that is run once per barrier point, 
    /// after the last thread in the party arrives, but before any threads are 
    /// released. This <i>barrier action</i> is useful for updating 
    /// shared-state before any of the parties continue.
    /// </para>
    /// <para>
    /// The <see cref="CyclicBarrier"/> uses an all-or-none breakage model
    /// for failed synchronization attempts: If a thread leaves a barrier
    /// point prematurely because of interruption, failure, or timeout, all
    /// other threads waiting at that barrier point will also leave
    /// abnormally via <see cref="BrokenBarrierException"/> (or
    /// <see cref="ThreadInterruptedException"/> if they too were interrupted 
    /// at about the same time).
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// <b>Sample usage:</b> Here is an example of using a barrier in a 
    /// parallel decomposition design:
    /// </para>
    /// <code language="c#">
    /// public class Solver {
    ///		int N;
    /// 	float[][] data;
    /// 	CyclicBarrier barrier;
    /// 
    /// 	internal class Worker : IRunnable {
    ///			int myRow;
    /// 		Worker(int row) { myRow = row; }
    /// 		public void Run() {
    /// 				while (!IsDone) {
    /// 					processRow(myRow);
    /// 
    ///						try {
    /// 						barrier.Await();
    /// 					} catch (ThreadInterruptedException ex) {
    ///							return;
    ///						} catch (BrokenBarrierException ex) {
    ///							return;
    ///						}
    /// 				}
    ///			}
    ///			private void processRow(int myRow ) {
    ///				// Process row....
    ///			}
    ///			private bool IsDone { get { ..... } }
    ///		}
    ///		private void MergeRows() {
    ///			// Merge Rows.....
    ///		}
    ///		public Solver(float[][] matrix) {
    ///			data = matrix;
    /// 		N = matrix.length;
    /// 		barrier = new CyclicBarrier(N, MergeRows);
    ///			for (int i = 0; i &lt; N; ++i)
    ///				new Thread(new ThreadStart(new Worker(i).Run)).Start();
    /// 
    ///			WaitUntilDone();
    ///		}
    /// }
    /// </code>
    /// <para>
    /// Here, each worker thread processes a row of the matrix then waits at 
    /// the barrier until all rows have been processed. When all rows are 
    /// processed the supplied barrier action is executed and merges the rows. 
    /// If the merger determines that a solution has been found then IsDone 
    /// will return <see lang="true"/> and each worker will terminate. 
    /// </para>
    /// <para>
    /// If the barrier action does not rely on the parties being suspended when
    /// it is executed, then any of the threads in the party could execute that
    /// action when it is released. To facilitate this, each invocation of
    /// <see cref="Await()"/> returns the arrival index of that thread at the 
    /// barrier. You can then choose which thread should execute the barrier 
    /// action, for example:
    /// </para>
    /// <code language="c#">
    /// if (barrier.Await() == 0) {
    ///		// log the completion of this iteration
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CountDownLatch"/>
    /// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    /// <author>Griffin Caprio (.Net)</author>
    /// <author>Kenneth Xu</author>
    public class CyclicBarrier //JDK_1_6
#if !PHASED
        : IBarrier
#endif
    {
        /// <summary>The lock for guarding barrier entry </summary>
        private readonly ReentrantLock _lock = new ReentrantLock();

        /// <summary> Condition to wait on until tripped </summary>
        private readonly ICondition _trip;

        /// <summary>The number of parties </summary>
        private readonly int _parties;

        /// <summary>  The <see cref="Action"/> to run when tripped </summary>
        private readonly Action _barrierCommand;

        /// <summary>The current generation </summary>
        private Generation _generation;

        /// <summary> 
        /// Number of parties yet to arrive. Counts down from 
        /// <see cref="Parties"/> to 0 on each generation.  It is reset to 
        /// <see cref="Parties"/> on each new generation or when broken.
        /// </summary>
        private int _count;

        /// <summary> 
        /// Return the number of parties that must meet per barrier
        /// point. The number of parties is always at least 1.
        /// </summary>
        public virtual int Parties
        {
            get { return _parties; }

        }

        /// <summary> 
        /// Queries if this barrier is in a broken state. Return <c>true</c> 
        /// if one or more parties broke out of this barrier due to timeout
        /// or interruption since construction or the last reset, or a barrier 
        /// action failed due to an exception; <c>false</c> otherwise.
        /// </summary>
        public virtual bool IsBroken
        {
            get
            {
                using (_lock.Lock())
                {
                    return _generation.isBroken;
                }
            }

        }

        /// <summary> 
        /// Returns the number of parties currently waiting at the barrier.
        /// This method is primarily useful for debugging and assertions.
        /// </summary>
        public virtual int NumberOfWaitingParties
        {
            get
            {
                using (_lock.Lock())
                {
                    return _parties - _count;
                }
            }

        }

        /// <summary> 
        /// Creates a new <see cref="CyclicBarrier"/> that will trip when the
        /// given number of <paramref name="parties"/> (threads) are waiting 
        /// upon it, and which will execute the given barrier action when the 
        /// barrier is tripped, performed by the last thread entering the 
        /// barrier.
        /// </summary>
        /// <param name="parties">
        /// The number of threads that must invoke <see cref="Await()"/> or
        /// <see cref="Await(System.TimeSpan)"/> before the barrier is tripped.
        /// </param>
        /// <param name="barrierAction">
        /// The <see cref="IRunnable"/> to execute when the barrier is
        /// tripped, or <see lang="null"/>  if there is no action.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="parties"/> is less than 1.
        /// </exception>
        public CyclicBarrier(int parties, IRunnable barrierAction) : 
            this(parties, barrierAction==null? (Action) null : barrierAction.Run)
        {
        }

        /// <summary> 
        /// Creates a new <see cref="CyclicBarrier"/> that will trip when the
        /// given number of <paramref name="parties"/> (threads) are waiting 
        /// upon it, and which will execute the given barrier action when the 
        /// barrier is tripped, performed by the last thread entering the 
        /// barrier.
        /// </summary>
        /// <param name="parties">
        /// The number of threads that must invoke <see cref="Await()"/> or
        /// <see cref="Await(System.TimeSpan)"/> before the barrier is tripped.
        /// </param>
        /// <param name="barrierAction">
        /// The <see cref="Action"/> delegate to execute when the barrier is
        /// tripped, or <see lang="null"/>  if there is no action.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="parties"/> is less than 1.
        /// </exception>
        public CyclicBarrier(int parties, Action barrierAction)
        {
            if (parties <= 0)
            {
                throw new ArgumentOutOfRangeException("parties", parties, "parameter parties must be positive");
            }
            _parties = parties;
            _count = parties;
            _barrierCommand = barrierAction;
            _trip = _lock.NewCondition();
            _generation = new Generation();
        }

        /// <summary> 
        /// Creates a new <see cref="CyclicBarrier"/> that will trip when the
        /// given number of <paramref name="parties"/> (threads) are waiting 
        /// upon it, and does not perform a predefined action when the barrier 
        /// is tripped.
        /// </summary>
        /// <param name="parties">
        /// The number of threads that must invoke <see cref="Await()"/> or
        /// <see cref="Await(System.TimeSpan)"/> before the barrier is tripped.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="parties"/> is less than 1.
        /// </exception>
        public CyclicBarrier(int parties) : this(parties, (Action)null)
        {
        }

        /// <summary> 
        /// Waits until all <see cref="Parties"/> have invoked 
        /// <see cref="Await()"/> or <see cref="Await(System.TimeSpan)"/>
        /// on this barrier.
        /// </summary>
        /// <remarks> 
        /// <para>
        /// If the current thread is not the last to arrive then it is
        /// disabled for thread scheduling purposes and lies dormant until
        /// one of following things happens:
        /// </para>
        /// <list type="bullet">
        /// <item>The last thread arrives</item>
        /// <item>Some other thread <see cref="Thread.Interrupt()">interrupts</see> 
        /// the current thread</item>
        /// <item>Some other thread <see cref="Thread.Interrupt()">interrupts</see>
        /// one of the other waiting threads</item>
        /// <item>Some other thread times out while waiting for barrier</item>
        /// <item>Some other thread invokes <see cref="Reset()"/> on this barrier.</item>
        /// </list>
        /// <para>
        /// If current thread is <see cref="Thread.Interrupt()">interrupted</see>
        /// while waiting, a <see cref="ThreadInterruptedException"/> is thrown.
        /// </para>
        /// <para>
        /// If the barrier <see cref="Reset"/> while any thread is waiting, 
        /// or if the barrier <see cref="IsBroken"/> when <see cref="Await()"/> 
        /// or <see cref="Await(System.TimeSpan)"/> is invoked, or while any 
        /// thread is waiting, then a <see cref="BrokenBarrierException"/> is 
        /// thrown.
        /// </para>
        /// <para>
        /// If any thread is interrupted while waiting, then all other waiting 
        /// threads will throw <see cref="BrokenBarrierException"/> and the 
        /// barrier is placed in the broken state.
        /// </para>
        /// <para>
        /// If the current thread is the last thread to arrive, and a non-null 
        /// barrier action was supplied in the constructor, then the current 
        /// thread runs the action before allowing the other threads to
        /// continue. If an exception occurs during the barrier action then that 
        /// exception will be propagated in the current thread and the barrier 
        /// is placed in the broken state.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The arrival index of the current thread, where an index of 
        /// <see cref="Parties"/> - 1 indicates the first to arrive and zero 
        /// indicates the last to arrive.
        /// </returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the current thread was interrupted.
        /// </exception>
        /// <exception cref="BrokenBarrierException">
        /// If <b>another</b> thread was interrupted or timed out while the 
        /// current thread was waiting, or the barrier was reset, or the 
        /// barrier was broken when <see cref="Await()"/> was called, or the 
        /// barrier action (if present) failed due to an exception.
        /// </exception>
        public virtual int Await()
        {
            return DoWait(false, TimeSpan.Zero);
        }

        /// <summary> 
        /// Waits until all <see cref="Parties"/> have invoked 
        /// <see cref="Await()"/> or <see cref="Await(System.TimeSpan)"/>
        /// on this barrier.
        /// </summary>
        /// <remarks> 
        /// <para>
        /// If the current thread is not the last to arrive then it is
        /// disabled for thread scheduling purposes and lies dormant until
        /// one of following things happens:
        /// </para>
        /// <list type="bullet">
        /// <item>The last thread arrives</item>
        /// <item>Some other thread <see cref="Thread.Interrupt()">interrupts</see> 
        /// the current thread</item>
        /// <item>Some other thread <see cref="Thread.Interrupt()">interrupts</see>
        /// one of the other waiting threads</item>
        /// <item>Some other thread times out while waiting for barrier</item>
        /// <item>Some other thread invokes <see cref="Reset()"/> on this barrier.</item>
        /// </list>
        /// <para>
        /// If current thread is <see cref="Thread.Interrupt()">interrupted</see>
        /// while waiting, a <see cref="ThreadInterruptedException"/> is thrown.
        /// </para>
        /// <para>
        /// If the specified <paramref name="durationToWait"/> elapses then a 
        /// <see cref="TimeoutException"/> is thrown. If the time is less than 
        /// or equal to zero, the method will not wait at all.
        /// </para>
        /// <para>
        /// If the barrier <see cref="Reset"/> while any thread is waiting, 
        /// or if the barrier <see cref="IsBroken"/> when <see cref="Await()"/> 
        /// or <see cref="Await(System.TimeSpan)"/> is invoked, or while any 
        /// thread is waiting, then a <see cref="BrokenBarrierException"/> is 
        /// thrown.
        /// </para>
        /// <para>
        /// If any thread is interrupted while waiting, then all other waiting 
        /// threads will throw <see cref="BrokenBarrierException"/> and the 
        /// barrier is placed in the broken state.
        /// </para>
        /// <para>
        /// If the current thread is the last thread to arrive, and a non-null 
        /// barrier action was supplied in the constructor, then the current 
        /// thread runs the action before allowing the other threads to
        /// continue. If an exception occurs during the barrier action then that 
        /// exception will be propagated in the current thread and the barrier 
        /// is placed in the broken state.
        /// </para>
        /// </remarks>
        /// <param name="durationToWait">The time to wait for barrier.</param>
        /// <returns>
        /// The arrival index of the current thread, where an index of 
        /// <see cref="Parties"/> - 1 indicates the first to arrive and zero 
        /// indicates the last to arrive.
        /// </returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the current thread was interrupted.
        /// </exception>
        /// <exception cref="BrokenBarrierException">
        /// If <b>another</b> thread was interrupted or timed out while the 
        /// current thread was waiting, or the barrier was reset, or the 
        /// barrier was broken when <see cref="Await(System.TimeSpan)"/> was 
        /// called, or the barrier action (if present) failed due to an 
        /// exception.
        /// </exception>
        public virtual int Await(TimeSpan durationToWait)
        {
            return DoWait(true, durationToWait);
        }

        /// <summary> 
        /// Resets the barrier to its initial state.  
        /// </summary>
        /// <remarks>
        /// If any parties are currently waiting at the barrier, they will
        /// return with a <see cref="BrokenBarrierException"/>. Note that 
        /// resets <b>after</b> a breakage has occurred for other reasons 
        /// can be complicated to carry out; threads need to re-synchronize 
        /// in some other way, and choose one to perform the reset.  It may 
        /// be preferable to instead create a new barrier for subsequent use.
        /// </remarks>
        public virtual void Reset()
        {
            using (_lock.Lock())
            {
                BreakBarrier();
                NextGeneration();
            }
        }

        /// <summary> 
        /// Updates state on barrier trip and wakes up everyone.
        /// Called only while holding lock.
        /// </summary>
        private void NextGeneration()
        {
            // signal completion of last generation
            _trip.SignalAll();
            // set up next generation
            _count = _parties;
            _generation = new Generation();
        }

        /// <summary> 
        /// Sets current barrier generation as broken and wakes up everyone.
        /// Called only while holding lock.
        /// </summary>
        private void BreakBarrier()
        {
            _generation.isBroken = true;
            _count = _parties;
            _trip.SignalAll();
        }

        /// <summary> Main barrier code, covering the various policies.</summary>
        private int DoWait(bool timed, TimeSpan duration)
        {
            using (_lock.Lock())
            {
                Generation currentGeneration = _generation;

                if (currentGeneration.isBroken)
                    throw new BrokenBarrierException();

                int index = --_count;
                if (index == 0)
                {
                    bool ranAction = false;
                    try
                    {
                        Action command = _barrierCommand;
                        if (command != null) command();
                        ranAction = true;
                        NextGeneration();
                        return 0;
                    }
                    finally
                    {
                        if (!ranAction)
                            BreakBarrier();
                    }
                }

                // loop until tripped, broken, interrupted, or timed out
                DateTime deadline = DateTime.UtcNow.Add(duration);
                for (;;)
                {
                    try
                    {
                        if (!timed)
                        {
                            _trip.Await();
                        }
                        else if (duration.Ticks > 0)
                        {
                            _trip.Await(duration);
                            duration = deadline.Subtract(DateTime.UtcNow);
                        }
                    }
                    catch (ThreadInterruptedException e)
                    {
                        if (currentGeneration == _generation && ! currentGeneration.isBroken)
                        {
                            BreakBarrier();
                            throw SystemExtensions.PreserveStackTrace(e);
                        }
                        else
                        {
                            // We're about to finish waiting even if we had not
                            // been interrupted, so this interrupt is deemed to
                            // "belong" to subsequent execution.
                            Thread.CurrentThread.Interrupt();
                        }
                    }

                    if (currentGeneration.isBroken)
                        throw new BrokenBarrierException();

                    if (currentGeneration != _generation)
                        return index;

                    if (timed && duration.Ticks <= 0)
                    {
                        BreakBarrier();
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary> 
        /// Each use of the barrier is represented as a generation instance.
        /// </summary>
        /// <remarks>
        /// The generation changes whenever the barrier is tripped, or
        /// is reset. There can be many generations associated with threads
        /// using the barrier - due to the non-deterministic way the lock
        /// may be allocated to waiting threads - but only one of these
        /// can be active at a time and all the rest are either broken or tripped.
        /// There need not be an active generation if there has been a break
        /// but no subsequent reset.
        /// </remarks>
        private class Generation
        {
            internal bool isBroken;
        }
    }
}