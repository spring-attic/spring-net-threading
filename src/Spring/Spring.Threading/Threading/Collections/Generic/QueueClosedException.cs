using System;

namespace Spring.Threading.Collections.Generic
{
    /// <summary>
    /// Exception to indicate a queue is already closed.
    /// </summary>
    [Serializable]
    public class QueueClosedException : Exception
    {
    }
}