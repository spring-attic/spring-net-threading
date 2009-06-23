using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Spring.Threading.Loops
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: {0} TestCase", Assembly.GetExecutingAssembly().Location);
                return 1;
            }
            Type t = Type.GetType(args[0]);
            if (t==null)
            {
                Console.Error.WriteLine("{0}: No such test case.", args[0]);
                return 1;
            }
            MethodInfo methodInfo = t.GetMethod("main", BindingFlags.Public | BindingFlags.Static, null,
                                                new Type[] {typeof (string[])}, null);
            if (methodInfo == null || 
                !(methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == typeof(int)))
            {
                Console.Error.WriteLine("{0}: No matching main method.", args[0]);
                return 1;
            }
            string[] xargs = new string[args.Length-1];
            for (int i = 1; i < args.Length; i++)
            {
                xargs[i - 1] = args[i];
            }
            object result = methodInfo.Invoke(null, new object[] { xargs });
            return result == null ? 0 : Convert.ToInt32(result);
        }
    }
}
