using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Context
{
    public class NodeNameGeneratorTests
    {
        [Fact]
        public void GenerateNoSubcircuit()
        {
            var generator = new MainCircuitNodeNameGenerator(new string[] { "0" }, true, ".");

            // ground nodes
            Assert.Equal("0", generator.Generate("0"));
            Assert.Equal("gnd", generator.Generate("gnd"));
            Assert.Equal("Gnd", generator.Generate("Gnd"));
            Assert.Equal("GND", generator.Generate("GND"));

            // ordinary nodes
            Assert.Equal("a", generator.Generate("a"));
            Assert.Equal("Ab", generator.Generate("Ab"));
        }

        [Fact]
        public void GenerateWithSubcircuit()
        {
            var subcircuit = new Models.Netlist.Spice.Objects.SubCircuit();
            subcircuit.Pins = new Models.Netlist.Spice.Objects.ParameterCollection() { new WordParameter("IN"), new WordParameter("OUT") };

            subcircuit.DefaultParameters =
                new System.Collections.Generic.List<Models.Netlist.Spice.Objects.Parameters.AssignmentParameter>
                {
                    new Models.Netlist.Spice.Objects.Parameters.AssignmentParameter
                    {
                        Name = "L",
                        Value = "100",
                        Values = new List<string>(),
                    },
                    new Models.Netlist.Spice.Objects.Parameters.AssignmentParameter
                    {
                        Name = "C",
                        Value = "10",
                        Values = new List<string>(),
                    },
                };

            var generator = new SubcircuitNodeNameGenerator("x1", "x1", subcircuit, new System.Collections.Generic.List<string>() { "net2", "net3" }, new string[] { "0" }, true, ".");

            // ground nodes
            Assert.Equal("0", generator.Generate("0"));
            Assert.Equal("gnd", generator.Generate("gnd"));
            Assert.Equal("Gnd", generator.Generate("Gnd"));
            Assert.Equal("GND", generator.Generate("GND"));

            // ordinary nodes
            Assert.Equal("x1.a", generator.Generate("a"));
            Assert.Equal("x1.Ab", generator.Generate("Ab"));

            generator.SetGlobal("a");
            Assert.Equal("a", generator.Generate("a"));
            generator.SetGlobal("Ab");
            Assert.Equal("Ab", generator.Generate("Ab"));

            // subcircuit named nodes
            Assert.Equal("net2", generator.Generate("IN"));
            Assert.Equal("net3", generator.Generate("OUT"));
        }
    }
}