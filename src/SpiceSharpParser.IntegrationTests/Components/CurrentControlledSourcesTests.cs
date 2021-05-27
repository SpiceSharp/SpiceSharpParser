using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class CurrentControlledSourcesTests : BaseTests
    {
        [Fact]
        public void CurrentControlledCurrentSource()
        {
            var netlist = GetSpiceSharpModel(
                "Current controlled current source",
                "V1 1 0 100",
                "R1 1 0 10",
                "F1 2 0 V1 1.5",
                "R2 2 0 2",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R2)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void CurrentControlledVoltageSource()
        {
            var netlist = GetSpiceSharpModel(
                "Current controlled voltage source",
                "V1 1 0 100",
                "R1 1 0 10",
                "H1 2 0 V1 1.5",
                "R2 2 0 2",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R2)");
            Assert.NotNull(netlist);
            Assert.Equal(-7.5, export);
        }
    }
}