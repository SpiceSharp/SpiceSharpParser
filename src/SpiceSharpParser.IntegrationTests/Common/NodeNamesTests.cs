using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class NodeNamesTests : BaseTests
    {
        [Fact]
        public void When_NodeNameHasUnderline_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Diode circuit",
                "D1 1_a 0 1N914",
                "V1_a 1_a 0 0.0",
                ".model 1N914 D(Is=2.52e-9    Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1_a -1 1.0 10e-3",
                ".SAVE i(V1_a) v(1_a,0)",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(netlist, "v(1_a,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_NodeNameHasPlusPrefix_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Diode circuit",
                "D1 +1_a 0 1N914",
                "V1_a +1_a 0 0.0",
                ".model 1N914 D(Is=2.52e-9    Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1_a -1 1.0 10e-3",
                ".SAVE i(V1_a) v(+1_a,0)",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(netlist, "v(+1_a,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_NodeNameHasMinusPrefix_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Diode circuit",
                "D1 -1_a 0 1N914",
                "V1_a -1_a 0 0.0",
                ".model 1N914 D(Is=2.52e-9    Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1_a -1 1.0 10e-3",
                ".SAVE i(V1_a) v(-1_a,0)",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(netlist, "v(-1_a,0)"));
            Assert.Null(exception);
        }
    }
}