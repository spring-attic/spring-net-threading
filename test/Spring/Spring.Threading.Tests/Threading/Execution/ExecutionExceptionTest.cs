using System;
using NUnit.Framework;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="ExecutionException"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class ExecutionExceptionTest
    {
        const string s = "error message";
        Exception e = new Exception();

        [Test] public void ExecutionException()
        {
            new ExecutionException();
        }

        [Test] public void ExecutionExceptionWithMessage()
        {
            var sut = new ExecutionException(s);
            Assert.That(sut.Message, Is.EqualTo(s));
        }

        [Test] public void ExecutionExceptionWithMessageAndException()
        {
            var sut = new ExecutionException(s, e);
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException, Is.SameAs(e));
        }

        [Test] public virtual void DeserializedQueueIsSameAsOriginal()
        {
            var ee = new Exception("innner");
            var sut = TestHelper.SerializeAndDeserialize(new ExecutionException(s, e));
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException.Message, Is.EqualTo(e.Message));
        }
    }
}
