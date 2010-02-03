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
using System.Diagnostics;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.Generic;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.Framework;

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
        public CopyOnWriteListTest() : base(CollectionContractOptions.WeaklyConsistentEnumerator)
        {
            
        }
        protected override IList<T> NewList()
        {
            return new CopyOnWriteList<T>();
        }

        protected override IList<T> NewListFilledWithSample()
        {
            return new CopyOnWriteList<T>(TestData<T>.MakeTestArray(SampleSize));
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class AsNonGeneric : TypedListContract<T>
        {
            public AsNonGeneric()
                : base(CollectionContractOptions.WeaklyConsistentEnumerator)
            {
                
            }
            protected override IList NewList()
            {
                return new CopyOnWriteList<T>();
            }
        }
    }
}