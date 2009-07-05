#region License

/*
* Copyright (C)2008-2009 the original author or authors.
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

namespace Spring.Threading
{
    /// <summary>
    /// Class to convert <see cref="Action"/> to <see cref="IRunnable"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class Runnable : IRunnable
    {
        private readonly Action _action;

        /// <summary>
        /// Construct a new instance of <see cref="Runnable"/> which calls
        /// <paramref name="action"/> delegate with its <see cref="Run"/> method
        /// is invoked.
        /// </summary>
        /// <param name="action">
        /// The delegate to be called when <see cref="Run"/> is invoked.
        /// </param>
        public Runnable(Action action)
        {
            if (action == null) throw new ArgumentNullException("task");
            _action = action;
        }

        #region IRunnable Members

        /// <summary>
        /// The entry point. Invokes the delegate passed to the constructor
        /// <see cref="Runnable(Action)"/>.
        /// </summary>
        public void Run()
        {
            _action();
        }

        #endregion

        /// <summary>
        /// Implicitly converts <see cref="Action"/> delegate to an instance
        /// of <see cref="Runnable"/>.
        /// </summary>
        /// <param name="action">
        /// The delegate to be converted to <see cref="Runnable"/>.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Runnable"/> based on <paramref name="action"/>.
        /// </returns>
        public static implicit operator Runnable(Action action)
        {
            return action == null ? null : new Runnable(action);
        }

        /// <summary>
        /// Implicitly converts <see cref="Runnable"/> to <see cref="Action"/>
        /// delegate.
        /// </summary>
        /// <param name="runnable">
        /// The callable to be converted to <see cref="Action"/>.
        /// </param>
        /// <returns>
        /// The original <see cref="Action"/> delegate used to construct the
        /// <paramref name="runnable"/>.
        /// </returns>
        public static implicit operator Action(Runnable runnable)
        {
            return runnable == null ? null : runnable._action;
        }
    }
}
