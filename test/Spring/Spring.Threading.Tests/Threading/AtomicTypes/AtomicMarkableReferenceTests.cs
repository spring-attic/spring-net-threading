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
	public class AtomicMarkableReferenceTests : BaseThreadingTestCase
	{
	    [Test]
		public void DefaultConstructor()
		{
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
			Assert.AreEqual(one, ai.Value);
			Assert.IsFalse(ai.IsMarked);
            AtomicMarkableReference<object> a2 = new AtomicMarkableReference<object>(null, true);
			Assert.IsNull(a2.Value);
			Assert.IsTrue(a2.IsMarked);
		}

		[Test]
		public void GetSet()
		{
			bool mark;
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
			Assert.AreEqual(one, ai.Value);
			Assert.IsFalse(ai.IsMarked);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.IsFalse(mark);
			ai.SetNewAtomicValue(two, false);
			Assert.AreEqual(two, ai.Value);
			Assert.IsFalse(ai.IsMarked);
			Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.IsFalse(mark);
			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.Value);
			Assert.IsTrue(ai.IsMarked);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.IsTrue(mark);

			ai.SetNewAtomicValue(one, true);
			Assert.AreEqual(one, ai.Value);
			Assert.IsTrue(ai.IsMarked);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.IsTrue(mark);
		}

		[Test]
		public void AttemptMark()
		{
			bool mark;
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
			Assert.IsFalse(ai.IsMarked, "Reference is marked.");
			Assert.IsTrue(ai.AttemptMark(one, true), "Reference was not marked");
			Assert.IsTrue(ai.IsMarked, "Reference is not marked.");
			Assert.AreEqual(one, ai.GetValue(out mark), "Reference does not equal.");
			Assert.IsTrue(mark, "Mark returned is false");
		}

		[Test]
		public void CompareAndSet()
		{
			bool mark;
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.IsFalse(ai.IsMarked);
			Assert.IsFalse(mark);

			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.IsFalse(mark);

			Assert.IsTrue(ai.CompareAndSet(two, m3, false, true));
			Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.IsTrue(mark);

			Assert.IsFalse(ai.CompareAndSet(two, m3, true, true));
			Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.IsTrue(mark);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
            Thread t = new Thread(delegate()
            {
                while (!ai.CompareAndSet(two, three, false, false))
                    Thread.Sleep(SHORT_DELAY);
            });
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, false, false));
			t.Join(LONG_DELAY);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Value, three);
			Assert.IsFalse(ai.IsMarked);
		}

		[Test]
		public void CompareAndSetInMultipleThreadsChangedMarkBits()
		{
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
            Thread t = new Thread(delegate()
            {
                while (!ai.CompareAndSet(one, one, true, false))
                    Thread.Sleep(SHORT_DELAY);
            });
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, false, true));
			t.Join(LONG_DELAY);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Value, one);
			Assert.IsFalse(ai.IsMarked);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			bool mark;
            AtomicMarkableReference<Integer> ai = new AtomicMarkableReference<Integer>(one, false);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.IsFalse(ai.IsMarked);
			Assert.IsFalse(mark);

			while (!ai.WeakCompareAndSet(one, two, false, false)){}
			Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.IsFalse(mark);

			while (!ai.WeakCompareAndSet(two, m3, false, true)) {}
			Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.IsTrue(mark);
		}
		[Test]
			public void SerializeAndDeseralize()
		{
            AtomicMarkableReference<Integer> atomicMarkableReference = new AtomicMarkableReference<Integer>(one, true);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicMarkableReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicMarkableReference<Integer> atomicMarkableReference2 = (AtomicMarkableReference<Integer>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicMarkableReference.Value, atomicMarkableReference2.Value);
			Assert.IsTrue(atomicMarkableReference2.IsMarked);
		}
	}
}