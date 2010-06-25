using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Spring.Expressions;
using Spring.Reflection.Dynamic;
using Spring.Threading.Execution;
using Spring.Threading.Future;
using Spring.Util;

namespace FutureExample
{

    /// <summary>
    /// Based on examples from http://www.dotnetcurry.com/ShowArticle.aspx?ID=489&AspxAutoDetectCookieSupport=1
    /// and http://www.vogella.de/articles/JavaConcurrency/article.html#futures
    /// </summary>
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
            
            var futures = new List<IFuture<long>>();
            
            // Call without method arguments
            for (int i = 0; i < 20000; i++)
            {
                SumNumbers sumNumbers = new SumNumbers(100 + i);
                IFuture<long> submit = executorService.Submit<long>(sumNumbers.CalculateSum);
                futures.Add(submit);
            }

            long sum = 0;
            foreach (var future in futures)
            {
                sum += future.GetResult();
            }
            Console.WriteLine("Sum = " + sum);

            futures.Clear();

            Console.WriteLine("---------");

            Console.WriteLine("Calculating sums...");

            // Call with method arguments
            for (int i = 0; i < 20000; i++ )
            {
                SumNumbers2 sumNumbers2 = new SumNumbers2();                
                int i1 = i;  // copy to local variable for closure.
                IFuture<long> submit =
                    executorService.Submit(() => sumNumbers2.CalculateSumWithArgsAndReturnValue(100 + i1));
                futures.Add(submit);                
            }
            


            sum = 0;
            foreach (var future in futures)
            {
                sum += future.GetResult();
            }
            Console.WriteLine("Sum = " + sum);


            //Say this was created at runtime and we don't know the type or parameter values at compile time.
            object obj = new SumNumbers2();         
            object[] parameters = new object[] {100};

            //Find the method we want to invoke
            MethodInfo methodInfo = ReflectionUtils.GetMethod(obj.GetType(), 
                                                             "CalculateSumWithArgsAndReturnValue", 
                                                             ReflectionUtils.GetTypes(parameters));


            //Use expression trees to generate code to invoke method and assign to a delegate.
            LateBoundMethod methodCallback = DelegateFactory.Create(methodInfo);

            IFuture<object> futureLong = executorService.Submit(() => methodCallback(obj, parameters));
            var result = futureLong.GetResult();            
            Console.WriteLine("LateBoundMethod Style : Result = " + result);

            ///Use Spring's IL generation to invoke method dynamically.
            IDynamicMethod method = DynamicMethod.Create(methodInfo);    
                    
            IFuture<object> futureLongViaDM = executorService.Submit(() => method.Invoke(obj, parameters));
            var resultViaDM = futureLongViaDM.GetResult();
            Console.WriteLine("Spring's IDynamicMethod Style: Result = " + resultViaDM);


            // This will make the executor accept no new threads
            // and finish all existing threads in the queue
            executorService.Shutdown();


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
