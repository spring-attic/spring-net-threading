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
using NUnit.Framework;
using Spring.TestFixtures.Threading.Collections.Generic;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Class DelayQueueTest
    /// </summary>
    /// <author>Kenneth Xu</author>
    //[TestFixture] 
    public class DelayQueueTest<T> where T : IDelayed
    {
        [TestFixture(typeof(DelayedStruct))]
        [TestFixture(typeof(DelayedClass))]
        public class AsGeneric : BlockingQueueContract<T>
        {
            static AsGeneric()
            {
                DelayedClass.Load();
                DelayedStruct.Load();
            }

            public AsGeneric()
                : base(CollectionContractOptions.Unbounded | CollectionContractOptions.NoNull)
            {
            }

            protected override IBlockingQueue<T> NewBlockingQueue()
            {
                return new DelayQueue<T>();
            }
        }
    }

    [Serializable]
    public struct DelayedStruct : IDelayed
    {
        static DelayedStruct()
        {
            TestDataGenerator.RegisterConverter(i => new DelayedStruct(i));
        }

        public static void Load()
        {
        }

        private readonly int _delay;

        public DelayedStruct(int delay)
        {
            _delay = delay;
        }
        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Seconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            // delay less than 1000 are expired. Hence delay queue behaves as 
            // normal queue for all test samples used in QueueContract.
            return TimeSpan.FromSeconds(_delay - 1000);
        }

        public override string ToString()
        {
            return (_delay - 1000).ToString();
        }
    }

    [Serializable]
    public class DelayedClass : IDelayed
    {
        static DelayedClass()
        {
            TestDataGenerator.RegisterConverter(i => new DelayedClass(i));
        }

        public static void Load()
        {
        }

        private readonly int _delay;

        public DelayedClass(int delay)
        {
            _delay = delay;
        }
        public int CompareTo(IDelayed other)
        {
            return (GetRemainingDelay() - other.GetRemainingDelay()).Seconds;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((IDelayed)obj);
        }

        public TimeSpan GetRemainingDelay()
        {
            // delay less than 1000 are expired. Hence delay queue behaves as 
            // normal queue for all test samples used in QueueContract.
            return TimeSpan.FromSeconds(_delay - 1000);
        }

        public bool Equals(DelayedClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._delay == _delay;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DelayedClass)) return false;
            return Equals((DelayedClass)obj);
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
}
