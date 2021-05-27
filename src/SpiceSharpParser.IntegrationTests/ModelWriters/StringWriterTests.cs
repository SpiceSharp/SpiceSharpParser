using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System.IO;
using System.Text;
using SpiceSharpParser.Common;
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

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);

            var writer = new SpiceSharpParser.ModelWriters.Netlist.StringWriter();
            var writerContent = writer.Write(parseResult.FinalModel);
            Assert.Equal(netlistContent, writerContent);

            parseResult = parser.ParseNetlist(writerContent);


            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => parser.Settings.WorkingDirectory, Encoding.Default);
            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            var spiceSharpModel =  spiceSharpReader.Read(parseResult.FinalModel);

            double[] exports = RunOpSimulation(spiceSharpModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            Assert.True(EqualsWithTol(1.0970919064909939, exports[0]));
            Assert.True(EqualsWithTol(0.014696545624995935, exports[1]));
            Assert.True(EqualsWithTol(0.014715219080886419, exports[2]));
        }
    }
}
