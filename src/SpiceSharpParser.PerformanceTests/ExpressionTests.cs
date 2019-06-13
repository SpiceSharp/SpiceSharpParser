using SpiceSharpParser.Common.Evaluation;
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
        public void ExpressionParse()
        {
            var expressionParser = new SpiceExpressionParser();
            var result = expressionParser.Parse("1 + 1 + 1 + 1 + 1 + 1 + 1", new ExpressionParserContext());

            double sum = 0;
            int n = 2000000;
            for (var i = 0; i < n; i++)
            {
                sum += result.Value(new ExpressionEvaluationContext());
            }
        }

        [Fact]
        public void EvaluateDouble()
        {
            var expressionParser = new SpiceEvaluator(string.Empty, new SpiceExpressionParser(false), false, false);
            double sum = 0;
            int n = 2000000;
            for (var i = 0; i < n; i++)
            {
                sum += expressionParser.EvaluateValueExpression("1 + 1 + 1 + 1 + 1 + 1 + 1", new ExpressionContext(string.Empty, false, false, false, new Randomizer()));
            }
        }
    }
}
