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
    /// <see cref="Atomic{T}"/> provide atomic access to an instance of type
    /// <typeparamref name="T"/> with the equality defined as value equals. 
    /// Always consider to use other type specific atomic whenever possible.
    /// </summary>
    /// <remarks>
    /// Based on the on the back port of JCP JSR-166.
    /// <p/>
    /// <b>Note:</b>This implementation boxes the value in an private
    /// reference type holder and uses <see cref="AtomicReference{T}"/> to
    /// accomplish the atomic access.
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Kenneth Xu (.NET)</author>
    [Serializable]
    public class Atomic<T> : IAtomic<T>
    {
        /// <summary>
        /// Holds the <see cref="Atomic{T}"/> reference
        /// </summary>
        private readonly AtomicReference<ValueHolder<T>> _atomicReference;

        /// <summary> 
        /// Creates a new <see cref="Atomic{T}"/> with the given
        /// initial values.
        /// </summary>
        /// <param name="initialValue">
        /// the initial value.
        /// </param>
        public Atomic(T initialValue)
        {
            _atomicReference = new AtomicReference<ValueHolder<T>>(new ValueHolder<T>(initialValue));
        }

        /// <summary> 
        /// Creates a new <see cref="Atomic{T}"/> with <c>default<typeparamref name="T"/></c> 
        /// as initial value.
        /// </summary>
        public Atomic()
            : this(default(T)) {
        }

        /// <summary>
        /// Returns the <see cref="ValueHolder{T}"/> held but this instance.
        /// </summary>
        private ValueHolder<T> Holder
        {
            get { return _atomicReference.Value; }

        }
        /// <summary> 
        /// Gets and sets the current value.
        /// </summary>
        public T Value
        {
            get { return Holder.Value; }
            set { _atomicReference.Value = new ValueHolder<T>(value);}
        }

        /// <summary> 
        /// Eventually sets to the given value.
        /// </summary>
        /// <param name="newValue">
        /// the new value
        /// </param>
        public void LazySet(T newValue)
        {
            _atomicReference.Value = new ValueHolder<T>(newValue);
        }

        /// <summary> 
        /// Atomically sets the value to the <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>
        /// determined by <see cref="object.Equals(object,object)"/>.
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
        public bool CompareAndSet(T expectedValue, T newValue)
        {
            ValueHolder<T> current = Holder;

            return Equals(expectedValue, current.Value) &&
                (Equals(newValue, current.Value) || _atomicReference.CompareAndSet(current, new ValueHolder<T>(newValue)));
        }

        /// <summary> 
        /// Atomically sets the value to the <paramref name="newValue"/>
        /// if the current value equals the <paramref name="expectedValue"/>
        /// determined by <see cref="object.Equals(object,object)"/>.
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
        public virtual bool WeakCompareAndSet(T expectedValue, T newValue)
        {
            ValueHolder<T> current = Holder;

            return Equals(expectedValue, current.Value) &&
                (Equals(newValue, current.Value) || _atomicReference.CompareAndSet(current, new ValueHolder<T>(newValue)));
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
        public T Exchange(T newValue)
        {
            return _atomicReference.Exchange(new ValueHolder<T>(newValue)).Value;
        }

        /// <summary> 
        /// Returns the String representation of the current value.
        /// </summary>
        /// <returns> 
        /// The String representation of the current value.
        /// </returns>
        public override string ToString()
        {
            return _atomicReference.Value.Value.ToString();
        }

        /// <summary>
        /// Implicit converts <see cref="Atomic{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="atomic">
        /// Instance of <see cref="Atomic{T}"/>.
        /// </param>
        /// <returns>
        /// The converted int value of <paramref name="atomic"/>.
        /// </returns>
        public static implicit operator T(Atomic<T> atomic)
        {
            return atomic.Value;
        }
    }

}