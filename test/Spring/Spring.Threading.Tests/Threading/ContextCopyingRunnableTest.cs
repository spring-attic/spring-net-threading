using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="ContextCopyingRunnable"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class ContextCopyingRunnableTest
    {
        private ContextCopyingRunnable _sut;
        private IRunnable _runnable;
        private IContextCarrier _contextCarrier;

        [SetUp] public void SetUp()
        {
            _runnable = MockRepository.GenerateStub<IRunnable>();
            _contextCarrier = MockRepository.GenerateStub<IContextCarrier>();
            _sut = new ContextCopyingRunnable(_runnable, _contextCarrier);
        }

        [Test] public void ContextCarrierAccess()
        {
            Assert.That(_sut.ContextCarrier, Is.SameAs(_contextCarrier));
            _sut.ContextCarrier = null;
            Assert.IsNull(_sut.ContextCarrier);
            _sut.ContextCarrier = _contextCarrier;
            Assert.That(_sut.ContextCarrier, Is.SameAs(_contextCarrier));
        }

        [Test] public void RunRestoresContextWhenCarrierIsNotNull()
        {
            _sut.Run();
            Mockery.Assert(
                _contextCarrier.ActivityOf(x=>x.Restore()) 
                < _runnable.ActivityOf(x=>x.Run()));
        }

        [Test] public void RunSuccessWhenCarrierIsNull()
        {
            _sut.ContextCarrier = null;
            _sut.Run();
        }

        [Test] public void ContextCopyingRunnableEqualsRunnableFromSameSource()
        {
            var r = new Runnable(_runnable.Run);
            Assert.That(_sut, Is.EqualTo(r));
            Assert.IsTrue(r.Equals(_sut));
            Assert.IsTrue(_sut.Equals(r));
            Assert.IsTrue(r.Equals(new ContextCopyingRunnable(r, null)));
            Assert.IsTrue(new ContextCopyingRunnable(r, null).Equals(r));
        }
    }
}
