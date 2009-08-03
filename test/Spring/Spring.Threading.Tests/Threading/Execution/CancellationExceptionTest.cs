using System;
using NUnit.Framework;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="CancellationException"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class CancellationExceptionTest
    {
        const string s = "error message";
        Exception e = new Exception();

        [Test] public void CancellationException()
        {
            new CancellationException();
        }

        [Test] public void CancellationExceptionWithMessage()
        {
            var sut = new CancellationException(s);
            Assert.That(sut.Message, Is.EqualTo(s));
        }

        [Test] public void CancellationExceptionWithMessageAndException()
        {
            var sut = new CancellationException(s, e);
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException, Is.SameAs(e));
        }
    
        [Test] public virtual void DeserializedQueueIsSameAsOriginal()
        {
            var ee = new Exception("innner");
            var sut = TestHelper.SerializeAndDeserialize(new CancellationException(s, e));
            Assert.That(sut.Message, Is.EqualTo(s));
            Assert.That(sut.InnerException.Message, Is.EqualTo(e.Message));
        }
    }
}
