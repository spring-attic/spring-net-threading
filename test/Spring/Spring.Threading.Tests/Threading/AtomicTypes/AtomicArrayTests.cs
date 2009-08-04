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
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AtomicArrayTests<T> : ThreadingTestFixture<T>
    {
        [Test]
        public void ConstructAtomicArryWithGivenSize()
        {
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.That(ai[i], Is.EqualTo(default(T)));
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void ConstructorChokesOnNullArgument()
        {
            T[] a = null;
            new AtomicArray<T>(a);
        }

        [Test]
        public void ConstructFromExistingArray()
        {
            T[] a = new T[] { two, one, three, four, seven };
            AtomicArray<T> ai = new AtomicArray<T>(a);
            Assert.AreEqual(a.Length, ai.Count);
            for (int i = 0; i < a.Length; ++i)
                Assert.AreEqual(a[i], ai[i]);
        }


        [Test]
        public void IndexerChokesOnOutOfRangeIndex()
        {
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
            T a = default(T);
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[DEFAULT_COLLECTION_SIZE]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { a = ai[-1]; });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(DEFAULT_COLLECTION_SIZE, zero); });
            TestHelper.AssertException<IndexOutOfRangeException>(
                delegate { ai.Exchange(-1, zero); });
            Assert.That(a, Is.EqualTo(default(T)));
        }

        [Test]
        public void GetReturnsLastValueSetAtIndex()
        {
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicArray<T> a = new AtomicArray<T>(1);
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
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                ai.Exchange(i, one);
                while (!ai.WeakCompareAndSet(i, one, two)) { }
                while (!ai.WeakCompareAndSet(i, two, m4)) { }
                Assert.AreEqual(m4, ai[i]);
                while (!ai.WeakCompareAndSet(i, m4, seven)) { }
                Assert.AreEqual(seven, ai[i]);
            }
        }

        [Test]
        public void Exchange()
        {
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
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
            AtomicArray<T> ai = new AtomicArray<T>(DEFAULT_COLLECTION_SIZE);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                ai.Exchange(i, TestData<T>.MakeData(-i));
            }
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, ai);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicArray<T> r = (AtomicArray<T>)formatter2.Deserialize(bin);

            Assert.AreEqual(ai.Count, r.Count);
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                Assert.AreEqual(r[i], ai[i]);
            }

        }

        [Test]
        public void ToStringTest()
        {
            T[] a = new T[] { two, one, three, four, seven };
            AtomicArray<T> ai = new AtomicArray<T>(a);
            Assert.AreEqual(ConvertArrayToString(a), ai.ToString());
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