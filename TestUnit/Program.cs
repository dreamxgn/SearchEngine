using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestUnit
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime old = DateTime.Now;

            while (true)
            {
                double seconds = (DateTime.Now - old).TotalSeconds;
                Console.WriteLine(seconds);
                if (seconds > 50)
                {
                    break;
                }
                Thread.Sleep(1000);
               
            }
            Console.WriteLine("OK");
            Console.ReadKey();

        }
    }
}
