using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Examples
{
    public class Example01Tests : BaseTests
    {
        [Fact]
        public void When_Simulated_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);

            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.ExpandSubcircuits = false;
            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);

            double[] exports = RunOpSimulation(spiceSharpModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            Assert.True(EqualsWithTol(1.0970919064909939, exports[0]));
            Assert.True(EqualsWithTol(0.014696545624995935, exports[1]));
            Assert.True(EqualsWithTol(0.014715219080886419, exports[2]));
        }

        [Fact]
        public void When_Simulated_2_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01_without_params.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);

            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.ExpandSubcircuits = false;
            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);

            double[] exports = RunOpSimulation(spiceSharpModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            Assert.True(EqualsWithTol(1.0970919064909939, exports[0]));
            Assert.True(EqualsWithTol(0.014696545624995935, exports[1]));
            Assert.True(EqualsWithTol(0.014715219080886419, exports[2]));
        }
        [Fact]
        public void When_Simulated_3_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);
            var reader = new SpiceSharpReader();
            reader.Settings.ExpandSubcircuits = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            double[] exports = RunOpSimulation(spiceModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            Assert.True(EqualsWithTol(1.0970919064909939, exports[0]));
            Assert.True(EqualsWithTol(0.014696545624995935, exports[1]));
            Assert.True(EqualsWithTol(0.014715219080886419, exports[2]));
        }
    }
}