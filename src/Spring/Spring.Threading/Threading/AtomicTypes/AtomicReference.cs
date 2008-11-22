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
    /// A object reference that may be updated atomically. 
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [Serializable]
    public class AtomicReference<T> {
        /// <summary>
        /// Holds the object reference.
        /// </summary>
        private T _value;

        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReference{T}"/> with the given initial value.
        /// </summary>
        /// <param name="initialValue">
        /// The initial value
        /// </param>
        public AtomicReference(T initialValue) {
            _value = initialValue;
        }

        /// <summary> 
        /// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReference{T}"/> with null initial value.
        /// </summary>
        public AtomicReference()
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
        /// Atomically sets the value to the <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>.
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
        public bool CompareAndSet(T expectedValue, T newValue) {
            lock(this) {
                // TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
                if(_value.Equals(expectedValue)) {
                    _value = newValue;
                    return true;
                }
                return false;
            }
        }
        
        /// <summary> 
        /// Atomically sets the value to the <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>.
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
        public virtual bool WeakCompareAndSet(T expectedValue, T newValue) {
            lock(this) {
                if(_value.Equals(expectedValue)) {
                    _value = newValue;
                    return true;
                }
                return false;
            }
        }

        /// <summary> 
        /// Atomically sets to the given value and returns the previous value.
        /// </summary>
        /// <param name="newValue">
        /// The new value for the instance.
        /// </param>
        /// <returns> 
        /// the previous value of the instance.
        /// </returns>
        public T GetAndSet(T newValue) {
            lock(this) {
                T old = _value;
                _value = newValue;
                return old;
            }
        }

        /// <summary> 
        /// Returns the String representation of the current value.
        /// </summary>
        /// <returns> 
        /// The String representation of the current value.
        /// </returns>
        public override string ToString() {
            return Convert.ToString(Value);
        }
    }
}