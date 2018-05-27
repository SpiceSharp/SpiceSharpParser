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

            parser.CustomFunctions.Add("random", RandomFunctions.CreateRandom());

            // act
            var result = parser.Parse("10 * random()");

            // assert
            Assert.True(result >= 0 && result <= 10);
        }

        [Fact]
        public void MinTest()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            parser.CustomFunctions.Add("min", MathFunctions.CreateMin());

            // act
            var result = parser.Parse("10 * min(-1,-10, -20, 1)");

            // assert
            Assert.Equal(-200, result);
        }

        [Fact]
        public void MaxTest()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            parser.CustomFunctions.Add("max", MathFunctions.CreateMax());

            // act
            var result = parser.Parse("10 * max(-1,-10, -20, 1)");

            // assert
            Assert.Equal(10, result);
        }
    }
}
