#region License

/*
 * Copyright (C) 2002-2009 the original author or authors.
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
using System.Collections.Generic;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Support for parallel functions don't use local object.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <author>Kenneth Xu</author>
    internal class ParallelCompletion<TSource> : ParallelCompletionBase<TSource> // NET_ONLY
    {
        private readonly Action<TSource, ILoopState> _body;

        internal ParallelCompletion(IExecutor executor, Action<TSource, ILoopState> body)
            : base(executor)
        {
            if (body == null) throw new ArgumentNullException("body");
            _body = body;
        }

        protected override void Process(ILoopState state, IEnumerable<TSource> sources)
        {
            foreach (var source in sources)
            {
                _body(source, state);
            }
        }
    }

    /// <summary>
    /// Support for parallel functions use local object.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TLocal"></typeparam>
    /// <author>Kenneth Xu</author>
    internal class ParallelCompletion<TSource, TLocal> : ParallelCompletionBase<TSource>
    {
        private readonly Func<TLocal> _localInit;
        private readonly Func<TSource, ILoopState, TLocal, TLocal> _body;
        private readonly Action<TLocal> _localFinally;

        internal ParallelCompletion(
            IExecutor executor, 
            Func<TLocal> localInit, 
            Func<TSource, ILoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
            : base(executor)
        {
            if (localInit == null) throw new ArgumentNullException("localInit");
            if (body == null) throw new ArgumentNullException("body");
            if (localFinally == null) throw new ArgumentNullException("localFinally");
            _localInit = localInit;
            _body = body;
            _localFinally = localFinally;
        }

        protected override void Process(ILoopState state, IEnumerable<TSource> sources)
        {
            var local = _localInit();
            try
            {
                foreach (var source in sources)
                {
                    local = _body(source, state, local);
                }
            }
            catch(Exception e)
            {
                // we must handle it here otherwise exception from
                // finally block will mask this exception.
                HandleException(e);
            }
            finally
            {
                _localFinally(local);
            }
        }
    }
}