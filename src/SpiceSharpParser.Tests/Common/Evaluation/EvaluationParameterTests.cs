using NSubstitute;
using SpiceSharpParser.Common.Evaluation;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Evaluation
{
    public class EvaluationParameterTests
    {
        [Fact]
        public void SetCallsSetParameter()
        {
            var c = Substitute.For<ExpressionContext>();
            var parameter = new EvaluationParameter(c, "a");
            parameter.Value = 12;
            c.Received().SetParameter("a", 12);
        }
    }
}
