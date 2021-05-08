using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.ModelWriters
{
    public class StringWriterTests : BaseTests
    {
        [Fact]
        public void Test01()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01_stringwriter.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);

            var writer = new SpiceSharpParser.ModelWriters.Netlist.StringWriter();
            var writerContent = writer.Write(parseResult.PreprocessedInputModel);
            Assert.Equal(netlistContent, writerContent);

            parseResult = parser.ParseNetlist(writerContent);

            double[] exports = RunOpSimulation(parseResult.SpiceModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            EqualsWithTol(1.0970919064909939, exports[0]);
            EqualsWithTol(0.014696545624995935, exports[1]);
            EqualsWithTol(0.014715219080886419, exports[2]);
        }
    }
}
