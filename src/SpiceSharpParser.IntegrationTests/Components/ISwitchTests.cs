using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ISwitchTests : BaseTests
    {
        [Fact]
        public void PartialOnTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                "W1 1 0 V1 imodel",
                "V2 1 0 10",
                "V1 2 0 -1",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=100 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.True(EqualsWithTol(-0.316228, export));
        }

        [Fact]
        public void PartialOnInSubcktTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                ".SUBCKT customSwitch 1 2",
                "W1 1 0 V1 imodel",
                "V1 2 0 -1",
                ".ENDS",
                "X1 1 2 customSwitch",
                "V2 1 0 10",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=100 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.True(EqualsWithTol(-0.316228, export));
        }

        [Fact]
        public void OnTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                "W1 1 0 R1 imodel",
                "V2 1 0 10",
                "V1 2 0 2",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=1000000 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.Equal(-1, export);
        }

        [Fact]
        public void OnMoreTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                "W1 1 0 R1 imodel",
                "V2 1 0 10",
                "V1 2 0 1000",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=1000000 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.Equal(-1, export);
        }

        [Fact]
        public void OffTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                "W1 1 0 R1 imodel",
                "V2 1 0 10",
                "V1 2 0 0",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=1000000 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.True(EqualsWithTol(-10.0 / 1000000.0, export));
        }

        [Fact]
        public void OffMoreTest()
        {
            var netlist = GetSpiceSharpModel(
                "ISwitch test",
                "W1 1 0 R1 imodel",
                "V2 1 0 10",
                "V1 2 0 -10",
                "R1 2 0 1",
                ".model imodel ISWITCH (roff=1000000 ron=10 ioff = 0 ion = 2)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(netlist, "I(V2)");
            Assert.NotNull(netlist);
            Assert.True(EqualsWithTol(-10.0 / 1000000.0, export));
        }
    }
}