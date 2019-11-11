using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Parsers.Expression;
using Xunit;

namespace SpiceSharpParser.PerformanceTests
{
    public class ExpressionTests : BaseTests
    {
        [Fact]
        public void ModelIsUsingExpression()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs={1 + 0 + 0 + 1 + 0 + 0 + 0.568} N={0 + 1 + 0 + 1 + 0 + 0 + 1.752} Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 1e-5",
                ".SAVE i(V1)",
                ".END");

            RunDCSimulation(netlist, "i(V1)");
        }

        [Fact]
        public void EvaluateDouble()
        {
            var expressionParser = new Evaluator(string.Empty);
            double sum = 0;
            int n = 2000000;
            var randomizer = new Randomizer();
            for (var i = 0; i < n; i++)
            {
                sum += expressionParser.Evaluate(
                    "1 + 1 + 1 + 1 + 1 + 1 + 1",
                    new ExpressionContext(
                        string.Empty,
                        false,
                        false,
                        false,
                        randomizer),
                    null,
                    null);
            }
        }
    }
}
