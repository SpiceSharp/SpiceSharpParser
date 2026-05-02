using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal sealed class Polynomial
    {
        private readonly double[] _coefficients;

        public Polynomial(IEnumerable<double> coefficients)
        {
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }

            _coefficients = coefficients.ToArray();
            if (_coefficients.Length == 0)
            {
                throw new ArgumentException("A polynomial must contain at least one coefficient.", nameof(coefficients));
            }

            ValidateFinite(_coefficients);
        }

        public static Polynomial Zero { get; } = new Polynomial(new[] { 0.0 });

        public static Polynomial One { get; } = new Polynomial(new[] { 1.0 });

        public static Polynomial S { get; } = new Polynomial(new[] { 0.0, 1.0 });

        public IReadOnlyList<double> Coefficients => _coefficients;

        public int Degree => IsZero ? 0 : _coefficients.Length - 1;

        public bool IsZero => _coefficients.Length == 1 && _coefficients[0] == 0.0;

        public double[] ToArray()
        {
            var copy = new double[_coefficients.Length];
            Array.Copy(_coefficients, copy, _coefficients.Length);
            return copy;
        }

        public Polynomial Add(Polynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var length = Math.Max(_coefficients.Length, other._coefficients.Length);
            var coefficients = new double[length];
            for (var i = 0; i < length; i++)
            {
                coefficients[i] = GetCoefficient(i) + other.GetCoefficient(i);
            }

            return new Polynomial(coefficients);
        }

        public Polynomial Subtract(Polynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var length = Math.Max(_coefficients.Length, other._coefficients.Length);
            var coefficients = new double[length];
            for (var i = 0; i < length; i++)
            {
                coefficients[i] = GetCoefficient(i) - other.GetCoefficient(i);
            }

            return new Polynomial(coefficients);
        }

        public Polynomial Multiply(Polynomial other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (IsZero || other.IsZero)
            {
                return Zero;
            }

            var coefficients = new double[_coefficients.Length + other._coefficients.Length - 1];
            for (var leftIndex = 0; leftIndex < _coefficients.Length; leftIndex++)
            {
                for (var rightIndex = 0; rightIndex < other._coefficients.Length; rightIndex++)
                {
                    coefficients[leftIndex + rightIndex] += _coefficients[leftIndex] * other._coefficients[rightIndex];
                }
            }

            return new Polynomial(coefficients);
        }

        public Polynomial Scale(double factor)
        {
            EnsureFinite(factor);
            if (factor == 0.0 || IsZero)
            {
                return Zero;
            }

            var coefficients = new double[_coefficients.Length];
            for (var i = 0; i < _coefficients.Length; i++)
            {
                coefficients[i] = _coefficients[i] * factor;
            }

            return new Polynomial(coefficients);
        }

        public Polynomial Pow(int exponent)
        {
            if (exponent < 0)
            {
                throw new LaplaceExpressionException("laplace transfer powers must be non-negative integers");
            }

            var result = One;
            var factor = this;
            var remaining = exponent;

            while (remaining > 0)
            {
                if ((remaining & 1) == 1)
                {
                    result = result.Multiply(factor);
                }

                remaining >>= 1;
                if (remaining > 0)
                {
                    factor = factor.Multiply(factor);
                }
            }

            return result;
        }

        public double EvaluateReal(double value)
        {
            EnsureFinite(value);
            var result = _coefficients[_coefficients.Length - 1];
            for (var i = _coefficients.Length - 2; i >= 0; i--)
            {
                result = (result * value) + _coefficients[i];
            }

            return result;
        }

        public Complex EvaluateComplex(Complex value)
        {
            var result = new Complex(_coefficients[_coefficients.Length - 1], 0.0);
            for (var i = _coefficients.Length - 2; i >= 0; i--)
            {
                result = (result * value) + _coefficients[i];
            }

            return result;
        }

        public Polynomial Normalize(double zeroTolerance, double relativeTolerance)
        {
            if (zeroTolerance < 0.0 || double.IsNaN(zeroTolerance) || double.IsInfinity(zeroTolerance))
            {
                throw new ArgumentOutOfRangeException(nameof(zeroTolerance));
            }

            if (relativeTolerance < 0.0 || double.IsNaN(relativeTolerance) || double.IsInfinity(relativeTolerance))
            {
                throw new ArgumentOutOfRangeException(nameof(relativeTolerance));
            }

            var maxAbs = 0.0;
            for (var i = 0; i < _coefficients.Length; i++)
            {
                maxAbs = Math.Max(maxAbs, Math.Abs(_coefficients[i]));
            }

            if (maxAbs == 0.0)
            {
                return Zero;
            }

            var tolerance = Math.Max(zeroTolerance, relativeTolerance * maxAbs);
            var lastIndex = _coefficients.Length - 1;
            while (lastIndex > 0 && Math.Abs(_coefficients[lastIndex]) <= tolerance)
            {
                lastIndex--;
            }

            var coefficients = new double[lastIndex + 1];
            for (var i = 0; i <= lastIndex; i++)
            {
                coefficients[i] = Math.Abs(_coefficients[i]) <= tolerance ? 0.0 : _coefficients[i];
            }

            return new Polynomial(coefficients);
        }

        private static void ValidateFinite(IEnumerable<double> coefficients)
        {
            foreach (var coefficient in coefficients)
            {
                EnsureFinite(coefficient);
            }
        }

        private static void EnsureFinite(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new LaplaceExpressionException("laplace transfer coefficients must be finite");
            }
        }

        private double GetCoefficient(int index)
        {
            return index < _coefficients.Length ? _coefficients[index] : 0.0;
        }
    }
}
