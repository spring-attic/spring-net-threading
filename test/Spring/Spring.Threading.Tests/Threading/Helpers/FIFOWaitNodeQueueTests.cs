using System;
using System.Threading;
using NUnit.Framework;

namespace Spring.Threading.Helpers
{
	[TestFixture]
	public class FIFOWaitNodeQueueTests
	{
		[Test]
			public void FIFOWaitQueueNoNodesCount()
		{
			FIFOWaitQueue queue = new FIFOWaitQueue();
			Assert.AreEqual(0, queue.Length);
		}
		[Test]
			public void EnqueueAndDequeueElements()
		{
			WaitNode node = new WaitNode();
			WaitNode node1 = new WaitNode();
			FIFOWaitQueue queue = new FIFOWaitQueue();
			Assert.AreEqual(null, queue.Dequeue());
			queue.Enqueue(node1);
			queue.Enqueue(node);
			Assert.AreEqual(2, queue.Length);
			Assert.IsTrue(queue.HasNodes);
			Assert.AreEqual(node1, queue.Dequeue());
			Assert.AreEqual(node, queue.Dequeue());
			Assert.IsFalse(queue.HasNodes);
		}
		[Test]
			public void WaitingThreads()
		{
			WaitNode node = new WaitNode();
			FIFOWaitQueue queue = new FIFOWaitQueue();
			Assert.AreEqual(0, queue.WaitingThreads.Count);
			Assert.IsFalse(queue.IsWaiting(Thread.CurrentThread));
			queue.Enqueue(node);
			Assert.AreEqual(1, queue.WaitingThreads.Count);
			Assert.IsTrue(queue.IsWaiting(Thread.CurrentThread));
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void IsWaitingWithNullThread()
		{
			FIFOWaitQueue queue = new FIFOWaitQueue();
			Assert.IsFalse(queue.IsWaiting(null));
		}
	}
}