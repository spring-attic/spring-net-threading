using DotNetMock.Dynamic;
using NUnit.Framework;

namespace Spring.Threading.Helpers
{
	[TestFixture]
	public class WaitNodeTests
	{
		[Test]
		public void SignalOnWaitingThread()
		{
			WaitNode node = new WaitNode();
			DynamicMock mock = new DynamicMock(typeof (IQueuedSync));
			mock.Expect("TakeOver", new object[] {node});
			node.Signal((IQueuedSync) mock.Object);
			mock.Verify();
		}
	}
}