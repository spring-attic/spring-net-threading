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
using System.Collections;
using System.Collections.Generic;
using Spring.Collections.Generic;

namespace Spring.Threading.AtomicTypes
{
    /// <summary>
    /// Provide common implementation that making atomic arrays also
    /// <see cref="IList{T}"/>s.
    /// </summary>
    /// <typeparam name="T">Type of the atomic array element.</typeparam>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public abstract class AbstractAtomicArray<T> : AbstractList<T>
    {
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// Subclass must implement this method.
        /// </remarks>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate 
        /// through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator<T> GetEnumerator()
        {
            int length = Count;
            for (int i = 0; i < length; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Called by implicit implementation of <see cref="IList.IsFixedSize"/>.
        /// All atomic arrays are fixed size so this always return <c>false</c>.
        /// </summary>
        protected override bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is 
        /// read-only.  All atomic arrays are designed to be updated so this 
        /// implementation always return <c>false</c>;
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
        /// This implementation always return <c>false</c>;
        /// </returns>
        /// 
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> 
        /// is synchronized (thread safe).
        /// </summary>
        /// <remarks>
        /// As all atomic arrays are thread safe, this implementaiton always return 
        /// <see langword="true"/>.
        /// </remarks>
        /// <returns>
        /// true if access to the <see cref="ICollection"/> is synchronized (thread 
        /// safe); otherwise, false. This implementaiton always return <c>true</c>.
        /// 
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected override bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <remarks>
        /// All atomic array must implement this indexer.
        /// </remarks>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">
        /// The zero-based index of the element to get or set.</param>
        /// <exception cref="IndexOutOfRangeException">
        /// index is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        public abstract override T this[int index] { get; set; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <remarks>
        /// All atomic array must implement this property.
        /// </remarks>
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        /// 
        public abstract override int Count{ get; }
    }
}