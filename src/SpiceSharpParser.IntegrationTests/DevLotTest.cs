using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class DevLotTest : BaseTest
    {
        [Fact]
        public void DevLotMultipleComponentsSameModelTest()
        {
            var netlist = ParseNetlist(
                "Diodes circuit",
                "D1 OUT 0 1N914",
                "D2 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 LOT 10% Rs=0.568 DEV 20%)",
                ".OP",
                ".SAVE @1N914[Is] @1N914#D1[Is] @1N914#D2[Is]",
                "+ @1N914[Rs] @1N914#D1[Rs] @1N914#D2[Rs]",
                ".END");

            var exports = RunSimulationsAndReturnExports(netlist);

            Assert.NotEqual(exports[0], exports[1]);
            Assert.Equal(exports[1], exports[2]);

            Assert.NotEqual(exports[3], exports[4]);
            Assert.NotEqual(exports[3], exports[5]);
            Assert.NotEqual(exports[4], exports[5]);
        }
    }
}
