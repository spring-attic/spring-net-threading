using System;
using System.Collections.Generic;
using System.Text;
using Spring.Threading.Examples;
using Spring.Threading.Execution;

namespace ExecutorService
{
    class Program
    {
        private static readonly int THREAD_POOL_SIZE = 10;

        static void Main(string[] args)
        {
            IExecutorService executorService = Executors.NewFixedThreadPool(THREAD_POOL_SIZE);
            for (int i = 0; i < 100; i++)
            {
                SumNumbers sumNumbers = new SumNumbers(10000000 + i);
                executorService.Execute(sumNumbers.CalculateSum);
            }

            // This will make the executor accept no new threads
            // and finish all existing threads in the queue
            executorService.Shutdown();

            // Wait until all threads are finish
            while (!executorService.IsTerminated) { }

            Console.WriteLine("Finished all threads.  Hit enter to exit");
            Console.ReadLine();

        }
    }
}
