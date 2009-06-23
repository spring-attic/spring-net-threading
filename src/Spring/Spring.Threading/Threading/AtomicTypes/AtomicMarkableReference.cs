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
    /// An <see cref="AtomicMarkableReference{T}"/> maintains an object reference
    /// along with a mark bit, that can be updated atomically.
    /// <p/>
    /// <b>Note:</b>This implementation maintains markable
    /// references by creating internal objects representing "boxed"
    /// [reference, boolean] pairs.
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    /// <author>Kenneth Xu (.NET)</author>
    [Serializable]
    public class AtomicMarkableReference<T> : AtomicMarkable<T> where T : class
    {

        /// <summary> 
        /// Creates a new <see cref="AtomicMarkable{T}"/> with the given
        /// initial values.
        /// </summary>
        /// <param name="initialReference">
        /// the initial value of the reference
        /// </param>
        /// <param name="initialMark">
        /// the initial mark
        /// </param>
        public AtomicMarkableReference(T initialReference, bool initialMark)
            : base(initialReference, initialMark)
        {
        }

        /// <summary>
        /// Determine if two instances are equals. This implementation uses
        /// <see cref="object.ReferenceEquals(object,object)"/> to determine 
        /// the equality.
        /// </summary>
        /// <param name="x">first instance to compare.</param>
        /// <param name="y">second instance to compare</param>
        /// <returns>
        /// <c>true</c> when and only when <paramref name="x"/> equals 
        /// <paramref name="y"/>.
        /// </returns>
        protected override bool AreEqual(T x, T y)
        {
            return ReferenceEquals(x, y);
        }
    }
}