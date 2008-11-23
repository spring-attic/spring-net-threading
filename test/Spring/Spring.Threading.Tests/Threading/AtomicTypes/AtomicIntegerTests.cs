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
    /// Unit tests for the AtomicLong class
    /// </summary>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class AtomicIntegerTests : BaseThreadingTestCase {
        private class AnonymousClassRunnable {
            private readonly AtomicInteger _ai;

            public AnonymousClassRunnable(AtomicInteger ai) {
                _ai = ai;
            }

            public void Run() {
                while(!_ai.CompareAndSet(2, 3))
                    Thread.Sleep(100);
            }
        }

        [Test]
        public void Constructor() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(1, ai.IntegerValue);
        }

        [Test]
        public void Constructor2() {
            AtomicInteger ai = new AtomicInteger();
            Assert.AreEqual(0, ai.IntegerValue);
        }

        [Test]
        public void GetSet() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(1, ai.IntegerValue);
            ai.IntegerValue = 2;
            Assert.AreEqual(2, ai.IntegerValue);
            ai.IntegerValue = -3;
            Assert.AreEqual(-3, ai.IntegerValue);
        }

        [Test]
        public void GetLazySet() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(1, ai.IntegerValue);
            ai.LazySet(2);
            Assert.AreEqual(2, ai.IntegerValue);
            ai.LazySet(-3);
            Assert.AreEqual(-3, ai.IntegerValue);
        }

        [Test]
        public void CompareAndSet() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.IsTrue(ai.CompareAndSet(1, 2));
            Assert.IsTrue(ai.CompareAndSet(2, -4));
            Assert.AreEqual(-4, ai.IntegerValue);
            Assert.IsFalse(ai.CompareAndSet(-5, 7));
            Assert.IsFalse((7 == ai.IntegerValue));
            Assert.IsTrue(ai.CompareAndSet(-4, 7));
            Assert.AreEqual(7, ai.IntegerValue);
        }

        [Test]
        public void CompareAndSetInMultipleThreads() {
            AtomicInteger ai = new AtomicInteger(1);
            Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(ai).Run));
            t.Start();
            Assert.IsTrue(ai.CompareAndSet(1, 2));
            t.Join(LONG_DELAY_MS);
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(ai.IntegerValue, 3);
        }

        [Test]
        public void WeakCompareAndSet() {
            AtomicInteger ai = new AtomicInteger(1);
            while(!ai.WeakCompareAndSet(1, 2))
                ;
            while(!ai.WeakCompareAndSet(2, -4))
                ;
            Assert.AreEqual(-4, ai.IntegerValue);
            while(!ai.WeakCompareAndSet(-4, 7))
                ;
            Assert.AreEqual(7, ai.IntegerValue);
            Assert.IsFalse(ai.WeakCompareAndSet(-4, 7));
        }

        [Test]
        public void GetAndSet() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(1, ai.SetNewAtomicValue(0));
            Assert.AreEqual(0, ai.SetNewAtomicValue(-10));
            Assert.AreEqual(-10, ai.SetNewAtomicValue(1));
        }

        [Test]
        public void GetAndAdd() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(1, ai.AddDeltaAndReturnPreviousValue(2));
            Assert.AreEqual(3, ai.IntegerValue);
            Assert.AreEqual(3, ai.AddDeltaAndReturnPreviousValue(-4));
            Assert.AreEqual(-1, ai.IntegerValue);
        }

		[Test] public void GetReturnValueAndDecrement()
		{
			AtomicInteger ai = new AtomicInteger(1);
			Assert.AreEqual(1, ai.ReturnValueAndDecrement());
			Assert.AreEqual(0, ai.ReturnValueAndDecrement());
			Assert.AreEqual(- 1, ai.ReturnValueAndDecrement());
		}

		[Test] public void GetReturnValueAndIncrement()
		{
			AtomicInteger ai = new AtomicInteger(1);
			Assert.AreEqual(1, ai.ReturnValueAndIncrement());
			Assert.AreEqual(2, ai.IntegerValue);
			ai.IntegerValue = - 2;
			Assert.AreEqual(- 2, ai.ReturnValueAndIncrement());
			Assert.AreEqual(- 1, ai.ReturnValueAndIncrement());
			Assert.AreEqual(0, ai.ReturnValueAndIncrement());
			Assert.AreEqual(1, ai.IntegerValue);
		}

		[Test] public void AddAndGet()
		{
			AtomicInteger ai = new AtomicInteger(1);
			Assert.AreEqual(3, ai.AddDeltaAndReturnNewValue(2));
			Assert.AreEqual(3, ai.IntegerValue);
			Assert.AreEqual(- 1, ai.AddDeltaAndReturnNewValue(- 4));
			Assert.AreEqual(- 1, ai.IntegerValue);
		}

		[Test] public void DecrementAndGet()
		{
			AtomicInteger ai = new AtomicInteger(1);
			Assert.AreEqual(0, ai.DecrementValueAndReturn());
			Assert.AreEqual(- 1, ai.DecrementValueAndReturn());
			Assert.AreEqual(- 2, ai.DecrementValueAndReturn());
			Assert.AreEqual(- 2, ai.IntegerValue);
		}

        [Test]
        public void IncrementAndGet() {
            AtomicInteger ai = new AtomicInteger(1);
            Assert.AreEqual(2, ai.IncrementValueAndReturn());
            Assert.AreEqual(2, ai.IntegerValue);
            ai.IntegerValue = -2;
            Assert.AreEqual(-1, ai.IncrementValueAndReturn());
            Assert.AreEqual(0, ai.IncrementValueAndReturn());
            Assert.AreEqual(1, ai.IncrementValueAndReturn());
            Assert.AreEqual(1, ai.IntegerValue);
        }

        [Test]
        public void Serialization() {
            AtomicInteger l = new AtomicInteger();

            l.IntegerValue = 22;
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, l);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicInteger r = (AtomicInteger)formatter2.Deserialize(bin);
            Assert.AreEqual(l.IntegerValue, r.IntegerValue);
        }

        [Test]
        public void AtomicIntegerToString() {
            AtomicInteger ai = new AtomicInteger();
            for(int i = -12; i < 6; ++i) {
                ai.IntegerValue = i;
                Assert.AreEqual(ai.ToString(), Convert.ToString(i));
            }
        }

        [Test]
        public void IntValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.LongValue, 42);
        }

        [Test]
        public void LongValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.LongValue, 42L);
        }

        [Test]
        public void FloatValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.FloatValue, 42.0F);
        }
 
        [Test]
        public void DoubleValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.DoubleValue, 42.0D);
        }
 
        [Test]
        public void ShortValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.ShortValue, 42);
            ai.IntegerValue = short.MaxValue + 5;
            Assert.AreEqual(ai.ShortValue, short.MinValue + 4);
        }

        [Test]
        public void ByteValue() {
            AtomicInteger ai = new AtomicInteger(42);
            Assert.AreEqual(ai.ShortValue, 42);
            ai.IntegerValue = byte.MaxValue + 5;
            Assert.AreEqual(ai.ByteValue, byte.MinValue + 4);
        }
    }
}