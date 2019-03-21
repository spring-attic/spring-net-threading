#region License
/*
* Copyright (C) 2002-2009 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      https://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace Spring.Threading
{
    /// <summary>
    /// A factory that creates instance of <see cref="IContextCarrier"/> that
    /// copies specified data slots in the "Spring.Threading.LogicalThreadContext, Spring.Core"
    /// and <see cref="Thread.CurrentPrincipal"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class LogicalThreadContextCarrierFactory : IContextCarrierFactory //NET_ONLY
    {
        private static readonly Func<string, object> _getData;
        private static readonly Action<string, object> _setData;

        private IEnumerable<string> _names;

        static LogicalThreadContextCarrierFactory()
        {
            var type = Type.GetType("Spring.Threading.LogicalThreadContext, Spring.Core");
            var getDataMethod = type.GetMethod("GetData", BindingFlags.Static | BindingFlags.Public);
            _getData = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), getDataMethod);
            var setDataMethod = type.GetMethod("SetData", BindingFlags.Static | BindingFlags.Public);
            _setData = (Action<string, object>)Delegate.CreateDelegate(typeof(Action<string, object>), setDataMethod);
        }

        /// <summary>
        /// Gets and sets the names of the data slots to be copied.
        /// </summary>
        public IEnumerable<string> Names
        {
            get { return _names; }
            set { _names = value; }
        }

        /// <summary>
        /// Create a new instance of <see cref="IContextCarrier"/>
        /// </summary>
        /// <returns>A new instance of <see cref="IContextCarrier"/>.</returns>
        public IContextCarrier CreateContextCarrier()
        {
            return new ContextCarrier(_names);
        }

        private class ContextCarrier : IContextCarrier
        {
            private readonly IDictionary<string, object> _contexts;
            private readonly Thread _creatorThread = Thread.CurrentThread;
            private readonly IPrincipal _principal = Thread.CurrentPrincipal;

            internal ContextCarrier(IEnumerable<string> names)
            {
                if (names == null) return;
                _contexts = new Dictionary<string, object>();
                foreach (string name in names)
                {
                    _contexts[name] = _getData(name);
                }
            }

            public void Restore()
            {
                if (Thread.CurrentThread != _creatorThread)
                {
                    Thread.CurrentPrincipal = _principal;
                    if (_contexts == null) return;
                    foreach (KeyValuePair<string, object> pair in _contexts)
                    {
                        _setData(pair.Key, pair.Value);
                    }
                }

            }
        }
    }
}