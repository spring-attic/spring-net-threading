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
            private AtomicStampedReference<int> ai;

            public AnonymousClassChangingReference(AtomicStampedReference<int> ai)
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
            private AtomicStampedReference<int> ai;

            public AnonymousClassChangingStamp(AtomicStampedReference<int> ai)
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
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
			Assert.AreEqual(one, ai.Reference);
			Assert.AreEqual(0, ai.Stamp);
            AtomicStampedReference<object> a2 = new AtomicStampedReference<object>(null, 1);
			Assert.IsNull(a2.Reference);
			Assert.AreEqual(1, a2.Stamp);
		}

		[Test]
		public void GetSet()
		{
			int[] mark = new int[1];
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
			Assert.AreEqual(one, ai.Reference);
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);
			ai.SetNewAtomicValue(two, 0);
			Assert.AreEqual(two, ai.Reference);
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(two, ai.GetObjectReference(mark));
			Assert.AreEqual(0, mark[0]);
			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.Reference);
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);

			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.Reference);
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetObjectReference(mark));
			Assert.AreEqual(1, mark[0]);
		}

		[Test]
		public void AttemptStamp()
		{
			int[] mark = new int[1];
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
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
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
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
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
			Thread t = new Thread(new ThreadStart(new AnonymousClassChangingReference(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, 0, 0));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Reference, three);
			Assert.AreEqual(ai.Stamp, 0);
		}

		[Test]
		public void CompareAndSetInMultipleThreads2()
		{
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
            Thread t = new Thread(new ThreadStart(new AnonymousClassChangingStamp(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, 0, 1));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Reference, one);
			Assert.AreEqual(ai.Stamp, 2);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			int[] mark = new int[1];
            AtomicStampedReference<int> ai = new AtomicStampedReference<int>(one, 0);
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
            AtomicStampedReference<int> atomicStampedReference = new AtomicStampedReference<int>(one, 0987654321);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicStampedReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicStampedReference<int> atomicStampedReference2 = (AtomicStampedReference<int>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicStampedReference.Reference, atomicStampedReference2.Reference);
			Assert.AreEqual( atomicStampedReference.Stamp, atomicStampedReference2.Stamp);
		}
	}
}