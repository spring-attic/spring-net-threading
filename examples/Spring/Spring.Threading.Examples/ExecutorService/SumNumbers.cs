using System;
using System.Threading;

namespace Spring.Threading.Examples
{
    public class SumNumbers
    {
        private long countUntil = 100;

        public SumNumbers(long countUntil)
        {
            this.countUntil = countUntil;
        }

        public void CalculateSum()
        {
            long sum = 0;
            for (int i = 1; i < countUntil; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.Name + "> CalculateSum: Sum = " + sum);
            Thread.Sleep(1000);            
        }
    }
}