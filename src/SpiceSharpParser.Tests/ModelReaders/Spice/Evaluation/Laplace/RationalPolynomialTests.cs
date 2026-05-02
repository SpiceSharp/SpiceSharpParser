using System.Numerics;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation.Laplace
{
    public class RationalPolynomialTests
    {
        [Fact]
        public void When_DenominatorIsZero_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => new RationalPolynomial(Polynomial.One, Polynomial.Zero));
        }

        [Fact]
        public void When_Adding_Expect_CommonDenominator()
        {
            var left = new RationalPolynomial(new Polynomial(new[] { 1.0 }), new Polynomial(new[] { 1.0, 1.0 }));
            var right = new RationalPolynomial(new Polynomial(new[] { 2.0 }), new Polynomial(new[] { 2.0, 1.0 }));

            var sum = left.Add(right);

            AssertCoefficients(new[] { 4.0, 3.0 }, sum.Numerator);
            AssertCoefficients(new[] { 2.0, 3.0, 1.0 }, sum.Denominator);
        }

        [Fact]
        public void When_Subtracting_Expect_CommonDenominator()
        {
            var left = new RationalPolynomial(new Polynomial(new[] { 1.0 }), new Polynomial(new[] { 1.0, 1.0 }));
            var right = new RationalPolynomial(new Polynomial(new[] { 2.0 }), new Polynomial(new[] { 2.0, 1.0 }));

            var difference = left.Subtract(right);

            AssertCoefficients(new[] { 0.0, -1.0 }, difference.Numerator);
            AssertCoefficients(new[] { 2.0, 3.0, 1.0 }, difference.Denominator);
        }

        [Fact]
        public void When_Multiplying_Expect_NumeratorsAndDenominatorsMultiplied()
        {
            var left = new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), new Polynomial(new[] { 2.0 }));
            var right = new RationalPolynomial(new Polynomial(new[] { 3.0 }), new Polynomial(new[] { 4.0, 1.0 }));

            var product = left.Multiply(right);

            AssertCoefficients(new[] { 3.0, 3.0 }, product.Numerator);
            AssertCoefficients(new[] { 8.0, 2.0 }, product.Denominator);
        }

        [Fact]
        public void When_Dividing_Expect_RightSideInverted()
        {
            var left = new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), new Polynomial(new[] { 2.0 }));
            var right = new RationalPolynomial(new Polynomial(new[] { 3.0 }), new Polynomial(new[] { 4.0, 1.0 }));

            var quotient = left.Divide(right);

            AssertCoefficients(new[] { 4.0, 5.0, 1.0 }, quotient.Numerator);
            AssertCoefficients(new[] { 6.0 }, quotient.Denominator);
        }

        [Fact]
        public void When_DividingByZeroNumerator_Expect_Rejection()
        {
            var left = RationalPolynomial.One;

            Assert.Throws<LaplaceExpressionException>(() => left.Divide(RationalPolynomial.Zero));
        }

        [Fact]
        public void When_RaisedToZero_Expect_OneOverOne()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), new Polynomial(new[] { 2.0, 1.0 }));

            var power = rational.Pow(0);

            AssertCoefficients(new[] { 1.0 }, power.Numerator);
            AssertCoefficients(new[] { 1.0 }, power.Denominator);
        }

        [Fact]
        public void When_RaisedToTwo_Expect_NumeratorAndDenominatorPowers()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), new Polynomial(new[] { 2.0, 1.0 }));

            var power = rational.Pow(2);

            AssertCoefficients(new[] { 1.0, 2.0, 1.0 }, power.Numerator);
            AssertCoefficients(new[] { 4.0, 4.0, 1.0 }, power.Denominator);
        }

        [Fact]
        public void When_RaisedToNegativePower_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => RationalPolynomial.SymbolS.Pow(-1));
        }

        [Fact]
        public void When_EvaluatingComplex_Expect_RationalValue()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0 }), new Polynomial(new[] { 1.0, 1.0 }));

            var value = rational.EvaluateComplex(new Complex(0.0, 1.0));

            AssertComplex(new Complex(0.5, -0.5), value);
        }

        [Fact]
        public void When_AddingZero_Expect_IdentityShapeWithoutGcdSimplification()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0 }), new Polynomial(new[] { 1.0, 1.0 }));

            var sum = rational.Add(RationalPolynomial.Zero);

            AssertCoefficients(new[] { 1.0 }, sum.Numerator);
            AssertCoefficients(new[] { 1.0, 1.0 }, sum.Denominator);
        }

        [Fact]
        public void When_MultiplyingByOne_Expect_IdentityShape()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0, 2.0 }), new Polynomial(new[] { 3.0, 4.0 }));

            var product = rational.Multiply(RationalPolynomial.One);

            AssertCoefficients(new[] { 1.0, 2.0 }, product.Numerator);
            AssertCoefficients(new[] { 3.0, 4.0 }, product.Denominator);
        }

        [Fact]
        public void When_NoGcdSimplification_Expect_CommonFactorsRemain()
        {
            var rational = new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), Polynomial.One);

            var quotient = rational.Divide(new RationalPolynomial(new Polynomial(new[] { 1.0, 1.0 }), Polynomial.One));

            AssertCoefficients(new[] { 1.0, 1.0 }, quotient.Numerator);
            AssertCoefficients(new[] { 1.0, 1.0 }, quotient.Denominator);
        }

        [Fact]
        public void When_NormalizedDenominatorBecomesZero_Expect_Rejection()
        {
            var rational = new RationalPolynomial(Polynomial.One, new Polynomial(new[] { 1e-30 }));

            Assert.Throws<LaplaceExpressionException>(() => rational.Normalize(1e-18, 1e-12));
        }

        [Fact]
        public void When_FromConstant_Expect_ConstantOverOne()
        {
            var rational = RationalPolynomial.FromConstant(2.5);

            AssertCoefficients(new[] { 2.5 }, rational.Numerator);
            AssertCoefficients(new[] { 1.0 }, rational.Denominator);
        }

        private static void AssertCoefficients(double[] expected, Polynomial actual)
        {
            Assert.Equal(expected.Length, actual.Coefficients.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual.Coefficients[i], 12);
            }
        }

        private static void AssertComplex(Complex expected, Complex actual)
        {
            Assert.Equal(expected.Real, actual.Real, 12);
            Assert.Equal(expected.Imaginary, actual.Imaginary, 12);
        }
    }
}
