#region License

/*
 * Copyright (C) 2009-2010 the original author or authors.
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
using System.Collections;
using System.Collections.Generic;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.Generic;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.TestFixtures.Collections;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="CopyOnWriteList{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class CopyOnWriteListTest<T> : ListContract<T>
    {
        public CopyOnWriteListTest()
            : base(CollectionContractOptions.WeaklyConsistentEnumerator){}

        protected override IList<T> NewList()
        {
            return new CopyOnWriteList<T>();
        }

        protected override IList<T> NewListFilledWithSample()
        {
            return new CopyOnWriteList<T>(Samples);
        }

        [Test] public void DefaultConstructorMakesEmptyList()
        {
            Assert.That(NewList().Count, Is.EqualTo(0));
        }

        [Test] public void ConstructorChokesOnNulCollection()
        {
            var e = Assert.Catch<ArgumentNullException>(() => new CopyOnWriteList<T>(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void ConstructorPopulatesTheListWithGivenCollection()
        {
            var samples = Samples;
            var sut = new CopyOnWriteList<T>(samples);
            for (int i = SampleSize - 1; i >= 0; i--)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i]));
            }
            sut[0] = TestData<T>.M1;
            Assert.That(samples[0], Is.Not.EqualTo(sut[0]));
        }

        [Test] public void AddIfAbsentDoesNotAddWhenExist()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            sut.AddIfAbsent(TestData<T>.One);
            Assert.That(sut.Count, Is.EqualTo(SampleSize));
        }

        [Test] public void AddIfAbsentDoesAddWhenNotExist()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            sut.AddIfAbsent(TestData<T>.M1);
            Assert.IsTrue(sut.Contains(TestData<T>.M1));
        }

        [Test] public void AddRangeIfAbsentChokesOnNullCollection()
        {
            var e = Assert.Catch<ArgumentNullException>(
                ()=>new CopyOnWriteList<T>().AddRangeAbsent(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void AddRangeIfAbsentAddOnlyThoseNotExist()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var c = new[]
                        {
                            TestData<T>.M1,
                            TestData<T>.M2,
                            TestData<T>.One, // Will no add.
                        };
            Assert.That(sut.AddRangeAbsent(c), Is.EqualTo(2));
            Assert.That(sut.Count, Is.EqualTo(SampleSize + 2));
            Assert.IsTrue(sut.Contains(TestData<T>.M1));
            Assert.IsTrue(sut.Contains(TestData<T>.M2));
        }

        [Test] public void ContainsAllChokesOnNullCollection()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentNullException>(()=>sut.ContainsAll(null));
        }

        [Test] public void ContainsAllReturnTrueWhenAllElementsExist()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.IsTrue(sut.ContainsAll(NewSamples()));
        }

        [Test] public void ContainsAllReturnTrueWhenOneElementNotExist()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var c = new[]
                        {
                            TestData<T>.M1, // doesn't exist
                            TestData<T>.Zero,
                            TestData<T>.One,
                        };
            Assert.IsFalse(sut.ContainsAll(c));
        }

        [Test] public void InsertRangeChokesOnNullCollection()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var e = Assert.Catch<ArgumentNullException>(()=>sut.InsertRange(0, null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [TestCase(Position.Before)]
        [TestCase(Position.Beyond)]
        public void InsertRangeChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            var e = Assert.Catch<ArgumentOutOfRangeException>(() => sut.InsertRange(index, Samples));
            Assert.That(e.ParamName, Is.EqualTo("index"));
        }

        [TestCase(Position.Head)]
        [TestCase(Position.Middle)]
        [TestCase(Position.After)]
        public void InsertRangeInsertsCollectionInRightPosition(Position position)
        {
            var index = PositionToIndex(position);
            var samples = Samples;
            var sampleSize = SampleSize;
            var sut = new CopyOnWriteList<T>(samples);
            var c = new[]
                        {
                            TestData<T>.M1,
                            TestData<T>.M2,
                            TestData<T>.M3,
                        };
            sut.InsertRange(index, c);
            for (int i = 0; i < index; i++)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i]));
            }
            var addCount = c.Length;
            for (int i = 0; i < addCount; i++)
            {
                Assert.That(sut[index+i], Is.EqualTo(c[i]));
            }
            for (int i = index + addCount; i < sampleSize; i++)
            {
                Assert.That(sut[i+addCount], Is.EqualTo(samples[i]));
            }
        }

        [Test] public void InsertRangeInsertCollectionIntoEmptyList()
        {
            var sut = new CopyOnWriteList<T>();
            sut.InsertRange(0, Samples);
            CollectionAssert.AreEqual(Samples, sut);
        }

        [Test] public void RemoveAllChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var e = Assert.Catch<ArgumentNullException>(
                ()=>sut.RemoveAll(null));
            Assert.That(e.ParamName, Is.EqualTo("match"));
        }

        [Test] public void RemoveAllKeepListUnchangedWhenNoMatch()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.That(sut.RemoveAll(r=>false), Is.EqualTo(0));
            CollectionAssert.AreEqual(Samples, sut);
        }

        [Test] public void RemoveAllRemovesElementMatchsCriteria()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            IList<T> c = new List<T>();
            for (int i = SampleSize - 1; i >= 0; i-=2)
            {
                c.Add(TestData<T>.MakeData(i));
            }
            Assert.That(sut.RemoveAll(c.Contains), Is.EqualTo(c.Count));
            Assert.That(sut.Count, Is.EqualTo(SampleSize - c.Count));

            c = sut;
            sut = new CopyOnWriteList<T>(Samples);
            Assert.That(sut.RemoveAll(c.Contains), Is.EqualTo(c.Count));
            Assert.That(sut.Count, Is.EqualTo(SampleSize - c.Count));
        }

        [TestCase(Position.Before)]
        [TestCase(Position.After)]
        [Test] public void RemoveRangeChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            var e = Assert.Catch<ArgumentOutOfRangeException>(() => sut.RemoveRange(index, 1));
            Assert.That(e.ParamName, Is.EqualTo("index"));
        }

        [Test] public void RemoveRangeChokesOnBadCount()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var e = Assert.Catch<ArgumentOutOfRangeException>(() => sut.RemoveRange(0, -1));
            Assert.That(e.ParamName, Is.EqualTo("count"));
        }

        [TestCase(Position.Head, 5)]
        [TestCase(Position.Head, 0)]
        [TestCase(Position.Tail, 1)]
        [TestCase(Position.Tail, 0)]
        [TestCase(Position.Middle, 5)]
        [TestCase(Position.Middle, 3)]
        [TestCase(Position.Tail, 2, ExpectedException = typeof(ArgumentException))]
        public void RemoveRangeRemovesElementsInSpecifiedRange(Position position, int count)
        {
            var index = PositionToIndex(position);
            var samples = Samples;
            var sut = new CopyOnWriteList<T>(samples);
            sut.RemoveRange(index, count);
            for (int i = 0; i < index; i++)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i]));
            }
            for (int i = index; i < index-count; i++)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i+count]));
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsNonGeneric : TypedListContract<T>
        {
            public AsNonGeneric()
                : base(CollectionContractOptions.WeaklyConsistentEnumerator){}

            protected override IList NewList()
            {
                return new CopyOnWriteList<T>();
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsAbstractCollection : AbstractCollectionContract<T>
        {
            public AsAbstractCollection()
                : base(CollectionContractOptions.WeaklyConsistentEnumerator){}

            protected override AbstractCollection<TE> NewCollection<TE>()
            {
                return new CopyOnWriteList<TE>();
            }
        }
    }
}