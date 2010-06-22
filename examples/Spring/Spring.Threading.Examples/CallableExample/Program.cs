using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Spring.Threading.Execution;
using Spring.Threading.Future;

namespace FutureExample
{

    class Program
    {
        private static readonly int THREAD_POOL_SIZE = 10;

        static void Main(string[] args)
        {

            IExecutorService executorService = Executors.NewFixedThreadPool(THREAD_POOL_SIZE);

            IFuture<int>  f1 = executorService.Submit<int>(GenerateNumbers);
            IFuture<string> f2 = executorService.Submit<string>(PrintCharacters);
            IFuture<int> f3 = executorService.Submit<int>(PrintArray);

            Console.WriteLine("Numbers generated till {0}", f1.GetResult());
            Console.WriteLine("Original String {0}", f2.GetResult());
            Console.WriteLine("Array Count {0}", f3.GetResult());


            Console.WriteLine("---------");
            Console.WriteLine("Calculating sums...");
            var list = new List<IFuture<long>>();

            for (int i = 0; i < 20000; i++)
            {
                SumNumbers sumNumbers = new SumNumbers(100 + i);
                IFuture<long> submit = executorService.Submit<long>(sumNumbers.CalculateSum);                
                list.Add(submit);
            }


            // This will make the executor accept no new threads
            // and finish all existing threads in the queue
            executorService.Shutdown();

            long sum = 0;
            foreach (var future in list)
            {
                sum += future.GetResult();
            }
            Console.WriteLine("Sum = " + sum);

            Console.WriteLine("Hit return to exit");
            Console.ReadLine();

        }

        static int GenerateNumbers()
        {
            int i;
            for (i = 0; i < 7; i++)
            {
                Console.WriteLine("Method1 - Number: {0}", i);
                Thread.Sleep(1000);
            }
            return i;
        }

        static string PrintCharacters()
        {
            string str = "dotnet";
            for (int i = 0; i < str.Length; i++)
            {
                Console.WriteLine("Method2 - Character: {0}", str[i]);
                Thread.Sleep(1000);
            }
            return str;
        }

        static int PrintArray()
        {
            int[] arr = { 1, 2, 3, 4, 5 };
            foreach (int i in arr)
            {
                Console.WriteLine("Method3 - Array: {0}", i);
                Thread.Sleep(1000);
            }
            return arr.Count();
        }
    }
}
