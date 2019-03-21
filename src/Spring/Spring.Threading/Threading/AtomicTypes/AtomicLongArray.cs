#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Spring.Collections.Generic;

namespace Spring.Threading.AtomicTypes
{
    /// <summary> 
    /// A long array in which elements may be updated atomically.
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [Serializable]
    public class AtomicLongArray : AbstractAtomicArray<long>, IAtomicArray<long> //JDK_1_6
    {
        private readonly long[] _longArray;

        /// <summary> 
        /// Creates a new <see cref="AtomicLongArray"/> of given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the array
        /// </param>
        public AtomicLongArray(int length) {
            _longArray = new long[length];
        }

        /// <summary> 
        /// Creates a new <see cref="AtomicLongArray"/> with the same length as, and
        /// all elements copied from, <paramref name="array"/>.
        /// </summary>
        /// <param name="array">
        /// The array to copy elements from
        /// </param>
        /// <exception cref="ArgumentNullException"> if the array is null</exception>
        public AtomicLongArray(long[] array) {
            if(array == null) throw new ArgumentNullException("array");
            _longArray = (long[]) array.Clone();
            if (array.Length > 0) Thread.MemoryBarrier();
        }

        /// <summary> 
        /// Returns the length of the array.
        /// </summary>
        /// <returns> 
        /// The length of the array
        /// </returns>
        public override int Count {
            get { return _longArray.Length; }
        }

        /// <summary> 
        /// Gets / Sets the current value at position index.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <returns> 
        /// The current value
        /// </returns>
        public override long this[int index] {
            get { return Interlocked.Read(ref _longArray[index]); }
            set { Interlocked.Exchange(ref _longArray[index], value); }
        }

        /// <summary> 
        /// Eventually sets the element at position <paramref name="index"/> to the given <paramref name="newValue"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        public void LazySet(int index, long newValue) {
            Interlocked.Exchange(ref _longArray[index], newValue);
        }

        /// <summary> 
        /// Atomically sets the element at <paramref name="index"/> to the <paramref name="newValue"/>
        /// and returns the old value.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <returns> 
        /// The previous value
        /// </returns>
        public long Exchange(int index, long newValue) {
            return Interlocked.Exchange(ref _longArray[index], newValue);
        }

        /// <summary>
        /// Atomically sets the value to <paramref name="newValue"/>
        /// if the current value == <paramref name="expectedValue"/>
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <returns> true if successful. False return indicates that
        /// the actual value was not equal to the expected value.
        /// </returns>
        public bool CompareAndSet(int index, long expectedValue, long newValue) {
            return expectedValue == Interlocked.CompareExchange(
                ref _longArray[index], newValue, expectedValue);
        }

        /// <summary> 
        /// Atomically sets the value to <paramref name="newValue"/>
        /// if the current value == <paramref name="expectedValue"/>
        /// May fail spuriously.
        /// </summary>
        /// <param name="index">the index
        /// </param>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <returns> 
        /// True if successful.
        /// </returns>
        public virtual bool WeakCompareAndSet(int index, long expectedValue, long newValue) {
            return expectedValue == Interlocked.CompareExchange(
                ref _longArray[index], newValue, expectedValue);
        }

        /// <summary> 
        /// Atomically increments by one the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <returns> 
        /// The previous value
        /// </returns>
        public long ReturnValueAndIncrement(int index) {
            return Interlocked.Increment(ref _longArray[index]) - 1;
        }

        /// <summary> 
        /// Atomically decrements by one the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <returns> 
        /// The previous value
        /// </returns>
        public long ReturnValueAndDecrement(int index) {
            return Interlocked.Decrement(ref _longArray[index]) + 1;
        }

        /// <summary> 
        /// Atomically adds the given value to the element at <paramref name="index"/>. 
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <param name="deltaValue">
        /// The value to add
        /// </param>
        /// <returns> 
        /// The previous value
        /// </returns>
        public long AddDeltaAndReturnPreviousValue(int index, long deltaValue) {
            return Interlocked.Add(ref _longArray[index], deltaValue) - deltaValue;
        }

        /// <summary> 
        /// Atomically increments by one the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <returns> 
        /// The updated value
        /// </returns>
        public long IncrementValueAndReturn(int index) {
            return Interlocked.Increment(ref _longArray[index]);
        }

        /// <summary> 
        /// Atomically decrements by one the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <returns> 
        /// The updated value
        /// </returns>
        public long DecrementValueAndReturn(int index) {
            return Interlocked.Decrement(ref _longArray[index]);
        }

        /// <summary> 
        /// Atomically adds <paramref name="deltaValue"/> to the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index
        /// </param>
        /// <param name="deltaValue">
        /// The value to add
        /// </param>
        /// <returns> 
        /// The updated value
        /// </returns>
        public long AddDeltaAndReturnNewValue(int index, long deltaValue) {
            return Interlocked.Add(ref _longArray[index], deltaValue);
        }

        /// <summary> 
        /// Returns the String representation of the current values of array.
        /// </summary>
        /// <returns> 
        /// The String representation of the current values of array.
        /// </returns>
        public override String ToString() {
            if(_longArray.Length == 0)
                return "[]";
            // force volatile read
            Thread.VolatileRead(ref _longArray[0]);

            StringBuilder buf = new StringBuilder();
            buf.Append('[');
            buf.Append(_longArray[0]);

            for(int i = 1; i < _longArray.Length; i++) {
                buf.Append(", ");
                buf.Append(_longArray[i]);
            }

            buf.Append("]");
            return buf.ToString();
        }
    }
}