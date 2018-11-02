using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class ReferenceTests : BaseTests
    {
        [Fact]
        public void Reference()
        {
            var result = ParseNetlist(
                "ExpressionTests - MC + OP + RANDOM",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[resistance]*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            var exports = RunSimulationsAndReturnExports(result);
        }
    }
}
