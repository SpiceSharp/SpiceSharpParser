using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Evaluation
{
    public class SpiceEvaluatorTest
    {
        [Fact]
        public void GetParameterNames()
        {
            // arrange
            var p = new SpiceEvaluator();
            p.SetParameter("a", 1);
            p.SetParameter("xyz", 13.0);

            Assert.Equal(3, p.GetParameterNames().Count()); // +1 for TEMP parameter
        }

        [Fact]
        public void ParentEvalautor()
        {
            // arrange
            var p = new SpiceEvaluator();
            p.SetParameter("a", 1);

            // act and assert
            var v = p.CreateChildEvaluator();

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
            var v = new SpiceEvaluator();
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
            Evaluator v = new SpiceEvaluator();
            Assert.Throws<Exception>(() => v.EvaluateDouble("{1}"));
        }

        [Fact]
        public void EvaluateParameterTest()
        {
            Evaluator v = new SpiceEvaluator();
            v.SetParameter("xyz", 13.0);

            Assert.Equal(14, v.EvaluateDouble("xyz + 1"));
        }

        [Fact]
        public void GetVariablesTest()
        {
            // prepare
            Evaluator v = new SpiceEvaluator();
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
            Evaluator v = new SpiceEvaluator();
            Assert.Equal(2, v.EvaluateDouble("1V + 1"));
        }

        [Fact]
        public void EvaluateTableTest()
        {
            Evaluator v = new SpiceEvaluator();
            v.SetParameter("N", 1.0);
            Assert.Equal(10, v.EvaluateDouble("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)"));
        }

        [Fact]
        public void EvaluateWithComaTest()
        {
            Evaluator v = new SpiceEvaluator();
            Assert.Equal(1.99666833293656, v.EvaluateDouble("1,99666833293656"));
        }

        [Fact]
        public void PowerInfixTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(8, parser.EvaluateDouble("2**3"));
        }

        [Fact]
        public void PowerInfixPrecedenceTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(7, parser.EvaluateDouble("2**3-1"));
        }

        [Fact]
        public void PowerInfixSecondPrecedenceTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(8, parser.EvaluateDouble("1+2**3-1"));
        }

        [Fact]
        public void PowerInfixThirdPrecedenceTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(17, parser.EvaluateDouble("1+2**3*2"));
        }

        [Fact]
        public void MinusPowerLtSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void MinusPowerInfixLtSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("-2**1.5"));
        }

        [Fact]
        public void MinusPowerSmartSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);

            // act and assert
            Assert.Equal(Math.Pow(2, (int)1.5), parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void MinusPowerHSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);

            // act and assert
            Assert.Equal(Math.Pow(-2, (int)1.5), parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void SqrtTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(2, parser.EvaluateDouble("sqrt(4)"));
        }

        [Fact]
        public void SqrtMinusHSpiceTest()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);

            // act and assert
            Assert.Equal(-2, parser.EvaluateDouble("sqrt(-4)"));
        }

        [Fact]
        public void SqrtMinusSmartSpiceTest()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);

            // act and assert
            Assert.Equal(2, parser.EvaluateDouble("sqrt(-4)"));
        }

        [Fact]
        public void SqrtMinusLtSpiceTest()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("sqrt(-4)"));
        }
    }
}
