using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class TableExpressionTests : BaseTests
    {
        [Fact]
        public void TableVoltage()
        {
            var result = GetSpiceSharpModel(
                "ExpressionTests - Table + Voltage",
                "V1 0 1 1.5",
                "V2 1 2 {X}",
                "R1 1 0 100",
                ".OP",
                ".SAVE V(1,2)",
                ".PARAM X={table(V(0,1), 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Single(result.Exports);
            Assert.Single(result.Simulations);

            var export = RunOpSimulation(result, "V(1,2)");

            Assert.Equal(15, export);
        }

        [Fact]
        public void TableMin()
        {
            var result = GetSpiceSharpModel(
                "ExpressionTests - Table min",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(0, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Single(result.Exports);
            Assert.Single(result.Simulations);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150.0 / 10.0, export);
        }

        [Fact]
        public void TableAvg()
        {
            var result = GetSpiceSharpModel(
                "ExpressionTests - Table interpolation",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(1.5, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Single(result.Exports);
            Assert.Single(result.Simulations);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150.0 / 15.0, export);
        }

        [Fact]
        public void TableMax()
        {
            var result = GetSpiceSharpModel(
                "ExpressionTests - Table max",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(4, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Single(result.Exports);
            Assert.Single(result.Simulations);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150.0 / 30.0, export);
        }
    }
}