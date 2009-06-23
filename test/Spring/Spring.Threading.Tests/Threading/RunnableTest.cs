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
    /// Test cases for class <see cref="Runnable"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class RunnableTest
    {
        private Runnable _runnable;
        private Task _task;
        private bool _isDelegateTasked;

        [SetUp]
        public void SetUp()
        {
            _isDelegateTasked = false;
            _task = delegate
                        {
                            _isDelegateTasked = true;
                        };
        }

        [Test]
        public void ConstructorChokesOnNullParameter()
        {
            Assert.That(
                delegate { new Runnable(null); },
                Throws.InstanceOf(typeof(ArgumentNullException)));
        }

        [Test]
        public void TaskReturnsTheResultOfDelegate()
        {
            _runnable = new Runnable(_task);
            Assert.That(!_isDelegateTasked);
            _runnable.Run();
            Assert.That(_isDelegateTasked);
        }

        [Test]
        public void ImplicitConvertTaskDelegateToRunnable()
        {
            _runnable = _task;
            Assert.That(_runnable, Is.Not.Null);
            Assert.That(!_isDelegateTasked);
            _runnable.Run();
            Assert.That(_isDelegateTasked);
        }

        [Test]
        public void ImplicitConvertNullTaskDelegateToNullRunnable()
        {
            _task = null;
            _runnable = _task;
            Assert.That(_runnable, Is.Null);
        }

        [Test]
        public void ImplicitConvertRunnableToTaskDelegate()
        {
            _runnable = new Runnable(_task);
            Task task = _runnable;
            Assert.That(task, Is.SameAs(_task));
        }

        [Test]
        public void ImplicitConvertNullRunnableToNullTaskDelegate()
        {
            _runnable = null;
            Task task = _runnable;
            Assert.That(task, Is.Null);
        }

    }
}