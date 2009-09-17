using System.Collections;

namespace Spring.TestFixture.Collections.NonGeneric
{
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