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
        public void TableTest()
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
        public void Round()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("round(1.2)"));
            Assert.Equal(2, parser.EvaluateDouble("round(1.9)"));
        }

        [Fact]
        public void PowMinusLtSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void PwrLtSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);

            // act and assert
            Assert.Equal(8, parser.EvaluateDouble("pwr(-2,3)"));
        }

        [Fact]
        public void PwrHSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);

            // act and assert
            Assert.Equal(-8, parser.EvaluateDouble("pwr(-2,3)"));
        }

        [Fact]
        public void PwrSmartSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);

            // act and assert
            Assert.Equal(-8, parser.EvaluateDouble("pwr(-2,3)"));
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
        public void PowMinusSmartSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);

            // act and assert
            Assert.Equal(Math.Pow(2, (int)1.5), parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void PowMinusHSpice()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);

            // act and assert
            Assert.Equal(Math.Pow(-2, (int)1.5), parser.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void SgnTest()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("sgn(0)"));
            Assert.Equal(-1, parser.EvaluateDouble("sgn(-1)"));
            Assert.Equal(1, parser.EvaluateDouble("sgn(0.1)"));
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

        [Fact]
        public void DefPositiveTest()
        {
            // arrange
            var parser = new SpiceEvaluator();
            parser.SetParameter("x1", 1);

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("def(x1)"));
        }

        [Fact]
        public void DefNegativeTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("def(x1)"));
        }

        [Fact]
        public void AbsTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("abs(-1)"));
        }

        [Fact]
        public void BufTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("buf(0.6)"));
            Assert.Equal(0, parser.EvaluateDouble("buf(0.3)"));
        }

        [Fact]
        public void CbrtTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(2, parser.EvaluateDouble("cbrt(8)"));
        }

        [Fact]
        public void CeilTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(3, parser.EvaluateDouble("ceil(2.9)"));
        }

        [Fact]
        public void DbSmartSpiceTest()
        {
            // arrange
            var parser = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);

            // act and assert
            Assert.Equal(20, parser.EvaluateDouble("db(-10)"));
        }

        [Fact]
        public void DbTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(-20, parser.EvaluateDouble("db(-10)"));
        }

        [Fact]
        public void ExpTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(Math.Exp(2), parser.EvaluateDouble("exp(2)"));
        }

        [Fact]
        public void FabsTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(3, parser.EvaluateDouble("fabs(-3)"));
        }

        [Fact]
        public void FlatTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act
            var res = parser.EvaluateDouble("flat(10)");

            // assert
            Assert.True(res >= -10 && res <= 10);
        }

        [Fact]
        public void FloorTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(2, parser.EvaluateDouble("floor(2.3)"));
        }

        [Fact]
        public void HypotTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(5, parser.EvaluateDouble("hypot(3,4)"));
        }

        [Fact]
        public void IfTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(3, parser.EvaluateDouble("if(0.5, 2, 3)"));
            Assert.Equal(2, parser.EvaluateDouble("if(0.6, 2, 3)"));
        }

        [Fact]
        public void IntTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("int(1.3)"));
        }

        [Fact]
        public void InvTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(0, parser.EvaluateDouble("inv(0.51)"));
            Assert.Equal(1, parser.EvaluateDouble("inv(0.5)"));
        }

        [Fact]
        public void LnTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("ln(e)"));
        }

        [Fact]
        public void LogTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("log(e)"));
        }

        [Fact]
        public void Log10Test()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("log10(10)"));
        }

        [Fact]
        public void MaxTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(100, parser.EvaluateDouble("max(10, -10, 1, 20, 100, 2)"));
        }

        [Fact]
        public void MinTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(-10, parser.EvaluateDouble("min(10, -10, 1, 20, 100, 2)"));
        }

        [Fact]
        public void NintTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("nint(1.2)"));
            Assert.Equal(2, parser.EvaluateDouble("nint(1.9)"));
        }

        [Fact]
        public void URampTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1.2, parser.EvaluateDouble("uramp(1.2)"));
            Assert.Equal(0, parser.EvaluateDouble("uramp(-0.1)"));
        }

        [Fact]
        public void UTest()
        {
            // arrange
            var parser = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, parser.EvaluateDouble("u(1.2)"));
            Assert.Equal(0, parser.EvaluateDouble("u(-1)"));
        }
    }
}
