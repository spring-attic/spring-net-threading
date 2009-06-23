using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="AbstractBlockingQueue{T}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture(typeof(string))]
    [TestFixture(typeof(int))]
    public class AbstractBlockingQueueTest<T>
    {
        private AbstractBlockingQueue<T> _testee;
        private MockRepository _mockery;
        private Action<T> _mockAction;
        private ICollection<T> _mockCollection;

        [SetUp] public void SetUp()
        {
            _mockery = new MockRepository();
            _testee = _mockery.PartialMock<AbstractBlockingQueue<T>>();
            _mockAction = _mockery.Stub<Action<T>>();
            _mockCollection = _mockery.Stub<ICollection<T>>();
        }

        [Test] public void IsSynchronizedReturnsTrue()
        {
            _mockery.ReplayAll();
            Assert.IsTrue(((System.Collections.ICollection)_testee).IsSynchronized);
        }

        [Test] public void DrainChokesOnNullAction()
        {
            _mockery.ReplayAll();
            var e = Assert.Throws<ArgumentNullException>(() => _testee.Drain(null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
            e = Assert.Throws<ArgumentNullException>(() => _testee.Drain(null, 0));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void DrainToChokesOnNullCollection()
        {
            _mockery.ReplayAll();
            var e = Assert.Throws<ArgumentNullException>(() => _testee.DrainTo(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
            e = Assert.Throws<ArgumentNullException>(() => _testee.DrainTo(null, 0));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void DrainToChokesWhenDrainToSelf()
        {
            _mockery.ReplayAll();
            var e = Assert.Throws<ArgumentException>(() => _testee.DrainTo(_testee));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
            e = Assert.Throws<ArgumentException>(() => _testee.DrainTo(_testee, 0));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void DrainDoesNothingOnNonPositiveMaxElement([Values(0, -1)] int maxElement)
        {
            _mockery.ReplayAll();
            Assert.That(_testee.Drain(_mockAction, maxElement), Is.EqualTo(0));
        }

        [Test] public void DrainToDoesNothingOnNonPositiveMaxElement([Values(0, -1)] int maxElement)
        {
            _mockery.ReplayAll();
            Assert.That(_testee.DrainTo(_mockCollection, maxElement), Is.EqualTo(0));
        }

        [Test] public void DrainToUnlimitedDelegateToDoDrainToVirtual()
        {
            const int result = 10;
            Expect.Call(_testee.DoDrainTo(_mockAction)).IgnoreArguments().Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.DrainTo(_mockCollection), Is.EqualTo(result));
        }

        [Test] public void DrainUnlimitedDelegateToDoDrainToVirtual()
        {
            const int result = 10;
            Expect.Call(_testee.DoDrainTo(_mockAction)).Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.Drain(_mockAction), Is.EqualTo(result));
        }

        [Test] public void DrainToUnlimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            Expect.Call(_testee.DoDrainTo(_mockAction, int.MaxValue)).IgnoreArguments().Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.DrainTo(_mockCollection), Is.EqualTo(result));
        }

        [Test] public void DrainUnlimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            Expect.Call(_testee.DoDrainTo(_mockAction, int.MaxValue)).Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.Drain(_mockAction), Is.EqualTo(result));
        }

        [Test] public void DrainToLimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            const int limit = 5;
            Expect.Call(_testee.DoDrainTo(_mockCollection.Add, limit)).Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.DrainTo(_mockCollection, limit), Is.EqualTo(result));
        }

        [Test] public void DrainLimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            const int limit = 5;
            Expect.Call(_testee.DoDrainTo(_mockAction, limit)).Return(result);
            _mockery.ReplayAll();
            Assert.That(_testee.Drain(_mockAction, limit), Is.EqualTo(result));
        }
    }
}
