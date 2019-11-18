using System;
using BenchmarkDotNet.Running;

namespace SpiceSharpParser.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
