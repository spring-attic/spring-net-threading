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

namespace Spring.Threading.AtomicTypes {
    /// <summary>
    /// Unit tests for the AtomicIntegerArray class
    /// </summary>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class AtomicIntegerArrayTests : BaseThreadingTestCase {
        private class AnonymousClassRunnable {
            public AnonymousClassRunnable(AtomicIntegerArray a) {
                this.a = a;
            }

            private AtomicIntegerArray a;

            public void Run() {
                while(!a.CompareAndSet(0, 2, 3))
                    Thread.Sleep(0);
            }
        }

        internal const int COUNTDOWN = 100000;

        internal class Counter {
            internal AtomicIntegerArray ai;
            internal volatile int counts;
            private AtomicIntegerArrayTests enclosingInstance;

            private void InitBlock(AtomicIntegerArrayTests enclosingInstance) {
                this.enclosingInstance = enclosingInstance;
            }
            public AtomicIntegerArrayTests Enclosing_Instance {
                get { return enclosingInstance; }

            }
            internal Counter(AtomicIntegerArrayTests enclosingInstance, AtomicIntegerArray a) {
                InitBlock(enclosingInstance);
                ai = a;
            }

            [Test]
            public void Run() {
                for(; ; ) {
                    bool done = true;
                    for(int i = 0; i < ai.Length; ++i) {
                        int v = ai[i];
                        Assert.IsTrue(v >= 0);
                        if(v != 0) {
                            done = false;
                            if(ai.CompareAndSet(i, v, v - 1))
                                ++counts;
                        }
                    }
                    if(done)
                        break;
                }
            }
        }

        [Test]
        public void Constructor() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                Assert.AreEqual(0, ai[i]);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Constructor2NPE() {
            int[] a = null;
            new AtomicIntegerArray(a);
        }


        [Test]
        public void Constructor2() {
            int[] a = new int[] { 17, 3, -42, 99, -7 };
            AtomicIntegerArray ai = new AtomicIntegerArray(a);
            Assert.AreEqual(a.Length, ai.Length);
            for(int i = 0; i < a.Length; ++i)
                Assert.AreEqual(a[i], ai[i]);
        }


        [Test]
        public void Indexing() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            try {
                int a = ai[DEFAULT_COLLECTION_SIZE];
            }
            catch(IndexOutOfRangeException) {
            }
            try {
                int a = ai[-1];
            }
            catch(IndexOutOfRangeException) {
            }
            try {
                ai[DEFAULT_COLLECTION_SIZE] = 0;
            }
            catch(IndexOutOfRangeException) {
            }
            try {
                ai[-1] = 0;
            }
            catch(IndexOutOfRangeException) {
            }
        }


        [Test]
        public void GetSet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(1, ai[i]);
                ai[i] = 2;
                Assert.AreEqual(2, ai[i]);
                ai[i] = -3;
                Assert.AreEqual(-3, ai[i]);
            }
        }


        [Test]
        public void GetLazySet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai.LazySet(i, 1);
                Assert.AreEqual(1, ai[i]);
                ai.LazySet(i, 2);
                Assert.AreEqual(2, ai[i]);
                ai.LazySet(i, -3);
                Assert.AreEqual(-3, ai[i]);
            }
        }

        [Test]
        public void CompareAndSet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.IsTrue(ai.CompareAndSet(i, 1, 2));
                Assert.IsTrue(ai.CompareAndSet(i, 2, -4));
                Assert.AreEqual(-4, ai[i]);
                Assert.IsFalse(ai.CompareAndSet(i, -5, 7));
                Assert.IsFalse(7 == ai[i]);
                Assert.IsTrue(ai.CompareAndSet(i, -4, 7));
                Assert.AreEqual(7, ai[i]);
            }
        }


        [Test]
        public void CompareAndSetInMultipleThreads() {
            AtomicIntegerArray a = new AtomicIntegerArray(1);
            a[0] = 1;
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(a).Run));
            t.Start();
            Assert.IsTrue(a.CompareAndSet(0, 1, 2));
            t.Join(LONG_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(a[0], 3);
        }


        [Test]
        public void WeakCompareAndSet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                while(!ai.WeakCompareAndSet(i, 1, 2))
                    ;
                while(!ai.WeakCompareAndSet(i, 2, -4))
                    ;
                Assert.AreEqual(-4, ai[i]);
                while(!ai.WeakCompareAndSet(i, -4, 7))
                    ;
                Assert.AreEqual(7, ai[i]);
                Assert.IsFalse(ai.WeakCompareAndSet(i, -4, 7));
            }
        }


        [Test]
        public void GetAndSet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(1, ai.SetNewAtomicValue(i, 0));
                Assert.AreEqual(0, ai.SetNewAtomicValue(i, -10));
                Assert.AreEqual(-10, ai.SetNewAtomicValue(i, 1));
            }
        }


        [Test]
        public void GetAndAdd() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(1, ai.AddDeltaAndReturnPreviousValue(i, 2));
                Assert.AreEqual(3, ai[i]);
                Assert.AreEqual(3, ai.AddDeltaAndReturnPreviousValue(i, -4));
                Assert.AreEqual(-1, ai[i]);
            }
        }


        [Test]
        public void GetAndDecrement() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(1, ai.ReturnValueAndDecrement(i));
                Assert.AreEqual(0, ai.ReturnValueAndDecrement(i));
                Assert.AreEqual(-1, ai.ReturnValueAndDecrement(i));
            }
        }


        [Test]
        public void GetAndIncrement() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(1, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(2, ai[i]);
                ai[i] = -2;
                Assert.AreEqual(-2, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(-1, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(0, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(1, ai[i]);
            }
        }


        [Test]
        public void AddAndGet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(3, ai.AddDeltaAndReturnNewValue(i, 2));
                Assert.AreEqual(3, ai[i]);
                Assert.AreEqual(-1, ai.AddDeltaAndReturnNewValue(i, -4));
                Assert.AreEqual(-1, ai[i]);
            }
        }


        [Test]
        public void DecrementAndGet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(0, ai.DecrementValueAndReturn(i));
                Assert.AreEqual(-1, ai.DecrementValueAndReturn(i));
                Assert.AreEqual(-2, ai.DecrementValueAndReturn(i));
                Assert.AreEqual(-2, ai[i]);
            }
        }


        [Test]
        public void IncrementAndGet() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                ai[i] = 1;
                Assert.AreEqual(2, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(2, ai[i]);
                ai[i] = -2;
                Assert.AreEqual(-1, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(0, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(1, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(1, ai[i]);
            }
        }


        [Test]
        public void CountingInMultipleThreads() {
            AtomicIntegerArray ai = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                ai[i] = COUNTDOWN;
            Counter c1 = new Counter(this, ai);
            Counter c2 = new Counter(this, ai);
            Thread t1 = new Thread(new ThreadStart(c1.Run));
            Thread t2 = new Thread(new ThreadStart(c2.Run));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.AreEqual(c1.counts + c2.counts, DEFAULT_COLLECTION_SIZE * COUNTDOWN);
        }


        [Test]
        public void Serialization() {
            AtomicIntegerArray l = new AtomicIntegerArray(DEFAULT_COLLECTION_SIZE);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
                l[i] = -i;

            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicIntegerArray r = (AtomicIntegerArray)formatter2.Deserialize(bin);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                Assert.AreEqual(l[i], r[i]);
            }
        }


        [Test]
        public void ToStringTest() {
            int[] a = new int[] { 17, 3, -42, 99, -7 };
            AtomicIntegerArray ai = new AtomicIntegerArray(a);
            Assert.AreEqual(toString(a), ai.ToString());

            int[] b = new int[0];
            Assert.AreEqual("[]", new AtomicIntegerArray(b).ToString());
        }

        private static String toString(int[] array) {
            if(array.Length == 0)
                return "[]";

            StringBuilder buf = new StringBuilder();
            buf.Append('[');
            buf.Append(array[0]);

            for(int i = 1; i < array.Length; i++) {
                buf.Append(", ");
                buf.Append(array[i]);
            }

            buf.Append("]");
            return buf.ToString();
        }
    }
}