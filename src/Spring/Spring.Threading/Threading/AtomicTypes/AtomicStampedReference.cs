using System;

namespace Spring.Threading.AtomicTypes
{
	/// <summary>
	/// An <see cref="Spring.Threading.AtomicTypes.AtomicStampedReference"/> maintains an object reference
	/// along with an integer "stamp", that can be updated atomically.
	/// 
	/// <p/>
	/// <b>Note:</b>This implementation maintains stamped
	/// references by creating internal objects representing "boxed"
	/// [reference, integer] pairs.
	/// <p/>
	/// Based on the on the back port of JCP JSR-166.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class AtomicStampedReference
	{
		private AtomicReference _atomicReference;

		[Serializable]
		private class ReferenceIntegerPair
		{
			private object _reference;
			private int _integer;

			internal ReferenceIntegerPair(object reference, int integer)
			{
				_reference = reference;
				_integer = integer;
			}

			public object Reference
			{
				get { return _reference; }
			}

			public int Integer
			{
				get { return _integer; }
			}
		}

		/// <summary> 
		///	Returns the current value of the reference.
		/// </summary>
		/// <returns> 
		/// The current value of the reference
		/// </returns>
		public Object ObjectReference
		{
			get { return Pair.Reference; }

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
		/// Gets the <see cref="ReferenceIntegerPair"/> represented by this instance.
		/// </summary>
		private ReferenceIntegerPair Pair
		{
			get { return (ReferenceIntegerPair) _atomicReference.ObjectReference; }

		}
		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicStampedReference"/> with the given
		/// initial values.
		/// </summary>
		/// <param name="initialRef">
		/// The initial reference
		/// </param>
		/// <param name="initialStamp">
		/// The initial stamp
		/// </param>
		public AtomicStampedReference(object initialRef, int initialStamp)
		{
			_atomicReference = new AtomicReference(new ReferenceIntegerPair(initialRef, initialStamp));
		}

		/// <summary> 
		/// Returns the current values of both the reference and the stamp.
		/// Typical usage is:
		/// <code>
		/// int[1] holder;
		/// object reference = v.GetobjectReference(holder);
		/// </code> 
		/// </summary>
		/// <param name="stampHolder">
		/// An array of size of at least one.  On return,
		/// <tt>stampholder[0]</tt> will hold the value of the stamp.
		/// </param>
		/// <returns> 
		/// The current value of the reference
		/// </returns>
		public object GetObjectReference(int[] stampHolder)
		{
			ReferenceIntegerPair p = Pair;
			stampHolder[0] = p.Integer;
			return p.Reference;
		}
		/// <summary> 
		/// Atomically sets the value of both the reference and stamp
		/// to the given update values if the
		/// current reference is equals to the expected reference
		/// and the current stamp is equal to the expected stamp.  Any given
		/// invocation of this operation may fail (return
		/// false) spuriously, but repeated invocation when
		/// the current value holds the expected value and no other thread
		/// is also attempting to set the value will eventually succeed.
		/// </summary>
		/// <param name="expectedReference">
		/// The expected value of the reference
		/// </param>
		/// <param name="newReference">
		/// The new value for the reference
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
		public virtual bool WeakCompareAndSet(Object expectedReference, Object newReference, int expectedStamp, int newStamp)
		{
			ReferenceIntegerPair current = Pair;
			// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
			return expectedReference.Equals(current.Reference) && expectedStamp == current.Integer && ((newReference.Equals(current.Reference) && newStamp == current.Integer) || _atomicReference.WeakCompareAndSet(current, new ReferenceIntegerPair(newReference, newStamp)));
		}

		/// <summary> 
		/// Atomically sets the value of both the reference and stamp
		/// to the given update values if the
		/// current reference is equal to the expected reference
		/// and the current stamp is equal to the expected stamp.
		/// </summary>
		/// <param name="expectedReference">
		/// The expected value of the reference
		/// </param>
		/// <param name="newReference">
		/// The new value for the reference
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
		public virtual bool CompareAndSet(Object expectedReference, Object newReference, int expectedStamp, int newStamp)
		{
			ReferenceIntegerPair current = Pair;
			return expectedReference.Equals(current.Reference) && expectedStamp == current.Integer && ((newReference.Equals(current.Reference) && newStamp == current.Integer) || _atomicReference.WeakCompareAndSet(current, new ReferenceIntegerPair(newReference, newStamp)));
		}
		/// <summary> 
		/// Unconditionally sets the value of both the reference and stamp.
		/// </summary>
		/// <param name="newReference">
		/// The new value for the reference
		/// </param>
		/// <param name="newStamp">
		/// The new value for the stamp
		/// </param>
		public void SetNewAtomicValue(Object newReference, int newStamp)
		{
			ReferenceIntegerPair current = Pair;
			if (newReference != current.Reference || newStamp != current.Integer)
				_atomicReference.ObjectReference = new ReferenceIntegerPair(newReference, newStamp);
		}

		/// <summary> 
		/// Atomically sets the value of the stamp to the given update value
		/// if the current reference is equal to the expected
		/// reference.  Any given invocation of this operation may fail
		/// (return false) spuriously, but repeated invocation
		/// when the current value holds the expected value and no other
		/// thread is also attempting to set the value will eventually
		/// succeed.
		/// </summary>
		/// <param name="expectedReference">
		/// The expected value of the reference
		/// </param>
		/// <param name="newStamp">
		/// The new value for the stamp
		/// </param>
		/// <returns> 
		/// True if successful
		/// </returns>
		public virtual bool AttemptStamp(object expectedReference, int newStamp)
		{
			ReferenceIntegerPair current = Pair;
			return expectedReference.Equals(current.Reference) && (newStamp == current.Integer || _atomicReference.CompareAndSet(current, new ReferenceIntegerPair(expectedReference, newStamp)));
		}
	}
}