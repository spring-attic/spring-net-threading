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

namespace Spring.Threading.AtomicTypes
{
    /// <summary> 
    /// An <see cref="AtomicMarkable{T}"/> maintains an instance of type
    /// <typeparamref name="T"/> along with a mark bit, that can be updated 
    /// atomically.
    /// <p/>
    /// <b>Note:</b>This implementation maintains markable
    /// value by creating internal objects representing "boxed"
    /// [value, boolean] pairs.
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    /// <author>Kenneth Xu (.NET)</author>
    [Serializable]
    public class AtomicMarkable<T> //NET_ONLY
    {
        /// <summary>
        /// Holds the <see cref="AtomicReference{T}"/> reference
        /// </summary>
        private readonly AtomicReference<ValueBooleanPair> _atomicReference;

        [Serializable]
        private class ValueBooleanPair {
            internal readonly T _value;
            internal readonly bool _markBit;

            internal ValueBooleanPair(T value, bool markBit) {
                _value = value;
                _markBit = markBit;
            }
        }

        /// <summary> 
        /// Creates a new <see cref="AtomicMarkable{T}"/> with the given
        /// initial values.
        /// </summary>
        /// <param name="initialValue">
        /// the initial value
        /// </param>
        /// <param name="initialMark">
        /// the initial mark
        /// </param>
        public AtomicMarkable(T initialValue, bool initialMark) {
            _atomicReference = new AtomicReference<ValueBooleanPair>(new ValueBooleanPair(initialValue, initialMark));
        }

        /// <summary>
        /// Returns the <see cref="ValueBooleanPair"/> held but this instance.
        /// </summary>
        private ValueBooleanPair Pair {
            get { return _atomicReference.Value; }

        }

        /// <summary> 
        /// Returns the current value.
        /// </summary>
        /// <returns> 
        /// The current value
        /// </returns>
        public T Value {
            get { return Pair._value; }
        }

        /// <summary> 
        /// Returns the current value of the mark.
        /// </summary>
        /// <returns> 
        /// The current value of the mark
        /// </returns>
        public bool IsMarked {
            get { return Pair._markBit; }

        }

        /// <summary> 
        /// Returns both the current value and the current mark.
        /// Typical usage is:
        /// <code>
        /// bool isMarked;
        /// var reference = v.GetValue(out isMarked);
        /// </code>
        /// </summary>
        /// <param name="isMarked">
        /// return the value of the mark.
        /// </param>
        /// <returns> 
        /// The current value.
        /// </returns>
        public T GetValue(out bool isMarked) {
            ValueBooleanPair p = Pair;
            isMarked = p._markBit;
            return p._value;
        }

        /// <summary> 
        /// Atomically sets both the value and mark to the given update values if the
        /// current value and <paramref name="expectedValue"/> <see cref="AreEqual"/>
        /// and the current mark is equal to the <paramref name="expectedMark"/>.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <param name="expectedMark">
        /// The expected value of the mark
        /// </param>
        /// <param name="newMark">
        /// The new value for the mark
        /// </param>
        /// <returns> 
        /// <c>true</c> if successful, <c>false</c> otherwise
        /// </returns>
        public virtual bool WeakCompareAndSet(T expectedValue, T newValue, bool expectedMark, bool newMark) {
            ValueBooleanPair current = Pair;

            return AreEqual(expectedValue, current._value) && expectedMark == current._markBit &&
                ((AreEqual(newValue, current._value) && newMark == current._markBit) || _atomicReference.CompareAndSet(current, new ValueBooleanPair(newValue, newMark)));
        }

        /// <summary> 
        /// Atomically sets both the value and mark to the given update values if the
        /// current value and <paramref name="expectedValue"/> <see cref="AreEqual"/>
        /// and the current mark is equal to the <paramref name="expectedMark"/>.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <param name="expectedMark">
        /// The expected value of the mark
        /// </param>
        /// <param name="newMark">
        /// The new value for the mark
        /// </param>
        /// <returns> 
        /// <c>true</c> if successful, <c>false</c> otherwise
        /// </returns>
        public bool CompareAndSet(T expectedValue, T newValue, bool expectedMark, bool newMark) {
            ValueBooleanPair current = Pair;

            return AreEqual(expectedValue, current._value) && expectedMark == current._markBit &&
                ((AreEqual(newValue, current._value) && newMark == current._markBit) || _atomicReference.CompareAndSet(current, new ValueBooleanPair(newValue, newMark)));
        }

        /// <summary> 
        /// Unconditionally sets both the value and mark if any of them are
        /// not equal. Method <see cref="AreEqual"/> is used to compare the 
        /// value.
        /// </summary>
        /// <param name="newValue">the new value
        /// </param>
        /// <param name="newMark">the new value for the mark
        /// </param>
        public void SetNewAtomicValue(T newValue, bool newMark) {
            ValueBooleanPair current = Pair;
            if (!AreEqual(newValue, current._value) || newMark != current._markBit)
                _atomicReference.Exchange(new ValueBooleanPair(newValue, newMark));
        }

        /// <summary> 
        /// Atomically sets the value of the mark to the given update value
        /// if the current value and the expected value <see cref="AreEqual"/>.  
        /// Any given invocation of this operation may fail
        /// (return false) spuriously, but repeated invocation
        /// when the current value holds the expected value and no other
        /// thread is also attempting to set the value will eventually
        /// succeed.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newMark">
        /// The new value for the mark
        /// </param>
        /// <returns> 
        /// <c>true</c> if successful, <c>false</c> otherwise
        /// </returns>
        public bool AttemptMark(T expectedValue, bool newMark) {
            ValueBooleanPair current = Pair;

            return AreEqual(expectedValue, current._value) 
                && (newMark == current._markBit || _atomicReference.CompareAndSet(current, new ValueBooleanPair(expectedValue, newMark)));
        }

        /// <summary>
        /// Determine if two instances are equals. This implementation uses
        /// <see cref="object.Equals(object,object)"/> to determine the equality.
        /// </summary>
        /// <param name="x">first instance to compare.</param>
        /// <param name="y">second instance to compare</param>
        /// <returns>
        /// <c>true</c> when and only when <paramref name="x"/> equals 
        /// <paramref name="y"/>.
        /// </returns>
        protected virtual bool AreEqual(T x, T y)
        {
            return Equals(x, y);
        }
    }
}