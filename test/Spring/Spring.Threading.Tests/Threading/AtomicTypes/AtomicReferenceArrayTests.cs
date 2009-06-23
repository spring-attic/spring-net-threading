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
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
	[TestFixture]
	public class AtomicReferenceArrayTests : BaseThreadingTestCase
	{
	    [Test]
        public void ConstructAtomicIntegerArryWithGivenSize()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsNull(ai[i]);
			}
		}

		[ExpectedException(typeof (ArgumentNullException))]
		[Test]
        public void ConstructorChokesOnNullArgument()
		{
			object[] a = null;
			new AtomicReferenceArray<object>(a);
		}

		[Test]
        public void ConstructFromExistingArray()
		{
			object[] a = new object[] {two, one, three, four, seven};
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(a);
			Assert.AreEqual(a.Length, ai.Count);
			for (int i = 0; i < a.Length; ++i)
				Assert.AreEqual(a[i], ai[i]);
		}


		[Test]
        public void IndexerChokesOnOutOfRangeIndex()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
		    object a = null;
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[DEFAULT_COLLECTION_SIZE]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[-1]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(DEFAULT_COLLECTION_SIZE, 0); });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(-1, 0); });
            Assert.IsNull(a);
		}

		[Test]
		public void GetReturnsLastValueSetAtIndex()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                ai[i] = one;
				Assert.AreEqual(one, ai[i]);
                ai[i] = two;
				Assert.AreEqual(two, ai[i]);
                ai[i] = m3;
				Assert.AreEqual(m3, ai[i]);
			}
		}

        [Test]
		public void GetReturnsLastValueLazySetAtIndex()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai.LazySet(i, one);
				Assert.AreEqual(one, ai[i]);
				ai.LazySet(i, two);
				Assert.AreEqual(two, ai[i]);
				ai.LazySet(i, m3);
				Assert.AreEqual(m3, ai[i]);
			}
		}

		[Test]
		public void CompareExistingValueAndSetNewValue()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                ai.Exchange(i, one);
				Assert.IsTrue(ai.CompareAndSet(i, one, two));
				Assert.IsTrue(ai.CompareAndSet(i, two, m4));
				Assert.AreEqual(m4, ai[i]);
				Assert.IsFalse(ai.CompareAndSet(i, m5, seven));
				Assert.IsFalse((seven.Equals(ai[i])));
				Assert.IsTrue(ai.CompareAndSet(i, m4, seven));
				Assert.AreEqual(seven, ai[i]);
			}
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
            AtomicReferenceArray<object> a = new AtomicReferenceArray<object>(1);
            a.Exchange(0, one);
            Thread t = new Thread(delegate()
            {
                while (!a.CompareAndSet(0, two, three))
                    Thread.Sleep(SHORT_DELAY);
            });

			t.Start();
			Assert.IsTrue(a.CompareAndSet(0, one, two));
			t.Join(LONG_DELAY);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(a[0], three);
		}

		[Test]
		public void WeakCompareAndSet()
		{
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                ai.Exchange(i, one);
				while (!ai.WeakCompareAndSet(i, one, two)) {}
				while (!ai.WeakCompareAndSet(i, two, m4)) {}
				Assert.AreEqual(m4, ai[i]);
				while (!ai.WeakCompareAndSet(i, m4, seven)) {}
				Assert.AreEqual(seven, ai[i]);
			}
		}

		[Test]
        public void Exchange()
		{
            AtomicReferenceArray<Integer> ai = new AtomicReferenceArray<Integer>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                ai.Exchange(i, one);
                Assert.AreEqual(one, ai.Exchange(i, zero));
                Assert.AreEqual(0, ai.Exchange(i, m10));
                Assert.AreEqual(m10, ai.Exchange(i, one));
			}
		}

		[Test]
		public void SerializeAndDeserialize()
		{
            AtomicReferenceArray<object> atomicReferenceArray = new AtomicReferenceArray<object>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                atomicReferenceArray.Exchange(i, -i);
			}
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicReferenceArray);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicReferenceArray<object> r = (AtomicReferenceArray<object>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicReferenceArray.Count, r.Count);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(r[i], atomicReferenceArray[i]);
			}

		}

		[Test]
		public void ToStringTest()
		{
			object[] a = new object[] {two, one, three, four, seven};
            AtomicReferenceArray<object> ai = new AtomicReferenceArray<object>(a);
			Assert.AreEqual(ConvertArrayToString(a), ai.ToString());
            Assert.AreEqual("[]", new AtomicReferenceArray<object>(0).ToString());
        }

		private static string ConvertArrayToString(object[] array)
		{
			if (array.Length == 0)
				return "[]";

			StringBuilder buf = new StringBuilder();
			buf.Append('[');
			buf.Append(array[0]);

			for (int i = 1; i < array.Length; i++)
			{
				buf.Append(", ");
				buf.Append(array[i]);
			}

			buf.Append("]");
			return buf.ToString();
		}
	}
}