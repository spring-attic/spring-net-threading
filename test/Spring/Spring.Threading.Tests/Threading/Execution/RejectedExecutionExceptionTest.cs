using System;
using NUnit.Framework;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="RejectedExecutionException"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class RejectedExecutionExceptionTest
    {
        const string s = "error message";
        Exception e = new Exception();

        [Test]
        public void RejectedExecutionException()
        {
            new RejectedExecutionException();
        }

        [Test]
        public void RejectedExecutionExceptionWithMessage()
        {
            var sut = new RejectedExecutionException(s);
            Assert.That(sut.Message, Is.EqualTo(s));
        }

        [Test]
        public void RejectedExecutionExceptionWithMessageAndException()
        {
            var sut = new RejectedExecutionException(s, e);
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException, Is.SameAs(e));
        }

        [Test]
        public void RejectedExecutionExceptionWithException()
        {
            var sut = new RejectedExecutionException(e);
            Assert.That(sut.InnerException, Is.SameAs(e));
        }

        [Test]
        public virtual void DeserializedQueueIsSameAsOriginal()
        {
            var ee = new Exception("innner");
            var sut = TestHelper.SerializeAndDeserialize(new RejectedExecutionException(s, e));
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException.Message, Is.EqualTo(e.Message));
        }
    }
}
