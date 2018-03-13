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
            var parameter = new EvaluationParameter(evaluator, "a", 1);
            parameter.Set(12);

            evaluator.Received().SetParameter("a", 12);
        }

        [Fact]
        public void SetDontCallSetParameter()
        {
            var evaluator = Substitute.For<IEvaluator>();
            var parameter = new EvaluationParameter(evaluator, "a", 1);
            parameter.Set(1);

            evaluator.DidNotReceive().SetParameter("a", 1);
        }
    }
}
