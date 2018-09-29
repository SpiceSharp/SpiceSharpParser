using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Context
{
    public class NodeNameGeneratorTest
    {
        [Fact]
        public void GenerateNoSubcircuitTest()
        {
            var generator = new MainCircuitNodeNameGenerator(new string[] { "0" });

            // ground nodes
            Assert.Equal("0", generator.Generate("0"));
            Assert.Equal("GND", generator.Generate("gnd"));
            Assert.Equal("GND", generator.Generate("Gnd"));
            Assert.Equal("GND", generator.Generate("GND"));

            // ordinary nodes
            Assert.Equal("A", generator.Generate("a"));
            Assert.Equal("AB", generator.Generate("Ab"));
        }

        [Fact]
        public void GenerateWithSubcircuitTest()
        {
            var subcircuit = new Models.Netlist.Spice.Objects.SubCircuit();
            subcircuit.Pins = new System.Collections.Generic.List<string>() { "IN", "OUT" };
            subcircuit.DefaultParameters =
                new System.Collections.Generic.List<Models.Netlist.Spice.Objects.Parameters.AssignmentParameter>() {
                    new Models.Netlist.Spice.Objects.Parameters.AssignmentParameter() { Name = "L", Value = "100" },
                    new Models.Netlist.Spice.Objects.Parameters.AssignmentParameter() { Name = "C", Value = "10" } };

            var generator = new SubcircuitNodeNameGenerator("x1", "x1", subcircuit, new System.Collections.Generic.List<string>() { "net2", "net3" }, new string[] { "0" }, true);

            // ground nodes
            Assert.Equal("0", generator.Generate("0"));
            Assert.Equal("GND", generator.Generate("gnd"));
            Assert.Equal("GND", generator.Generate("Gnd"));
            Assert.Equal("GND", generator.Generate("GND"));

            // ordinary nodes
            Assert.Equal("x1.A", generator.Generate("a"));
            Assert.Equal("x1.AB", generator.Generate("Ab"));

            generator.SetGlobal("a");
            Assert.Equal("A", generator.Generate("a"));
            generator.SetGlobal("Ab");
            Assert.Equal("AB", generator.Generate("Ab"));

            // subcircuit named nodes
            Assert.Equal("NET2", generator.Generate("IN"));
            Assert.Equal("NET3", generator.Generate("OUT"));
        }
    }
}
