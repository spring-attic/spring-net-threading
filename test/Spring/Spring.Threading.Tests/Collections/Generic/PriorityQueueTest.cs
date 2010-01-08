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
using NUnit.Framework;
using Spring.TestFixtures.Collections.Generic;
using Spring.TestFixtures.Collections.NonGeneric;
#if !PHASED
using IQueue = Spring.Collections.IQueue;
#else
using IQueue = System.Collections.ICollection;
#endif

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Class PriorityQueueTest
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class PriorityQueueTest<T>
    {
        private const CollectionContractOptions _defaultContractOptions =
            CollectionContractOptions.Unbounded |
            CollectionContractOptions.NoNull |
            CollectionContractOptions.ToStringPrintItems;

        [Test] public void ConstructorChokesOnNullEnumerableSource()
        {
            var e = Assert.Catch<ArgumentNullException>(
                ()=>new PriorityQueue<T>((IEnumerable<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ConstructorChokesOnNullPriorityQueueSource()
        {
            var e = Assert.Catch<ArgumentNullException>(
                ()=>new PriorityQueue<T>((PriorityQueue<T>)null));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ConstructorChokesOnInitialCapacityLessThenOne(
            [Values(-1, 0)]int initialCapacity)
        {
            var e = Assert.Catch<ArgumentOutOfRangeException>(
                () => new PriorityQueue<T>(initialCapacity));
            Assert.That(e.ParamName, Is.EqualTo("initialCapacity"));
        }

        [Test] public void DrainStopsWhenStopCriteriaMeats()
        {
            const int size = 9;
            const int half = size / 2;
            Predicate<T> criteria = o => o.Equals(TestData<T>.MakeData(half));
            var q = new PriorityQueue<T>(TestData<T>.MakeTestArray(size));
            var sent = new List<T>();
            q.Drain(sent.Add, int.MaxValue, null, criteria);
            Assert.That(sent, Is.EquivalentTo(TestData<T>.MakeTestArray(half)));
        }

        [Test] public void ComparerIsSameAsThePriorityQueueUsedToConstruct()
        {
            PriorityQueue<T> q1, q2, q3;

            q1 = new PriorityQueue<T>();
            q2 = new PriorityQueue<T>(q1);
            q3 = new PriorityQueue<T>((IEnumerable<T>)q1);
            Assert.That(q2.Comparer, Is.Null);
            Assert.That(q3.Comparer, Is.Null);

            q1 = new PriorityQueue<T>(11, new ReverseOrder());
            q2 = new PriorityQueue<T>(q1);
            q3 = new PriorityQueue<T>((IEnumerable<T>)q1);
            Assert.That(q2.Comparer, Is.SameAs(q1.Comparer));
            Assert.That(q3.Comparer, Is.SameAs(q1.Comparer));

            q1 = new PriorityQueue<T>(11, (x, y) => ((IComparable)y).CompareTo(x));
            q2 = new PriorityQueue<T>(q1);
            q3 = new PriorityQueue<T>((IEnumerable<T>)q1);
            Assert.That(q2.Comparer, Is.SameAs(q1.Comparer));
            Assert.That(q3.Comparer, Is.SameAs(q1.Comparer));
        }

        [TestFixture(typeof(int))]
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int), PriorityQueueTestOrdering.Comparer)]
        [TestFixture(typeof(string), PriorityQueueTestOrdering.Comparer)]
        [TestFixture(typeof(int), PriorityQueueTestOrdering.Comparison)]
        [TestFixture(typeof(string), PriorityQueueTestOrdering.Comparison)]
        public class AsGeneric : QueueContract<T>
        {
            private static readonly int[] _randomSamples = new[] { 8,2,3,6,2,4,9,0,9,4,1 };
            private readonly PriorityQueueTestOrdering _order;
            private ReverseOrder _comparer;

            public AsGeneric() : this(PriorityQueueTestOrdering.None)
            {
            }

            public AsGeneric(PriorityQueueTestOrdering order) : base(
                _defaultContractOptions)
            {
                _order = order;
                switch (_order)
                {
                    case PriorityQueueTestOrdering.None:
                        SampleSize = 150;
                        break;
                    case PriorityQueueTestOrdering.Comparison:
                        SampleSize = _randomSamples.Length;
                        break;
                }
            }

            protected override T[] NewSamples()
            {
                if (_order == PriorityQueueTestOrdering.Comparison)
                {
                    var samples = new T[_randomSamples.Length];
                    for (int i = samples.Length - 1; i >= 0; i--)
                    {
                        samples[i] = TestData<T>.MakeData(_randomSamples[i]);
                    }
                    return samples;
                }
                return base.NewSamples();
            }

            protected override IQueue<T> NewQueue()
            {
                return NewPriorityQueue();
            }

            protected override IQueue<T> NewQueueFilledWithSample()
            {
                if (_order == PriorityQueueTestOrdering.None)
                    return new PriorityQueue<T>(Samples);
                return base.NewQueueFilledWithSample();
            }

            private PriorityQueue<T> NewPriorityQueue()
            {
                switch (_order)
                {
                    case PriorityQueueTestOrdering.Comparer:
                        _comparer = new ReverseOrder();
                        return new PriorityQueue<T>(11, _comparer);
                    case PriorityQueueTestOrdering.Comparison:
                        return new PriorityQueue<T>(11, (x, y) => ((IComparable)y).CompareTo(x));
                    default:
                        return new PriorityQueue<T>();
                }
            }

            [Test] public void CapacityIsUnbounded()
            {
                var c = NewPriorityQueue();
                Assert.That(c.Capacity, Is.EqualTo(int.MaxValue));
            }

            [Test] public void ComparerReturnsOneGivenInConstructor()
            {
                var c = NewPriorityQueue();
                switch (_order)
                {
                    case PriorityQueueTestOrdering.Comparer:
                        Assert.That(c.Comparer, Is.SameAs(_comparer));
                        break;
                    case PriorityQueueTestOrdering.Comparison:
                        Assert.That(c.Comparer, Is.Not.Null);
                        break;
                    default:
                        Assert.That(c.Comparer, Is.Null);
                        break;
                }
            }
        }

        [TestFixture(typeof(int))]
        [TestFixture(typeof(string))]
        public class AsNonGeneric : QueueContract
        {
            public AsNonGeneric()
                : base(_defaultContractOptions)
            {
            }

            protected override IQueue NewQueue()
            {
                return new PriorityQueue<T>();
            }
        }

        [Serializable]
        private class ReverseOrder : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return ((IComparable)y).CompareTo(x);
            }
        }
    }

    public enum PriorityQueueTestOrdering
    {
        None, Comparer, Comparison,
    }
}