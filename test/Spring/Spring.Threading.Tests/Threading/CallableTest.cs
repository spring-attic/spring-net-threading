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
using NUnit.Framework;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for class <see cref="Callable{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class CallableTest<T>
    {
        private readonly T _testData = (T)Convert.ChangeType(123, typeof(T));
        private Callable<T> _callable;
        private Call<T> _call;
        private bool _isDelegateCalled;

        [SetUp] public void SetUp()
        {
            _isDelegateCalled = false;
            _call = delegate
                        {
                            _isDelegateCalled = true;
                            return _testData;
                        };
        }

        [Test] public void ConstructorChokesOnNullParameter()
        {
            Assert.That(
                delegate { new Callable<T>(null); }, 
                Throws.InstanceOf(typeof(ArgumentNullException)));
        }

        [Test] public void CallReturnsTheResultOfDelegate()
        {
            _callable = new Callable<T>(_call);
            Assert.That(!_isDelegateCalled);
            Assert.That(_callable.Call(), Is.EqualTo(_testData));
            Assert.That(_isDelegateCalled);
        }

        [Test] public void ImplicitConvertCallDelegateToCallable()
        {
            _callable = _call;
            Assert.That(_callable, Is.Not.Null);
            Assert.That(!_isDelegateCalled);
            Assert.That(_callable.Call(), Is.EqualTo(_testData));
            Assert.That(_isDelegateCalled);
        }

        [Test] public void ImplicitConvertNullCallDelegateToNullCallable()
        {
            _call = null;
            _callable = _call;
            Assert.That(_callable, Is.Null);
        }

        [Test]
        public void ImplicitConvertCallableToCallDelegate()
        {
            _callable = new Callable<T>(_call);
            Call<T> call = _callable;
            Assert.That(call, Is.SameAs(_call));
        }

        [Test] public void ImplicitConvertNullCallableToNullCallDelegate()
        {
            _callable = null;
            Call<T> call = _callable;
            Assert.That(call, Is.Null);
        }

    }
}