using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Evaluation
{
    public class EvaluationParameterTest
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
