using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class StepTest : BaseTest
    {
        [Fact]
        public void ParamListTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulations(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamLinTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N 1 4 1",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulations(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }
    }
}
