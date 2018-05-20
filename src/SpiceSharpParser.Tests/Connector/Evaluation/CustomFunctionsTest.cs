using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Evaluation.CustomFunctions;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Evaluation
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
            Assert.Equal(5, result);
        }
    }
}
