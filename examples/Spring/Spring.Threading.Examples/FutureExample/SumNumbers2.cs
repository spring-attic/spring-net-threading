using System;
using System.Threading;

namespace FutureExample
{
    public class SumNumbers2
    {

        public long CalculateSumWithArgsAndReturnValue(int max)
        {
            long sum = 0;
            for (int i = 1; i < max; i++)
            {
                sum += i;
            }
            return sum;
        }        
    }
}