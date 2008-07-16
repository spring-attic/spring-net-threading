using System;

namespace Spring.Threading.AtomicTypes
{
	/// <summary>
	/// A object reference that may be updated atomically. 
	/// <p/>
	/// Based on the on the back port of JCP JSR-166.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class AtomicReference
	{
		/// <summary>
		/// Holds the object reference.
		/// </summary>
		private volatile object _objectReference;

		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReference"/> with the given initial value.
		/// </summary>
		/// <param name="initialValue">
		/// The initial value
		/// </param>
		public AtomicReference(object initialValue)
		{
			_objectReference = initialValue;
		}

		
		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicReference"/> with null initial value.
		/// </summary>
		public AtomicReference() : this(null)
		{
		}

		/// <summary> 
		/// Gets / Sets the current value.
		/// <p/>
		/// <b>Note:</b> The setting of this value occurs within a <see lang="lock"/>.
		/// </summary>
		public object ObjectReference
		{
			get { return _objectReference; }
			set
			{
				lock (this)
				{
					_objectReference = value;
				}
			}
		}
		/// <summary> 
		/// Eventually sets to the given value.
		/// </summary>
		/// <param name="newValue">
		/// the new value
		/// </param>
		/// TODO: This method doesn't differ from the set() method, which was converted to a property.  For now
		/// the property will be called for this method.
		[Obsolete("This method will be removed.  Please use AtomicReference.ObjectReference property instead.")]
		public void LazySet(object newValue)
		{
			ObjectReference = newValue;
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
		public bool CompareAndSet(object expectedValue, object newValue)
		{
			lock (this)
			{
				// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
				if ( _objectReference.Equals(expectedValue) )
				{
					_objectReference = newValue;
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
		public virtual bool WeakCompareAndSet(object expectedValue, object newValue)
		{
			lock (this)
			{
				// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
				if ( _objectReference.Equals(expectedValue) )
				{
					_objectReference = newValue;
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
		public object SetNewAtomicValue(object newValue)
		{
			lock (this)
			{
				object old = _objectReference;
				_objectReference = newValue;
				return old;
			}
		}
		/// <summary> 
		/// Returns the String representation of the current value.
		/// </summary>
		/// <returns> 
		/// The String representation of the current value.
		/// </returns>
		public override String ToString()
		{
			return Convert.ToString(ObjectReference);
		}
	}
}