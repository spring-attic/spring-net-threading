using System;
using System.Collections.Generic;
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
        private ICollection<T> _mockCollection;

        [SetUp] public void SetUp()
        {
            _testee = Mockery.GeneratePartialMock<AbstractBlockingQueue<T>>();
            _mockCollection = MockRepository.GenerateStub<ICollection<T>>();
        }

        [Test] public void IsSynchronizedReturnsTrue()
        {
            Assert.IsTrue(((System.Collections.ICollection)_testee).IsSynchronized);
        }

        [Test] public void DrainToChokesOnNullCollection()
        {
            var e = Assert.Throws<ArgumentNullException>(() => _testee.DrainTo(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
            e = Assert.Throws<ArgumentNullException>(() => _testee.DrainTo(null, 0));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void DrainToChokesWhenDrainToSelf()
        {
            var e = Assert.Throws<ArgumentException>(() => _testee.DrainTo(_testee));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
            e = Assert.Throws<ArgumentException>(() => _testee.DrainTo(_testee, 0));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void DrainToDoesNothingOnNonPositiveMaxElement([Values(0, -1)] int maxElement)
        {
            Assert.That(_testee.DrainTo(_mockCollection, maxElement), Is.EqualTo(0));
            _testee.AssertWasNotCalled(x=>x.DoDrain(null, 0, null), m=>m.IgnoreArguments());
        }

        [Test] public void DrainToUnlimitedDelegateToDoDrainToVirtual()
        {
            const int result = 10;
            _testee.Stub(x=>x.DoDrain(null, null)).IgnoreArguments().Return(result);
            Assert.That(_testee.DrainTo(_mockCollection), Is.EqualTo(result));
            _testee.AssertWasCalled(x => x.DoDrain(_mockCollection.Add, null));
        }

        [Test] public void DrainToUnlimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            _testee.Stub(x=>x.DoDrain(null, 0, null)).IgnoreArguments().Return(result);
            Assert.That(_testee.DrainTo(_mockCollection), Is.EqualTo(result));
            _testee.AssertWasCalled(x => x.DoDrain(_mockCollection.Add, int.MaxValue, null));
        }

        [Test] public void DrainToLimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            const int limit = 5;
            _testee.Stub(x=>x.DoDrain(null, 0, null)).IgnoreArguments().Return(result);
            Assert.That(_testee.DrainTo(_mockCollection, limit), Is.EqualTo(result));
            _testee.AssertWasCalled(x => x.DoDrain(_mockCollection.Add, limit, null));
        }

    	[Test] public void AddRangeChokesWhenNotEnoughRoom() {
            _testee.Stub(x => x.Add(Arg<T>.Is.Anything)).Repeat.Once();
            _testee.Stub(x => x.Add(Arg<T>.Is.Anything)).Throw(new InvalidOperationException()).Repeat.Once();

            Assert.Throws<InvalidOperationException>(
                ()=>_testee.AddRange(TestData<T>.MakeTestArray(2)));
    	}

    }
}
