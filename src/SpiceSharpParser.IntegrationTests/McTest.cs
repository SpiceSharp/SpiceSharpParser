using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class McTest : BaseTest
    {
        [Fact]
        public void McOpTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            Assert.Equal(1000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);

            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);

            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("power", histPlot.XUnit);
        }

        [Fact]
        public void McWrongFunctionTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX1",
                ".END");

            Assert.Equal(1000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);

            var mcResult = result.MonteCarloResult;
            try
            {
                var histPlot = mcResult.GetPlot(10);
                Assert.False(true, "There should be an expcetion");
            }
            catch
            {
            }
        }

        [Fact]
        public void McOpDeviceParameterTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[resistance]*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            Assert.Equal(1000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);
            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);
            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("power", histPlot.XUnit);
        }

        [Fact]
        public void McTurnOffTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".END");

            Assert.False(result.MonteCarloResult.Enabled);
        }

        [Fact]
        public void McOpGaussTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={abs(gauss(10))*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 10000 OP power MAX",
                ".END");

            Assert.Equal(10000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);
            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);
            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("power", histPlot.XUnit);
        }

        [Fact]
        public void McOpGaussSeedTest()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (power test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={abs(gauss(10))*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 100 OP power MAX SEED = 90",
                ".END");

            Assert.Equal(100, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);
            Assert.Equal(90, result.MonteCarloResult.RandomSeed);
            Assert.Equal(90, result.UsedEvaluatorRandomSeed);
        }

        [Fact]
        public void McTranTest()
        {
            var result = ParseNetlist(
                "The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT {10e3 + 100 * random()}",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".MC 10000 TRAN V(OUT) MAX",
                ".END");

            Assert.Equal(10000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);
            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);
            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("V(OUT)", histPlot.XUnit);
        }
    }
}
