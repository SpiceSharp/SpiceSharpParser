using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation
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
            var v = p.CreateChildEvaluator("child");

            v.SetParameter("xyz", 13.0);
            Assert.Equal(1, v.GetParameterValue("a", null));

            v.SetParameter("a", 2);
            Assert.Equal(2, v.GetParameterValue("a", null));
            Assert.Equal(1, p.GetParameterValue("a", null));
        }

        [Fact]
        public void AddActionExpressionTest()
        {
            // arrange
            var v = new SpiceEvaluator();
            v.SetParameter("xyz", 13.0);

            double expressionValue = 0;

            // act
            v.AddAction("noname", "xyz + 1", (simulation, newValue) => { expressionValue = newValue; });
            v.SetParameter("xyz", 14);

            var val = v.GetParameterValue("xyz", null);

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
            var parameters = v.GetParametersFromExpression("xyz + 1 + a");

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
        public void TableBasicTest()
        {
            SpiceEvaluator v = new SpiceEvaluator();
            v.SetParameter("N", 1.0);
            Assert.Equal(10, v.EvaluateDouble("table(N, 1, pow(10, 1), 2 + 0, 20, 3, 30)"));
        }

        [Fact]
        public void TableInterpolationTest()
        {
            SpiceEvaluator v = new SpiceEvaluator();

            v.SetParameter("N", 1.5);
            Assert.Equal(-5, v.EvaluateDouble("table(N, 1, 0, 2, -10"));

            v.SetParameter("N", 3);
            Assert.Equal(-20, v.EvaluateDouble("table(N, 1, 0, 2, -10"));

            v.SetParameter("N", 0);
            Assert.Equal(10, v.EvaluateDouble("table(N, 1, 0, 2, -10"));

            v.SetParameter("N", -1);
            Assert.Equal(20, v.EvaluateDouble("table(N, 1, 0, 2, -10"));
        }

        [Fact]
        public void TableAdvancedTest()
        {
            SpiceEvaluator v = new SpiceEvaluator();
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
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(8, evaluator.EvaluateDouble("2**3"));
        }

        [Fact]
        public void PowerInfixPrecedenceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(7, evaluator.EvaluateDouble("2**3-1"));
        }

        [Fact]
        public void PowerInfixSecondPrecedenceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(8, evaluator.EvaluateDouble("1+2**3-1"));
        }

        [Fact]
        public void PowerInfixThirdPrecedenceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(17, evaluator.EvaluateDouble("1+2**3*2"));
        }

        [Fact]
        public void Round()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("round(1.2)"));
            Assert.Equal(2, evaluator.EvaluateDouble("round(1.9)"));
        }

        [Fact]
        public void PowMinusLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void PwrLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);
            // act and assert
            Assert.Equal(8, evaluator.EvaluateDouble("pwr(-2,3)"));
        }

        [Fact]
        public void PwrHSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);
            // act and assert
            Assert.Equal(-8, evaluator.EvaluateDouble("pwr(-2,3)"));
        }

        [Fact]
        public void PwrSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);
            // act and assert
            Assert.Equal(-8, evaluator.EvaluateDouble("pwr(-2,3)"));
        }

        [Fact]
        public void MinusPowerInfixLtSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("-2**1.5"));
        }

        [Fact]
        public void PowMinusSmartSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);
            // act and assert
            Assert.Equal(Math.Pow(2, (int)1.5), evaluator.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void PowMinusHSpice()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);
            // act and assert
            Assert.Equal(Math.Pow(-2, (int)1.5), evaluator.EvaluateDouble("pow(-2,1.5)"));
        }

        [Fact]
        public void SgnTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("sgn(0)"));
            Assert.Equal(-1, evaluator.EvaluateDouble("sgn(-1)"));
            Assert.Equal(1, evaluator.EvaluateDouble("sgn(0.1)"));
        }

        [Fact]
        public void SqrtTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(2, evaluator.EvaluateDouble("sqrt(4)"));
        }

        [Fact]
        public void SqrtMinusHSpiceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.HSpice);
            // act and assert
            Assert.Equal(-2, evaluator.EvaluateDouble("sqrt(-4)"));
        }

        [Fact]
        public void SqrtMinusSmartSpiceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);
            // act and assert
            Assert.Equal(2, evaluator.EvaluateDouble("sqrt(-4)"));
        }

        [Fact]
        public void SqrtMinusLtSpiceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.LtSpice);
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("sqrt(-4)"));
        }

        [Fact]
        public void DefPositiveTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            evaluator.SetParameter("x1", 1);
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("def(x1)"));
        }

        [Fact]
        public void DefNegativeTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("def(x1)"));
        }

        [Fact]
        public void AbsTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("abs(-1)"));
        }

        [Fact]
        public void BufTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("buf(0.6)"));
            Assert.Equal(0, evaluator.EvaluateDouble("buf(0.3)"));
        }

        [Fact]
        public void CbrtTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(2, evaluator.EvaluateDouble("cbrt(8)"));
        }

        [Fact]
        public void CeilTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(3, evaluator.EvaluateDouble("ceil(2.9)"));
        }

        [Fact]
        public void DbSmartSpiceTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator(SpiceEvaluatorMode.SmartSpice);
            // act and assert
            Assert.Equal(20, evaluator.EvaluateDouble("db(-10)"));
        }

        [Fact]
        public void DbTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(-20, evaluator.EvaluateDouble("db(-10)"));
        }

        [Fact]
        public void ExpTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(Math.Exp(2), evaluator.EvaluateDouble("exp(2)"));
        }

        [Fact]
        public void FabsTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(3, evaluator.EvaluateDouble("fabs(-3)"));
        }

        [Fact]
        public void FlatTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act
            var res = evaluator.EvaluateDouble("flat(10)");

            // assert
            Assert.True(res >= -10 && res <= 10);
        }

        [Fact]
        public void FloorTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(2, evaluator.EvaluateDouble("floor(2.3)"));
        }

        [Fact]
        public void HypotTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(5, evaluator.EvaluateDouble("hypot(3,4)"));
        }

        [Fact]
        public void IfTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(3, evaluator.EvaluateDouble("if(0.5, 2, 3)"));
            Assert.Equal(2, evaluator.EvaluateDouble("if(0.6, 2, 3)"));
        }

        [Fact]
        public void IntTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("int(1.3)"));
        }

        [Fact]
        public void InvTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(0, evaluator.EvaluateDouble("inv(0.51)"));
            Assert.Equal(1, evaluator.EvaluateDouble("inv(0.5)"));
        }

        [Fact]
        public void LnTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("ln(e)"));
        }

        [Fact]
        public void LogTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("log(e)"));
        }

        [Fact]
        public void Log10Test()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("log10(10)"));
        }

        [Fact]
        public void MaxTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(100, evaluator.EvaluateDouble("max(10, -10, 1, 20, 100, 2)"));
        }

        [Fact]
        public void MinTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(-10, evaluator.EvaluateDouble("min(10, -10, 1, 20, 100, 2)"));
        }

        [Fact]
        public void NintTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(1, evaluator.EvaluateDouble("nint(1.2)"));
            Assert.Equal(2, evaluator.EvaluateDouble("nint(1.9)"));
        }

        [Fact]
        public void URampTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();

            // act and assert
            Assert.Equal(1.2, evaluator.EvaluateDouble("uramp(1.2)"));
            Assert.Equal(0, evaluator.EvaluateDouble("uramp(-0.1)"));
        }

        [Fact]
        public void UTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            Assert.Equal(1, evaluator.EvaluateDouble("u(1.2)"));
            Assert.Equal(0, evaluator.EvaluateDouble("u(-1)"));
        }

        [Fact]
        public void UnitInExpressionTest()
        {
            // arrange
            var evaluator = new SpiceEvaluator();
            // act and assert
            Assert.Equal(100 * 1000, evaluator.EvaluateDouble("300kHz/3"));
        }

        [Fact]
        public void FibonacciCustomFunction()
        {
            // arrange
            var p = new SpiceEvaluator();

            //TODO: It shouldn't be that messy ...
            Func<object[], object, IEvaluator, object> fibLogic = null; //TODO: Use smarter methods to define anonymous recursion in C# (there is a nice post on some nice blog on msdn)
            fibLogic = (object[] args, object context, IEvaluator evaluator) =>
            {
                double x = (double)args[0];

                if (x == 0.0)
                {
                    return 0.0;
                }

                if (x == 1.0)
                {
                    return 1.0;
                }

                return (double)fibLogic(new object[1] { (x - 1) }, context, evaluator) + (double)fibLogic(new object[1] { (x - 2) }, context, evaluator);
            };

            var fib = new CustomFunction()
            {
                ArgumentsCount = 1,
                Logic = fibLogic,
                VirtualParameters = false,
            };
            p.CustomFunctions.Add("fib",  fib);

            Assert.Equal(0, p.EvaluateDouble("fib(0)"));
            Assert.Equal(1, p.EvaluateDouble("fib(1)"));
            Assert.Equal(1, p.EvaluateDouble("fib(2)"));
            Assert.Equal(2, p.EvaluateDouble("fib(3)"));
            Assert.Equal(3, p.EvaluateDouble("fib(4)"));
            Assert.Equal(5, p.EvaluateDouble("fib(5)"));
            Assert.Equal(8, p.EvaluateDouble("fib(6)"));
        }

        [Fact]
        public void FibonacciAsParam()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "fib",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : lazy(#fib(x-1) + fib(x-2)#))");

            Assert.Equal(0, p.EvaluateDouble("fib(0)"));
            Assert.Equal(1, p.EvaluateDouble("fib(1)"));
            Assert.Equal(1, p.EvaluateDouble("fib(2)"));
            Assert.Equal(2, p.EvaluateDouble("fib(3)"));
            Assert.Equal(3, p.EvaluateDouble("fib(4)"));
            Assert.Equal(5, p.EvaluateDouble("fib(5)"));
            Assert.Equal(8, p.EvaluateDouble("fib(6)"));
        }

        [Fact]
        public void FibonacciAsWithoutLazyParam()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "fib",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : (fib(x-1) + fib(x-2)))");

            Assert.Equal(0, p.EvaluateDouble("fib(0)"));
            Assert.Equal(1, p.EvaluateDouble("fib(1)"));
            Assert.Equal(1, p.EvaluateDouble("fib(2)"));
            Assert.Equal(2, p.EvaluateDouble("fib(3)"));
            Assert.Equal(3, p.EvaluateDouble("fib(4)"));
            Assert.Equal(5, p.EvaluateDouble("fib(5)"));
            Assert.Equal(8, p.EvaluateDouble("fib(6)"));
        }

        [Fact]
        public void FactAsParam()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "fact",
                new System.Collections.Generic.List<string>() { "x" },
                "x == 0 ? 1 : (x * lazy(#fact(x-1)#))");

            Assert.Equal(1, p.EvaluateDouble("fact(0)"));
            Assert.Equal(1, p.EvaluateDouble("fact(1)"));
            Assert.Equal(2, p.EvaluateDouble("fact(2)"));
            Assert.Equal(6, p.EvaluateDouble("fact(3)"));
        }

        [Fact]
        public void LazySimpleTest()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "test_lazy",
                new System.Collections.Generic.List<string>() { "x" },
                "x == 0 ? 1: lazy(#3+2#)");

            Assert.Equal(1, p.EvaluateDouble("test_lazy(0)"));
            Assert.Equal(5, p.EvaluateDouble("test_lazy(1)"));
        }

        [Fact]
        public void LazyErrorTest()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "test_lazy",
                new System.Collections.Generic.List<string>() { "x" },
                "x == 0 ? 1: lazy(#1/#)");

            Assert.Equal(1, p.EvaluateDouble("test_lazy(0)"));
        }

        [Fact]
        public void ComplexCondBrokenTest()
        {
            var p = new SpiceEvaluator();
            var expr = "x <= 9 ? 3 : (x == 5 ? 1 : lazy(#2/-#))";

            p.SetParameter("x", 9);
            Assert.Equal(3, p.EvaluateDouble(expr));
        }

        [Fact]
        public void SimpleCondTest()
        {
            var p = new SpiceEvaluator();
            var expr = "x <= 0 ? 0 : lazy(#2#)";

            p.SetParameter("x", 0);
            Assert.Equal(0, p.EvaluateDouble(expr));

            p.SetParameter("x", 1);
            Assert.Equal(2, p.EvaluateDouble(expr));
        }

        [Fact]
        public void LazyTest()
        {
            var p = new SpiceEvaluator();
            Assert.Equal(4, p.EvaluateDouble("2 >= 0 ? lazy(#3+1#) : lazy(#4+5#)"));
            Assert.Equal(8, p.EvaluateDouble("1 <= 0 ? lazy(#1+1#) : lazy(#3+5#)"));
        }

        [Fact]
        public void LazyFuncTest()
        {
            var p = new SpiceEvaluator();
            p.AddCustomFunction(
                "test",
                new System.Collections.Generic.List<string>(),
                "5");

            p.AddCustomFunction(
                "test2",
                new System.Collections.Generic.List<string>() { "x" },
                "x <= 0 ? 0 : (x == 1 ? 1 : lazy(#test()#))");

            //Assert.Equal(0, p.EvaluateDouble("test2(0)"));
            //Assert.Equal(1, p.EvaluateDouble("test2(1)"));
            Assert.Equal(5, p.EvaluateDouble("test2(2)"));
        }
    }
}
