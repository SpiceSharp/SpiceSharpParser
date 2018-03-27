using SpiceSharpParser.Connector.Evaluation;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Evaluation
{
    public class EvaluatorTest
    {
        [Fact]
        public void GetParameterNames()
        {
            // arrange
            var p = new Evaluator();
            p.SetParameter("a", 1);
            p.SetParameter("xyz", 13.0);

            Assert.Equal(2, p.GetParameterNames().Count());
        }

        [Fact]
        public void ParentEvalautor()
        {
            // arrange
            var p = new Evaluator();
            p.SetParameter("a", 1);

            // act and assert
            var v = new Evaluator(p);

            v.SetParameter("xyz", 13.0);
            Assert.Equal(1, v.GetParameterValue("a"));

            v.SetParameter("a", 2);
            Assert.Equal(2, v.GetParameterValue("a"));
            Assert.Equal(1, p.GetParameterValue("a"));
        }

        [Fact]
        public void AddDynamicExpressionTest()
        {
            // arrange
            var v = new Evaluator();
            v.SetParameter("xyz", 13.0);

            double expressionValue = 0;

            // act
            v.AddDynamicExpression(new DoubleExpression("xyz +1", (double newValue) => { expressionValue = newValue; }), new string[] { "xyz" });
            v.SetParameter("xyz", 14);

            // assert
            Assert.Equal(15, expressionValue);
        }

        [Fact]
        public void EvaluateFailsWhenThereCurrlyBraces()
        {
            Evaluator v = new Evaluator();
            Assert.Throws<Exception>(() => v.EvaluateDouble("{1}"));
        }

        [Fact]
        public void EvaluateParameterTest()
        {
            Evaluator v = new Evaluator();
            v.SetParameter("xyz", 13.0);

            Assert.Equal(14, v.EvaluateDouble("xyz + 1"));
        }

        [Fact]
        public void GetVariablesTest()
        {
            // prepare
            Evaluator v = new Evaluator();
            v.SetParameter("xyz", 13.0);
            v.SetParameter("a", 1.0);

            // act
            var parameters = v.GetVariables("xyz + 1 + a");

            // assert
            Assert.Contains("a", parameters);
            Assert.Contains("xyz", parameters);
        }

        [Fact]
        public void EvaluateSuffixTest()
        {
            Evaluator v = new Evaluator();
            Assert.Equal(2, v.EvaluateDouble("1V + 1"));
        }
    }
}
