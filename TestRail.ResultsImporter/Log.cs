using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    public class Log
    {
        public static void Info(string message)
        {
            //TODO Where should we log? Stdout is probably the best option
            Console.WriteLine($"Info:  {message}");
        }
        public static void Warn(string message)
        {
            Console.WriteLine($"Warn:  {message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"Error:  {message}");
        }

        public static void Error(string message, Exception exception)
        {
            //TODO Use better exception expansion
            Console.WriteLine($"Error: {message}\n\n{exception.ToString()}");
        }
    }
}
