using System.IO;
using System.Linq;
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
    }
}
