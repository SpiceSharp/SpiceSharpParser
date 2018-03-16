using SpiceNetlist.SpiceSharpConnector.Evaluation;
using System;
using System.Linq;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Evaluation
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
            v.AddDynamicExpression(new DoubleExpression("xyz +1", (double newValue) => { expressionValue = newValue; }));
            v.SetParameter("xyz", 14);

            // assert
            Assert.Equal(15, expressionValue);
        }

        [Fact]
        public void EvaluateFailsWhenThereCurrlyBraces()
        {
            Evaluator v = new Evaluator();
            Assert.Throws<Exception>(() => v.EvaluateDouble("{1}", out _));
        }

        [Fact]
        public void EvaluateParameterTest()
        {
            Evaluator v = new Evaluator();
            v.SetParameter("xyz", 13.0);

            Assert.Equal(14, v.EvaluateDouble("xyz + 1", out _));
        }

        [Fact]
        public void EvaluateReturnsParameterTest()
        {
            // prepare
            Evaluator v = new Evaluator();
            v.SetParameter("xyz", 13.0);
            v.SetParameter("a", 1.0);

            // act
            v.EvaluateDouble("xyz + 1 + a", out var parameters);

            // assert
            Assert.Contains("a", parameters);
            Assert.Contains("xyz", parameters);
        }

        [Fact]
        public void EvaluateSuffixTest()
        {
            Evaluator v = new Evaluator();
            Assert.Equal(2, v.EvaluateDouble("1V + 1", out _));
        }
    }
}
