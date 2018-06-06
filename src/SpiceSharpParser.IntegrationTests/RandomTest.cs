using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class RandomTest : BaseTest
    {
        [Fact]
        public void BasicTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R+N}",
                ".OP",
                ".SAVE i(R1) @R1[resistance]",
                ".PARAM N=0",
                ".PARAM R={random() * 1000}",
                ".STEP PARAM N LIST 2 3",
                ".END");

            Assert.Equal(4, result.Exports.Count);
            Assert.Equal(2, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);
        }
    }
}
