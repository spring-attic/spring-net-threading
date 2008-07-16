using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
	[TestFixture]
	public class AtomicStampedReferenceTests : BaseThreadingTestCase
	{
		private class AnonymousClassChangingReference
		{
			private AtomicStampedReference ai;

			public AnonymousClassChangingReference(AtomicStampedReference ai)
			{
				this.ai = ai;
			}

			public void Run()
			{
				while (!ai.CompareAndSet(two, three, 0, 0))
					Thread.Sleep(0);
			}
		}

		private class AnonymousClassChangingStamp
		{
			private AtomicStampedReference ai;

			public AnonymousClassChangingStamp(AtomicStampedReference ai)
			{
				this.ai = ai;
			}

			public void Run()
			{
				while (!ai.CompareAndSet(one, one, 1, 2))
					Thread.Sleep(0);
			}
		}

		[Test]
		public void Constructor()
		{
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.AreEqual(0, ai.Stamp);
			AtomicStampedReference a2 = new AtomicStampedReference(null, 1);
			Assert.IsNull(a2.ObjectReference);
			Assert.AreEqual(1, a2.Stamp);
		}

		[Test]
		public void GetSet()
		{
			int[] mark = new int[1];
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);
			ai.SetNewAtomicValue(two, 0);
			Assert.AreEqual(two, ai.ObjectReference);
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(two, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);
			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);

			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.ObjectReference);
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);
		}

		[Test]
		public void AttemptStamp()
		{
			int[] mark = new int[1];
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Assert.AreEqual(0, ai.Stamp);
			Assert.IsTrue(ai.AttemptStamp(one, 1));
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);
		}

		[Test]
		public void CompareAndSet()
		{
			int[] mark = new int[1];
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(0, mark[0]);

			Assert.IsTrue(ai.CompareAndSet(one, two, 0, 0));
			Assert.AreEqual(two, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);

			Assert.IsTrue(ai.CompareAndSet(two, m3, 0, 1));
			Assert.AreEqual(m3, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);

			Assert.IsFalse(ai.CompareAndSet(two, m3, 1, 1));
			Assert.AreEqual(m3, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Thread t = new Thread(new ThreadStart(new AnonymousClassChangingReference(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, 0, 0));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.ObjectReference, three);
			Assert.AreEqual(ai.Stamp, 0);
		}

		[Test]
		public void CompareAndSetInMultipleThreads2()
		{
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Thread t = new Thread(new ThreadStart(new AnonymousClassChangingStamp(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, 0, 1));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.ObjectReference, one);
			Assert.AreEqual(ai.Stamp, 2);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			int[] mark = new int[1];
			AtomicStampedReference ai = new AtomicStampedReference(one, 0);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(0, mark[0]);

			while (!ai.WeakCompareAndSet(one, two, 0, 0))
				;
			Assert.AreEqual(two, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);

			while (!ai.WeakCompareAndSet(two, m3, 0, 1))
				;
			Assert.AreEqual(m3, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);
		}
		[Test]
		public void SerializeAndDeseralize()
		{
			AtomicStampedReference  atomicStampedReference = new AtomicStampedReference(one, 0987654321);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicStampedReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
			AtomicStampedReference atomicStampedReference2 = (AtomicStampedReference) formatter2.Deserialize(bin);

			Assert.AreEqual(atomicStampedReference.ObjectReference, atomicStampedReference2.ObjectReference);
			Assert.AreEqual( atomicStampedReference.Stamp, atomicStampedReference2.Stamp);
		}
	}
}