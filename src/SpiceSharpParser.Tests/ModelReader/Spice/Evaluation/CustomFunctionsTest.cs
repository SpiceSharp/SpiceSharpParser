using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Spice.Evaluation.CustomFunctions;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Evaluation
{
    public class CustomFunctionsTest
    {
        [Fact]
        public void RadomTest()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>(),
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };
            parser.CustomFunctions.Add("random", RandomFunctions.CreateRandom());

            // act
            var result = parser.Parse("10 * random()");

            // assert
            Assert.True(result >= 0 && result <= 10);
        }
    }
}
