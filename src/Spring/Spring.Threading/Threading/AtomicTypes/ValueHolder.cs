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

namespace Spring.Threading.AtomicTypes
{
    /// <summary>
    /// Used to hold a value type in a reference type.
    /// </summary>
    /// <typeparam name="T">The type of the value to hold.</typeparam>
    /// <author>Kenneth Xu</author>
    [Serializable]
    internal class ValueHolder<T>
    {
        internal readonly T Value;
        internal ValueHolder(){}
        internal ValueHolder(T value) { Value = value; }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}