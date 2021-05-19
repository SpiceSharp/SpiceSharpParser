using System.IO;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.ModelWriters
{
    public class CSharpWriterTests
    {
        [Fact]
        public void Example01()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example01.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example04.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();

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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example04_reversed.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example04_nested.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example05.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example06.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example07.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example08.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example09.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example10.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/example11.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);
            var spiceSharpWriter = new SpiceSharpWriter();
            var classNode = spiceSharpWriter.WriteCreateCircuitClass("Example", parseResult.FinalModel);
            var classText = classNode.GetText().ToString();
            var circuit = spiceSharpWriter.CreateCircuit(parseResult.FinalModel);

            Assert.NotNull(classText);
            Assert.NotNull(circuit);
        }
    }
}
