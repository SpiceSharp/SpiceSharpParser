using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class IfExpressionTests : BaseTests
    {
        [Fact]
        public void WhenIfUsedInExpression()
        {
            var netlist = GetSpiceSharpModel(
                "Test circuit",
                "R1 1 0 100",
                "V1 1 0 1",
                "V2 2 0 2",
                "V3 3 0 3",
                "V4 4 0 4",
                "V5 5 0 5",
                "X1 3 6 4 COMP1",
                ".SUBCKT COMP1 4 5 2",
                "ESource 4 5 VALUE = { if(v(2) > 0, v(2) + 2, 0) }",
                ".ENDS",
                ".OP",
                ".SAVE V(3,6)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(3,6)");
            Assert.Equal(6, export);
        }
    }
}
