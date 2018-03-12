using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Evaluation
{
    public class EvaluatorTest
    {
        [Fact]
        public void EvaluatorParameterTest()
        {
            var parameters = new System.Collections.Generic.Dictionary<string, double>();
            parameters["xyz"] = 13.0;

            Evaluator v = new Evaluator(parameters);
            Assert.Equal(14, v.EvaluteDouble("xyz + 1"));
        }

        [Fact]
        public void EvaluatorExpressionTest()
        {
            var parameters = new System.Collections.Generic.Dictionary<string, double>();
            parameters["xyz"] = 13.0;

            Evaluator v = new Evaluator(parameters);
            Assert.Equal(14, v.EvaluteDouble("{xyz + 1}"));
        }

        [Fact]
        public void EvaluatorSuffixTest()
        {
            var parameters = new System.Collections.Generic.Dictionary<string, double>();

            Evaluator v = new Evaluator(parameters);
            Assert.Equal(2, v.EvaluteDouble("{1V + 1}"));
        }
    }
}
