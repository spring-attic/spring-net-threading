using System.Collections;

namespace Spring.Collections
{
    /* Example usage of EnumeratorTestFixture

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ListEnumerableTest<T> : EnumerableTestFixture
        {
            protected override IEnumerable NewEnumerable()
            {
                return new ArrayList(TestData<T>.MakeTestArray(55));
            }
        }

        [TestFixture(typeof(string))]
        [TestFixture(typeof(int))]
        public class ArrayEnumerableTest<T> : EnumerableTestFixture
        {
            [SetUp] public void SetUp()
            {
                AntiHangingLimit = 600;
            }
            protected override IEnumerable NewEnumerable()
            {
                return TestData<T>.MakeTestArray(555);
            }
        }

     */

    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IEnumerable"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class EnumerableTestFixture : EnumeratorTestFixture 
    {
        protected abstract IEnumerable NewEnumerable();

        protected override sealed IEnumerator NewEnumerator()
        {
            return NewEnumerable().GetEnumerator();
        }
    }
}
