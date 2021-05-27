using Xunit;

namespace SpiceSharpParser.IntegrationTests.Stochastic
{
    public class LotTests : BaseTests
    {
        [Fact]
        public void LotMultipleComponentsSameModel()
        {
            var netlist = GetSpiceSharpModel(
                "Lot - Diodes circuit",
                "D1 OUT 0 1N914",
                "D2 OUT 0 1N914",
                "D3 OUT 0 1N914",
                "D4 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 LOT 10%)",
                ".OP",
                ".SAVE @1N914[Is] @1N914#D1[Is] @1N914#D2[Is] @1N914#D3[Is] @1N914#D4[Is]",
                ".END");

            var exports = RunSimulationsAndReturnExports(netlist);

            for (var i = 2; i < exports.Count; i++)
            {
                Assert.Equal(exports[1], exports[i]);
            }

            Assert.NotEqual(exports[0], exports[1]);
        }

        [Fact]
        public void LotMultipleComponentsSameModelCustomDistribution()
        {
            var netlist = GetSpiceSharpModel(
                "Lot - Diodes circuit",
                "D1 OUT 0 1N914",
                "D2 OUT 0 1N914",
                "D3 OUT 0 1N914",
                "D4 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 LOT/uniform 10%)",
                ".OP",
                ".DISTRIBUTION uniform (-1,1) (1, 1)",
                ".SAVE @1N914[Is] @1N914#D1[Is] @1N914#D2[Is] @1N914#D3[Is] @1N914#D4[Is]",
                ".END");

            var exports = RunSimulationsAndReturnExports(netlist);

            for (var i = 2; i < exports.Count; i++)
            {
                Assert.Equal(exports[1], exports[i]);
            }

            Assert.NotEqual(exports[0], exports[1]);
        }
    }
}