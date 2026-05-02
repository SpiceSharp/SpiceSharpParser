using System;
using System.Numerics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal sealed class RationalPolynomial
    {
        public RationalPolynomial(Polynomial numerator, Polynomial denominator)
        {
            Numerator = numerator ?? throw new ArgumentNullException(nameof(numerator));
            Denominator = denominator ?? throw new ArgumentNullException(nameof(denominator));

            if (Denominator.IsZero)
            {
                throw new LaplaceExpressionException("laplace transfer denominator cannot be zero");
            }
        }

        public static RationalPolynomial Zero { get; } = FromConstant(0.0);

        public static RationalPolynomial One { get; } = FromConstant(1.0);

        public static RationalPolynomial SymbolS { get; } = new RationalPolynomial(Polynomial.S, Polynomial.One);

        public Polynomial Numerator { get; }

        public Polynomial Denominator { get; }

        public static RationalPolynomial FromConstant(double value)
        {
            return new RationalPolynomial(new Polynomial(new[] { value }), Polynomial.One);
        }

        public RationalPolynomial Add(RationalPolynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var numerator = Numerator.Multiply(other.Denominator).Add(other.Numerator.Multiply(Denominator));
            var denominator = Denominator.Multiply(other.Denominator);
            return new RationalPolynomial(numerator, denominator);
        }

        public RationalPolynomial Subtract(RationalPolynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var numerator = Numerator.Multiply(other.Denominator).Subtract(other.Numerator.Multiply(Denominator));
            var denominator = Denominator.Multiply(other.Denominator);
            return new RationalPolynomial(numerator, denominator);
        }

        public RationalPolynomial Multiply(RationalPolynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (TryGetConstantValue(other, out var otherValue))
            {
                return Scale(otherValue);
            }

            if (TryGetConstantValue(this, out var value))
            {
                return other.Scale(value);
            }

            return new RationalPolynomial(Numerator.Multiply(other.Numerator), Denominator.Multiply(other.Denominator));
        }

        public RationalPolynomial Divide(RationalPolynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (other.Numerator.IsZero)
            {
                throw new LaplaceExpressionException("laplace transfer denominator cannot be zero");
            }

            if (TryGetConstantValue(other, out var value))
            {
                return Scale(1.0 / value);
            }

            return new RationalPolynomial(Numerator.Multiply(other.Denominator), Denominator.Multiply(other.Numerator));
        }

        public RationalPolynomial Scale(double factor)
        {
            return new RationalPolynomial(Numerator.Scale(factor), Denominator);
        }

        public RationalPolynomial Pow(int exponent)
        {
            if (exponent < 0)
            {
                throw new LaplaceExpressionException("laplace transfer powers must be non-negative integers");
            }

            return new RationalPolynomial(Numerator.Pow(exponent), Denominator.Pow(exponent));
        }

        public RationalPolynomial Normalize(double zeroTolerance, double relativeTolerance)
        {
            return new RationalPolynomial(
                Numerator.Normalize(zeroTolerance, relativeTolerance),
                Denominator.Normalize(zeroTolerance, relativeTolerance));
        }

        public Complex EvaluateComplex(Complex value)
        {
            return Numerator.EvaluateComplex(value) / Denominator.EvaluateComplex(value);
        }

        private static bool TryGetConstantValue(RationalPolynomial rational, out double value)
        {
            if (rational.Numerator.Degree == 0 && rational.Denominator.Degree == 0)
            {
                value = rational.Numerator.Coefficients[0] / rational.Denominator.Coefficients[0];
                return true;
            }

            value = 0.0;
            return false;
        }
    }
}
