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

namespace Spring.Threading.AtomicTypes {
    [TestFixture]
    public class AtomicReferenceTests : BaseThreadingTestCase {
        private class AnonymousClassRunnable {
            private readonly AtomicReference<int> _atomicReference;

            public AnonymousClassRunnable(AtomicReference<int> ai) {
                _atomicReference = ai;
            }

            public void Run() {
                while(!_atomicReference.CompareAndSet(two, three))
                    Thread.Sleep(SHORT_DELAY_MS);
            }
        }

        [Test]
        public void Constructor() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            Assert.AreEqual(one, ai.Reference);
        }

        [Test]
        public void Constructor2() {
            AtomicReference<object> ai = new AtomicReference<object>();
            Assert.IsNull(ai.Reference);
        }

        [Test]
        public void GetSet() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            Assert.AreEqual(one, ai.Reference);
            ai.Reference = two;
            Assert.AreEqual(two, ai.Reference);
            ai.Reference = m3;
            Assert.AreEqual(m3, ai.Reference);
        }
		[Test]
		public void GetLazySet()
		{
			AtomicReference<int> ai = new AtomicReference<int>(one);
			Assert.AreEqual(one, ai.Reference);
			ai.LazySet(two);
			Assert.AreEqual(two, ai.Reference);
			ai.LazySet(m3);
			Assert.AreEqual(m3, ai.Reference);
		}

        [Test]
        public void CompareAndSet() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            //CS1718 Assert.IsTrue(one == one);
            Assert.IsTrue(ai.CompareAndSet(one, two), "Object reference comparison 1");
            Assert.IsTrue(ai.CompareAndSet(two, m4), "Object reference comparison 2");
            Assert.AreEqual(m4, ai.Reference);
            Assert.IsFalse(ai.CompareAndSet(m5, seven), "Object reference comparison 3");
            Assert.IsFalse((seven.Equals(ai.Reference)));
            Assert.IsTrue(ai.CompareAndSet(m4, seven));
            Assert.AreEqual(seven, ai.Reference);
        }

        [Test]
        public void CompareAndSetWithNullReference() {
            AtomicReference<string> sar = new AtomicReference<string>();
            string expected = "test";
            Assert.IsTrue(sar.CompareAndSet(null, expected));
            Assert.IsTrue(sar.Reference.Equals(expected));
        }

        [Test]
        public void CompareAndSetInMultipleThreads() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));
            t.Start();
            Assert.IsTrue(ai.CompareAndSet(one, two), "Reference did not equal 'one' reference");
            t.Join(SMALL_DELAY_MS);
            Assert.IsFalse(t.IsAlive, "Thread is still alive");
            Assert.AreEqual(ai.Reference, three, "Object reference not switched from 'two' to 'three'");
        }

        [Test]
        public void WeakCompareAndSet() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            while(!ai.WeakCompareAndSet(one, two))
                ;
            while(!ai.WeakCompareAndSet(two, m4))
                ;
            Assert.AreEqual(m4, ai.Reference);
            while(!ai.WeakCompareAndSet(m4, seven))
                ;
            Assert.AreEqual(seven, ai.Reference);
            Assert.IsFalse(ai.WeakCompareAndSet(m4, seven));

        }

        [Test]
        public void GetAndSet() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            Assert.AreEqual(one, ai.SetNewAtomicValue(zero));
            Assert.AreEqual(zero, ai.SetNewAtomicValue(m10));
            Assert.AreEqual(m10, ai.SetNewAtomicValue(one));
        }

        [Test]
        public void Serialization() {
            AtomicReference<int> l = new AtomicReference<int>();

            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicReference<int> r = (AtomicReference<int>)formatter2.Deserialize(bin);
            Assert.AreEqual(l.Reference, r.Reference);
        }

        [Test]
        public void ToStringRepresentation() {
            AtomicReference<int> ai = new AtomicReference<int>(one);
            Assert.AreEqual(ai.ToString(), one.ToString());
            ai.Reference = two;
            Assert.AreEqual(ai.ToString(), two.ToString());
        }
    }
}