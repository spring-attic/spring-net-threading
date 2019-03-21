#region License

/*
 * Copyright 2002-2008 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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
using NUnit.CommonFixtures;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AtomicTests<T> : ThreadingTestFixture<T>
    {
        [Test]
        public void Constructor() {
            Atomic<T> ai = new Atomic<T>(one);
            Assert.AreEqual(one, ai.Value);
        }

        [Test]
        public void Constructor2() {
            Atomic<object> ai = new Atomic<object>();
            Assert.IsNull(ai.Value);
        }

        [Test]
        public void GetSet() {
            Atomic<T> ai = new Atomic<T>(one);
            Assert.AreEqual(one, ai.Value);
            ai.Value = two;
            Assert.AreEqual(two, ai.Value);
            ai.Value = m3;
            Assert.AreEqual(m3, ai.Value);
        }
		[Test]
		public void GetLazySet()
		{
			Atomic<T> ai = new Atomic<T>(one);
			Assert.AreEqual(one, ai.Value);
			ai.LazySet(two);
			Assert.AreEqual(two, ai.Value);
			ai.LazySet(m3);
			Assert.AreEqual(m3, ai.Value);
		}

        [Test]
        public void CompareAndSet() {
            Atomic<T> ai = new Atomic<T>(one);
            //CS1718 Assert.IsTrue(one == one);
            Assert.IsTrue(ai.CompareAndSet(one, two), "Object reference comparison 1");
            Assert.IsTrue(ai.CompareAndSet(two, m4), "Object reference comparison 2");
            Assert.AreEqual(m4, ai.Value);
            Assert.IsFalse(ai.CompareAndSet(m5, seven), "Object reference comparison 3");
            Assert.IsFalse((seven.Equals(ai.Value)));
            Assert.IsTrue(ai.CompareAndSet(m4, seven));
            Assert.AreEqual(seven, ai.Value);
        }

        [Test]
        public void CompareAndSetWithNullReference() {
            Atomic<string> sar = new Atomic<string>();
            string expected = "test";
            Assert.IsTrue(sar.CompareAndSet(null, expected));
            Assert.IsTrue(sar.Value.Equals(expected));
        }

        [Test]
        public void CompareAndSetInMultipleThreads() {
            Atomic<T> ai = new Atomic<T>(one);
            Thread t = new Thread(delegate()
            {
                while (!ai.CompareAndSet(two, three))
                    Thread.Sleep(Delays.Short);
            });
            t.Start();
            Assert.IsTrue(ai.CompareAndSet(one, two), "Value did not equal 'one' reference");
            t.Join(Delays.Small);
            Assert.IsFalse(t.IsAlive, "Thread is still alive");
            Assert.AreEqual(ai.Value, three, "Object reference not switched from 'two' to 'three'");
        }

        [Test]
        public void WeakCompareAndSet() {
            Atomic<T> ai = new Atomic<T>(one);
            while(!ai.WeakCompareAndSet(one, two)){}
            while(!ai.WeakCompareAndSet(two, m4)){}
            Assert.AreEqual(m4, ai.Value);
            while(!ai.WeakCompareAndSet(m4, seven)){}
            Assert.AreEqual(seven, ai.Value);
            Assert.IsFalse(ai.WeakCompareAndSet(m4, seven));

        }

        [Test]
        public void GetAndSet() {
            Atomic<T> ai = new Atomic<T>(one);
            Assert.AreEqual(one, ai.Exchange(zero));
            Assert.AreEqual(zero, ai.Exchange(m10));
            Assert.AreEqual(m10, ai.Exchange(one));
        }

        [Test]
        public void Serialization() {
            Atomic<T> l = new Atomic<T>();

            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            Atomic<T> r = (Atomic<T>)formatter2.Deserialize(bin);
            Assert.AreEqual(l.Value, r.Value);
        }

        [Test]
        public void ToStringRepresentation() {
            Atomic<T> ai = new Atomic<T>(one);
            Assert.AreEqual(ai.ToString(), one.ToString());
            ai.Value = two;
            Assert.AreEqual(ai.ToString(), two.ToString());
        }

        [Test]
        public void ImplicitConverter()
        {
            Atomic<T> ai = new Atomic<T>(one);
            T oneone = ai;
            Assert.AreEqual(one, oneone);
        }
    }
}