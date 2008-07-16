using System;

namespace Spring.Threading.AtomicTypes
{
	/// <summary> 
	/// An <see cref="Spring.Threading.AtomicTypes.AtomicMarkableReference"/> maintains an object reference
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
	[Serializable]
	public class AtomicMarkableReference
	{
		/// <summary>
		/// Holds the <see cref="Spring.Threading.AtomicTypes.AtomicReference"/> reference
		/// </summary>
		private AtomicReference _atomicReference;

		[Serializable]
		private class ReferenceBooleanPair
		{
			private object _objectReference;
			private bool _markBit;

			internal ReferenceBooleanPair(object objectReference, bool markBit)
			{
				_objectReference = objectReference;
				_markBit = markBit;
			}

			public object ObjectReference
			{
				get { return _objectReference; }
			}

			public bool MarkBit
			{
				get { return _markBit; }
			}
		}

		/// <summary> 
		/// Creates a new <see cref="Spring.Threading.AtomicTypes.AtomicMarkableReference"/> with the given
		/// initial values.
		/// </summary>
		/// <param name="initialReference">
		/// the initial reference
		/// </param>
		/// <param name="initialMark">
		/// the initial mark
		/// </param>
		public AtomicMarkableReference(object initialReference, bool initialMark)
		{
			_atomicReference = new AtomicReference(new ReferenceBooleanPair(initialReference, initialMark));
		}

		/// <summary>
		/// Returns the <see cref="ReferenceBooleanPair"/> held but this instance.
		/// </summary>
		private ReferenceBooleanPair Pair
		{
			get { return (ReferenceBooleanPair) _atomicReference.ObjectReference; }

		}

		/// <summary> 
		/// Returns the current value of the reference.
		/// </summary>
		/// <returns> 
		/// The current value of the reference
		/// </returns>
		public object ObjectReference
		{

			get { return Pair.ObjectReference; }

		}

		/// <summary> 
		/// Returns the current value of the mark.
		/// </summary>
		/// <returns> 
		/// The current value of the mark
		/// </returns>
		public bool IsReferenceMarked
		{
			get { return Pair.MarkBit; }

		}

		/// <summary> 
		/// Returns the current values of both the reference and the mark.
		/// Typical usage is:
		/// <code>
		/// bool[1] holder;
		/// object reference = v.GetobjectReference(holder);
		/// </code>
		/// </summary>
		/// <param name="markHolder">
		/// An array of size of at least one. On return,
		/// markholder[0] will hold the value of the mark.
		/// </param>
		/// <returns> 
		/// The current value of the reference
		/// </returns>
		public object GetObjectReference(ref bool[] markHolder)
		{
			ReferenceBooleanPair p = Pair;
			markHolder[0] = p.MarkBit;
			return p.ObjectReference;
		}

		/// <summary> 
		/// Atomically sets the value of both the reference and mark
		/// to the given update values if the
		/// current reference is equal to <paramref name="expectedReference"/> 
		/// and the current mark is equal to the <paramref name="expectedMark"/>.
		/// </summary>
		/// <param name="expectedReference">
		/// The expected value of the reference
		/// </param>
		/// <param name="newReference">
		/// The new value for the reference
		/// </param>
		/// <param name="expectedMark">
		/// The expected value of the mark
		/// </param>
		/// <param name="newMark">
		/// The new value for the mark
		/// </param>
		/// <returns> 
		/// <see lang="true"/> if successful, <see lang="false"/> otherwise
		/// </returns>
		public virtual bool WeakCompareAndSet(object expectedReference, object newReference, bool expectedMark, bool newMark)
		{
			ReferenceBooleanPair current = Pair;
			// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
			return expectedReference.Equals(current.ObjectReference) && expectedMark == current.MarkBit && ((newReference == current.ObjectReference && newMark == current.MarkBit) || _atomicReference.CompareAndSet(current, new ReferenceBooleanPair(newReference, newMark)));
		}

		/// <summary> 
		/// Atomically sets the value of both the reference and mark
		/// to the given update values if the
		/// current reference is equal to <paramref name="expectedReference"/> 
		/// and the current mark is equal to the <paramref name="expectedMark"/>.
		/// </summary>
		/// <param name="expectedReference">
		/// The expected value of the reference
		/// </param>
		/// <param name="newReference">
		/// The new value for the reference
		/// </param>
		/// <param name="expectedMark">
		/// The expected value of the mark
		/// </param>
		/// <param name="newMark">
		/// The new value for the mark
		/// </param>
		/// <returns> 
		/// <see lang="true"/> if successful, <see lang="false"/> otherwise
		/// </returns>
		public bool CompareAndSet(object expectedReference, object newReference, bool expectedMark, bool newMark)
		{
			ReferenceBooleanPair current = Pair;
			// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
			return expectedReference.Equals(current.ObjectReference) && expectedMark == current.MarkBit && ((newReference.Equals(current.ObjectReference) && newMark == current.MarkBit) || _atomicReference.CompareAndSet(current, new ReferenceBooleanPair(newReference, newMark)));
		}

		/// <summary> 
		/// Unconditionally sets the value of both the reference and mark.
		/// </summary>
		/// <param name="newReference">the new value for the reference
		/// </param>
		/// <param name="newMark">the new value for the mark
		/// </param>
		public void SetNewAtomicValue(object newReference, bool newMark)
		{
			ReferenceBooleanPair current = Pair;
			if (newReference != current.ObjectReference || newMark != current.MarkBit)
				_atomicReference.SetNewAtomicValue(new ReferenceBooleanPair(newReference, newMark));
		}

		/// <summary> 
		/// Atomically sets the value of the mark to the given update value
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
		/// <param name="newMark">
		/// The new value for the mark
		/// </param>
		/// <returns> 
		/// <see lang="true"/> if successful, <see lang="false"/> otherwise
		/// </returns>
		public bool AttemptMark(object expectedReference, bool newMark)
		{
			ReferenceBooleanPair current = Pair;
			// TODO: This is crap.  Need to figure out why =='s doesn't work here.  It should.
			return expectedReference.Equals(current.ObjectReference) && (newMark == current.MarkBit || _atomicReference.CompareAndSet(current, new ReferenceBooleanPair(expectedReference, newMark)));
		}
	}
}