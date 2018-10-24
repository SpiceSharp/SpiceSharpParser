using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class PolyTests : BaseTests
    {
        [Fact]
        public void VoltageControlledVoltageSourceFirstFormat()
        {
            var netlist = ParseNetlist(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY(1) 1 0 2 1", // V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceSecondFormat()
        {
            var netlist = ParseNetlist(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY(1) (1,0) 2 1", // V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceFirstFormatSecondDimension()
        {
            var netlist = ParseNetlist(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) 1 0 3 0 2 1 1", // V(1) + V(3) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(7, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceSecondFormatSecondDimension()
        {
            var netlist = ParseNetlist(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) (1,0) (3,0) 2 1 1 ", // V(1) + V(3) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(7, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceFirstFormat()
        {
            var netlist = ParseNetlist(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY(1) 2 0 2 1", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceSecondFormat()
        {
            var netlist = ParseNetlist(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY(1) (2,0) 2 1", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledCurrentSource()
        {
            var netlist = ParseNetlist(
                "Poly(1) F test circuit",
                "R1 1 0 100",
                "R2 1 0 200",
                "I1 1 0 2",
                "FSource 2 0 POLY(1) I1 2 1", // I(I1) + 2 
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(FSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledVoltageSource()
        {
            var netlist = ParseNetlist(
                "Poly(1) H test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 POLY(1) I1 2 1", // I(I1) + 2 
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }
    }
}