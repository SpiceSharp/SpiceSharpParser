using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using System;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Evaluation
{
    public class EvaluatorTest
    {
        [Fact]
        public void AddDynamicExpressionTest()
        {
            // arrange
            var parameters = new System.Collections.Generic.Dictionary<string, double>();
            parameters["xyz"] = 13.0;
            var v = new Evaluator(parameters);
            double expressionValue = 0;

            // act
            v.AddDynamicExpression(new DoubleExpression("xyz +1", (double newValue) => { expressionValue = newValue; }));
            v.SetParameter("xyz", 14);

            // assert
            Assert.Equal(15, expressionValue);
        }

        [Fact]
        public void EvaluatorFailsWhenThereCurrlyBraces()
        {
            var parameters = new System.Collections.Generic.Dictionary<string, double>();

            Evaluator v = new Evaluator(parameters);
            Assert.Throws<Exception>(() => v.EvaluteDouble("{1}"));
        }

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
            Assert.Equal(14, v.EvaluteDouble("xyz + 1"));
        }

        [Fact]
        public void EvaluatorSuffixTest()
        {
            var parameters = new System.Collections.Generic.Dictionary<string, double>();

            Evaluator v = new Evaluator(parameters);
            Assert.Equal(2, v.EvaluteDouble("1V + 1"));
        }
    }
}
