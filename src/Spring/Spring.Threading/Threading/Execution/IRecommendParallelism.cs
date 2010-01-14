using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Threading.Execution
{
    internal interface IRecommendParallelism // NET_ONLY
    {
        int MaxParallelism { get; }
    }
}
