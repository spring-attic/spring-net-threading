using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Threading.Execution
{
    internal interface IRecommendParallelism
    {
        int MaxParallelism { get; }
    }
}
