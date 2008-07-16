using System;
using System.Text;

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
	// TODO: This is only a object array reference because of boxing and unboxing.  Need to figure out how to implement this class without hurting performance.
	[Serializable]
	public class AtomicReferenceArray
	{
		/// <summary>
		/// Holds the object array reference
		/// </summary>
		private object[] _referenceArray;

		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReferenceArray"/> of <paramref name="length"/>.</summary>
		/// <param name="length">
		/// the length of the array
		/// </param>
		public AtomicReferenceArray(int length)
		{
			_referenceArray = new object[length];
		}

		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReferenceArray"/> with the same length as, and
		/// all elements copied from <paramref name="array"/>
		/// </summary>
		/// <param name="array">
		/// The array to copy elements from
		/// </param>
		/// <throws><see cref="NullReferenceException"/>if array is null</throws>
		public AtomicReferenceArray(object[] array) : this(array.Length)
		{
			if (array == null)
				throw new NullReferenceException();
			Array.Copy(array, 0, _referenceArray, 0, array.Length);
		}

		/// <summary> 
		/// Returns the length of the array.
		/// </summary>
		/// <returns> 
		/// The length of the array
		/// </returns>
		public int Length()
		{
			return _referenceArray.Length;
		}

		/// <summary> 
		/// Indexer for getting and setting the current value at position <paramref name="index"/>.
		/// <p/>
		/// </summary>
		/// <param name="index">
		/// The index to use.
		/// </param>
		public object this[int index]
		{
			get
			{
				lock (this)
				{
					return _referenceArray[index];
				}
			}
			set
			{
				lock (this)
				{
					_referenceArray[index] = value;
				}
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
		/// TODO: This method doesn't differ from the set() method, which was converted to a property.  For now
		/// the property will be called for this method.
		[Obsolete("This method will be removed.  Please use indexer instead.")]
		public virtual void LazySet(int index, object newValue)
		{
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
		public object SetNewAtomicValue(int index, object newValue)
		{
			lock (this)
			{
				object old = _referenceArray[index];
				_referenceArray[index] = newValue;
				return old;
			}
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
		public bool CompareAndSet(int index, object expectedValue, object newValue)
		{
			lock (this)
			{
				// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
				if (_referenceArray[index].Equals(expectedValue))
				{
					_referenceArray[index] = newValue;
					return true;
				}
				else
				{
					return false;
				}
			}
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
		public bool WeakCompareAndSet(int index, object expectedValue, object newValue)
		{
			lock (this)
			{
				// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
				if (_referenceArray[index].Equals(expectedValue))
				{
					_referenceArray[index] = newValue;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary> 
		/// Returns the String representation of the current values of array.</summary>
		/// <returns> the String representation of the current values of array.
		/// </returns>
		// TODO: Should be updated to use Spring.Core.StringUtils class.
		public override String ToString()
		{
			if (_referenceArray.Length == 0)
				return "[]";

			StringBuilder buf = new StringBuilder();

			for (int i = 0; i < _referenceArray.Length; i++)
			{
				if (i == 0)
					buf.Append('[');
				else
					buf.Append(", ");

				buf.Append(Convert.ToString(_referenceArray[i]));
			}

			buf.Append("]");
			return buf.ToString();
		}
	}
}