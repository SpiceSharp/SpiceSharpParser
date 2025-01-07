using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Examples
{
    public class ExampleBandPass : BaseTests
    {
        [Fact]
        public void When_SimulatedBandPass_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/band-pass_1.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);

            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.ExpandSubcircuits = false;
            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);

            RunSimulations(spiceSharpModel);
        }

        [Fact]
        public void When_SimulatedBandPass2_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/band-pass_2.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);

            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.ExpandSubcircuits = false;
            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);

            RunSimulations(spiceSharpModel);
        }
    }
}
