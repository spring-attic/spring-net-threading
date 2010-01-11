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
using System.ComponentModel;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Threading;
using NUnit.Framework;
using Spring.TestFixtures.Collections.NonGeneric;
using Spring.TestFixtures.Threading.Collections.Generic;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;
#if !PHASED
using IQueue = Spring.Collections.IQueue;
#else
using IQueue = System.Collections.ICollection;
#endif

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Class DelayQueueTest
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(MillisDelayStruct))]
    [TestFixture(typeof(MillisDelayClass))]
    public class DelayQueueTest<T>
        where T : IDelayed, new()
    {
        private const CollectionContractOptions _defaultContractOptions = 
            CollectionContractOptions.Unbounded | 
            CollectionContractOptions.NoNull |
            CollectionContractOptions.WeaklyConsistentEnumerator;

        private const int DEFAULT_COLLECTION_SIZE = 20;

        static DelayQueueTest()
            {
                MillisDelayClass.Load();
                MillisDelayStruct.Load();
            }

        [Test] public void CapacityAlwaysReturnMaxInt32Value()
        {
            Assert.That(new DelayQueue<T>().Capacity, Is.EqualTo(int.MaxValue));
        }

        [Test] public void ConstructorChokesOnNullSource()
        {
            var e = Assert.Catch<ArgumentNullException>(() => new DelayQueue<T>(null));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ConstructorChokesOnNullElementInSource()
        {
            if(typeof(T).IsValueType) Assert.Pass("Value type can never be null.");
            const int size = 10;
            T[] source = new T[size];
            Assert.Catch<NullReferenceException>(() => new DelayQueue<T>(source));
            for (int i = 0; i < size-1; i++)
            {
                source[i] = new T();
            }
            Assert.Catch<NullReferenceException>(() => new DelayQueue<T>(source));
        }

        [Test] public void ConstructorPopulatesQueueWithSource()
        {
            const int size = 10;
            T[] source = new T[size];
            for (int i = size - 1; i >= 0; i--)
            {
                source[size - 1 - i] = TestData<T>.MakeData((i-size)*100);
            }

            var q = new DelayQueue<T>(source);

            for (int i = size - 1; i >= 0; i--)
            {
                T delay;
                Assert.IsTrue(q.Poll(out delay));
                Assert.That(delay, Is.EqualTo(source[i]));
            }
        }

        [Test] public void TakeReturnsWhenDelayExpires()
        {
            DelayQueue<T> q = new DelayQueue<T>();
            T[] elements = new T[DEFAULT_COLLECTION_SIZE];
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                elements[i] = TestData<T>.MakeData(DEFAULT_COLLECTION_SIZE - i);
            }
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                q.Add(elements[i]);
            }

            DateTime last = DateTime.MinValue;
            for (int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i)
            {
                IMillesDelayed e = (IMillesDelayed)(q.Take());
                DateTime tt = e.TriggerTime;
                Assert.That(tt, Is.LessThanOrEqualTo(DateTime.UtcNow));
                if (i != 0)
                {
                    Assert.That(tt, Is.GreaterThanOrEqualTo(last));
                }
                last = tt;
            }
        }

        [Test] public void PeekReturnsFalseWhenDelayed()
        {
            var q = new DelayQueue<T> {TestData<T>.MakeData(Delays.LongMillis)};
            T nd;
            Assert.IsFalse(q.Peek(out nd));
        }

        [Test] public void PollReturnsFalesWhenDelayed()
        {
            var q = new DelayQueue<T> {TestData<T>.MakeData(Delays.LongMillis)};
            T nd;
            Assert.IsFalse(q.Poll(out nd));
        }

        [Test, Timeout(Delays.MediumMillis)] 
        public void TimedPollReturnsTrueWhenDelayExpired()
        {
            var q = new DelayQueue<T> { TestData<T>.MakeData(Delays.ShortMillis) };
            T nd;
            Assert.IsFalse(q.Poll(out nd));
            Assert.IsTrue(q.Poll(Delays.Small, out nd));
        }

        [Test, Timeout(Delays.MediumMillis)]
        public void TimedPollReturnsFalseBeforeDelayExpires()
        {
            var q = new DelayQueue<T> { TestData<T>.MakeData(Delays.SmallMillis) };
            T nd;
            Assert.IsFalse(q.Poll(Delays.Short, out nd));
        }


        [TestFixture(typeof(PseudoDelayStruct))]
        [TestFixture(typeof(PseudoDelayClass))]
        public class AsGeneric : BlockingQueueContract<T>
        {

            static AsGeneric()
            {
                PseudoDelayClass.Load();
                PseudoDelayStruct.Load();
            }

            public AsGeneric() : base(_defaultContractOptions) {}

            protected override IBlockingQueue<T> NewBlockingQueue()
            {
                return new DelayQueue<T>();
            }

            protected override IBlockingQueue<T> NewBlockingQueueFilledWithSample()
            {
                return new DelayQueue<T>(Samples);
            }
        }

        [TestFixture(typeof(PseudoDelayStruct))]
        [TestFixture(typeof(PseudoDelayClass))]
        public class AsNonGeneric : TypedQueueContract<T>
        {
            static AsNonGeneric()
            {
                PseudoDelayClass.Load();
                PseudoDelayStruct.Load();
            }

            public AsNonGeneric() : base(_defaultContractOptions) {}

            protected override IQueue NewQueue()
            {
                return new DelayQueue<T>();
            }
        }
    }

    [Serializable]
    public struct PseudoDelayStruct : IDelayed
    {
        static PseudoDelayStruct()
        {
            TestDataGenerator.RegisterConverter(i => new PseudoDelayStruct(i));
        }

        public static void Load()
        {
        }

        private readonly int _delay;

        public PseudoDelayStruct(int delay)
        {
            _delay = delay;
        }
        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Milliseconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            // delay less than 1000 are expired. Hence delay queue behaves as 
            // normal queue for all test samples used in QueueContract.
            return TimeSpan.FromMilliseconds(_delay - 1000);
        }

        public override string ToString()
        {
            return (_delay - 1000).ToString();
        }
    }

    [Serializable]
    public class PseudoDelayClass : IDelayed
    {
        static PseudoDelayClass()
        {
            TestDataGenerator.RegisterConverter(i => new PseudoDelayClass(i));
        }

        public static void Load()
        {
        }

        private readonly int _delay;

        public PseudoDelayClass()
        {
        }

        public PseudoDelayClass(int delay)
        {
            _delay = delay;
        }
        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Milliseconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            // delay less than 1000 are expired. Hence delay queue behaves as 
            // normal queue for all test samples used in QueueContract.
            return TimeSpan.FromMilliseconds(_delay - 1000);
        }

        public bool Equals(PseudoDelayClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._delay == _delay;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PseudoDelayClass)) return false;
            return Equals((PseudoDelayClass)obj);
        }

        public override int GetHashCode()
        {
            return _delay;
        }

        public override string ToString()
        {
            return (_delay - 1000).ToString();
        }
    }

    public interface IMillesDelayed : IDelayed
    {
        DateTime TriggerTime { get; }
    }

    [Serializable]
    public struct MillisDelayStruct : IMillesDelayed
    {
        static MillisDelayStruct()
        {
            TestDataGenerator.RegisterConverter(i => new MillisDelayStruct(i));
        }

        public static void Load()
        {
        }

        private readonly DateTime _trigger;

        public DateTime TriggerTime
        {
            get { return _trigger; }
        }

        public MillisDelayStruct(int delay)
        {
            _trigger = DateTime.UtcNow.AddMilliseconds(delay);
        }

        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Milliseconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            return _trigger - DateTime.UtcNow;
        }

        public override string ToString()
        {
            return _trigger.TimeOfDay.ToString();
        }
    }

    [Serializable]
    public class MillisDelayClass : IMillesDelayed
    {
        static MillisDelayClass()
        {
            TestDataGenerator.RegisterConverter(i => new MillisDelayClass(i));
        }

        public static void Load()
        {
        }

        private readonly DateTime _trigger;

        public DateTime TriggerTime
        {
            get { return _trigger; }
        }

        public MillisDelayClass()
        {
        }

        public MillisDelayClass(int delay)
        {
            _trigger = DateTime.UtcNow.AddMilliseconds(delay);
        }

        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Milliseconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            return _trigger - DateTime.UtcNow;
        }

        public bool Equals(MillisDelayClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._trigger == _trigger;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MillisDelayClass)) return false;
            return Equals((MillisDelayClass)obj);
        }

        public override int GetHashCode()
        {
            return _trigger.GetHashCode();
        }

        public override string ToString()
        {
            return _trigger.TimeOfDay.ToString();
        }
    }
}
