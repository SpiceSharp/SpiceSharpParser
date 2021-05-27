using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class PolyExpressionTests : BaseTests
    {
        [Fact]
        public void ExpressionOneVariableSimpleSum()
        {
            var netlist = GetSpiceSharpModel(
                "Poly in expression test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { poly(1, V( 1 ), 2, 1) }", // Value = V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void ExpressionOneVariableSquare()
        {
            var netlist = GetSpiceSharpModel(
                "Poly in expression test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource1 3 0 VALUE = { V(1) + 2 }",
                "ESource 2 0 VALUE = { poly(1, V(3, 0), 2, 0, 1)  }", //V(3,0) *  V(3,0)  + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(18, export);
        }
    }
}