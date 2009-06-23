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

namespace Spring.Threading.AtomicTypes
{
    /// <summary> 
    /// An array of instances of type <typeparamref name="T"/> in which 
    /// elements may be updated atomically. Always consider to user other
    /// type specific atomic arraies whenever possible.
    /// </summary>
    /// <remarks>
    /// Based on the on the back port of JCP JSR-166.
    /// <p/>
    /// <b>Note:</b>This implementation boxes the value in an private
    /// reference type holder and uses <see cref="AtomicReferenceArray{T}"/> to
    /// accomplish the atomic access.
    /// </remarks>
    /// <seealso cref="AtomicIntegerArray"/>
    /// <seealso cref="AtomicLongArray"/>
    /// <seealso cref="AtomicReferenceArray{T}"/>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu (.NET)</author>
    [Serializable]
    public class AtomicArray<T> : AbstractAtomicArray<T>, IAtomicArray<T>
    {
        /// <summary>
        /// Holds the object array reference
        /// </summary>
        private readonly AtomicReferenceArray<ValueHolder<T>> _atomicReferenceArray;

        /// <summary> 
        /// Creates a new <see cref="AtomicReferenceArray{T}"/> of <paramref name="length"/>.
        /// </summary>
        /// <param name="length">
        /// the length of the array
        /// </param>
        public AtomicArray(int length) {
            ValueHolder<T> holder = new ValueHolder<T>();
            ValueHolder<T>[] holders = new ValueHolder<T>[length];
            for (int i = 0; i < length; i++) holders[i] = holder;
            _atomicReferenceArray = new AtomicReferenceArray<ValueHolder<T>>(holders);
        }

        /// <summary> 
        /// Creates a new <see cref="AtomicReferenceArray{T}"/> with the same length as, and
        /// all elements copied from <paramref name="array"/>
        /// </summary>
        /// <param name="array">
        /// The array to copy elements from
        /// </param>
        /// <throws><see cref="ArgumentNullException"/>if array is null</throws>
        public AtomicArray(T[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            int length = array.Length;
            ValueHolder<T>[] holders = new ValueHolder<T>[length];
            for (int i = 0; i < length; i++) holders[i] = new ValueHolder<T>(array[i]);
            _atomicReferenceArray = new AtomicReferenceArray<ValueHolder<T>>(holders);
        }

        /// <summary> 
        /// Returns the length of the array.
        /// </summary>
        /// <returns> 
        /// The length of the array
        /// </returns>
        public override int Count
        {
            get { return _atomicReferenceArray.Count; }
        }

        /// <summary> 
        /// Indexer for getting and setting the current value at position <paramref name="index"/>.
        /// <p/>
        /// </summary>
        /// <param name="index">
        /// The index to use.
        /// </param>
        public override T this[int index] {
            get { return _atomicReferenceArray[index].Value; }
            set { _atomicReferenceArray[index] = new ValueHolder<T>(value); }
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
            _atomicReferenceArray[index] = new ValueHolder<T>(newValue);
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
            return _atomicReferenceArray.Exchange(index, new ValueHolder<T>(newValue)).Value;
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
            ValueHolder<T> current = _atomicReferenceArray[index];

            return Equals(expectedValue, current.Value) &&
                (Equals(newValue, current.Value) || _atomicReferenceArray.CompareAndSet(index, current, new ValueHolder<T>(newValue)));
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
            ValueHolder<T> current = _atomicReferenceArray[index];

            return Equals(expectedValue, current.Value) &&
                (Equals(newValue, current.Value) || _atomicReferenceArray.CompareAndSet(index, current, new ValueHolder<T>(newValue)));
        }

        /// <summary> 
        /// Returns the String representation of the current values of array.</summary>
        /// <returns> the String representation of the current values of array.
        /// </returns>
        public override string ToString() {
            return _atomicReferenceArray.ToString();
        }
    }
}