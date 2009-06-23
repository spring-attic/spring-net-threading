using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading
{
    [TestFixture]
	public class SyncHolderTest
	{
        ISync sync;

        [SetUp]
        public void SetUp ()
        {
            sync = MockRepository.GenerateMock<ISync>();
        }

        class MySemaphore : Semaphore
        {
            public MySemaphore (long initialPermits) : base (initialPermits)
            {}
        }

        [Test]
        public void CanBeUsedWithTheUsingCSharpIdiomToAttemptOnAnISync ()
        {
            MySemaphore sync = new MySemaphore(1);
            using (new SyncHolder(sync, 100))
            {
                Assert.AreEqual(0, sync.Permits);
            }
            Assert.AreEqual(1, sync.Permits);

            sync = new MySemaphore(0);
            try
            {
                using (new SyncHolder(sync, 100))
                {
                    Assert.IsTrue(false, "wrongly entered sync block");
                }
            }
            catch (TimeoutException)
            {
                Assert.AreEqual(0, sync.Permits);
            }
        }

        [Test]
        public void CanBeUsedWithTheUsingCSharpIdiomToAcquireAnIsync()
        {
            Assert.Throws<ThreadStateException>(
                delegate
                    {
                        using (new SyncHolder(sync))
                        {
                            throw new ThreadStateException();
                        }
                    });
            sync.AssertWasCalled(s=>s.Acquire());
            sync.AssertWasCalled(s=>s.Release());
        }
	}
}
