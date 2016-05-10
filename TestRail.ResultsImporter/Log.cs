using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRail.ResultsImporter
{
    using System.Diagnostics;

    public class Log
    {
        public static void Info(string message)
        {
            Console.WriteLine($"Info:  {message}");
            Debug.WriteLine($"Info:  {message}");
        }
        public static void Warn(string message)
        {
            Console.WriteLine($"Warn:  {message}");
            Debug.WriteLine($"Warn:  {message}");
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine($"Error:  {message}");
            Debug.WriteLine($"Error:  {message}");
        }

        public static void Error(string message, Exception exception)
        {
            Console.Error.WriteLine($"Error: {message}\n\n{exception}");
            Debug.WriteLine($"Error: {message}\n\n{exception}");
        }

        public static void Error(Exception exception)
        {
            Console.Error.WriteLine($"Error:\n\n{exception}");
            Debug.WriteLine($"Error:\n\n{exception}");
        }
    }
}
