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
    public class AtomicReferenceTests : BaseThreadingTestCase {
        [Test]
        public void Constructor() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            Assert.AreEqual(one, ai.Value);
        }

        [Test]
        public void Constructor2() {
            AtomicReference<object> ai = new AtomicReference<object>();
            Assert.IsNull(ai.Value);
        }

        [Test]
        public void GetSet() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            Assert.AreEqual(one, ai.Value);
            ai.Value = two;
            Assert.AreEqual(two, ai.Value);
            ai.Value = m3;
            Assert.AreEqual(m3, ai.Value);
        }
		[Test]
		public void GetLazySet()
		{
			AtomicReference<object> ai = new AtomicReference<object>(one);
			Assert.AreEqual(one, ai.Value);
			ai.LazySet(two);
			Assert.AreEqual(two, ai.Value);
			ai.LazySet(m3);
			Assert.AreEqual(m3, ai.Value);
		}

        [Test]
        public void CompareAndSet() {
            object o1 = new object(), o2 = new object(), o4 = new object(), o5 = new object(), o7 = new object();
            AtomicReference<object> ai = new AtomicReference<object>(o1);
            //CS1718 Assert.IsTrue(one == one);
            Assert.IsTrue(ai.CompareAndSet(o1, o2), "Object reference comparison 1");
            Assert.IsTrue(ai.CompareAndSet(o2, o4), "Object reference comparison 2");
            Assert.AreEqual(o4, ai.Value);
            Assert.IsFalse(ai.CompareAndSet(o5, o7), "Object reference comparison 3");
            Assert.IsFalse((o7.Equals(ai.Value)));
            Assert.IsTrue(ai.CompareAndSet(o4, o7));
            Assert.AreEqual(o7, ai.Value);
        }

        [Test]
        public void CompareAndSetWithNullReference() {
            AtomicReference<string> sar = new AtomicReference<string>();
            const string expected = "test";
            Assert.IsTrue(sar.CompareAndSet(null, expected));
            Assert.IsTrue(sar.Value.Equals(expected));
        }

        [Test]
        public void CompareAndSetInMultipleThreads() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            Thread t = new Thread(delegate()
                                      {
                                          while (!ai.CompareAndSet(two, three))
                                              Thread.Sleep(SHORT_DELAY);

                                      });
            t.Start();
            Assert.IsTrue(ai.CompareAndSet(one, two), "Value did not equal 'one' reference");
            t.Join(SMALL_DELAY);
            Assert.IsFalse(t.IsAlive, "Thread is still alive");
            Assert.AreEqual(ai.Value, three, "Object reference not switched from 'two' to 'three'");
        }

        [Test]
        public void WeakCompareAndSet() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            while(!ai.WeakCompareAndSet(one, two)){}
            while(!ai.WeakCompareAndSet(two, m4)){}
            Assert.AreEqual(m4, ai.Value);
            while(!ai.WeakCompareAndSet(m4, seven)){}
            Assert.AreEqual(seven, ai.Value);
            Assert.IsFalse(ai.WeakCompareAndSet(m4, seven));

        }

        [Test]
        public void GetAndSet() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            Assert.AreEqual(one, ai.Exchange(zero));
            Assert.AreEqual(zero, ai.Exchange(m10));
            Assert.AreEqual(m10, ai.Exchange(one));
        }

        [Test]
        public void Serialization() {
            AtomicReference<object> l = new AtomicReference<object>();

            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicReference<object> r = (AtomicReference<object>)formatter2.Deserialize(bin);
            Assert.AreEqual(l.Value, r.Value);
        }

        [Test]
        public void ToStringRepresentation() {
            AtomicReference<object> ai = new AtomicReference<object>(one);
            Assert.AreEqual(ai.ToString(), one.ToString());
            ai.Value = two;
            Assert.AreEqual(ai.ToString(), two.ToString());
        }

        [Test]
        public void ImplicitConverter()
        {
            AtomicReference<Integer> ai = new AtomicReference<Integer>(one);
            Integer result = ai;
            Assert.AreEqual(one, result);
            ai.Value = two;
            result = ai;
            Assert.AreEqual(two, result);
        }

    }
}