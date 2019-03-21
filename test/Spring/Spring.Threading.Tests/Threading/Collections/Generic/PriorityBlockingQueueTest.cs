/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain, as explained at
 * https://creativecommons.org/licenses/publicdomain
 * Other contributors include Andrew Wright, Jeffrey Hayes,
 * Pat Fisher, Mike Judd.
 */

using System;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.TestFixtures.Collections.NonGeneric;
using Spring.TestFixtures.Threading.Collections.Generic;
#if !PHASED
using IQueue = Spring.Collections.IQueue;
#else
using IQueue = System.Collections.ICollection;
#endif

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="PriorityBlockingQueue{T}"/>.
    /// </summary>
    /// <author>Doug Lea>author>
    /// <author>Andreas D�hring (.NET)</author>
    /// <author>Kenneth Xu</author>
    public class PriorityBlockingQueueTest {

        private const CollectionContractOptions _defaultContractOptions =
            CollectionContractOptions.NoNull |
            CollectionContractOptions.ToStringPrintItems |
            CollectionContractOptions.WeaklyConsistentEnumerator;

        [TestFixture(typeof(int))]
        [TestFixture(typeof(string))]
        [TestFixture(typeof(int), PriorityQueueTestOrdering.Comparer)]
        [TestFixture(typeof(string), PriorityQueueTestOrdering.Comparer)]
        [TestFixture(typeof(int), PriorityQueueTestOrdering.Comparison)]
        [TestFixture(typeof(string), PriorityQueueTestOrdering.Comparison)]
        public class AsGeneric<T> : BlockingQueueContract<T>
        {
            private static readonly int[] _randomSamples = new[] { 8, 2, 3, 6, 2, 4, 9, 0, 9, 4, 1 };
            private readonly PriorityQueueTestOrdering _order;
            private ReverseOrder<T> _comparer;

            public AsGeneric()
                : this(PriorityQueueTestOrdering.None)
            {
            }

            public AsGeneric(PriorityQueueTestOrdering order)
                : base(_defaultContractOptions)
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

            protected override IBlockingQueue<T> NewBlockingQueue()
            {
                return NewPriorityBlockingQueue();
            }

            protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
            {
                if (_order == PriorityQueueTestOrdering.None)
                    return new PriorityBlockingQueue<T>(Samples);
                return base.NewBlockingQueueFilledWithSample();
            }

            private PriorityBlockingQueue<T> NewPriorityBlockingQueue()
            {
                switch (_order)
                {
                    case PriorityQueueTestOrdering.Comparer:
                        _comparer = new ReverseOrder<T>();
                        return new PriorityBlockingQueue<T>(11, _comparer);
                    case PriorityQueueTestOrdering.Comparison:
                        return new PriorityBlockingQueue<T>(11, (x, y) => ((IComparable)y).CompareTo(x));
                    default:
                        return new PriorityBlockingQueue<T>();
                }
            }

            [Test]
            public void CapacityIsUnbounded()
            {
                var c = NewPriorityBlockingQueue();
                Assert.That(c.Capacity, Is.EqualTo(int.MaxValue));
            }

            [Test] public void ComparerReturnsOneGivenInConstructor()
            {
                var c = NewPriorityBlockingQueue();
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
        public class AsNonGeneric<T> : TypedQueueContract<T>
        {
            public AsNonGeneric() : base(_defaultContractOptions) {}

            protected override IQueue NewQueue()
            {
                return new PriorityBlockingQueue<T>();
            }
        }
    }
}