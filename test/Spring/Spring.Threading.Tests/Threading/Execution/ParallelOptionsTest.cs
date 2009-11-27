using System;
using NUnit.Framework;

namespace Spring.Threading.Execution
{
    /// <summary>
    /// Test cases for <see cref="ParallelOptions"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class ParallelOptionsTest
    {
        [Test] public void MaxDegreeOfParallelismDefaultToNegativeOne()
        {
            Assert.That(new ParallelOptions().MaxDegreeOfParallelism, Is.EqualTo(-1));
        }

        [TestCase(int.MaxValue, Result = int.MaxValue)]
        [TestCase(1, Result = 1)]
        [TestCase(0, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [TestCase(-1, Result = -1)]
        [TestCase(-2, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public int MaxDegreeOfParallelismSet(int value)
        {
            var po = new ParallelOptions {MaxDegreeOfParallelism = value};
            return po.MaxDegreeOfParallelism;
        }
    }
}
