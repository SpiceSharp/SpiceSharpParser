using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Parsers.Expression;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation
{
    public class SpiceEvaluatorTests
    {
        [Fact]
        public void ParentEvaluator()
        {
            // arrange
            var p = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("a", 1);

            // act and assert
            var v = c.CreateChildContext("child", false);

            v.SetParameter("xyz", 13.0);
            Assert.Equal(1, v.Parameters["a"].CurrentValue);

            v.SetParameter("a", 2);
            Assert.Equal(2, v.Parameters["a"].CurrentValue);
            Assert.Equal(1, c.Parameters["a"].CurrentValue);
        }

        [Fact]
        public void EvaluateParameter()
        {
            Evaluator v = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("xyz", 13.0);

            Assert.Equal(14, v.EvaluateValueExpression("xyz + 1", c));
        }

        [Fact]
        public void GetVariables()
        {
            // prepare
            IExpressionParser v = new SpiceExpressionParser(false);
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("xyz", 13.0);
            c.SetParameter("a", 1.0);

            // act
            var parameters = v.Parse("xyz + 1 + a", new ExpressionParserContext()).FoundParameters;

            // assert
            Assert.Contains("a", parameters);
            Assert.Contains("xyz", parameters);
        }

        [Fact]
        public void EvaluateSuffix()
        {
            Evaluator v = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            Assert.Equal(2, v.EvaluateValueExpression("1V + 1", c));
        }

        [Fact]
        public void TableBasic()
        {
            SpiceEvaluator v = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("N", 1.0);
            Assert.Equal(10, v.EvaluateValueExpression("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)", c));
        }

        [Fact]
        public void TableInterpolation()
        {
            SpiceEvaluator v = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            c.SetParameter("N", 1.5);
            Assert.Equal(-5, v.EvaluateValueExpression("table(N, 1, 0, 2, -10)", c));

            c.SetParameter("N", 3);
            Assert.Equal(-10, v.EvaluateValueExpression("table(N, 1, 0, 2, -10)", c));

            c.SetParameter("N", 0);
            Assert.Equal(0, v.EvaluateValueExpression("table(N, 1, 0, 2, -10)", c));

            c.SetParameter("N", -1);
            Assert.Equal(0, v.EvaluateValueExpression("table(N, 1, 0, 2, -10)", c));
        }

        [Fact]
        public void TableAdvanced()
        {
            SpiceEvaluator v = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("N", 1.0);
            Assert.Equal(10, v.EvaluateValueExpression("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)", c));
        }

        [Fact]
        public void PowerInfix()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(8, evaluator.EvaluateValueExpression("2**3", c));
        }

        [Fact]
        public void PowerInfixPrecedence()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(7, evaluator.EvaluateValueExpression("2**3-1", c));
        }

        [Fact]
        public void PowerInfixSecondPrecedence()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(8, evaluator.EvaluateValueExpression("1+2**3-1", c));
        }

        [Fact]
        public void PowerInfixThirdPrecedence()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(17, evaluator.EvaluateValueExpression("1+2**3*2", c));
        }

        [Fact]
        public void Round()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("round(1.2)", c));
            Assert.Equal(2, evaluator.EvaluateValueExpression("round(1.9)", c));
        }

        [Fact]
        public void PowMinusLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);

            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("pow(-2,1.5)", c));
        }

        [Fact]
        public void PwrLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);

            // act and assert
            Assert.Equal(8, evaluator.EvaluateValueExpression("pwr(-2,3)", c));
        }

        [Fact]
        public void PwrHSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.HSpice);

            // act and assert
            Assert.Equal(-8, evaluator.EvaluateValueExpression("pwr(-2,3)", c));
        }

        [Fact]
        public void PwrSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(-8, evaluator.EvaluateValueExpression("pwr(-2,3)", c));
        }

        [Fact]
        public void MinusPowerInfixLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(true);
            var c = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);

            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("-2**1.5", c));
        }

        [Fact]
        public void PowMinusSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(Math.Pow(2, (int)1.5), evaluator.EvaluateValueExpression("pow(-2,1.5)", c));
        }

        [Fact]
        public void PowMinusHSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.HSpice);
            // act and assert
            Assert.Equal(Math.Pow(-2, (int)1.5), evaluator.EvaluateValueExpression("pow(-2,1.5)", c));
        }

        [Fact]
        public void Sgn()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("sgn(0)", c));
            Assert.Equal(-1, evaluator.EvaluateValueExpression("sgn(-1)", c));
            Assert.Equal(1, evaluator.EvaluateValueExpression("sgn(0.1)", c));
        }

        [Fact]
        public void Sqrt()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.EvaluateValueExpression("sqrt(4)", c));
        }

        [Fact]
        public void SqrtMinusHSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.HSpice);

            // act and assert
            Assert.Equal(-2, evaluator.EvaluateValueExpression("sqrt(-4)", c));
        }

        [Fact]
        public void SqrtMinusSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(2, evaluator.EvaluateValueExpression("sqrt(-4)", c));
        }

        [Fact]
        public void SqrtMinusLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);

            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("sqrt(-4)", c));
        }

        [Fact]
        public void DefPositive()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            c.SetParameter("x1", 1);
            
            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("def(x1)", c));
        }

        [Fact]
        public void DefNegative()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("def(x1)", c));
        }

        [Fact]
        public void Abs()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("abs(-1)", c));
        }

        [Fact]
        public void Buf()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("buf(0.6)", c));
            Assert.Equal(0, evaluator.EvaluateValueExpression("buf(0.3)", c));
        }

        [Fact]
        public void Cbrt()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.EvaluateValueExpression("cbrt(8)", c));
        }

        [Fact]
        public void Ceil()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.EvaluateValueExpression("ceil(2.9)", c));
        }

        [Fact]
        public void DbSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.SmartSpice);

            // act and assert
            Assert.Equal(20, evaluator.EvaluateValueExpression("db(-10)", c));
        }

        [Fact]
        public void Db()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(-20, evaluator.EvaluateValueExpression("db(-10)", c));
        }

        [Fact]
        public void Exp()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(Math.Exp(2), evaluator.EvaluateValueExpression("exp(2)", c));
        }

        [Fact]
        public void Fabs()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.EvaluateValueExpression("fabs(-3)", c));
        }

        [Fact]
        public void Flat()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act
            var res = evaluator.EvaluateValueExpression("flat(10)", c);

            // assert
            Assert.True(res >= -10 && res <= 10);
        }

        [Fact]
        public void Floor()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(2, evaluator.EvaluateValueExpression("floor(2.3)", c));
        }

        [Fact]
        public void Hypot()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(5, evaluator.EvaluateValueExpression("hypot(3,4)", c));
        }

        [Fact]
        public void If()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(3, evaluator.EvaluateValueExpression("if(0.5, 2, 3)", c));
            Assert.Equal(2, evaluator.EvaluateValueExpression("if(0.6, 2, 3)", c));
        }

        [Fact]
        public void Int()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("int(1.3)", c));
        }

        [Fact]
        public void Inv()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(0, evaluator.EvaluateValueExpression("inv(0.51)", c));
            Assert.Equal(1, evaluator.EvaluateValueExpression("inv(0.5)", c));
        }

        [Fact]
        public void Ln()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("ln(e)", c));
        }

        [Fact]
        public void Limit()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(8, evaluator.EvaluateValueExpression("limit(10, 1, 8)", c));
            Assert.Equal(1, evaluator.EvaluateValueExpression("limit(-1, 1, 8)", c));
            Assert.Equal(4, evaluator.EvaluateValueExpression("limit(4, 1, 8)", c));
        }

        [Fact]
        public void Log()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("log(e)", c));
        }

        [Fact]
        public void Log10()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("log10(10)", c));
        }

        [Fact]
        public void Max()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(100, evaluator.EvaluateValueExpression("max(10, -10, 1, 20, 100, 2)", c));
        }

        [Fact]
        public void Min()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(-10, evaluator.EvaluateValueExpression("min(10, -10, 1, 20, 100, 2)", c));
        }

        [Fact]
        public void Nint()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1, evaluator.EvaluateValueExpression("nint(1.2)", c));
            Assert.Equal(2, evaluator.EvaluateValueExpression("nint(1.9)", c));
        }

        [Fact]
        public void URamp()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(1.2, evaluator.EvaluateValueExpression("uramp(1.2)", c));
            Assert.Equal(0, evaluator.EvaluateValueExpression("uramp(-0.1)", c));
        }

        [Fact]
        public void U()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            Assert.Equal(1, evaluator.EvaluateValueExpression("u(1.2)", c));
            Assert.Equal(0, evaluator.EvaluateValueExpression("u(-1)", c));
        }

        [Fact]
        public void UnitInExpression()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            // act and assert
            Assert.Equal(100 * 1000, evaluator.EvaluateValueExpression("300kHz/3", c));
        }

        //[Fact]
        public void FibonacciAsParam()
        {
            var functionFactory = new FunctionFactory();

            var p = new SpiceEvaluator();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            c.Functions.Add("fib",
                functionFactory.Create("fib",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : lazy(#fib(x-1) + fib(x-2)#))"));

            Assert.Equal(0, p.EvaluateValueExpression("fib(0)", c));
            Assert.Equal(1, p.EvaluateValueExpression("fib(1)", c));
            Assert.Equal(1, p.EvaluateValueExpression("fib(2)", c));
            Assert.Equal(2, p.EvaluateValueExpression("fib(3)", c));
            Assert.Equal(3, p.EvaluateValueExpression("fib(4)", c));
            Assert.Equal(5, p.EvaluateValueExpression("fib(5)", c));
            Assert.Equal(8, p.EvaluateValueExpression("fib(6)", c));
        }

        [Fact]
        public void FibonacciAsWithoutLazyParam()
        {
            var functionFactory = new FunctionFactory();
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);

            var p = new SpiceEvaluator();
            c.Functions.Add("fib",
                functionFactory.Create("fib",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : (fib(x-1) + fib(x-2)))"));

            Assert.Equal(0, p.EvaluateValueExpression("fib(0)", c));
            Assert.Equal(1, p.EvaluateValueExpression("fib(1)", c));
            Assert.Equal(1, p.EvaluateValueExpression("fib(2)", c));
            Assert.Equal(2, p.EvaluateValueExpression("fib(3)", c));
            Assert.Equal(3, p.EvaluateValueExpression("fib(4)", c));
            Assert.Equal(5, p.EvaluateValueExpression("fib(5)", c));
            Assert.Equal(8, p.EvaluateValueExpression("fib(6)", c));
        }

        [Fact]
        public void PolyThreeVariablesSum()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(15, p.EvaluateValueExpression("poly(3, 3, 5, 7, 0, 1, 1, 1)", c));
        }

        [Fact]
        public void PolyTwoVariablesSum()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(3, p.EvaluateValueExpression("poly(2, 1, 2, 0, 1, 1)", c));
        }

        [Fact]
        public void PolyTwoVariablesMult()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            var context = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            Assert.Equal(6, p.EvaluateValueExpression("poly(2, 3, 2, 0, 0, 0, 0, 1)", c));
        }

        [Fact]
        public void PolyOneVariableSquare()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(4, p.EvaluateValueExpression("poly(1, 2, 0, 0, 1)", c));
        }

        [Fact]
        public void PolyOneVariablePowerOfThree()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(8, p.EvaluateValueExpression("poly(1, 2, 0, 0, 0, 1)", c));
        }

        [Fact]
        public void PolyOneVariableMultiple()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(4, p.EvaluateValueExpression("poly(1, 2, 0, 2)", c));
        }

        [Fact]
        public void PolyOneVariableSquerePlusConstant()
        {
            var c = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            var p = new SpiceEvaluator();
            Assert.Equal(14, p.EvaluateValueExpression("poly(1, 2, 10, 0, 1)", c));
        }
    }
}
