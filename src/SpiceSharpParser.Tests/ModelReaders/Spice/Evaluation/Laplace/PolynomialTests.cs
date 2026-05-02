using System;
using System.Numerics;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation.Laplace
{
    public class PolynomialTests
    {
        [Fact]
        public void When_ConstructedFromArray_Expect_CoefficientsAreCopied()
        {
            var source = new[] { 1.0, 2.0, 3.0 };

            var polynomial = new Polynomial(source);
            source[0] = 99.0;

            AssertCoefficients(new[] { 1.0, 2.0, 3.0 }, polynomial);
        }

        [Fact]
        public void When_ToArrayIsMutated_Expect_PolynomialIsUnchanged()
        {
            var polynomial = new Polynomial(new[] { 1.0, 2.0 });

            var copy = polynomial.ToArray();
            copy[0] = 42.0;

            AssertCoefficients(new[] { 1.0, 2.0 }, polynomial);
        }

        [Fact]
        public void When_NoCoefficients_Expect_Rejection()
        {
            Assert.Throws<ArgumentException>(() => new Polynomial(new double[0]));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void When_CoefficientIsNotFinite_Expect_Rejection(double coefficient)
        {
            Assert.Throws<LaplaceExpressionException>(() => new Polynomial(new[] { 1.0, coefficient }));
        }

        [Fact]
        public void When_InteriorZeroExists_Expect_DegreePreservesPower()
        {
            var polynomial = new Polynomial(new[] { 1.0, 0.0, 2.0 });

            Assert.Equal(2, polynomial.Degree);
            AssertCoefficients(new[] { 1.0, 0.0, 2.0 }, polynomial);
        }

        [Fact]
        public void When_Normalized_Expect_HighOrderZerosTrimmed()
        {
            var polynomial = new Polynomial(new[] { 1.0, 2.0, 0.0, 0.0 });

            var normalized = polynomial.Normalize(1e-18, 1e-12);

            Assert.Equal(1, normalized.Degree);
            AssertCoefficients(new[] { 1.0, 2.0 }, normalized);
        }

        [Fact]
        public void When_Normalized_Expect_InteriorZerosPreserved()
        {
            var polynomial = new Polynomial(new[] { 1.0, 0.0, 2.0, 0.0 });

            var normalized = polynomial.Normalize(1e-18, 1e-12);

            AssertCoefficients(new[] { 1.0, 0.0, 2.0 }, normalized);
        }

        [Fact]
        public void When_AllCoefficientsAreZero_Expect_ZeroPolynomialAfterNormalize()
        {
            var polynomial = new Polynomial(new[] { 0.0, 0.0, 0.0 });

            var normalized = polynomial.Normalize(1e-18, 1e-12);

            Assert.True(normalized.IsZero);
            Assert.Equal(0, normalized.Degree);
            AssertCoefficients(new[] { 0.0 }, normalized);
        }

        [Fact]
        public void When_Normalized_Expect_RelativeToleranceTrimsTinyHighOrderTerms()
        {
            var polynomial = new Polynomial(new[] { 1e6, 2.0, 1e-8 });

            var normalized = polynomial.Normalize(1e-18, 1e-12);

            AssertCoefficients(new[] { 1e6, 2.0 }, normalized);
        }

        [Fact]
        public void When_AddingDifferentLengths_Expect_CoefficientsAligned()
        {
            var left = new Polynomial(new[] { 1.0, 2.0 });
            var right = new Polynomial(new[] { 3.0, 4.0, 5.0 });

            var sum = left.Add(right);

            AssertCoefficients(new[] { 4.0, 6.0, 5.0 }, sum);
        }

        [Fact]
        public void When_SubtractingDifferentLengths_Expect_CoefficientsAligned()
        {
            var left = new Polynomial(new[] { 1.0, 2.0 });
            var right = new Polynomial(new[] { 3.0, 4.0, 5.0 });

            var difference = left.Subtract(right);

            AssertCoefficients(new[] { -2.0, -2.0, -5.0 }, difference);
        }

        [Fact]
        public void When_Multiplying_Expect_Convolution()
        {
            var left = new Polynomial(new[] { 1.0, 2.0 });
            var right = new Polynomial(new[] { 3.0, 4.0, 5.0 });

            var product = left.Multiply(right);

            AssertCoefficients(new[] { 3.0, 10.0, 13.0, 10.0 }, product);
        }

        [Fact]
        public void When_MultiplyingByZero_Expect_ZeroPolynomial()
        {
            var product = Polynomial.Zero.Multiply(new Polynomial(new[] { 1.0, 2.0 }));

            Assert.True(product.IsZero);
            AssertCoefficients(new[] { 0.0 }, product);
        }

        [Fact]
        public void When_Scaling_Expect_AllCoefficientsScaled()
        {
            var polynomial = new Polynomial(new[] { 1.0, -2.0, 3.0 });

            var scaled = polynomial.Scale(-2.0);

            AssertCoefficients(new[] { -2.0, 4.0, -6.0 }, scaled);
        }

        [Fact]
        public void When_ScalingByZero_Expect_ZeroPolynomial()
        {
            var scaled = new Polynomial(new[] { 1.0, 2.0, 3.0 }).Scale(0.0);

            Assert.True(scaled.IsZero);
            AssertCoefficients(new[] { 0.0 }, scaled);
        }

        [Fact]
        public void When_ScalingByNonFiniteValue_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => Polynomial.One.Scale(double.NaN));
        }

        [Fact]
        public void When_RaisedToZero_Expect_One()
        {
            var power = new Polynomial(new[] { 2.0, 3.0 }).Pow(0);

            AssertCoefficients(new[] { 1.0 }, power);
        }

        [Fact]
        public void When_RaisedToOne_Expect_SamePolynomial()
        {
            var power = new Polynomial(new[] { 2.0, 3.0 }).Pow(1);

            AssertCoefficients(new[] { 2.0, 3.0 }, power);
        }

        [Fact]
        public void When_RaisedToThree_Expect_MultipliedPolynomial()
        {
            var power = new Polynomial(new[] { 1.0, 1.0 }).Pow(3);

            AssertCoefficients(new[] { 1.0, 3.0, 3.0, 1.0 }, power);
        }

        [Fact]
        public void When_RaisedToNegativePower_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => Polynomial.S.Pow(-1));
        }

        [Fact]
        public void When_EvaluatingReal_Expect_HornerResult()
        {
            var polynomial = new Polynomial(new[] { 1.0, 2.0, 3.0 });

            var value = polynomial.EvaluateReal(2.0);

            Assert.Equal(17.0, value);
        }

        [Fact]
        public void When_EvaluatingRealAtNonFiniteValue_Expect_Rejection()
        {
            Assert.Throws<LaplaceExpressionException>(() => Polynomial.One.EvaluateReal(double.PositiveInfinity));
        }

        [Fact]
        public void When_EvaluatingComplex_Expect_HornerResult()
        {
            var polynomial = new Polynomial(new[] { 1.0, 2.0, 3.0 });

            var value = polynomial.EvaluateComplex(new Complex(0.0, 1.0));

            AssertComplex(new Complex(-2.0, 2.0), value);
        }

        [Fact]
        public void When_Adding_Expect_InputsAreNotMutated()
        {
            var left = new Polynomial(new[] { 1.0, 2.0 });
            var right = new Polynomial(new[] { 3.0, 4.0 });

            left.Add(right);

            AssertCoefficients(new[] { 1.0, 2.0 }, left);
            AssertCoefficients(new[] { 3.0, 4.0 }, right);
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
