using BenchmarkDotNet.Running;

namespace SpiceSharpParser.PerformanceTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}