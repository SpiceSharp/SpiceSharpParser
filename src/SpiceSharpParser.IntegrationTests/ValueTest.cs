using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class ValueTest : BaseTest
    {
        [Fact]
        public void ValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { V(1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(4, export);

        }
    }
}
