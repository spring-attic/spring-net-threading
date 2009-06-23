using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading.Helpers
{
	[TestFixture]
	public class WaitNodeTests
	{
		[Test]
		public void SignalOnWaitingThread()
		{
			WaitNode node = new WaitNode();
            IQueuedSync mockQueuedSync = MockRepository.GenerateMock<IQueuedSync>();
		    node.Signal(mockQueuedSync);
            mockQueuedSync.AssertWasCalled(m=>m.TakeOver(node));
		}
	}
}