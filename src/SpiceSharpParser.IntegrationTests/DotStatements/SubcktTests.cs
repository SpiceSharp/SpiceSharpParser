using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class SubcktTests : BaseTests
    {
        [Fact]
        public void UsingGlobalModel()
        {
            var model = GetSpiceSharpModel(
                "ST + SUBCKT + DIODE",
                ".SUBCKT diode node1 node2",
                "D1 node1 node2 1N914",
                ".ENDS diode",
                "X1 OUT 0 diode",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST LIST 1N914(N) 1.752 1.234 1.2 1.0 0.1",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
        }
    }
}