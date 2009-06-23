#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using System.Text;
using System.Threading;

namespace Spring.Threading.AtomicTypes
{
    /// <summary> 
    /// An array of object references in which elements may be updated
    /// atomically. 
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    /// <author>Kenneth Xu (Interlocked)</author>
    [Serializable]
    public class AtomicReferenceArray<T> : AbstractAtomicArray<T>, IAtomicArray<T> where T : class
    {
        /// <summary>
        /// Holds the object array reference
        /// </summary>
        private readonly T[] _referenceArray;

        /// <summary> 
        /// Creates a new <see cref="AtomicReferenceArray{T}"/> of <paramref name="length"/>.
        /// </summary>
        /// <param name="length">
        /// the length of the array
        /// </param>
        public AtomicReferenceArray(int length) {
            _referenceArray = new T[length];
        }

        /// <summary> 
        /// Creates a new <see cref="AtomicReferenceArray{T}"/> with the same length as, and
        /// all elements copied from <paramref name="array"/>
        /// </summary>
        /// <param name="array">
        /// The array to copy elements from
        /// </param>
        /// <throws><see cref="ArgumentNullException"/>if array is null</throws>
        public AtomicReferenceArray(T[] array) {
            if(array == null) throw new ArgumentNullException("array");
            _referenceArray = (T[])array.Clone();
            if (array.Length > 0) Thread.MemoryBarrier();
        }

        /// <summary> 
        /// Returns the length of the array.
        /// </summary>
        /// <returns> 
        /// The length of the array
        /// </returns>
        public override int Count
        {
            get { return _referenceArray.Length; }
        }

        /// <summary> 
        /// Indexer for getting and setting the current value at position <paramref name="index"/>.
        /// <p/>
        /// </summary>
        /// <param name="index">
        /// The index to use.
        /// </param>
        public override T this[int index] {
            get
            {
                Thread.MemoryBarrier(); 
                return _referenceArray[index];
            }
            set
            {
                _referenceArray[index] = value;
                Thread.MemoryBarrier();
            }
        }

        /// <summary> 
        /// Eventually sets to the given value at the given <paramref name="index"/>
        /// </summary>
        /// <param name="newValue">
        /// the new value
        /// </param>
        /// <param name="index">
        /// the index to set
        /// </param>
        public virtual void LazySet(int index, T newValue) {
            this[index] = newValue; 
        }


        /// <summary> 
        /// Atomically sets the element at position <paramref name="index"/> to <paramref name="newValue"/> 
        /// and returns the old value.
        /// </summary>
        /// <param name="index">
        /// Ihe index
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        public T Exchange(int index, T newValue) {
            return Interlocked.Exchange(ref _referenceArray[index], newValue);
        }

        /// <summary> 
        /// Atomically sets the element at <paramref name="index"/> to <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>.
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
        /// <returns> 
        /// true if successful. False return indicates that
        /// the actual value was not equal to the expected value.
        /// </returns>
        public bool CompareAndSet(int index, T expectedValue, T newValue) {
            return ReferenceEquals(expectedValue,
                Interlocked.CompareExchange(ref _referenceArray[index], newValue, expectedValue));
        }

        /// <summary> 
        /// Atomically sets the element at <paramref name="index"/> to <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>.
        /// May fail spuriously.
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
        /// <returns> 
        /// True if successful, false otherwise.
        /// </returns>
        public bool WeakCompareAndSet(int index, T expectedValue, T newValue) {
            return ReferenceEquals(expectedValue,
                Interlocked.CompareExchange(ref _referenceArray[index], newValue, expectedValue));
        }

        /// <summary> 
        /// Returns the String representation of the current values of array.</summary>
        /// <returns> the String representation of the current values of array.
        /// </returns>
        public override string ToString() {
            if(_referenceArray.Length == 0)
                return "[]";
            // force volatile read
            T dummy = this[0];

            StringBuilder buf = new StringBuilder();

            for(int i = 0; i < _referenceArray.Length; i++) {
                if(i == 0)
                    buf.Append('[');
                else
                    buf.Append(", ");

                buf.Append(_referenceArray[i].ToString());
            }

            buf.Append("]");
            return buf.ToString();
        }
    }
}