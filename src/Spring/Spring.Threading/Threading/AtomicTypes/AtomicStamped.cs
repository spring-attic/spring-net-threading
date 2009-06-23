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
    /// An <see cref="AtomicStamped{T}"/> maintains an instance of type
    /// <typeparamref name="T"/> along with an integer "stamp", that can 
    /// be updated atomically.
    /// 
    /// <p/>
    /// <b>Note:</b>This implementation maintains stamped
    /// value by creating internal objects representing "boxed"
    /// [value, integer] pairs.
    /// <p/>
    /// Based on the on the back port of JCP JSR-166.
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    /// <author>Kenneth Xu (.NET)</author>
    [Serializable]
    public class AtomicStamped<T>
    {
        private readonly AtomicReference<ValueIntegerPair> _atomicReference;

        [Serializable]
        private class ValueIntegerPair
        {
            internal readonly T Value;
            internal readonly int Integer;

            internal ValueIntegerPair(T value, int integer)
            {
                Value = value;
                Integer = integer;
            }
        }

        /// <summary> 
        ///	Returns the current value.
        /// </summary>
        /// <returns> 
        /// The current value
        /// </returns>
        public T Value
        {
            get { return Pair.Value; }

        }

        /// <summary> 
        /// Returns the current value of the stamp.
        /// </summary>
        /// <returns> 
        /// The current value of the stamp
        /// </returns>
        public int Stamp
        {
            get { return Pair.Integer; }

        }

        /// <summary>
        /// Gets the <see cref="ValueIntegerPair"/> represented by this instance.
        /// </summary>
        private ValueIntegerPair Pair
        {
            get { return _atomicReference.Value; }

        }

        /// <summary> 
        /// Creates a new <see cref="AtomicStamped{T}"/> with the given
        /// initial values.
        /// </summary>
        /// <param name="initialValue">
        /// The initial value
        /// </param>
        /// <param name="initialStamp">
        /// The initial stamp
        /// </param>
        public AtomicStamped(T initialValue, int initialStamp)
        {
            _atomicReference = new AtomicReference<ValueIntegerPair>(new ValueIntegerPair(initialValue, initialStamp));
        }

        /// <summary> 
        /// Returns both the current value and the stamp.
        /// Typical usage is:
        /// <code>
        /// int stamp;
        /// object value = v.GetValue(out stamp);
        /// </code> 
        /// </summary>
        /// <param name="stamp">
        /// An array of size of at least one.  On return,
        /// <tt>stampholder[0]</tt> will hold the value of the stamp.
        /// </param>
        /// <returns> 
        /// The current value
        /// </returns>
        public T GetValue(out int stamp)
        {
            ValueIntegerPair p = Pair;
            stamp = p.Integer;
            return p.Value;
        }

        /// <summary> 
        /// Atomically sets both the value and stamp to the given update values 
        /// if the current value and the expected value <see cref="AreEqual"/>
        /// and the current stamp is equal to the expected stamp.  Any given
        /// invocation of this operation may fail (return
        /// false) spuriously, but repeated invocation when
        /// the current value holds the expected value and no other thread
        /// is also attempting to set the value will eventually succeed.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <param name="expectedStamp">
        /// The expected value of the stamp
        /// </param>
        /// <param name="newStamp">
        /// The new value for the stamp
        /// </param>
        /// <returns> 
        /// True if successful
        /// </returns>
        public virtual bool WeakCompareAndSet(T expectedValue, T newValue, int expectedStamp, int newStamp)
        {
            ValueIntegerPair current = Pair;

            return AreEqual(expectedValue, current.Value) && expectedStamp == current.Integer
                && ((AreEqual(newValue, current.Value) && newStamp == current.Integer) || _atomicReference.WeakCompareAndSet(current, new ValueIntegerPair(newValue, newStamp)));
        }

        /// <summary> 
        /// Atomically sets both the value and stamp to the given update values 
        /// if the current value and the expected value <see cref="AreEqual"/>
        /// and the current stamp is equal to the expected stamp.
        /// </summary>
        /// <param name="expectedValue">
        /// The expected value
        /// </param>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <param name="expectedStamp">
        /// The expected value of the stamp
        /// </param>
        /// <param name="newStamp">
        /// The new value for the stamp
        /// </param>
        /// <returns> 
        /// True if successful, false otherwise.
        /// </returns>
        public virtual bool CompareAndSet(T expectedValue, T newValue, int expectedStamp, int newStamp)
        {
            ValueIntegerPair current = Pair;
            return AreEqual(expectedValue, current.Value) && expectedStamp == current.Integer
                && ((newValue.Equals(current.Value) && newStamp == current.Integer) || _atomicReference.WeakCompareAndSet(current, new ValueIntegerPair(newValue, newStamp)));
        }

        /// <summary> 
        /// Unconditionally sets both the value and stamp if any of them are
        /// not equal. Method <see cref="AreEqual"/> is used to compare the 
        /// value.
        /// </summary>
        /// <param name="newValue">
        /// The new value
        /// </param>
        /// <param name="newStamp">
        /// The new value for the stamp
        /// </param>
        public void SetNewAtomicValue(T newValue, int newStamp)
        {
            ValueIntegerPair current = Pair;
            if (!AreEqual(newValue, current.Value) || newStamp != current.Integer)
                _atomicReference.Value = new ValueIntegerPair(newValue, newStamp);
        }

        /// <summary> 
        /// Atomically sets the value of the stamp to the given update value
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
        /// <param name="newStamp">
        /// The new value for the stamp
        /// </param>
        /// <returns> 
        /// True if successful
        /// </returns>
        public virtual bool AttemptStamp(T expectedValue, int newStamp)
        {
            ValueIntegerPair current = Pair;
            return AreEqual(expectedValue, current.Value)
                && (newStamp == current.Integer || _atomicReference.CompareAndSet(current, new ValueIntegerPair(expectedValue, newStamp)));
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