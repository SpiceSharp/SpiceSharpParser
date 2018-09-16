using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class TableTest : BaseTest
    {
        [Fact]
        public void ParsingFirstFormatTest()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "E12 3 2 TABLE {V(1,0)} = (0,1) (1m,2) (2m,3M)",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ParsingSecondFormatTest()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "E12 3 2 TABLE {V(1,0)} ((0,1) (1m,2) (2m,3))",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ParsingAdvancedFormatTest()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "E12 3 2 TABLE {210K * (V(1,0) - V(2,0))} ((-10,-10) (10, 10))",
                ".END");

            Assert.NotNull(netlist);
        }
    }
}
