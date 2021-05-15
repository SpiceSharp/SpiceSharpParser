using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class VoltageControlledSourcesTests : BaseTests
    {
        [Fact]
        public void VoltageControlledVoltageSourceWithVoltagePointFormat()
        {
            var model = GetSpiceSharpModel(
                "Voltage controlled voltage source - point format",
                "V1 1 0 100",
                "R1 1 0 10",
                "E1 (2, 0) (1, 0) 1.5",
                ".SAVE V(2,0)",
                ".OP",
                ".END");

            var export = RunOpSimulation(model, "V(2,0)");
            Assert.NotNull(model);
            Assert.Equal(150, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceWithVoltagePointFormat()
        {
            var model = GetSpiceSharpModel(
                "Voltage controlled current source - point format",
                "V1 1 0 100",
                "R1 1 0 10",
                "G1 (2, 0) (1, 0) 1.5",
                "R2 2 0 100",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(model, "I(R2)");
            Assert.NotNull(model);
            Assert.Equal(-150, export);
        }

        [Fact]
        public void VoltageControlledCurrentSource()
        {
            var model = GetSpiceSharpModel(
                "Voltage controlled current source - point format",
                "V1 1 0 200",
                "R1 1 0 10",
                "G1 2 0 1 0 1.5",
                "R2 2 0 100",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            var export = RunOpSimulation(model, "I(R2)");
            Assert.NotNull(model);
            Assert.Equal(-300, export);
        }
    }
}