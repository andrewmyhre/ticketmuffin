using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace LoadTest.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:63695";
            int threadCount = 10;

            if (args.Length == 2)
                url = args[1];
            if (args.Length >= 1)
                threadCount = int.Parse(args[0]);

            List<Thread> workers = new List<Thread>();
            for(int i=0;i<threadCount;i++)
            {
                Thread pt = new Thread(new ParameterizedThreadStart(DoRequest));
                workers.Add(pt);
                
            }

            System.Console.WriteLine("Execeuting requests");
            foreach(var worker in workers)
                worker.Start(url);

            System.Console.WriteLine("done");
            System.Console.ReadKey();
        }

        private static void DoRequest(object obj)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            System.Console.WriteLine(string.Format("#{0} request", Thread.CurrentThread.ManagedThreadId));
            var request = WebRequest.Create((string)obj);

            var response = (HttpWebResponse)request.GetResponse();
            var httpStatusCode = response.StatusCode;
            sw.Stop();
            System.Console.WriteLine(string.Format("#{0} returned {1}\t took {2}", Thread.CurrentThread.ManagedThreadId, httpStatusCode, sw.Elapsed));
        }

    }
}
