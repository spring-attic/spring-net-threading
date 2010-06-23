using System;
using System.Threading;

namespace ExecutorService
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
        public void CalculateSumWithArgs(int max)
        {
            long sum = 0;
            for (int i = 1; i < max; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.Name + "> CalculateSum: Sum = " + sum);
            Thread.Sleep(1000);            
        }
    }
}