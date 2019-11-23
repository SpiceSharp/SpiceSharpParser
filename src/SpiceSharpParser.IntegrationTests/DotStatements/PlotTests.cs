using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class PlotTests : BaseTests
    {
        [Fact]
        public void When_InvalidExportForSimulationWithoutFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
                "PLOT - Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PLOT V(OUT) I(C1)",
                ".END");

            RunSimulations(parseResult);
            Assert.Empty(parseResult.XyPlots);
        }

        [Fact]
        public void When_InvalidExportForSimulationWithFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
                "PLOT - Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PLOT OP V(OUT) I(C1)",
                ".END");

            RunSimulations(parseResult);
            Assert.Empty(parseResult.XyPlots);
        }

        [Fact]
        public void When_PrintTran_Expect_Reference()
        {
            var parseResult = ParseNetlist(
               "PLOT - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".PLOT TRAN",
                ".END");

            RunSimulations(parseResult);
            Assert.Single(parseResult.XyPlots);
            Assert.Equal("#1 TRAN", parseResult.XyPlots[0].Name);
        }

        [Fact]
        public void When_PrintOpWithoutFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
                "PLOT - Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PLOT V(OUT) I(V1)",
                ".END");

            RunSimulations(parseResult);
            Assert.Empty(parseResult.XyPlots);
        }

        [Fact]
        public void When_PrintOpWithoutArgumentsWithoutFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
                "PLOT - Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PLOT",
                ".END");

            RunSimulations(parseResult);
            Assert.Empty(parseResult.XyPlots);
        }

        [Fact]
        public void When_PrintDcWithoutFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
              "PLOT - DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PLOT V(in) I(R1)",
              ".END");

            RunSimulations(parseResult);
            Assert.Single(parseResult.XyPlots);
            Assert.Equal("#1 DC", parseResult.XyPlots[0].Name);
        }

        [Fact]
        public void When_PrintDcWithFilter_Expect_Reference()
        {
            var parseResult = ParseNetlist(
              "PLOT - DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PLOT DC V(in) I(R1)",
              ".END");
            RunSimulations(parseResult);

            Assert.Single(parseResult.XyPlots);
            Assert.Equal("#1 DC", parseResult.XyPlots[0].Name);
        }

        [Fact]
        public void When_LetIsUsedInPlot_Expect_Reference()
        {
            var parseResult = ParseNetlist(
              "PLOT - DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PLOT DC V(in) I(R1) V_in_db",
              ".LET V_in_db {log10(V(in))*2}",
              ".END");
            RunSimulations(parseResult);

            Assert.Single(parseResult.XyPlots);
            Assert.Equal("#1 DC", parseResult.XyPlots[0].Name);
        }
    }
}