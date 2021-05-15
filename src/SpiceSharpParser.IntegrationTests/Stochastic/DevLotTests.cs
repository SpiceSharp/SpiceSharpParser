using Xunit;

namespace SpiceSharpParser.IntegrationTests.Stochastic
{
    public class DevLotTests : BaseTests
    {
        [Fact]
        public void DevLotMultipleComponentsSameModel()
        {
            var netlist = GetSpiceSharpModel(
                "DevLot - Diodes circuit",
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

        [Fact]
        public void When_Mc_Expect_NoException()
        {
            var result = GetSpiceSharpModel(
                "Monte Carlo Analysis - DevLot - Diodes",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 LOT 10% Rs=0.568 DEV 20%)",
                ".OP",
                ".LET is {@1N914#D1[Is]}",
                ".MC 1000 OP is MAX",
                ".END");

            Assert.NotNull(RunSimulationsAndReturnExports(result));
        }
    }
}