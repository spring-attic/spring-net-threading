using System;
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
        private MockRepository _mockery;
        private AbstractQueue<T> _testee;
        private IQueue _testeeNonGeneric;

        [SetUp] public void SetUp()
        {
            _mockery = new MockRepository();
            _testee = _mockery.PartialMock<AbstractQueue<T>>();
            _testeeNonGeneric = _testee;
        }

        [Test] public void NonGenericAddDelegatesToGenericAdd()
        {
            _testee.Add(TestData<T>.one);
            LastCall.Repeat.Once();

            _mockery.ReplayAll();

            _testeeNonGeneric.Add(TestData<T>.one);

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericAddChokesOnNonCompatibleParameterType()
        {
            Assert.Throws<InvalidCastException>(delegate {
                _testeeNonGeneric.Add(new object());
            });
        }

        [Test] public void NonGenericRemoveDelegatesToGenericRemove()
        {
            Expect.Call(_testee.Remove()).Return(TestData<T>.two);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Remove(), Is.EqualTo(TestData<T>.two));

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericElementDelegatesToGenericElement()
        {
            Expect.Call(_testee.Element()).Return(TestData<T>.three);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Element(), Is.EqualTo(TestData<T>.three));

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericPeekDelegatesToGenericPeek()
        {
            T result;
            Expect.Call(_testee.Peek(out result)).Return(true).OutRef(TestData<T>.four);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Peek(), Is.EqualTo(TestData<T>.four));

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericPeekReturnsNullWhenQueueEmpty()
        {
            T result;
            Expect.Call(_testee.Peek(out result)).Return(false);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Peek(), Is.Null);

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericPollDelegatesToGenericPoll()
        {
            T result;
            Expect.Call(_testee.Poll(out result)).Return(true).OutRef(TestData<T>.five);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Poll(), Is.EqualTo(TestData<T>.five));

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericPollReturnsNullWhenQueueEmpty()
        {
            T result;
            Expect.Call(_testee.Poll(out result)).Return(false);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Poll(), Is.Null);

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericOfferDelegatesToGenericOffer()
        {
            Expect.Call(_testee.Offer(TestData<T>.one)).Return(true);
            Expect.Call(_testee.Offer(TestData<T>.two)).Return(false);

            _mockery.ReplayAll();

            Assert.That(_testeeNonGeneric.Offer(TestData<T>.one), Is.True);
            Assert.That(_testeeNonGeneric.Offer(TestData<T>.two), Is.False);

            _mockery.VerifyAll();
        }

        [Test] public void NonGenericOfferChokesOnNonCompatibleParameterType()
        {
            Assert.Throws<InvalidCastException>(delegate {
                _testeeNonGeneric.Offer(new object());
            });
        }

        [Test] public void AddDelegatesToOffer()
        {
            Expect.Call(_testee.Offer(TestData<T>.one)).Return(true);
            _mockery.ReplayAll();
            _testee.Add(TestData<T>.one);
            _mockery.VerifyAll();
        }

        [Test] public void AddChokesWhenOfferReturnFalse()
        {
            SetupResult.For(_testee.Offer(TestData<T>.one)).IgnoreArguments().Return(false);
            _mockery.ReplayAll();
            Assert.Throws<InvalidOperationException>(
                delegate
                    {
                        _testee.Add(TestData<T>.one);
                    });
        }

        [Test] public void RemoveDelegatesToPoll()
        {
            T result;
            Expect.Call(_testee.Poll(out result)).Return(true).OutRef(TestData<T>.two);

            _mockery.ReplayAll();

            Assert.That(_testee.Remove(), Is.EqualTo(TestData<T>.two));

            _mockery.VerifyAll();
        }

        [Test] public void RemoveChokesWhenPollReturnFalse()
        {
            T result;
            Expect.Call(_testee.Poll(out result)).Return(false);
            _mockery.ReplayAll();
            Assert.Throws<NoElementsException>(
                delegate
                    {
                        _testee.Remove();
                    });
        }

        [Test] public void ElementDelegatesToPeek()
        {
            T result;
            Expect.Call(_testee.Peek(out result)).Return(true).OutRef(TestData<T>.two);

            _mockery.ReplayAll();

            Assert.That(_testee.Element(), Is.EqualTo(TestData<T>.two));

            _mockery.VerifyAll();
        }

        [Test] public void ElementChokesWhenPeekReturnFalse()
        {
            T result;
            Expect.Call(_testee.Peek(out result)).Return(false);
            _mockery.ReplayAll();
            Assert.Throws<NoElementsException>(
                delegate
                    {
                        _testee.Element();
                    });
        }

        [Test] public void ClearPollsUntilEmpty()
        {
            using(_mockery.Ordered())
            {
                T result;
                Expect.Call(_testee.Poll(out result)).Return(true).Repeat.Times(5);
                Expect.Call(_testee.Poll(out result)).Return(false);
            }
            _mockery.ReplayAll();
            _testee.Clear();
            _mockery.VerifyAll();
        }

        [Test] public void IsEmptyReturnTrueWhenCountIsZero()
        {
            Expect.Call(_testee.Count).Return(0);
            _mockery.ReplayAll();
            Assert.That(_testeeNonGeneric.IsEmpty, Is.True);
            _mockery.VerifyAll();
        }

        [Test] public void IsReadOnlyAlwaysReturnFalse()
        {
            _mockery.ReplayAll();
            Assert.IsFalse(_testee.IsReadOnly);
        }

        [Test] public void IsEmptyReturnFalseWhenCountIsNotZero()
        {
            Expect.Call(_testee.Count).Return(1);
            _mockery.ReplayAll();
            Assert.That(_testeeNonGeneric.IsEmpty, Is.False);
            _mockery.VerifyAll();
        }

        [Test] public void AddRangeCallsAddOneByOne()
        {
            T[] testData = TestData<T>.MakeTestArray(5);
            foreach (T data in testData) _testee.Add(data);
            _mockery.ReplayAll();
            Assert.That(_testee.AddRange(testData), Is.True);
            _mockery.VerifyAll();
        }

        [Test] public void AddRangeReturnsFalseWhenParameterIsEmptyCollection()
        {
            _mockery.ReplayAll();
            T[] testData = new T[0];
            Assert.That(_testee.AddRange(testData), Is.False);
        }

        [Test] public void AddRageChokesOnNullParameter()
        {
            _mockery.ReplayAll();
            Assert.Throws<ArgumentNullException>(
                delegate { _testee.AddRange(null); });
        }

        [Test] public void AddRangeChokesWhenParameterIsItself()
        {
            _mockery.ReplayAll();
            Assert.Throws<ArgumentException>(
                delegate { _testee.AddRange(_testee); });
        }
    }
}
