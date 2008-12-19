using System.Collections;
using System.Collections.Generic;

namespace Spring.Collections.Generic
{
    public interface IOmniList<T> : IOmniCollection<T>, IList<T>, IList
    {
    }
}
