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
            private AtomicMarkableReference<int> ai;

            public AnonymousClassRunnable(AtomicMarkableReference<int> ai)
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
            private AtomicMarkableReference<int> ai;

            public AnonymousClassRunnable1(AtomicMarkableReference<int> ai)
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
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Assert.AreEqual(one, ai.Reference);
			Assert.IsFalse(ai.IsReferenceMarked);
            AtomicMarkableReference<object> a2 = new AtomicMarkableReference<object>(null, true);
			Assert.IsNull(a2.Reference);
			Assert.IsTrue(a2.IsReferenceMarked);
		}

		[Test]
		public void GetSet()
		{
			bool[] mark = new bool[1];
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Assert.AreEqual(one, ai.Reference);
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetReference(ref mark));
			Assert.IsFalse(mark[0]);
			ai.SetNewAtomicValue(two, false);
			Assert.AreEqual(two, ai.Reference);
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.AreEqual(two, ai.GetReference(ref mark));
			Assert.IsFalse(mark[0]);
			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.Reference);
			Assert.IsTrue(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetReference(ref mark));
			Assert.IsTrue(mark[0]);

			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.Reference);
			Assert.IsTrue(ai.IsReferenceMarked);
			Assert.AreEqual(one, ai.GetReference(ref mark));
			Assert.IsTrue(mark[0]);
		}

		[Test]
		public void AttemptMark()
		{
			bool[] mark = new bool[1];
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Assert.IsFalse(ai.IsReferenceMarked, "Reference is marked.");
			Assert.IsTrue(ai.AttemptMark(one, true), "Reference was not marked");
			Assert.IsTrue(ai.IsReferenceMarked, "Reference is not marked.");
			Assert.AreEqual(one, ai.GetReference(ref mark), "Reference does not equal.");
			Assert.IsTrue(mark[0], "Mark returned is false");
		}

		[Test]
		public void CompareAndSet()
		{
			bool[] mark = new bool[1];
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Assert.AreEqual(one, ai.GetReference(ref mark));
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.IsFalse(mark[0]);

			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			Assert.AreEqual(two, ai.GetReference(ref mark));
			Assert.IsFalse(mark[0]);

			Assert.IsTrue(ai.CompareAndSet(two, m3, false, true));
			Assert.AreEqual(m3, ai.GetReference(ref mark));
			Assert.IsTrue(mark[0]);

			Assert.IsFalse(ai.CompareAndSet(two, m3, true, true));
			Assert.AreEqual(m3, ai.GetReference(ref mark));
			Assert.IsTrue(mark[0]);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Reference, three);
			Assert.IsFalse(ai.IsReferenceMarked);
		}

		[Test]
		public void CompareAndSetInMultipleThreadsChangedMarkBits()
		{
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable1(ai).Run));
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, false, true));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Reference, one);
			Assert.IsFalse(ai.IsReferenceMarked);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			bool[] mark = new bool[1];
            AtomicMarkableReference<int> ai = new AtomicMarkableReference<int>(one, false);
			Assert.AreEqual(one, ai.GetReference(ref mark));
			Assert.IsFalse(ai.IsReferenceMarked);
			Assert.IsFalse(mark[0]);

			while (!ai.WeakCompareAndSet(one, two, false, false))
				;
			Assert.AreEqual(two, ai.GetReference(ref mark));
			Assert.IsFalse(mark[0]);

			while (!ai.WeakCompareAndSet(two, m3, false, true))
				;
			Assert.AreEqual(m3, ai.GetReference(ref mark));
			Assert.IsTrue(mark[0]);
		}
		[Test]
			public void SerializeAndDeseralize()
		{
            AtomicMarkableReference<int> atomicMarkableReference = new AtomicMarkableReference<int>(one, true);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicMarkableReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicMarkableReference<int> atomicMarkableReference2 = (AtomicMarkableReference<int>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicMarkableReference.Reference, atomicMarkableReference2.Reference);
			Assert.IsTrue(atomicMarkableReference2.IsReferenceMarked);
		}
	}
}