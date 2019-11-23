using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class SParamTests : BaseTests
    {
        [Fact]
        public void ParamValueIsCached()
        {
            var netlist = ParseNetlist(
                "DC Sweep - Current",
                "V1 0 in 0",
                "R1 in 0 {R}",
                "R2 in 0 {R}",
                "R3 in 0 {K}",
                "R4 in 0 {K}",
                ".OP",
                ".PARAM R = {random() * 10 + 5}",
                ".SPARAM K = {random() * 10 + 5}",
                ".SAVE @R1[resistance] @R2[resistance] @R3[resistance] @R4[resistance]",
                ".END");

            var exports = RunOpSimulation(netlist, "@R1[resistance]", "@R2[resistance]", "@R3[resistance]", "@R4[resistance]");

            Assert.NotEqual(exports[0], exports[1]);
            Assert.Equal(exports[2], exports[3]);
        }
    }
}