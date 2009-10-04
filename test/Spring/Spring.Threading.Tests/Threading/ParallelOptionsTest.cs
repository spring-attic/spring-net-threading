using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="ParallelOptions"/>.
    /// </summary>
    [TestFixture] public class ParallelOptionsTest
    {
        [Test] public void MaxDegreeOfParallelismDefaultToNegativeOne()
        {
            Assert.That(new ParallelOptions().MaxDegreeOfParallelism, Is.EqualTo(-1));
        }

        [Test] public void MaxDegreeOfParallelismSetterChokesOnZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                ()=>new ParallelOptions{MaxDegreeOfParallelism = 0});
        }

        [Test] public void MaxDegreeOfParallelismSetterChokesOnValueLessThenNegativeOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ParallelOptions { MaxDegreeOfParallelism = -2 });
        }

        [Test] public void MaxDegreeOfParallelismPropertyBehavior()
        {
            var po = new ParallelOptions {MaxDegreeOfParallelism = 10};
            Assert.That(po.MaxDegreeOfParallelism, Is.EqualTo(10));
            po.MaxDegreeOfParallelism = 3;
            Assert.That(po.MaxDegreeOfParallelism, Is.EqualTo(3));
        }
    }
}
