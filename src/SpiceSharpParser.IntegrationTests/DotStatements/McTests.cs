using System;
using SpiceSharpParser.Common;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class McTests : BaseTests
    {
        [Fact]
        public void McWithoutSaveLet()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
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
        public void McWithoutSaveVoltage()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".MC 1000 OP V(1) MAX",
                ".END");

            Assert.Equal(1000, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);

            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);

            Assert.Single(histPlot.Bins);
            Assert.Equal("V(1)", histPlot.XUnit);
        }

        [Fact]
        public void McOp()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
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
        public void McWrongFunction()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - O - POWER",
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

            Assert.Throws<SpiceSharpParserException>(() => mcResult.GetPlot(10));
        }

        [Fact]
        public void McOpDeviceParameter()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[i]*I(R1)}",
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
        public void McTurnOff()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
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
        public void McOpGauss()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 {100 + 100 / 100 - 1 + 5 + 3 + 10 -100}",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={abs(gauss(10))*1000}",
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
        public void McOpGaussSeed()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
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
            Assert.Equal(90, result.MonteCarloResult.Seed);
            //Assert.Equal(90, result.Seed); TODO: What to return
        }

        [Fact]
        public void McTran()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - TRAN - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT {10e3 + 100 * random()}",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".MC 100 TRAN V(OUT) MAX",
                ".END");

            Assert.Equal(100, result.Simulations.Count);
            Assert.True(result.MonteCarloResult.Enabled);
            RunSimulations(result);
            var mcResult = result.MonteCarloResult;
            var histPlot = mcResult.GetPlot(10);
            Assert.Equal(10, histPlot.Bins.Count);
            Assert.Equal("V(OUT)", histPlot.XUnit);
        }
    }
}