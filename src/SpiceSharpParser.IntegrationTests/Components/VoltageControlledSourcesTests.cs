using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    using System;

    using SpiceSharp;
    using SpiceSharp.Components;
    using SpiceSharp.Simulations;

    using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports;

    public class VoltageControlledSourcesTests : BaseTests
    {
        [Fact]
        public void VoltageControlledVoltageSourceWithVoltagePointFormat()
        {
            var netlist = ParseNetlist(
                "Voltage controlled voltage source - point format",
                "V1 1 0 100",
                "R1 1 0 10",
                "E1 (2, 0) (1, 0) 1.5",
                ".SAVE V(2,0)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "V(2,0)");
            Assert.NotNull(netlist);
            Assert.Equal(150, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceWithValuePointFormat()
        {
            var netlist = ParseNetlist(
                "Voltage controlled voltage source - point format",
                "V1 1 0 100",
                "R1 1 0 10",
                "E1 2 0 1 0 (1.5)",
                ".SAVE V(2,0)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "V(2,0)");
            Assert.NotNull(netlist);
            Assert.Equal(150, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceWithVoltagePointFormat()
        {
            var netlist = ParseNetlist(
                "Voltage controlled current source - point format",
                "V1 1 0 100",
                "R1 1 0 10",
                "G1 (2, 0) (1, 0) 1.5",
                "R2 2 0 100",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R2)");
            Assert.NotNull(netlist);
            Assert.Equal(-150, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceWithValuePointFormat()
        {
            var netlist = ParseNetlist(
                "Voltage controlled current source - point format",
                "V1 1 0 200",
                "R1 1 0 10",
                "G1 2 0 1 0 (1.5)",
                "R2 2 0 100",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R2)");
            Assert.NotNull(netlist);
            Assert.Equal(-300, export);
        }

        [Fact]
        public void VoltageControlledCurrentSource()
        {
            var netlist = ParseNetlist(
                "Voltage controlled current source - point format",
                "V1 1 0 200",
                "R1 1 0 10",
                "G1 2 0 1 0 1.5",
                "R2 2 0 100",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R2)");
            Assert.NotNull(netlist);
            Assert.Equal(-300, export);
        }
    }
}
