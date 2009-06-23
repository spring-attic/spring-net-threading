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

using System.Collections.Generic;

namespace Spring.Threading.AtomicTypes
{
    /// <summary>
    /// Provide atomic access to an array of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the array in which the elements are updated atomically.
    /// </typeparam>
    /// <author>Kenneth Xu</author>
    public interface IAtomicArray<T> : IList<T>
    {
        /// <summary> 
        /// Eventually sets to the given value at the given <paramref name="index"/>
        /// </summary>
        /// <param name="newValue">
        /// the new value
        /// </param>
        /// <param name="index">
        /// the index to set
        /// </param>
        void LazySet(int index, T newValue);

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
        T Exchange(int index, T newValue);

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
        bool CompareAndSet(int index, T expectedValue, T newValue);

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
        bool WeakCompareAndSet(int index, T expectedValue, T newValue);

    }
}
