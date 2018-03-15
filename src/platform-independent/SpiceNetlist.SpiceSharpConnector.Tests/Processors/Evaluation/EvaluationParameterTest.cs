using NSubstitute;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Evaluation
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
