using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class DistributionTests : BaseTests
    {
        [Fact]
        public void When_CustomDistribiton_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".OPTIONS DISTRIBUTION = triangle_dist",
                ".DISTRIBUTION triangle_dist (-1,0) (0, 1) (1, 0)",
                ".END");

            Assert.Equal(1000, model.Simulations.Count);
            Assert.True(model.MonteCarloResult.Enabled);
            RunSimulations(model);

            var mcResult = model.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);

            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("power", histPlot.XUnit);
        }
    }
}