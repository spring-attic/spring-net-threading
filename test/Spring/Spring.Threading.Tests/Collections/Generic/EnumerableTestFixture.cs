using System.Collections.Generic;

namespace Spring.Collections.Generic
{
    /* Example usage of EnumeratorTestFixture

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ListEnumerableTest<T> : EnumerableTestFixture<T>
        {
            protected override IEnumerable<T> NewEnumerable()
            {
                return new List<T>(TestData<T>.MakeTestArray(55));
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ArrayEnumerableTest<T> : EnumerableTestFixture<T>
        {
            [SetUp] public void SetUp()
            {
                AntiHangingLimit = 600;
            }
            protected override IEnumerable<T> NewEnumerable()
            {
                return TestData<T>.MakeTestArray(555);
            }
        }

     */

    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class EnumerableTestFixture<T> : EnumeratorTestFixture<T>
    {
        protected abstract IEnumerable<T> NewEnumerable();

        protected override sealed IEnumerator<T> NewEnumerator()
        {
            return NewEnumerable().GetEnumerator();
        }
    }
}
