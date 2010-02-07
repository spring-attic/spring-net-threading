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
using System.Linq;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.Generic;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.CommonFixtures.Threading;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.TestFixtures.Collections;
using Spring.Threading.AtomicTypes;
using Spring.Utility;

namespace Spring.Threading.Collections.Generic.AA
{
    /// <summary>
    /// Test cases for <see cref="CopyOnWriteList{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class CopyOnWriteListTest<T> : ListContract<T>
    {
        private static readonly Comparison<T> _reverseComparison = 
            ((x, y)=>Comparer<T>.Default.Compare(x, y) * -1);
        private static readonly ComparisonComparer<T> _reverseComparer = 
            new ComparisonComparer<T>(_reverseComparison);

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

        [Test] public void AddRangeIfAbsentNopOnEmptyCollection()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            sut.AddRangeAbsent(new T[0]);
            Assert.That(sut.Count, Is.EqualTo(SampleSize));
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

        [TestCase(Position.Before)]
        [TestCase(Position.After)]
        public void IndexOfChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.IndexOf(TestData<T>.One, index));
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.IndexOf(TestData<T>.One, index, 0));
        }

        [Test] public void IndexOfChokesOnBadCount()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.IndexOf(TestData<T>.One, 0, -1));
        }

        private IEnumerable<TestCaseData> IndexOfTestSource()
        {
            var size = SampleSize;
            var half = size/2;
            var middle = TestData<T>.MakeData(half);
            return new[]
                       {
                           new TestCaseData(TestData<T>.M1, 0).Returns(-1),
                           new TestCaseData(TestData<T>.Zero, 0).Returns(0),
                           new TestCaseData(TestData<T>.Zero, size).Returns(size),
                           new TestCaseData(TestData<T>.Zero, size+1).Returns(-1),
                           new TestCaseData(TestData<T>.Two, 0).Returns(2),
                           new TestCaseData(middle, half).Returns(half),
                           new TestCaseData(middle, half+1).Returns(size+half),
                           new TestCaseData(middle, half+size+1).Returns(-1),
                       };
        }

        [TestCaseSource("IndexOfTestSource")]
        public int IndexOfReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.IndexOf(item, index);
        }

        private IEnumerable<TestCaseData> IndexOfWithCountTestSource()
        {
            var size = SampleSize;
            var half = size/2;
            var middle = TestData<T>.MakeData(half);
            return new[]
                       {
                           new TestCaseData(TestData<T>.M1, 0, size).Returns(-1),
                           new TestCaseData(TestData<T>.Zero, 0, size).Returns(0),
                           new TestCaseData(TestData<T>.Two, 0, size).Returns(2),
                           new TestCaseData(TestData<T>.Two, 0, 0).Returns(-1),
                           new TestCaseData(middle, half, 1).Returns(half),
                           new TestCaseData(middle, 0, half).Returns(-1),
                           new TestCaseData(middle, 0, half+1).Returns(half),
                           new TestCaseData(TestData<T>.Zero, half, size).Throws(typeof(ArgumentException)),
                       };
        }

        [TestCaseSource("IndexOfWithCountTestSource")]
        public int IndexOfWithCountReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index, int count)
        {
            var sut = new CopyOnWriteList<T>(Samples);
            return sut.IndexOf(item, index, count);
        }

        [Test] public void LastIndexOfReturnsLastPositionWhenFoundOtherwiseNegativeOne()
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            var size = SampleSize;
            Assert.That(sut.LastIndexOf(TestData<T>.M1), Is.EqualTo(-1));
            Assert.That(sut.LastIndexOf(TestData<T>.Two), Is.EqualTo(size + 2));
            Assert.That(sut.LastIndexOf(TestData<T>.MakeData(size-1)), Is.EqualTo(size*2 - 1));
        }

        [TestCase(Position.Before)]
        [TestCase(Position.After)]
        public void LastIndexOfChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.LastIndexOf(TestData<T>.One, index));
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.LastIndexOf(TestData<T>.One, index, 0));
        }

        [Test] public void LastIndexOfChokesOnBadCount()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.LastIndexOf(TestData<T>.One, 0, -1));
        }

        private IEnumerable<TestCaseData> LastIndexOfTestSource()
        {
            var size = SampleSize;
            var half = size / 2;
            var middle = TestData<T>.MakeData(half);
            return new[]
                       {
                           new TestCaseData(TestData<T>.M1, 0).Returns(-1),
                           new TestCaseData(TestData<T>.Zero, size*2-1).Returns(size),
                           new TestCaseData(TestData<T>.Zero, size-1).Returns(0),
                           new TestCaseData(TestData<T>.Two, 0).Returns(-1),
                           new TestCaseData(TestData<T>.Two, size*2-1).Returns(size+2),
                           new TestCaseData(middle, size+half).Returns(size+half),
                           new TestCaseData(middle, size).Returns(half),
                           new TestCaseData(middle, half-1).Returns(-1),
                       };
        }

        [TestCaseSource("LastIndexOfTestSource")]
        public int LastIndexOfReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.LastIndexOf(item, index);
        }

        private IEnumerable<TestCaseData> LastIndexOfWithCountTestSource()
        {
            var size = SampleSize;
            var half = size / 2;
            var middle = TestData<T>.MakeData(half);
            var tail = size*2-1;
            return new[]
                       {
                           new TestCaseData(TestData<T>.M1, tail, size*2).Returns(-1),
                           new TestCaseData(TestData<T>.MakeData(size-1), tail, size*2).Returns(tail),
                           new TestCaseData(TestData<T>.Zero, tail, size*2).Returns(size),
                           new TestCaseData(TestData<T>.Two, tail, size).Returns(size+2),
                           new TestCaseData(TestData<T>.Two, tail, 0).Returns(-1),
                           new TestCaseData(middle, size+half, 1).Returns(size + half),
                           new TestCaseData(middle, tail, half).Returns(-1),
                           new TestCaseData(middle, tail, half+1).Returns(size + half),
                           new TestCaseData(TestData<T>.Zero, half, size).Throws(typeof(ArgumentException)),
                       };
        }

        [TestCaseSource("LastIndexOfWithCountTestSource")]
        public int LastIndexOfWithCountReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index, int count)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.LastIndexOf(item, index, count);
        }

        [Test] public void FindLastIndexChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentNullException>(() => sut.FindLastIndex(null));
            Assert.Catch<ArgumentNullException>(() => sut.FindLastIndex(0, null));
            Assert.Catch<ArgumentNullException>(() => sut.FindLastIndex(0, 0, null));
        }


        [Test] public void FindLastIndexReturnsLastPositionWhenFoundOtherwiseNegativeOne()
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            var size = SampleSize;
            Assert.That(sut.FindLastIndex(i=>i.Equals(TestData<T>.M1)), Is.EqualTo(-1));
            Assert.That(sut.FindLastIndex(i=>i.Equals(TestData<T>.Two)), Is.EqualTo(size + 2));
            Assert.That(sut.FindLastIndex(i=>i.Equals(TestData<T>.MakeData(size-1))), Is.EqualTo(size*2 - 1));
        }

        [TestCase(Position.Before)]
        [TestCase(Position.After)]
        public void FindLastIndexChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindLastIndex(index, i => true));
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindLastIndex(index, 0, i => true));
        }

        [Test] public void FindLastIndexChokesOnBadCount()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindLastIndex(0, -1, i=>true));
        }

        [TestCaseSource("LastIndexOfTestSource")]
        public int FindLastIndexReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.FindLastIndex(index, i => i.Equals(item));
        }

        [TestCaseSource("LastIndexOfWithCountTestSource")]
        public int FindLastIndexWithCountReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index, int count)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.FindLastIndex(index, count, i=>i.Equals(item));
        }

        [Test] public void FindIndexChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentNullException>(() => sut.FindIndex(null));
            Assert.Catch<ArgumentNullException>(() => sut.FindIndex(0, null));
            Assert.Catch<ArgumentNullException>(() => sut.FindIndex(0, 0, null));
        }


        [Test] public void FindIndexReturnsLastPositionWhenFoundOtherwiseNegativeOne()
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            var size = SampleSize;
            sut[size*2 - 1] = TestData<T>.MakeData(size);
            Assert.That(sut.FindIndex(i=>i.Equals(TestData<T>.M1)), Is.EqualTo(-1));
            Assert.That(sut.FindIndex(i => i.Equals(TestData<T>.Zero)), Is.EqualTo(0));
            Assert.That(sut.FindIndex(i => i.Equals(TestData<T>.Two)), Is.EqualTo(2));
            Assert.That(sut.FindIndex(i=>i.Equals(TestData<T>.MakeData(size))), Is.EqualTo(size*2 - 1));
        }

        [TestCase(Position.Before)]
        [TestCase(Position.After)]
        public void FindIndexChokesOnBadIndex(Position position)
        {
            var index = PositionToIndex(position);
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindIndex(index, i => true));
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindIndex(index, 0, i => true));
        }

        [Test] public void FindIndexChokesOnBadCount()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.Catch<ArgumentOutOfRangeException>(() => sut.FindIndex(0, -1, i=>true));
        }

        [TestCaseSource("IndexOfTestSource")]
        public int FindIndexReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index)
        {
            var sut = new CopyOnWriteList<T>(Samples.Concat(Samples));
            return sut.FindIndex(index, i => i.Equals(item));
        }

        [TestCaseSource("IndexOfWithCountTestSource")]
        public int FindIndexWithCountReturnsPositionWhenFoundOtherwiseNegativeOne(T item, int index, int count)
        {
            var sut = new CopyOnWriteList<T>(Samples);
            return sut.FindIndex(index, count, i=>i.Equals(item));
        }

        [Test] public void ExistsChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.Exists(null));
        }

        [Test] public void ExistsReportsExistenceOfItemMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.IsFalse(sut.Exists(b => b.Equals(TestData<T>.M1)));
            Assert.IsTrue(sut.Exists(b => b.Equals(TestData<T>.Zero)));
            Assert.IsTrue(sut.Exists(b => b.Equals(TestData<T>.MakeData(SampleSize - 1))));
            Assert.IsTrue(sut.Exists(b => b.Equals(TestData<T>.MakeData(SampleSize / 2))));            
        }

        [Test] public void TrueForAllChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.TrueForAll(null));
        }

        [Test] public void TrueForAllReturnsLastItemMatchCriteriaOtherwiseDefaultValue()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.IsTrue(sut.TrueForAll(b => !b.Equals(TestData<T>.M1)));
            Assert.IsFalse(sut.TrueForAll(b => !b.Equals(TestData<T>.Zero)));
            Assert.IsFalse(sut.TrueForAll(b => !b.Equals(TestData<T>.MakeData(SampleSize - 1))));
            Assert.IsFalse(sut.TrueForAll(b => !b.Equals(TestData<T>.MakeData(SampleSize / 2))));
        }

        [Test] public void FindChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.Find(null));
        }

        [Test] public void FindReturnsItemMatchCriteriaOtherwiseDefaultValue()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.That(sut.Find(b => b.Equals(TestData<T>.M1)), Is.EqualTo(default(T)));
            var head = TestData<T>.Zero;
            Assert.That(sut.Find(b => b.Equals(head)), Is.EqualTo(head));
            var tail = TestData<T>.MakeData(SampleSize - 1);
            Assert.That(sut.Find(b => b.Equals(tail)), Is.EqualTo(tail));
            var half = TestData<T>.MakeData(SampleSize / 2);
            Assert.That(sut.Find(b => b.Equals(half)), Is.EqualTo(half));
        }

        [Test] public void FindLastChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.FindLast(null));
        }

        [Test] public void FindLastReturnsLastItemMatchCriteriaOtherwiseDefaultValue()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            Assert.That(sut.FindLast(b => b.Equals(TestData<T>.M1)), Is.EqualTo(default(T)));
            var head = TestData<T>.Zero;
            Assert.That(sut.FindLast(b => b.Equals(head)), Is.EqualTo(head));
            var tail = TestData<T>.MakeData(SampleSize - 1);
            Assert.That(sut.FindLast(b => b.Equals(tail)), Is.EqualTo(tail));
            var half = TestData<T>.MakeData(SampleSize / 2);
            Assert.That(sut.FindLast(b => b.Equals(half)), Is.EqualTo(half));
            if (!typeof(T).IsValueType)
            {
                var zero = TestData<T>.MakeData(0);
                sut[SampleSize/2] = zero;
                Assert.That(sut.FindLast(b => b.Equals(zero)), Is.SameAs(zero));
            }
        }

        [Test] public void ConvertAllChokesOnNullConvertor()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.ConvertAll<object>(null));
        }

        [Test] public void ConvertAllConvertsToAnotherCopyOnWriteList()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var converted = sut.ConvertAll<object>(i => i);
            CollectionAssert.AreEqual(sut, converted);
            converted[0] = TestData<T>.M1;
            Assert.That(sut[0], Is.Not.EqualTo(TestData<T>.M1));
        }

        [Test] public void FindAllChokesOnNullMatchCriteria()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.FindAll(null));
        }

        [Test] public void FindAllReturnsNewCopyOnWriteListWithMatchItems()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var size = SampleSize;
            var odd = new List<T>((size+1) / 2);
            var even = new List<T>((size+1) / 2);
            for (int i = 0; i < size; i++)
            {
                var item = TestData<T>.MakeData(i);
                if (i%2 == 0) even.Add(item);
                else odd.Add(item);
            }

            Assert.That(sut.FindAll(i=>false).Count, Is.EqualTo(0));
            CollectionAssert.AreEqual(odd, sut.FindAll(odd.Contains));
            CollectionAssert.AreEqual(even, sut.FindAll(even.Contains));
            var all = sut.FindAll(i => true);
            CollectionAssert.AreEqual(sut, all);
            all[0] = TestData<T>.M1;
            Assert.That(sut[0], Is.Not.EqualTo(TestData<T>.M1));
        }

        [Test] public void ForEachChokesOnNullAction()
        {
            var sut = new CopyOnWriteList<T>();
            Assert.Catch<ArgumentNullException>(() => sut.ForEach(null));
        }

        [Test] public void ForEachCallsActionWithEachItem()
        {
            var sut = new CopyOnWriteList<T>(Samples);
            var all = new List<T>(SampleSize);
            sut.ForEach(all.Add);
            CollectionAssert.AreEqual(sut, all);
        }

        [Test] public void ReverseReversesOrderOfElement()
        {
            var samples = Samples;
            var sut = new CopyOnWriteList<T>(samples);
            sut.Reverse();
            var tail = SampleSize - 1;
            for (int i = tail; i >= 0; i--)
            {
                Assert.That(sut[i], Is.EqualTo(samples[tail - i]));
            }
        }

        private IEnumerable<TestCaseData> IndexAndCount()
        {
            var size = SampleSize;
            return new[]
                       {
                           new TestCaseData(-1, 1).Throws(typeof(ArgumentOutOfRangeException)),
                           new TestCaseData(size, 1).Throws(typeof(ArgumentOutOfRangeException)),
                           new TestCaseData(0, -1).Throws(typeof(ArgumentOutOfRangeException)),
                           new TestCaseData(0, size+1).Throws(typeof(ArgumentException)),
                           new TestCaseData(size/2, size/2+2).Throws(typeof(ArgumentException)),
                           new TestCaseData(0, 0),
                           new TestCaseData(0, size),
                           new TestCaseData(0, size/2),
                           new TestCaseData(size/2, size-size/2),
                           new TestCaseData(1, size-1),
                       };
        }

        [TestCaseSource("IndexAndCount")]
        public void ReverseWithIndexReversesOrderOfElementInRange(int index, int count)
        {
            var samples = Samples;
            var sut = new CopyOnWriteList<T>(samples);
            sut.Reverse(index, count);
            var size = SampleSize;
            for (int i = 0; i < index; i++)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i]));
            }
            for (int i = 0; i < count; i++)
            {
                Assert.That(sut[index+i], Is.EqualTo(samples[index + count - 1 - i]));
            }
            for (int i = index + count; i < size; i++)
            {
                Assert.That(sut[i], Is.EqualTo(samples[i]));
            }
        }

        [TestCaseSource("IndexAndCount")]
        public void SortWithIndexSortsTheElementsInRangeTheSameWayAsArraySort(int index, int count)
        {
            T[] samples = NewPsuedoRandomSamples();
            var sut = new CopyOnWriteList<T>(samples);

            sut.Sort(index, count, null);
            Array.Sort(samples, index, count, null);
            CollectionAssert.AreEqual(samples, sut);

            sut.Sort(index, count, _reverseComparer);
            Array.Sort(samples, index, count, _reverseComparer);
            CollectionAssert.AreEqual(samples, sut);
        }

        [Test] public void SortSortsListTheSameWayAsArraySort()
        {
            T[] samples = NewPsuedoRandomSamples();
            var sut = new CopyOnWriteList<T>(samples);

            sut.Sort();
            Array.Sort(samples);
            CollectionAssert.AreEqual(samples, sut);

            sut.Sort(_reverseComparison);
            Array.Sort(samples, _reverseComparison);
            CollectionAssert.AreEqual(samples, sut);
        }

        [TestCaseSource("IndexAndCount")]
        public void BinarySearchWithIndexFindsElementInRange(int index, int count)
        {
            var size = SampleSize;
            var old = new T[size];
            var even = new T[size];
            for (int i = size - 1; i >= 0; i--)
            {
                // offset from 5 to get double digits so that elements are in
                // sort order of string.
                old[i] = TestData<T>.MakeData((i + 5)*2 + 1);
                even[i] = TestData<T>.MakeData((i + 5) * 2);
            }
            var min = TestData<T>.One;
            var max = TestData<T>.MakeData((size + 5) * 2);

            var sut = new CopyOnWriteList<T>(even);

            // This shoud trigger any argument exception
            Assert.That(sut.BinarySearch(index, count, min, null), Is.EqualTo(-(index + 1)));
            Assert.That(sut.BinarySearch(index, count, max, null), Is.EqualTo(-(index+count+1)));

            for (int i = 0; i < count; i++)
            {
                var j = index + i;
                var result = sut.BinarySearch(index, count, even[j], null);
                Assert.That(result, Is.EqualTo(j));
                result = sut.BinarySearch(index, count, old[j], null);
                Assert.That(result, Is.EqualTo(-j - 2));
            }

            sut.Reverse();

            Assert.That(sut.BinarySearch(index, count, max, _reverseComparer), Is.EqualTo(-(index + 1)));
            Assert.That(sut.BinarySearch(index, count, min, _reverseComparer), Is.EqualTo(-(index + count + 1)));

            for (int i = 0; i < count; i++)
            {
                var j = index + i;
                var result = sut.BinarySearch(index, count, even[size-1-j], _reverseComparer);
                Assert.That(result, Is.EqualTo(j));
                result = sut.BinarySearch(index, count, old[size-1-j], _reverseComparer);
                Assert.That(result, Is.EqualTo(-j - 1));
            }
        }

        [Test] public void BinarySearchFindsElements()
        {
            var size = SampleSize;
            T[] samples = NewPsuedoRandomSamples();
            var before = TestData<T>.Zero;
            var after = TestData<T>.MakeData(size);
            
            Array.Sort(samples);
            var sut = new CopyOnWriteList<T>(samples);
            Assert.That(sut.BinarySearch(before), Is.EqualTo(-1));
            Assert.That(sut.BinarySearch(after), Is.EqualTo(-size-1));

            var defaultComparer = Comparer<T>.Default;
            for (int i = 0; i <= size; i++)
            {
                var item = TestData<T>.MakeData(i);
                var index = sut.BinarySearch(item);
                if (index>=0)
                {
                    Assert.That(sut[index], Is.EqualTo(item));
                }
                else
                {
                    index = -index - 1;
                    if (index>0) 
                        Assert.That(defaultComparer.Compare(sut[index-1], item), Is.LessThan(0));
                    if (index < size)
                        Assert.That(defaultComparer.Compare(sut[index], item), Is.GreaterThan(0));
                }
            }

            Array.Sort(samples, _reverseComparer);
            sut = new CopyOnWriteList<T>(samples);
            Assert.That(sut.BinarySearch(before, _reverseComparison), Is.EqualTo(-size-1));
            Assert.That(sut.BinarySearch(after, _reverseComparison), Is.EqualTo(-1));

            for (int i = 0; i <= size; i++)
            {
                var item = TestData<T>.MakeData(i);
                var index = sut.BinarySearch(item, _reverseComparison);
                if (index >= 0)
                {
                    Assert.That(sut[index], Is.EqualTo(item));
                }
                else
                {
                    index = -index - 1;
                    if (index > 0)
                        Assert.That(_reverseComparison(sut[index - 1], item), Is.LessThan(0));
                    if (index < size)
                        Assert.That(_reverseComparison(sut[index], item), Is.GreaterThan(0));
                }
            }
        }

        private T[] NewPsuedoRandomSamples()
        {
            var size = SampleSize;
            var samples = new T[size];
            Random r = new Random(0);
            for (int i = size - 1; i >= 0; i--)
            {
                samples[i] = TestData<T>.MakeData(r.Next(size));
            }
            return samples;
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class Synchronization : TestThreadManager.Fixture<T>
        {
            [Test] public void IsSynchronizedReturnsTrue()
            {
                IList list = new CopyOnWriteList<T>();
                Assert.IsTrue(list.IsSynchronized);
            }

            [Test] public void SyncRootCanBlockWriteNotRead()
            {
                IList list = new CopyOnWriteList<T>(MakeTestArray(9));
                var syncRoot = list.SyncRoot;
                Assert.IsNotNull(syncRoot);
                var readCompete = new AtomicBoolean(false);
                var writeComplete = new AtomicBoolean(false);
                lock(syncRoot)
                {
                    ThreadManager.StartAndAssertRegistered(
                        "TRead",
                        () =>
                            {
                                Assert.That(list[0], Is.EqualTo(Zero));
                                readCompete.Value = true;
                            });
                    ThreadManager.StartAndAssertRegistered(
                        "TWrite",
                        () =>
                            {
                                list[0] = M1;
                                writeComplete.Value = true;
                            });
                    Thread.Sleep(Delays.Short);
                    Assert.IsTrue(readCompete);
                    Assert.IsFalse(writeComplete);
                }
                ThreadManager.JoinAndVerify();
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