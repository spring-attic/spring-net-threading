using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;

namespace Spring.Threading.Tests.Collections {
    [TestFixture]
    public class LinkedBlockingQueueTest {
        [Test]
        public void TestPollOnEmptyQueue() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            string retval;
            Assert.IsFalse(queue.Poll(TimeSpan.Zero, out retval));
        }

        [Test]
        public void TestEnumerator() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            Assert.That(queue.Count, Is.EqualTo(3));

            int count = 0;
            foreach(String s in queue) {
                ++count;
            }

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void TestEnumeratorWithConcurrentTakeBetweenMoveAndGetCurrent() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            iter.MoveNext();
            queue.Take();
            Assert.That(iter.Current, Is.EqualTo("test1"));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestEnumeratorWithChangingCollectionBetweenTwoMoves() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            iter.MoveNext();
            queue.Take();
            iter.MoveNext();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestEnumeratorWithChangingCollectionBeforeRest() {
            IBlockingQueue<string> queue = new LinkedBlockingQueue<string>();
            queue.Add("test1");
            queue.Add("test2");
            queue.Add("test3");
            queue.Add("test4");
            Assert.That(queue.Count, Is.EqualTo(4));

            IEnumerator<string> iter = queue.GetEnumerator();
            queue.Take();
            iter.Reset();
        }
    }
}