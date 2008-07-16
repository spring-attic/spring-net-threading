using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
	[TestFixture]
	public class AtomicReferenceTests : BaseThreadingTestCase
	{
		private class AnonymousClassRunnable
		{
			private AtomicReference _atomicReference;

			public AnonymousClassRunnable(AtomicReference ai)
			{
				this._atomicReference = ai;
			}

			public void Run()
			{
				while (!_atomicReference.CompareAndSet(two, three))
					Thread.Sleep(SHORT_DELAY_MS);
			}
		}

		[Test]
		public void Constructor()
		{
			AtomicReference ai = new AtomicReference(one);
			Assert.AreEqual(one, ai.ObjectReference);
		}

		[Test]
		public void Constructor2()
		{
			AtomicReference ai = new AtomicReference();
			Assert.IsNull(ai.ObjectReference);
		}

		[Test]
		public void GetSet()
		{
			AtomicReference ai = new AtomicReference(one);
			Assert.AreEqual(one, ai.ObjectReference);
			ai.ObjectReference = two;
			Assert.AreEqual(two, ai.ObjectReference);
			ai.ObjectReference = m3;
			Assert.AreEqual(m3, ai.ObjectReference);
		}

		[Test]
		public void GetLazySet()
		{
			AtomicReference ai = new AtomicReference(one);
			Assert.AreEqual(one, ai.ObjectReference);
			ai.LazySet(two);
			Assert.AreEqual(two, ai.ObjectReference);
			ai.LazySet(m3);
			Assert.AreEqual(m3, ai.ObjectReference);
		}

		[Test]
		public void CompareAndSet()
		{
			AtomicReference ai = new AtomicReference(one);
			//CS1718 Assert.IsTrue(one == one);
			Assert.IsTrue(ai.CompareAndSet(one, two), "Object reference comparison 1");
			Assert.IsTrue(ai.CompareAndSet(two, m4), "Object reference comparison 2");
			Assert.AreEqual(m4, ai.ObjectReference);
			Assert.IsFalse(ai.CompareAndSet(m5, seven), "Object reference comparison 3");
			Assert.IsFalse((seven.Equals(ai.ObjectReference)));
			Assert.IsTrue(ai.CompareAndSet(m4, seven));
			Assert.AreEqual(seven, ai.ObjectReference);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
			AtomicReference ai = new AtomicReference(one);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two), "Reference did not equal 'one' reference");
			t.Join(SMALL_DELAY_MS);
			Assert.IsFalse(t.IsAlive, "Thread is still alive");
			Assert.AreEqual(ai.ObjectReference, three, "Object reference not switched from 'two' to 'three'");
		}

		[Test]
		public void WeakCompareAndSet()
		{
			AtomicReference ai = new AtomicReference( one);
			while (!ai.WeakCompareAndSet( one,  two))
				;
			while (!ai.WeakCompareAndSet( two,  m4))
				;
			Assert.AreEqual( m4, ai.ObjectReference);
			while (!ai.WeakCompareAndSet( m4,  seven))
				;
			Assert.AreEqual( seven, ai.ObjectReference);
			Assert.IsFalse(ai.WeakCompareAndSet(m4, seven));

		}

		[Test]
		public void GetAndSet()
		{
			AtomicReference ai = new AtomicReference(one);
			Assert.AreEqual(one, ai.SetNewAtomicValue(zero));
			Assert.AreEqual(zero, ai.SetNewAtomicValue(m10));
			Assert.AreEqual(m10, ai.SetNewAtomicValue(one));
		}

		[Test]
		public void Serialization()
		{
			AtomicReference l = new AtomicReference();

			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, l);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
			AtomicReference r = (AtomicReference) formatter2.Deserialize(bin);
			Assert.AreEqual(l.ObjectReference, r.ObjectReference);
		}

		[Test]
		public void ToStringRepresentation()
		{
			AtomicReference ai = new AtomicReference(one);
			Assert.AreEqual(ai.ToString(), one.ToString());
			ai.ObjectReference = two;
			Assert.AreEqual(ai.ToString(), two.ToString());
		}
	}
}