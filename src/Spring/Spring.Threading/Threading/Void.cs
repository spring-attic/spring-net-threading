using System;

namespace Spring.Threading
{
    /// <summary>
    /// A never instantialable class that can be used as generic type paramter
    /// to indicate that the type is irrelevant and ignored.
    /// </summary>
    public sealed class Void
    {
        private Void()
        {
            throw new InvalidOperationException();
        }
    }
}
