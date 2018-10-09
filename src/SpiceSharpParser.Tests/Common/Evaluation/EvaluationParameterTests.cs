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
            var evaluator = Substitute.For<IEvaluator>();

            var parameter = new EvaluationParameter(evaluator, "a");
            parameter.Value = 12;

            evaluator.Received().SetParameter("a", 12);
        }
    }
}
