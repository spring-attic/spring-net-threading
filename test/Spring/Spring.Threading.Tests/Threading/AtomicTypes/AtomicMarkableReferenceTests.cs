using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
	[TestFixture]
	public class AtomicMarkableReferenceTests : BaseThreadingTestCase
	{
		private class AnonymousClassRunnable
		{
			private AtomicMarkableReference ai;

			public AnonymousClassRunnable(AtomicMarkableReference ai)
			{
				this.ai = ai;
			}

			public void Run()
			{
				while (!ai.CompareAndSet(two, three, false, false))
					Thread.Sleep(SHORT_DELAY_MS);
			}
		}

		private class AnonymousClassRunnable1
		{
			private AtomicMarkableReference ai;

			public AnonymousClassRunnable1(AtomicMarkableReference ai)
			{
				this.ai = ai;
			}

			public void Run()
			{
				while (!ai.CompareAndSet(one, one, true, false))
					Thread.Sleep(SHORT_DELAY_MS);
			}
		}

		[Test]
		public void DefaultConstructor()
		{
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.IsFalse(ai.IsReferenceMarked);
			AtomicMarkableReference a2 = new AtomicMarkableReference(null, true);
			Assert.IsNull(a2.ObjectReference);
			Assert.IsTrue(a2.IsReferenceMarked);
		}

		[Test]
		public void GetSet()
		{
			bool[] mark = new bool[1];
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetObjectReference(ref mark));
			Assert.IsFalse(mark[0]);
			ai.SetNewAtomicValue(two, false);
			Assert.AreEqual(two, ai.ObjectReference);
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.AreEqual(two, ai.GetObjectReference(ref mark));
			Assert.IsFalse(mark[0]);
			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.IsTrue(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetObjectReference(ref mark));
			Assert.IsTrue(mark[0]);

			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.IsTrue(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetObjectReference(ref mark));
			Assert.IsTrue(mark[0]);
		}

		[Test]
		public void AttemptMark()
		{
			bool[] mark = new bool[1];
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Assert.IsFalse(ai.IsReferenceMarked, "Reference is marked.");
			Assert.IsTrue(ai.AttemptMark(one, true), "Reference was not marked");
			Assert.IsTrue(ai.IsReferenceMarked, "Reference is not marked.");
			Assert.AreEqual(one, ai.GetObjectReference(ref mark), "Reference does not equal.");
			Assert.IsTrue(mark[0], "Mark returned is false");
		}

		[Test]
		public void CompareAndSet()
		{
			bool[] mark = new bool[1];
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Assert.AreEqual(one, ai.GetObjectReference(ref mark));
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.IsFalse(mark[0]);

			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			Assert.AreEqual(two, ai.GetObjectReference(ref mark));
			Assert.IsFalse(mark[0]);

			Assert.IsTrue(ai.CompareAndSet(two, m3, false, true));
			Assert.AreEqual(m3, ai.GetObjectReference(ref mark));
			Assert.IsTrue(mark[0]);

			Assert.IsFalse(ai.CompareAndSet(two, m3, true, true));
			Assert.AreEqual(m3, ai.GetObjectReference(ref mark));
			Assert.IsTrue(mark[0]);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.ObjectReference, three);
			Assert.IsFalse(ai.IsReferenceMarked);
		}

		[Test]
		public void CompareAndSetInMultipleThreadsChangedMarkBits()
		{
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, false, true));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.ObjectReference, one);
			Assert.IsFalse(ai.IsReferenceMarked);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			bool[] mark = new bool[1];
			AtomicMarkableReference ai = new AtomicMarkableReference(one, false);
			Assert.AreEqual(one, ai.GetObjectReference(ref mark));
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.IsFalse(mark[0]);

			while (!ai.WeakCompareAndSet(one, two, false, false))
				;
			Assert.AreEqual(two, ai.GetObjectReference(ref mark));
			Assert.IsFalse(mark[0]);

			while (!ai.WeakCompareAndSet(two, m3, false, true))
				;
			Assert.AreEqual(m3, ai.GetObjectReference(ref mark));
			Assert.IsTrue(mark[0]);
		}
		[Test]
			public void SerializeAndDeseralize()
		{
			AtomicMarkableReference atomicMarkableReference = new AtomicMarkableReference(one, true);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicMarkableReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
			AtomicMarkableReference atomicMarkableReference2 = (AtomicMarkableReference) formatter2.Deserialize(bin);

			Assert.AreEqual(atomicMarkableReference.ObjectReference, atomicMarkableReference2.ObjectReference);
			Assert.IsTrue(atomicMarkableReference2.IsReferenceMarked);
		}
	}
}