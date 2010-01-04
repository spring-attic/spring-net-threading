using System;
using NUnit.CommonFixtures;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="AbstractQueue{T}"/>
    /// </summary>
    [TestFixture(typeof(string))] // reference type
    [TestFixture(typeof(int))] // value type
    public class AbstractQueueTest<T>
    {
        private AbstractQueue<T> _sut;
#if !PHASED
        private IQueue _sutAsNonGeneric;
#endif
        readonly Action<T> _action = x => { };


        [SetUp] public void SetUp()
        {
            _sut = Mockery.GeneratePartialMock<AbstractQueue<T>>();
#if !PHASED
            _sutAsNonGeneric = _sut;
#endif
        }

#if !PHASED
        [Test] public void NonGenericAddDelegatesToGenericAdd()
        {
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything));
            _sutAsNonGeneric.Add(TestData<T>.One);
            _sut.AssertWasCalled(x => x.Add(TestData<T>.One));
        }

        [Test] public void NonGenericAddChokesOnNonCompatibleParameterType()
        {
            Assert.Throws<InvalidCastException>(delegate {
                _sutAsNonGeneric.Add(new object());
            });
        }

        [Test] public void NonGenericRemoveDelegatesToGenericRemove()
        {
            _sut.Stub(x => x.Remove()).Return(TestData<T>.Two);
            Assert.That(_sutAsNonGeneric.Remove(), Is.EqualTo(TestData<T>.Two));
            _sut.AssertWasCalled(x=>x.Remove());
        }

        [Test] public void NonGenericElementDelegatesToGenericElement()
        {
            _sut.Stub(x=>x.Element()).Return(TestData<T>.Three);
            Assert.That(_sutAsNonGeneric.Element(), Is.EqualTo(TestData<T>.Three));
            _sut.AssertWasCalled(x => x.Element());
        }

        [Test] public void NonGenericPeekDelegatesToGenericPeek()
        {
            _sut.Stub(x => x.Peek(out Arg<T>.Out(TestData<T>.Four).Dummy)).Return(true);
            Assert.That(_sutAsNonGeneric.Peek(), Is.EqualTo(TestData<T>.Four));
            T dummy; _sut.AssertWasCalled(x => x.Peek(out dummy));
        }

        [Test] public void NonGenericPeekReturnsNullWhenQueueEmpty()
        {
            T dummy;
            _sut.Stub(x=>x.Peek(out dummy)).Return(false);
            Assert.That(_sutAsNonGeneric.Peek(), Is.Null);
            _sut.AssertWasCalled(x => x.Peek(out dummy));
        }

        [Test] public void NonGenericPollDelegatesToGenericPoll()
        {
            T dummy;
            _sut.Stub(x=>x.Poll(out dummy)).Return(true).OutRef(TestData<T>.Five);
            Assert.That(_sutAsNonGeneric.Poll(), Is.EqualTo(TestData<T>.Five));
            _sut.AssertWasCalled(x => x.Poll(out dummy));
        }

        [Test] public void NonGenericPollReturnsNullWhenQueueEmpty()
        {
            T dummy;
            _sut.Stub(x=>x.Poll(out dummy)).Return(false);
            Assert.That(_sutAsNonGeneric.Poll(), Is.Null);
            _sut.AssertWasCalled(x => x.Poll(out dummy));
        }

        [Test] public void NonGenericOfferDelegatesToGenericOffer()
        {
            _sut.Stub(x=>x.Offer(TestData<T>.One)).Return(true);
            _sut.Stub(x=>x.Offer(TestData<T>.Two)).Return(false);

            Assert.That(_sutAsNonGeneric.Offer(TestData<T>.One), Is.True);
            Assert.That(_sutAsNonGeneric.Offer(TestData<T>.Two), Is.False);

            _sut.AssertWasCalled(x => x.Offer(Arg<T>.Is.Anything), m=>m.Repeat.Times(2, Int32.MaxValue));
        }

        [Test] public void NonGenericOfferChokesOnNonCompatibleParameterType()
        {
            Assert.Throws<InvalidCastException>(delegate {
                _sutAsNonGeneric.Offer(new object());
            });
        }

        [Test] public void IsEmptyReturnTrueWhenCountIsZero()
        {
            _sut.Stub(x=>x.Count).Return(0);
            Assert.That(_sutAsNonGeneric.IsEmpty, Is.True);
        }

        [Test] public void IsEmptyReturnFalseWhenCountIsNotZero()
        {
            _sut.Stub(x=>x.Count).Return(1);
            Assert.That(_sutAsNonGeneric.IsEmpty, Is.False);
        }

#endif

        [Test] public void AddDelegatesToOffer()
        {
            _sut.Stub(x=>x.Offer(Arg<T>.Is.Anything)).Return(true);
            _sut.Add(TestData<T>.One);
            _sut.AssertWasCalled(x => x.Offer(TestData<T>.One));
        }

        [Test] public void AddChokesWhenOfferReturnFalse()
        {
            _sut.Stub(x=>x.Offer(Arg<T>.Is.Anything)).Return(false);
            Assert.Throws<InvalidOperationException>(() => _sut.Add(TestData<T>.One));
            _sut.AssertWasCalled(x => x.Offer(TestData<T>.One));
        }

        [Test] public void RemoveDelegatesToPoll()
        {
            _sut.Stub(x=>x.Poll(out Arg<T>.Out(TestData<T>.Two).Dummy)).Return(true);
            Assert.That(_sut.Remove(), Is.EqualTo(TestData<T>.Two));
            T dummy;  _sut.AssertWasCalled(x => x.Poll(out dummy));
        }

        [Test] public void RemoveChokesWhenPollReturnFalse()
        {
            T dummy;
            _sut.Stub(x=>x.Poll(out dummy)).Return(false);
            Assert.Catch<InvalidOperationException>(() => _sut.Remove());
            _sut.AssertWasCalled(x => x.Poll(out dummy));
        }

        [Test] public void ElementDelegatesToPeek()
        {
            _sut.Stub(x=>x.Peek(out Arg<T>.Out(TestData<T>.Two).Dummy)).Return(true);
            Assert.That(_sut.Element(), Is.EqualTo(TestData<T>.Two));
            T dummy; _sut.AssertWasCalled(x => x.Peek(out dummy));
        }

        [Test] public void ElementChokesWhenPeekReturnFalse()
        {
            T dummy;
            _sut.Stub(x=>x.Peek(out dummy)).Return(false);
            Assert.Catch<InvalidOperationException>(() => _sut.Element());
            _sut.AssertWasCalled(x => x.Peek(out dummy));
        }

        [Test] public void ClearPollsUntilEmpty()
        {
            T dummy;
            _sut.Stub(x=>x.Poll(out dummy)).Return(true).Repeat.Times(5);
            _sut.Stub(x=>x.Poll(out dummy)).Return(false);
            _sut.Clear();
            _sut.AssertWasCalled(x => x.Poll(out dummy), m => m.Repeat.Times(6));
        }

        [Test] public void IsReadOnlyAlwaysReturnFalse()
        {
            Assert.IsFalse(_sut.IsReadOnly);
        }

        [Test] public void DrainChokesOnNullAction()
        {
            var e = Assert.Throws<ArgumentNullException>(() => _sut.Drain(null));
            Assert.That(e.ParamName, Is.EqualTo("action"));
            e = Assert.Throws<ArgumentNullException>(() => _sut.Drain(null, 0));
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test] public void DrainDoesNothingOnNonPositiveMaxElement([Values(0, -1)] int maxElement)
        {
            Assert.That(_sut.Drain(x=>{}, maxElement), Is.EqualTo(0));
        }

        [Test] public void DrainUnlimitedDelegateToDoDrainToVirtual()
        {
            const int result = 10;
            _sut.Stub(x => x.DoDrain(_action, null)).Return(result);
            Assert.That(_sut.Drain(_action), Is.EqualTo(result));
            _sut.AssertWasCalled(x => x.DoDrain(_action, null));
        }

        [Test] public void DrainUnlimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            _sut.Stub(x=>x.DoDrain(_action, int.MaxValue, null)).Return(result);
            Assert.That(_sut.Drain(_action), Is.EqualTo(result));
            _sut.AssertWasCalled(x => x.DoDrain(_action, int.MaxValue, null));
        }

        [Test] public void DrainLimitedDelegateToDoDrainToAbstract()
        {
            const int result = 10;
            const int limit = 5;
            _sut.Stub(x=>x.DoDrain(null, limit, null)).IgnoreArguments().Return(result);
            Assert.That(_sut.Drain(_action, limit), Is.EqualTo(result));
            _sut.Stub(x => x.DoDrain(_action, limit, null)).Return(result);
        }
    }
}
