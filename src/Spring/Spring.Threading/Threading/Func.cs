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

using Spring.Threading.Execution;

namespace Spring.Threading
{
    /// <summary> 
    /// TODO LinqBridge: This should be replaced by LinqBridge.
    /// A delegate that returns a result and may throw an exception.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>Func</c> delegate is similar to <see cref="Action"/>, in that 
    /// both are designed funcitons are potentially executed by another thread.
    /// An <c>Action</c>, however, does not return a result.
    /// </para>
    /// <para>
    /// The <see cref="Executors"/> class contains utility methods to
    /// convert from other common forms to <c>Func</c> delegate.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The result type of the delegate.
    /// </typeparam>
    /// <seealso cref="ICallable{T}"/>
    /// <author>Kenneth Xu</author>
    public delegate T Func<T>(); //NET_ONLY

    /// <summary>
    /// TODO LinqBridge: This should be replaced by LinqBridge.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <param name="a3"></param>
    /// <returns></returns>
    public delegate T Func<T1, T2, T3, T>(T1 a1, T2 a2, T3 a3); //NET_ONLY
    
}
