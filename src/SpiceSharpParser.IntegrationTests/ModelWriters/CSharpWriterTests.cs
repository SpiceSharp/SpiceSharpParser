using System.IO;
using System.Linq;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.ModelWriters
{
    public class CSharpWriterTests : BaseTests
    {
        [Fact]
        public void Example01()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example04()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example04.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();

            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example04_reversed()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example04_reversed.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }


        [Fact]
        public void Example04_nested()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example04_nested.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example05()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example05.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example06()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example06.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example07()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example07.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example08()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example08.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
        }

        [Fact]
        public void Example09()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example09.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
            Assert.Equal(2, simulations.Count);
        }

        [Fact]
        public void Example10()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example10.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);
            var simulations = spiceSharpWriter.CreateSimulations(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
            Assert.NotNull(simulations);
            Assert.Single(simulations);


            var dc = simulations.First();
            dc.Run(circuit);
        }

        [Fact]
        public void Example11()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example11.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
        }

        [Fact]
        public void Example12()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/Example12.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
        }

        [Fact]
        public void When_LaplaceSourcesAreWritten_Expect_GeneratedCircuitContainsLaplaceEntities()
        {
            var parseResult = ParseNetlistRaw(
                lines: new[]
                {
                    "Generated LAPLACE writer",
                    ".PARAM tau=1e-6",
                    "VIN in 0 1",
                    "ELOW eout 0 LAPLACE {V(in)} = {1/(1+s*tau)} M=2 TD=1e-9",
                    "GLOW gout 0 LAPLACE = {V(in)} {0.001/(1+s*tau)}",
                    "FLOW fout 0 LAPLACE {I(VIN)} {1/(1+s*tau)}",
                    "HLOW hout 0 LAPLACE {I(VIN)} = {1/(1+s*tau)}",
                    ".OP",
                    ".END",
                });

            var spiceSharpWriter = new SpiceSharpCSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(circuit["ELOW"]);
            Assert.IsType<LaplaceVoltageControlledCurrentSource>(circuit["GLOW"]);
            Assert.IsType<LaplaceCurrentControlledCurrentSource>(circuit["FLOW"]);
            Assert.IsType<LaplaceCurrentControlledVoltageSource>(circuit["HLOW"]);

            var e = (LaplaceVoltageControlledVoltageSource)circuit["ELOW"];
            Assert.Equal(new[] { 2.0 }, e.Parameters.Numerator);
            Assert.Equal(new[] { 1.0, 1e-6 }, e.Parameters.Denominator);
            Assert.Equal(1e-9, e.Parameters.Delay);
        }

        [Fact]
        public void When_LtspiceBehavioralRandomFunctionsAreWritten_Expect_GeneratedCircuitRuns()
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Compatibility = CompatibilityOptions.LTspice;
            var parseResult = parser.ParseNetlist(string.Join(
                System.Environment.NewLine,
                "Generated LTspice behavioral random functions",
                "B1 out 0 V={rand(time*4)+random(time*4)+white(time*4)}",
                "R1 out 0 1k",
                ".tran 0.02 0.2",
                ".end"));

            var writer = new SpiceSharpCSharpWriter
            {
                Compatibility = CompatibilityOptions.LTspice,
            };
            var classText = writer.WriteCreateCircuitClass("LtspiceRandom", parseResult.FinalModel).GetText().ToString();
            var circuit = writer.CreateCircuit(parseResult.FinalModel);
            var simulation = Assert.Single(writer.CreateSimulations(parseResult.FinalModel));

            Assert.DoesNotContain("rand(", classText, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("random(", classText, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("white(", classText, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("floor(", classText, System.StringComparison.OrdinalIgnoreCase);
            Assert.IsType<BehavioralVoltageSource>(circuit["B1"]);

            simulation.Run(circuit);
        }
    }
}
