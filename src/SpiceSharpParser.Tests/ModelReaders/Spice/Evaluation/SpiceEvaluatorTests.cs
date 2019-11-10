using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Parsers.Expression;
using System;
using SpiceSharpParser.Common.Evaluation.Expressions;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation
{
    public class SpiceEvaluatorTests
    {
        [Fact]
        public void ParentEvaluator()
        {
            // arrange
            var p = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("a", 1);

            // act and assert
            var v = c.CreateChildContext("child", false);

            v.SetParameter("xyz", 13.0);
            Assert.Equal(1, ((ConstantExpression)v.Parameters["a"]).Value);

            v.SetParameter("a", 2);
            Assert.Equal(2, ((ConstantExpression)v.Parameters["a"]).Value);
            Assert.Equal(1, ((ConstantExpression)c.Parameters["a"]).Value);
        }

        [Fact]
        public void EvaluateParameter()
        {
            Evaluator v = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("xyz", 13.0);

            Assert.Equal(14, v.Evaluate("xyz + 1", c, null, null));
        }

        [Fact]
        public void EvaluateSuffix()
        {
            Evaluator v = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            Assert.Equal(2, v.Evaluate("1V + 1", c, null, null));
        }

        [Fact]
        public void TableBasic()
        {
            Evaluator v = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("N", 1.0);
            Assert.Equal(10, v.Evaluate("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)", c, null, null));
        }

        [Fact]
        public void TableInterpolation()
        {
            Evaluator v = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            c.SetParameter("N", 1.5);
            Assert.Equal(-5, v.Evaluate("table(N, 1, 0, 2, -10)", c, null, null));

            c.SetParameter("N", 3);
            Assert.Equal(-10, v.Evaluate("table(N, 1, 0, 2, -10)", c, null, null));

            c.SetParameter("N", 0);
            Assert.Equal(0, v.Evaluate("table(N, 1, 0, 2, -10)", c, null, null));

            c.SetParameter("N", -1);
            Assert.Equal(0, v.Evaluate("table(N, 1, 0, 2, -10)", c, null, null));
        }

        [Fact]
        public void TableAdvanced()
        {
            Evaluator v = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("N", 1.0);
            Assert.Equal(10, v.Evaluate("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)", c, null, null));
        }

        [Fact]
        public void Round()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(1, evaluator.Evaluate("round(1.2)", c, null, null));
            Assert.Equal(2, evaluator.Evaluate("round(1.9)", c, null, null));
        }

        [Fact]
        public void PwrLtSpice()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);

            // act and assert
            Assert.Equal(8, evaluator.Evaluate("pwr(-2,3)", c, null, null));
        }

        [Fact]
        public void PwrHSpice()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.HSpice);

            // act and assert
            Assert.Equal(-8, evaluator.Evaluate("pwr(-2,3)", c, null, null));
        }

        [Fact]
        public void PwrSmartSpice()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(-8, evaluator.Evaluate("pwr(-2,3)", c, null, null));
        }

        [Fact]
        public void Sgn()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(0, evaluator.Evaluate("sgn(0)", c, null, null));
            Assert.Equal(-1, evaluator.Evaluate("sgn(-1)", c, null, null));
            Assert.Equal(1, evaluator.Evaluate("sgn(0.1)", c, null, null));
        }

        [Fact]
        public void Sqrt()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.Evaluate("sqrt(4)", c, null, null));
        }

        [Fact]
        public void DefPositive()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("x1", 1);
            
            // act and assert
            Assert.Equal(1, evaluator.Evaluate("def(x1)", c, null, null));
        }

        [Fact]
        public void DefNegative()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(0, evaluator.Evaluate("def(x1)", c, null, null));
        }

        [Fact]
        public void Abs()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("abs(-1)", c, null, null));
        }

        [Fact]
        public void AGauss()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("agauss(0, 1, 2)", c, null, null);
        }

        [Fact]
        public void AUnif()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("aunif(0, 1)", c, null, null);
        }

        [Fact]
        public void Unif()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("unif(1, 0.5)", c, null, null);
        }

        [Fact]
        public void LimitRandom()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("limit(0, 1)", c, null, null);
        }

        [Fact]
        public void Buf()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("buf(0.6)", c, null, null));
            Assert.Equal(0, evaluator.Evaluate("buf(0.3)", c, null, null));
        }

        [Fact]
        public void Cbrt()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.Evaluate("cbrt(8)", c, null, null));
        }

        [Fact]
        public void Ceil()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.Evaluate("ceil(2.9)", c, null, null));
        }

        [Fact]
        public void DbSmartSpice()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(20, evaluator.Evaluate("db(-10)", c, null, null));
        }

        [Fact]
        public void Db()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(-20, evaluator.Evaluate("db(-10)", c, null, null));
        }

        [Fact]
        public void Exp()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(Math.Exp(2), evaluator.Evaluate("exp(2)", c, null, null));
        }

        [Fact]
        public void Fabs()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.Evaluate("fabs(-3)", c, null, null));
        }

        [Fact]
        public void Flat()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act
            var res = evaluator.Evaluate("flat(10)", c, null, null);

            // assert
            Assert.True(res >= -10 && res <= 10);
        }

        [Fact]
        public void Floor()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.Evaluate("floor(2.3)", c, null, null));
        }

        [Fact]
        public void Hypot()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(5, evaluator.Evaluate("hypot(3,4)", c, null, null));
        }

        [Fact]
        public void Gauss()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("gauss(1.2)", c, null, null);
        }

        [Fact]
        public void ExtendedGauss()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            evaluator.Evaluate("gauss(1, 2.3, 4.5)", c, null, null);
        }

        [Fact]
        public void ExtendedGauss_TooManyArguments()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Throws<SpiceSharpBehavioral.Parsers.ParserException>(() => evaluator.Evaluate("gauss(1, 2.3, 4.5, 0)", c, null, null));
        }

        [Fact]
        public void If()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.Evaluate("if(0.5, 2, 3)", c, null, null));
            Assert.Equal(2, evaluator.Evaluate("if(0.6, 2, 3)", c, null, null));
        }

        [Fact]
        public void Int()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("int(1.3)", c, null, null));
        }

        [Fact]
        public void Inv()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(0, evaluator.Evaluate("inv(0.51)", c, null, null));
            Assert.Equal(1, evaluator.Evaluate("inv(0.5)", c, null, null));
        }

        [Fact]
        public void Ln()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("ln(e)", c, null, null));
        }

        [Fact]
        public void Limit()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(8, evaluator.Evaluate("limit(10, 1, 8)", c, null, null));
            Assert.Equal(1, evaluator.Evaluate("limit(-1, 1, 8)", c, null, null));
            Assert.Equal(4, evaluator.Evaluate("limit(4, 1, 8)", c, null, null));
        }

        [Fact]
        public void Log()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("log(e)", c, null, null));
        }

        [Fact]
        public void Log10()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("log10(10)", c, null, null));
        }

        [Fact]
        public void Max()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(100, evaluator.Evaluate("max(10, -10, 1, 20, 100, 2)", c, null, null));
        }

        [Fact]
        public void Min()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(-10, evaluator.Evaluate("min(10, -10, 1, 20, 100, 2)", c, null, null));
        }

        [Fact]
        public void Nint()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.Evaluate("nint(1.2)", c, null, null));
            Assert.Equal(2, evaluator.Evaluate("nint(1.9)", c, null, null));
        }

        [Fact]
        public void URamp()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1.2, evaluator.Evaluate("uramp(1.2)", c, null, null));
            Assert.Equal(0, evaluator.Evaluate("uramp(-0.1)", c, null, null));
        }

        [Fact]
        public void U()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            Assert.Equal(1, evaluator.Evaluate("u(1.2)", c, null, null));
            Assert.Equal(0, evaluator.Evaluate("u(-1)", c, null, null));
        }

        [Fact]
        public void UnitInExpression()
        {
            // arrange
            var evaluator = new Evaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(100 * 1000, evaluator.Evaluate("300kHz/3", c, null, null));
        }

        [Fact]
        public void Fibonacci()
        {
            var functionFactory = new FunctionFactory();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            var p = new Evaluator();
            c.AddFunction("fib",
                functionFactory.Create("fib",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : (fib(x-1) + fib(x-2)))"));

            Assert.Equal(0, p.Evaluate("fib(0)", c, null, null));
            Assert.Equal(1, p.Evaluate("fib(1)", c, null, null));
            Assert.Equal(1, p.Evaluate("fib(2)", c, null, null));
            Assert.Equal(2, p.Evaluate("fib(3)", c, null, null));
            Assert.Equal(3, p.Evaluate("fib(4)", c, null, null));
            Assert.Equal(5, p.Evaluate("fib(5)", c, null, null));
            Assert.Equal(8, p.Evaluate("fib(6)", c, null, null));
        }

        [Fact]
        public void PolyThreeVariablesSum()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(15, p.Evaluate("poly(3, 3, 5, 7, 0, 1, 1, 1)", c, null, null));
        }

        [Fact]
        public void PolyTwoVariablesSum()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(3, p.Evaluate("poly(2, 1, 2, 0, 1, 1)", c, null, null));
        }

        [Fact]
        public void PolyTwoVariablesMult()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            var context = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            Assert.Equal(6, p.Evaluate("poly(2, 3, 2, 0, 0, 0, 0, 1)", c, null, null));
        }

        [Fact]
        public void PolyOneVariableSquare()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(4, p.Evaluate("poly(1, 2, 0, 0, 1)", c, null, null));
        }

        [Fact]
        public void PolyOneVariablePowerOfThree()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(8, p.Evaluate("poly(1, 2, 0, 0, 0, 1)", c, null, null));
        }

        [Fact]
        public void PolyOneVariableMultiple()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(4, p.Evaluate("poly(1, 2, 0, 2)", c, null, null));
        }

        [Fact]
        public void PolyOneVariableSquerePlusConstant()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new Evaluator();
            Assert.Equal(14, p.Evaluate("poly(1, 2, 10, 0, 1)", c, null, null));
        }
    }
}
