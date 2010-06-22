using System;
using System.Threading;

namespace FutureExample
{
    public class SumNumbers
    {
        private long countUntil = 100;

        public SumNumbers(long countUntil)
        {
            this.countUntil = countUntil;
        }

        public long CalculateSum()
        {
            long sum = 0;
            for (int i = 1; i < countUntil; i++)
            {
                sum += i;
            }
            return sum;           
        }
    }
}