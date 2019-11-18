using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using SpiceSharpParser.Lexers.Netlist.Spice;

namespace SpiceSharpParser.PerformanceTests.Benchmarks
{
    [MemoryDiagnoser]
    public class Lexer
    {
        private string text;

        [Params(1, 100, 1000, 2000, 5000)]
        public int NumberOfLines { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Title");

            builder.AppendLine("seq 0");

            for (var i = 1; i < NumberOfLines; i++)
            {
                builder.AppendLine($" + seq {i}");
            }

            text = builder.ToString();
        }

        [Benchmark]
        public void LargeText()
        {
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
            var tokens = lexer.GetTokens(text).ToArray();
        }
    }
}
