#region License

/*
* Copyright (C)2008-2009 the original author or authors.
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
        private Action _action;
        private bool _isDelegateCalled;

        [SetUp]
        public void SetUp()
        {
            _isDelegateCalled = false;
            _action = delegate
                        {
                            _isDelegateCalled = true;
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
            _runnable = new Runnable(_action);
            Assert.That(!_isDelegateCalled);
            _runnable.Run();
            Assert.That(_isDelegateCalled);
        }

        [Test]
        public void ExplicitConvertTaskDelegateToRunnable()
        {
            _runnable = (Runnable)_action;
            Assert.That(_runnable, Is.Not.Null);
            Assert.That(!_isDelegateCalled);
            _runnable.Run();
            Assert.That(_isDelegateCalled);
        }

        [Test]
        public void ExplicitConvertNullTaskDelegateToNullRunnable()
        {
            _action = null;
            _runnable = (Runnable)_action;
            Assert.That(_runnable, Is.Null);
        }

        [Test]
        public void ExplicitConvertRunnableToTaskDelegate()
        {
            _runnable = new Runnable(_action);
            Action action = (Action)_runnable;
            Assert.That(action, Is.SameAs(_action));
        }

        [Test]
        public void ExplicitConvertNullRunnableToNullTaskDelegate()
        {
            _runnable = null;
            Action action = (Action)_runnable;
            Assert.That(action, Is.Null);
        }

        [Test] 
        public void EqualsWhenAndOnlyWhenActionEquals()
        {
            _runnable = new Runnable(_action);
            object run = new Runnable(Run);

            Assert.IsTrue(run.Equals(run));

            Assert.IsFalse(run.Equals(null));

            Assert.IsTrue(_runnable.Equals(_runnable));

            Assert.IsTrue(_runnable.Equals(new Runnable(_action)));

            Assert.IsTrue(run.Equals(new Runnable(Run)));

            Assert.IsFalse(_runnable.Equals(new object()));

            Assert.IsFalse(_runnable.Equals(null));

            Assert.IsFalse(_runnable.Equals(run));
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfInnerAction()
        {
            _runnable = new Runnable(_action);
            var run = new Runnable(Run);
            Assert.That(_runnable.GetHashCode(), Is.EqualTo(_action.GetHashCode()));
            Assert.That(run.GetHashCode(), Is.EqualTo(new Action(Run).GetHashCode()));
        }

        private static void Run()
        {
            
        }
    }
}