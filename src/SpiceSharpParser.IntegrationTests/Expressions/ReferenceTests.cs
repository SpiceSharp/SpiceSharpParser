using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class ReferenceTests : BaseTests
    {
        [Fact]
        public void Reference()
        {
            var result = GetSpiceSharpModel(
                "ExpressionTests - MC + OP + RANDOM",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[i]*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            RunSimulationsAndReturnExports(result);
        }

        [Fact]
        public void CloningIssue()
        {
            var netlist = GetSpiceSharpModel(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                ".IF (a == 1)",
                "E12 2 1 TABLE {V(1)} = (0,1) (1m,2) (2m,3)",
                ".ENDIF",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".PARAM a = 1",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }
    }
}