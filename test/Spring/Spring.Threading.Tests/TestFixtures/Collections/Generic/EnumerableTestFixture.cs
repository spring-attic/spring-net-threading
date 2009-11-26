using System.Collections.Generic;

namespace Spring.TestFixtures.Collections.Generic
{
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