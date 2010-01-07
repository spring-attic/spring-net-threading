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
    public class PriorityQueueTest
    {
        private const CollectionContractOptions _defaultContractOptions =
            CollectionContractOptions.Unbounded |
            CollectionContractOptions.NoNull |
            CollectionContractOptions.ToStringPrintItems;

        public enum Ordering
        {
            None, Comparer, Comparison,
        }

        [TestFixture(typeof(int))]
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int), Ordering.Comparer)]
        [TestFixture(typeof(string), Ordering.Comparer)]
        [TestFixture(typeof(int), Ordering.Comparison)]
        [TestFixture(typeof(string), Ordering.Comparison)]
        public class AsGeneric<T> : QueueContract<T>
        {
            private static readonly int[] _randomSamples = new[] { 8,2,3,6,2,4,9,0,9,4,1 };
            private readonly Ordering _order;
            private ReverseOrder<T> _comparer;

            public AsGeneric() : this(Ordering.None)
            {
            }

            public AsGeneric(Ordering order) : base(
                _defaultContractOptions)
            {
                _order = order;
                switch (_order)
                {
                    case Ordering.None:
                        SampleSize = 150;
                        break;
                    case Ordering.Comparison:
                        SampleSize = _randomSamples.Length;
                        break;
                }
            }

            protected override T[] NewSamples()
            {
                if (_order == Ordering.Comparison)
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
                if (_order == Ordering.None)
                    return new PriorityQueue<T>(Samples);
                return base.NewQueueFilledWithSample();
            }

            private PriorityQueue<T> NewPriorityQueue()
            {
                switch (_order)
                {
                    case Ordering.Comparer:
                        _comparer = new ReverseOrder<T>();
                        return new PriorityQueue<T>(11, _comparer);
                    case Ordering.Comparison:
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
                    case Ordering.Comparer:
                        Assert.That(c.Comparer, Is.SameAs(_comparer));
                        break;
                    case Ordering.Comparison:
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
        public class AsNonGeneric<T> : QueueContract
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
        private class ReverseOrder<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return ((IComparable)y).CompareTo(x);
            }
        }
    }
}