using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.Parser.Expressions;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Evaluation
{
    public class CustomFunctionsTest
    {
        [Fact]
        public void RadomTest()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            parser.CustomFunctions.Add("random", RandomFunction.Create());

            // act
            var result = parser.Parse("10 * random()");

            // assert
            Assert.True(result >= 0 && result <= 10);
        }
    }
}
