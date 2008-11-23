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

namespace Spring.Threading.AtomicTypes {
    /// <summary>
    /// Unit tests for the AtomicBoolean class
    /// </summary>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class AtomicBooleanTest : BaseThreadingTestCase {
        private class AnonymousClassRunnable {
            private readonly AtomicBoolean _atomicBoolean;

            public AnonymousClassRunnable(AtomicBoolean ai) {
                _atomicBoolean = ai;
            }

            public void Run() {
                while(!_atomicBoolean.CompareAndSet(false, true)) {
                    Thread.Sleep(SHORT_DELAY_MS);
                }
            }
        }

        [Test]
        public void Constructor() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Assert.AreEqual(true, ai.BooleanValue);
        }

        [Test]
        public void DefaultConstructor() {
            AtomicBoolean ai = new AtomicBoolean();
            Assert.AreEqual(false, ai.BooleanValue);
        }

        [Test]
        public void GetLastSetValue() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Assert.AreEqual(true, ai.BooleanValue);
            ai.BooleanValue = false;
            Assert.AreEqual(false, ai.BooleanValue);
            ai.BooleanValue = true;
            Assert.AreEqual(true, ai.BooleanValue);
        }

        [Test]
        public void GetLastLazySetValue() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Assert.AreEqual(true, ai.BooleanValue);
            ai.LazySet(false);
            Assert.AreEqual(false, ai.BooleanValue);
            ai.LazySet(true);
            Assert.AreEqual(true, ai.BooleanValue);
        }

        [Test]
        public void CompareExpectedValueAndSetNewValue() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Assert.AreEqual(true, ai.BooleanValue);
            Assert.IsTrue(ai.CompareAndSet(true, false));
            Assert.AreEqual(false, ai.BooleanValue);
            Assert.IsTrue(ai.CompareAndSet(false, false));
            Assert.AreEqual(false, ai.BooleanValue);
            Assert.IsFalse(ai.CompareAndSet(true, false));
            Assert.IsFalse((ai.BooleanValue));
            Assert.IsTrue(ai.CompareAndSet(false, true));
            Assert.AreEqual(true, ai.BooleanValue);
        }

        [Test]
        public void CompareExpectedValueAndSetNewValueInMultipleThreads() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));

            t.Start();
            Assert.IsTrue(ai.CompareAndSet(true, false), "Value");
            t.Join(SMALL_DELAY_MS);
            Assert.IsFalse(t.IsAlive, "Thread is still alive.");
            Assert.IsTrue(ai.BooleanValue);
        }

        [Test]
        public void WeakCompareExpectedValueAndSetNewValue() {
            AtomicBoolean ai = new AtomicBoolean(true);
            while(!ai.WeakCompareAndSet(true, false)) {
                ;
            }
            Assert.AreEqual(false, ai.BooleanValue);
            while(!ai.WeakCompareAndSet(false, false)) {
                ;
            }
            Assert.AreEqual(false, ai.BooleanValue);
            while(!ai.WeakCompareAndSet(false, true)) {
                ;
            }
            Assert.AreEqual(true, ai.BooleanValue);
            Assert.IsFalse(ai.WeakCompareAndSet(false, true));
        }

        [Test]
        public void GetOldValueAndSetNewValue() {
            AtomicBoolean ai = new AtomicBoolean(true);
            Assert.AreEqual(true, ai.SetNewAtomicValue(false));
            Assert.AreEqual(false, ai.SetNewAtomicValue(false));
            Assert.AreEqual(false, ai.SetNewAtomicValue(true));
            Assert.AreEqual(true, ai.BooleanValue);
        }

        [Test]
        public void SerializationOfAtomicBooleanValue() {
            AtomicBoolean atomicBoolean = new AtomicBoolean();

            atomicBoolean.BooleanValue = true;
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, atomicBoolean);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicBoolean r = (AtomicBoolean)formatter2.Deserialize(bin);
            Assert.AreEqual(atomicBoolean.BooleanValue, r.BooleanValue);
        }

        [Test]
        public void ToStringRepresentation() {
            AtomicBoolean ai = new AtomicBoolean();
            Assert.AreEqual(ai.ToString(), Boolean.FalseString);
            ai.BooleanValue = true;
            Assert.AreEqual(ai.ToString(), Boolean.TrueString);
        }
    }
}