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

namespace Spring.Threading
{
#if NET_2_0
    /// <summary> 
    /// Delegate for actions that bear results and/or throw Exceptions.
    /// </summary>
    /// <remarks>
    /// This delegate is designed to provide a common protocol for
    /// result-bearing actions that can be run independently in threads, 
    /// in which case they are ordinarily used as the bases of 
    /// <see cref="IRunnable"/>s or <see cref="Task"/>s that set
    /// <see cref="FutureResult{T}"/>.
    /// </remarks>
    /// <seealso cref="ICallable{T}"/>
    /// <seealso cref="FutureResult{T}"/>
    /// <author>Kenneth Xu</author>
    public delegate T Call<T>();
#endif
}
