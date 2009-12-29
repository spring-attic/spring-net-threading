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
using NUnit.CommonFixtures;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(object))]
	public class AtomicReferenceArrayTests<T> : ThreadingTestFixture<T>
        where T : class 
	{
	    [Test]
        public void ConstructAtomicIntegerArryWithGivenSize()
		{
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.IsNull(ai[i]);
			}
		}

		[ExpectedException(typeof (ArgumentNullException))]
		[Test]
        public void ConstructorChokesOnNullArgument()
		{
			T[] a = null;
			new AtomicReferenceArray<T>(a);
		}

		[Test]
        public void ConstructFromExistingArray()
		{
			T[] a = new T[] {two, one, three, four, seven};
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(a);
			Assert.AreEqual(a.Length, ai.Count);
			for (int i = 0; i < a.Length; ++i)
				Assert.AreEqual(a[i], ai[i]);
		}


		[Test]
        public void IndexerChokesOnOutOfRangeIndex()
		{
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
		    T a = null;
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[DEFAULT_COLLECTION_SIZE]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[-1]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(DEFAULT_COLLECTION_SIZE, zero); });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(-1, zero); });
            Assert.IsNull(a);
		}

		[Test]
		public void GetReturnsLastValueSetAtIndex()
		{
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicReferenceArray<T> a = new AtomicReferenceArray<T>(1);
            a.Exchange(0, one);
            Thread t = ThreadManager.StartAndAssertRegistered(
                "T1", () => { while (!a.CompareAndSet(0, two, three)) Thread.Sleep(Delays.Short); });
			Assert.IsTrue(a.CompareAndSet(0, one, two));
			ThreadManager.JoinAndVerify(Delays.Long);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(a[0], three);
		}

		[Test]
		public void WeakCompareAndSet()
		{
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                ai.Exchange(i, one);
                Assert.AreEqual(one, ai.Exchange(i, zero));
                Assert.AreEqual(zero, ai.Exchange(i, m10));
                Assert.AreEqual(m10, ai.Exchange(i, one));
			}
		}

		[Test]
		public void SerializeAndDeserialize()
		{
            AtomicReferenceArray<T> atomicReferenceArray = new AtomicReferenceArray<T>(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
                atomicReferenceArray.Exchange(i, TestData<T>.MakeData(-i));
			}
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicReferenceArray);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicReferenceArray<T> r = (AtomicReferenceArray<T>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicReferenceArray.Count, r.Count);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(r[i], atomicReferenceArray[i]);
			}

		}

		[Test]
		public void ToStringTest()
		{
			T[] a = new T[] {two, one, three, four, seven};
            AtomicReferenceArray<T> ai = new AtomicReferenceArray<T>(a);
			Assert.AreEqual(ConvertArrayToString(a), ai.ToString());
            Assert.AreEqual("[]", new AtomicReferenceArray<T>(0).ToString());
        }

		private static string ConvertArrayToString(T[] array)
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