using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using SpiceSharpParser.Lexers.Expressions;
using Xunit;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation.Laplace
{
    public class LaplaceExpressionParserTests
    {
        public static IEnumerable<object[]> ConstantExpressions
        {
            get
            {
                yield return new object[] { "1", new[] { 1.0 }, new[] { 1.0 } };
                yield return new object[] { "-10", new[] { -10.0 }, new[] { 1.0 } };
                yield return new object[] { "1e-6", new[] { 1e-6 }, new[] { 1.0 } };
                yield return new object[] { "PI", new[] { Math.PI }, new[] { 1.0 } };
                yield return new object[] { "2*PI", new[] { 2.0 * Math.PI }, new[] { 1.0 } };
            }
        }

        public static IEnumerable<object[]> RejectedExpressions
        {
            get
            {
                yield return new object[] { "1/(1+s^0.5)" };
                yield return new object[] { "1/(1+s^-1)" };
                yield return new object[] { "sin(s)/(1+s)" };
                yield return new object[] { "random()/(1+s)" };
                yield return new object[] { "gauss(1)/(1+s)" };
                yield return new object[] { "V(x)*s/(1+s)" };
                yield return new object[] { "I(Vsense)/(1+s)" };
                yield return new object[] { "@m[id]/(1+s)" };
                yield return new object[] { "1/s" };
                yield return new object[] { "s" };
                yield return new object[] { "s/(1)" };
                yield return new object[] { "1/0" };
                yield return new object[] { "1/(s-s)" };
                yield return new object[] { "unknown/(1+s)" };
                yield return new object[] { "1/(1+s+s^2+s^3+s^4+s^5+s^6+s^7+s^8+s^9+s^10+s^11)" };
            }
        }

        [Theory]
        [MemberData(nameof(ConstantExpressions))]
        public void When_ConstantExpressionIsParsed_Expect_ConstantTransfer(
            string expression,
            double[] expectedNumerator,
            double[] expectedDenominator)
        {
            var transfer = Parse(expression);

            AssertTransfer(expectedNumerator, expectedDenominator, transfer);
        }

        [Fact]
        public void When_ParameterizedConstantExpressionIsParsed_Expect_ExpandedCoefficient()
        {
            var context = CreateContext();
            context.SetParameter("fc", 1000.0);

            var transfer = Parse("2*PI*fc", context);

            AssertTransfer(new[] { 2.0 * Math.PI * 1000.0 }, new[] { 1.0 }, transfer);
        }

        [Fact]
        public void When_LowPassIsParsed_Expect_AscendingCoefficients()
        {
            var context = CreateContext();
            context.SetParameter("tau", 1e-6);

            var transfer = Parse("1/(1+s*tau)", context);

            AssertTransfer(new[] { 1.0 }, new[] { 1.0, 1e-6 }, transfer);
        }

        [Fact]
        public void When_LowPassNodeIsParsed_Expect_AscendingCoefficients()
        {
            var context = CreateContext();
            context.SetParameter("tau", 1e-6);
            var node = Parser.Parse(Lexer.FromString("1/(1+s*tau)"), true);

            var transfer = new LaplaceExpressionParser(context).Parse(node);

            AssertTransfer(new[] { 1.0 }, new[] { 1.0, 1e-6 }, transfer);
        }

        [Fact]
        public void When_LowPassUsesUppercaseS_Expect_SymbolicS()
        {
            var transfer = Parse("1/(1+S*1u)");

            AssertTransfer(new[] { 1.0 }, new[] { 1.0, 1e-6 }, transfer);
        }

        [Fact]
        public void When_HighPassIsParsed_Expect_ZeroConstantNumeratorPreserved()
        {
            var context = CreateContext();
            context.SetParameter("fc", 1000.0);
            context.SetParameter("wc", "2*PI*fc");
            var wc = 2.0 * Math.PI * 1000.0;

            var transfer = Parse("s/(s+wc)", context);

            AssertTransfer(new[] { 0.0, 1.0 }, new[] { wc, 1.0 }, transfer);
        }

        [Fact]
        public void When_STimesSIsParsed_Expect_SecondOrderNumerator()
        {
            var transfer = Parse("(s*s)/(s*s+s+1)");

            AssertTransfer(new[] { 0.0, 0.0, 1.0 }, new[] { 1.0, 1.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_SPowerTwoIsParsed_Expect_SecondOrderNumerator()
        {
            var transfer = Parse("(s^2)/(s^2+s+1)");

            AssertTransfer(new[] { 0.0, 0.0, 1.0 }, new[] { 1.0, 1.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_DenominatorHasInteriorZero_Expect_InteriorZeroPreserved()
        {
            var transfer = Parse("1/(s^2+1)");

            AssertTransfer(new[] { 1.0 }, new[] { 1.0, 0.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_InvertingLowPassIsParsed_Expect_NegativeNumerator()
        {
            var context = CreateContext();
            context.SetParameter("gain", -10.0);
            context.SetParameter("fc", 10000.0);
            context.SetParameter("wc", "2*PI*fc");
            var wc = 2.0 * Math.PI * 10000.0;

            var transfer = Parse("gain*wc/(s+wc)", context);

            AssertTransfer(new[] { -10.0 * wc }, new[] { wc, 1.0 }, transfer);
        }

        [Fact]
        public void When_LeadLagIsParsed_Expect_Coefficients()
        {
            var context = CreateContext();
            context.SetParameter("wz", 1000.0);
            context.SetParameter("wp", 10000.0);
            context.SetParameter("k", 2.0);

            var transfer = Parse("k*(1+s/wz)/(1+s/wp)", context);

            AssertTransfer(new[] { 2.0, 2.0 / 1000.0 }, new[] { 1.0, 1.0 / 10000.0 }, transfer);
        }

        [Fact]
        public void When_ButterworthBiquadIsParsed_Expect_SecondOrderDenominator()
        {
            var context = CreateContext();
            context.SetParameter("f0", 10000.0);
            context.SetParameter("w0", "2*PI*f0");
            context.SetParameter("q", 0.70710678);
            var w0 = 2.0 * Math.PI * 10000.0;

            var transfer = Parse("w0*w0/(s*s+s*w0/q+w0*w0)", context);

            Assert.Equal(2, transfer.Order);
            AssertTransfer(new[] { w0 * w0 }, new[] { w0 * w0, w0 / 0.70710678, 1.0 }, transfer);
        }

        [Fact]
        public void When_FunctionCoefficientIsParsed_Expect_ExpandedCoefficient()
        {
            var transfer = Parse("sqrt(4)/(1+s)");

            AssertTransfer(new[] { 2.0 }, new[] { 1.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_ButterworthBiquadUsesSqrtCoefficient_Expect_SecondOrderDenominator()
        {
            var context = CreateContext();
            context.SetParameter("fc", 1000.0);
            context.SetParameter("wc", "2*PI*fc");
            var wc = 2.0 * Math.PI * 1000.0;

            var transfer = Parse("wc^2/(s^2+sqrt(2)*wc*s+wc^2)", context);

            AssertTransfer(new[] { wc * wc }, new[] { wc * wc, Math.Sqrt(2.0) * wc, 1.0 }, transfer);
        }

        [Fact]
        public void When_BandPassBiquadIsParsed_Expect_FirstOrderNumerator()
        {
            var context = CreateContext();
            context.SetParameter("f0", 10000.0);
            context.SetParameter("q", 5.0);
            context.SetParameter("w0", "2*PI*f0");
            var w0 = 2.0 * Math.PI * 10000.0;

            var transfer = Parse("(s*w0/q)/(s*s+s*w0/q+w0*w0)", context);

            AssertTransfer(new[] { 0.0, w0 / 5.0 }, new[] { w0 * w0, w0 / 5.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_ChainedParametersAreParsed_Expect_ExpandedTransfer()
        {
            var context = CreateContext();
            context.SetParameter("fc", 1000.0);
            context.SetParameter("wc", "2*PI*fc");
            var wc = 2.0 * Math.PI * 1000.0;

            var transfer = Parse("wc/(s+wc)", context);

            AssertTransfer(new[] { wc }, new[] { wc, 1.0 }, transfer);
        }

        [Fact]
        public void When_CoefficientArraysAreMutated_Expect_TransferFunctionUnchanged()
        {
            var transfer = Parse("1/(1+s)");

            var numerator = transfer.NumeratorCoefficients;
            var denominator = transfer.DenominatorCoefficients;
            numerator[0] = 42.0;
            denominator[0] = 42.0;

            AssertTransfer(new[] { 1.0 }, new[] { 1.0, 1.0 }, transfer);
        }

        [Fact]
        public void When_EvaluatingLowPassAtCutoff_Expect_ExpectedComplexGain()
        {
            var context = CreateContext();
            context.SetParameter("tau", 1e-6);
            var transfer = Parse("1/(1+s*tau)", context);

            var value = transfer.EvaluateComplex(new Complex(0.0, 1.0 / 1e-6));

            AssertComplex(new Complex(0.5, -0.5), value);
        }

        [Fact]
        public void When_ZeroTransferIsParsed_Expect_ZeroNumerator()
        {
            var transfer = Parse("0/(1+s)");

            AssertTransfer(new[] { 0.0 }, new[] { 1.0, 1.0 }, transfer);
        }

        [Theory]
        [MemberData(nameof(RejectedExpressions))]
        public void When_ExpressionIsUnsafeOrUnsupported_Expect_Rejection(string expression)
        {
            Assert.Throws<LaplaceExpressionException>(() => Parse(expression));
        }

        [Fact]
        public void When_HiddenSymbolicParameterIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("pole", "s+1000");

            Assert.Throws<LaplaceExpressionException>(() => Parse("1/(1+pole)", context));
        }

        [Fact]
        public void When_HiddenStochasticParameterIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("gain", "random()");

            Assert.Throws<LaplaceExpressionException>(() => Parse("gain/(1+s)", context));
        }

        [Fact]
        public void When_HiddenVoltageProbeParameterIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("gain", "V(x)");

            Assert.Throws<LaplaceExpressionException>(() => Parse("gain/(1+s)", context));
        }

        [Fact]
        public void When_HiddenCurrentProbeParameterIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("gain", "I(Vsense)");

            Assert.Throws<LaplaceExpressionException>(() => Parse("gain/(1+s)", context));
        }

        [Fact]
        public void When_HiddenPropertyProbeParameterIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("gain", "@m[id]");

            Assert.Throws<LaplaceExpressionException>(() => Parse("gain/(1+s)", context));
        }

        [Fact]
        public void When_NaNCoefficientIsUsed_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => Parse("NaN/(1+s)"));
        }

        [Fact]
        public void When_InfiniteCoefficientIsUsed_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("inf", double.PositiveInfinity);

            Assert.Throws<LaplaceExpressionException>(() => Parse("inf/(1+s)", context));
        }

        [Fact]
        public void When_ReservedParameterNameSExists_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("s", 1.0);

            Assert.Throws<LaplaceExpressionException>(() => Parse("1/(1+s)", context));
        }

        [Fact]
        public void When_ReservedParameterNameUppercaseSExists_Expect_Rejection()
        {
            var context = CreateContext();
            context.SetParameter("S", 1.0);

            Assert.Throws<LaplaceExpressionException>(() => Parse("1/(1+s)", context));
        }

        private static LaplaceTransferFunction Parse(string expression)
        {
            return Parse(expression, CreateContext());
        }

        private static LaplaceTransferFunction Parse(string expression, EvaluationContext context)
        {
            return new LaplaceExpressionParser(context).Parse(expression);
        }

        private static EvaluationContext CreateContext()
        {
            var caseSettings = new SpiceNetlistCaseSensitivitySettings();
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                caseSettings.IsEntityNamesCaseSensitive,
                ".");

            var objectNameGenerator = new ObjectNameGenerator(string.Empty, ".");
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            var expressionParserFactory = new ExpressionParserFactory(caseSettings);
            var expressionResolverFactory = new ExpressionResolverFactory(caseSettings);

            var context = new SpiceEvaluationContext(
                string.Empty,
                caseSettings,
                new Randomizer(caseSettings.IsDistributionNameCaseSensitive, seed: 0),
                expressionParserFactory,
                new ExpressionFeaturesReader(expressionParserFactory, expressionResolverFactory),
                nameGenerator);

            context.Evaluator = new Evaluator(context, new ExpressionValueProvider(expressionParserFactory));
            return context;
        }

        private static void AssertTransfer(
            double[] expectedNumerator,
            double[] expectedDenominator,
            LaplaceTransferFunction transfer)
        {
            AssertCoefficients(expectedNumerator, transfer.Numerator);
            AssertCoefficients(expectedDenominator, transfer.Denominator);
            AssertCoefficients(expectedNumerator, transfer.NumeratorCoefficients);
            AssertCoefficients(expectedDenominator, transfer.DenominatorCoefficients);
        }

        private static void AssertCoefficients(double[] expected, IReadOnlyList<double> actual)
        {
            Assert.Equal(expected.Length, actual.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                var tolerance = Math.Max(1e-10, Math.Abs(expected[i]) * 1e-12);
                Assert.True(
                    Math.Abs(expected[i] - actual[i]) <= tolerance,
                    $"Expected coefficient {i} to be {expected[i]}, but found {actual[i]}.");
            }
        }

        private static void AssertComplex(Complex expected, Complex actual)
        {
            Assert.Equal(expected.Real, actual.Real, 12);
            Assert.Equal(expected.Imaginary, actual.Imaginary, 12);
        }
    }
}
