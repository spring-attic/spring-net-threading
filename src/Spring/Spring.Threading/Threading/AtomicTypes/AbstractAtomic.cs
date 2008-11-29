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

namespace Spring.Threading.AtomicTypes {
    /// <summary>
    /// the base class for non array atomic types. 
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [Serializable]
    public abstract class AbstractAtomic<T> {
        /// <summary>
        /// Holds a representation of the current value.
        /// </summary>
        private T _value;

        /// <summary> 
        /// Creates a new <see cref="AbstractAtomic{T}"/> with the given initial value.
        /// </summary>
        /// <param name="initialValue">The initial value</param>
        protected  AbstractAtomic(T initialValue) {
            _value = initialValue;
        }

        /// <summary> 
        /// Creates a new <see cref="AbstractAtomic{T}"/> with initial value of <see lang="default(T)"/>.
        /// </summary>
        protected AbstractAtomic()
            : this(default(T)) {
        }

        /// <summary> 
        /// Gets / Sets the current value.
        /// <p/>
        /// <b>Note:</b> The setting of this value occurs within a <see lang="lock"/>.
        /// </summary>
		public T Value {
            get { return _value; }
            set {
                lock(this) {
                    _value = value;
                }
            }
        }

        /// <summary> 
        /// Atomically sets the value to <paramref name="newValue"/>
        /// if the current value <see lang="Equals"/> <paramref name="expectedValue"/>
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value to use of the current value equals the expected value.
        /// </param>
        /// <returns> 
        /// <see lang="true"/> if the current value equaled the expected value, <see lang="false"/> otherwise.
        /// </returns>
        public abstract bool CompareAndSet(T expectedValue, T newValue);

        /// <summary> 
        /// Atomically sets the value to <paramref name="newValue"/>
        /// if the current value == <paramref name="expectedValue"/>
        /// May fail spuriously.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value to use of the current value equals the expected value.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if the current value equaled the expected value, <see lang="false"/> otherwise.
        /// </returns>
        public abstract bool WeakCompareAndSet(T expectedValue, T newValue);
        
        /// <summary> 
		/// Eventually sets to the given value.
		/// <remarks>this is exactly the same as the set'ter of <see cref="Value"/> and is implemented only for
		/// compatability with the according java classes</remarks>
		/// </summary>
		/// <param name="newValue">
		/// the new value
		/// </param>
		/// This method doesn't differ from the set() method, which was converted to a property.  For now
		/// the property will be called for this method.
        //[Obsolete("This method will be removed.  Please use AtomicBoolean.BooleanValue property instead.")]
        public void LazySet(T newValue) {
            Value = newValue;
        }

        /// <summary> 
        /// Atomically sets the current value to <parmref name="newValue"/> and returns the previous value.
        /// <remarks>in java this is the getAndSet method</remarks>
        /// </summary>
        /// <param name="newValue">
        /// The new value for the instance.
        /// </param>
        /// <returns> 
        /// the previous value of the instance.
        /// </returns>
        public abstract T SetNewAtomicValue(T newValue);

        /// <summary> 
        /// Returns the String representation of the current value.
        /// </summary>
        /// <returns> 
        /// The String representation of the current value.
        /// </returns>
        public override string ToString() {
            return Value.ToString();
        }
    }
}