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
	public class AtomicLongArrayTests : BaseThreadingTestCase
	{
		private class AnonymousClassRunnable
		{
			private AtomicLongArray a;

			public AnonymousClassRunnable(AtomicLongArray a)
			{
				this.a = a;
			}

			public void Run()
			{
				while (!a.CompareAndSet(0, 2, 3))
					Thread.Sleep(0);
			}
		}


		[Test]
		public void Constructor()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				Assert.AreEqual(0, ai[i]);
		}


		[ExpectedException(typeof (ArgumentNullException))]
		[Test]
		public void Constructor2NPE()
		{
			long[] a = null;
			AtomicLongArray ai = new AtomicLongArray(a);
		}


		[Test]
		public void Constructor2()
		{
			long[] a = new long[] {17L, 3L, - 42L, 99L, - 7L};
			AtomicLongArray ai = new AtomicLongArray(a);
			Assert.AreEqual(a.Length, ai.Length);
			for (int i = 0; i < a.Length; ++i)
				Assert.AreEqual(a[i], ai[i]);
		}


		[Test]
		public void Indexing()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			try
			{
				long foo = ai[DEFAULT_COLLECTION_SIZE];
			}
			catch (IndexOutOfRangeException success)
			{
                string s = success.Message;
			}
			try
			{
				long food = ai[-1];
			}
			catch (IndexOutOfRangeException success)
			{
                string s = success.Message;
			}
			try
			{
				ai[DEFAULT_COLLECTION_SIZE] = 0;
			}
			catch (IndexOutOfRangeException success)
			{
                string s = success.Message;
			}
			try
			{
				ai[- 1] = 0;
			}
			catch (IndexOutOfRangeException success)
			{
                string s = success.Message;
			}
		}


		[Test]
		public void GetSet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
				Assert.AreEqual(1, ai[i]);
				ai[i] = 2;
				Assert.AreEqual(2, ai[i]);
				ai[i] = - 3;
				Assert.AreEqual(- 3, ai[i]);
			}
		}


		[Test]
		public void GetLazySet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai.LazySet(i, 1);
				Assert.AreEqual(1, ai[i]);
				ai.LazySet(i, 2);
				Assert.AreEqual(2, ai[i]);
				ai.LazySet(i, - 3);
				Assert.AreEqual(- 3, ai[i]);
			}
		}


		[Test]
		public void CompareAndSet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
				Assert.IsTrue(ai.CompareAndSet(i, 1, 2));
				Assert.IsTrue(ai.CompareAndSet(i, 2, - 4));
				Assert.AreEqual(- 4, ai[i]);
				Assert.IsFalse(ai.CompareAndSet(i, - 5, 7));
				Assert.IsFalse((7 == ai[i]));
				Assert.IsTrue(ai.CompareAndSet(i, - 4, 7));
				Assert.AreEqual(7, ai[i]);
			}
		}


		[Test]
		public void CompareAndSetInMultipleThreads()
		{
			AtomicLongArray a = new AtomicLongArray(1);
			a[0] = 1;
			Thread t = new Thread(new ThreadStart(new AnonymousClassRunnable(a).Run));
			t.Start();
			Assert.IsTrue(a.CompareAndSet(0, 1, 2));
			t.Join(LONG_DELAY_MS);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(a[0], 3);
		}


		[Test]
		public void WeakCompareAndSet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
				while (!ai.WeakCompareAndSet(i, 1, 2))
					;
				while (!ai.WeakCompareAndSet(i, 2, - 4))
					;
				Assert.AreEqual(- 4, ai[i]);
				while (!ai.WeakCompareAndSet(i, - 4, 7))
					;
				Assert.AreEqual(7, ai[i]);
				Assert.IsFalse(ai.WeakCompareAndSet(i, -4, 7));
			}
		}


		[Test]
		public void GetAndSet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(1, ai.SetNewAtomicValue(i, 0));
                Assert.AreEqual(0, ai.SetNewAtomicValue(i, -10));
                Assert.AreEqual(-10, ai.SetNewAtomicValue(i, 1));
			}
		}


		[Test]
		public void GetAndAdd()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(1, ai.AddDeltaAndReturnPreviousValue(i, 2));
				Assert.AreEqual(3, ai[i]);
                Assert.AreEqual(3, ai.AddDeltaAndReturnPreviousValue(i, -4));
				Assert.AreEqual(- 1, ai[i]);
			}
		}


		[Test]
		public void GetAndDecrement()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(1, ai.ReturnValueAndDecrement(i));
                Assert.AreEqual(0, ai.ReturnValueAndDecrement(i));
                Assert.AreEqual(-1, ai.ReturnValueAndDecrement(i));
			}
		}


		[Test]
		public void GetAndIncrement()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(1, ai.ReturnValueAndIncrement(i));
				Assert.AreEqual(2, ai[i]);
				ai[i] = - 2;
                Assert.AreEqual(-2, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(-1, ai.ReturnValueAndIncrement(i));
                Assert.AreEqual(0, ai.ReturnValueAndIncrement(i));
				Assert.AreEqual(1, ai[i]);
			}
		}


		[Test]
		public void AddAndGet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(3, ai.AddDeltaAndReturnNewValue(i, 2));
				Assert.AreEqual(3, ai[i]);
                Assert.AreEqual(-1, ai.AddDeltaAndReturnNewValue(i, -4));
				Assert.AreEqual(- 1, ai[i]);
			}
		}


		[Test]
		public void DecrementAndGet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(0, ai.DecrementValueAndReturn(i));
                Assert.AreEqual(-1, ai.DecrementValueAndReturn(i));
                Assert.AreEqual(-2, ai.DecrementValueAndReturn(i));
				Assert.AreEqual(- 2, ai[i]);
			}
		}


		[Test]
		public void IncrementAndGet()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				ai[i] = 1;
                Assert.AreEqual(2, ai.IncrementValueAndReturn(i));
				Assert.AreEqual(2, ai[i]);
				ai[i] = - 2;
                Assert.AreEqual(-1, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(0, ai.IncrementValueAndReturn(i));
                Assert.AreEqual(1, ai.IncrementValueAndReturn(i));
				Assert.AreEqual(1, ai[i]);
			}
		}

		internal const long COUNTDOWN = 100000;
#if ! NET_1_0
		internal class Counter
		{
			private void InitBlock(AtomicLongArrayTests enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}

			private AtomicLongArrayTests enclosingInstance;

			public AtomicLongArrayTests Enclosing_Instance
			{
				get { return enclosingInstance; }

			}

			internal AtomicLongArray ai;
			internal long counts;

			internal Counter(AtomicLongArrayTests enclosingInstance, AtomicLongArray a)
			{
				InitBlock(enclosingInstance);
				ai = a;
			}

			[Test]
			public void Run()
			{
				for (;; )
				{
					bool done = true;
					for (int i = 0; i < ai.Length; ++i)
					{
						long v = ai[i];
						Assert.IsTrue(v >= 0);
						if (v != 0)
						{
							done = false;
							if (ai.CompareAndSet(i, v, v - 1))
								Thread.VolatileWrite(ref counts, ++counts);
						}
					}
					if (done)
						break;
				}
			}
		}


		[Test]
		public void CountingInMultipleThreads()
		{
			AtomicLongArray ai = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				ai[i] = COUNTDOWN;
			Counter c1 = new Counter(this, ai);
			Counter c2 = new Counter(this, ai);
			Thread t1 = new Thread(new ThreadStart(c1.Run));
			Thread t2 = new Thread(new ThreadStart(c2.Run));
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
			Assert.AreEqual(c1.counts + c2.counts, DEFAULT_COLLECTION_SIZE*COUNTDOWN);
		}

#endif
		[Test]
		public void Serialization()
		{
			AtomicLongArray l = new AtomicLongArray(DEFAULT_COLLECTION_SIZE);
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
				l[i] = - i;

			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, l);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
			AtomicLongArray r = (AtomicLongArray) formatter2.Deserialize(bin);
			;
			for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
			{
				Assert.AreEqual(l[i], r[i]);
			}
		}


		[Test]
		public void ToStringTest()
		{
			long[] a = new long[] {17, 3, - 42, 99, - 7};
			AtomicLongArray ai = new AtomicLongArray(a);
			Assert.AreEqual(toString(a), ai.ToString());

			long[] b = new long[0];
			Assert.AreEqual("[]", new AtomicLongArray(b).ToString());
		}

		private static String toString(long[] array)
		{
			if (array.Length == 0)
				return "[]";

			StringBuilder buf = new StringBuilder();
			buf.Append('[');
			buf.Append(array[0]);

			for (int i = 1; i < array.Length; i++)
			{
				buf.Append(",");
				buf.Append(array[i]);
			}

			buf.Append("]");
			return buf.ToString();
		}
	}
}