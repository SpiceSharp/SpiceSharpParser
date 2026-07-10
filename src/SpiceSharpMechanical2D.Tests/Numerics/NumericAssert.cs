using System;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Numerics
{
    internal static class NumericAssert
    {
        public static void Equal(
            double expected,
            double actual,
            double absoluteTolerance,
            double relativeTolerance,
            string context = null)
        {
            ValidateTolerance(absoluteTolerance, nameof(absoluteTolerance));
            ValidateTolerance(relativeTolerance, nameof(relativeTolerance));

            double difference = Math.Abs(expected - actual);
            double scale = Math.Max(Math.Abs(expected), Math.Abs(actual));
            double tolerance = absoluteTolerance + (relativeTolerance * scale);
            Assert.True(
                IsFinite(expected) && IsFinite(actual) && difference <= tolerance,
                $"{context ?? "Numeric comparison"}: expected {expected:R}, actual {actual:R}, " +
                $"difference {difference:R}, tolerance {tolerance:R}.");
        }

        public static NumericComparison JacobianEqual(
            double[,] expected,
            double[,] actual,
            double absoluteTolerance,
            double relativeTolerance,
            string context = null)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            Assert.Equal(expected.GetLength(0), actual.GetLength(0));
            Assert.Equal(expected.GetLength(1), actual.GetLength(1));

            double maximumAbsoluteMismatch = 0.0;
            double maximumRelativeMismatch = 0.0;
            for (int row = 0; row < expected.GetLength(0); row++)
            {
                for (int column = 0; column < expected.GetLength(1); column++)
                {
                    double expectedValue = expected[row, column];
                    double actualValue = actual[row, column];
                    double absoluteMismatch = Math.Abs(expectedValue - actualValue);
                    double scale = Math.Max(Math.Abs(expectedValue), Math.Abs(actualValue));
                    double relativeMismatch = scale > 0.0 ? absoluteMismatch / scale : 0.0;

                    maximumAbsoluteMismatch = Math.Max(maximumAbsoluteMismatch, absoluteMismatch);
                    maximumRelativeMismatch = Math.Max(maximumRelativeMismatch, relativeMismatch);
                    Equal(
                        expectedValue,
                        actualValue,
                        absoluteTolerance,
                        relativeTolerance,
                        $"{context ?? "Jacobian"}[{row}, {column}]");
                }
            }

            return new NumericComparison(maximumAbsoluteMismatch, maximumRelativeMismatch);
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private static void ValidateTolerance(double value, string parameterName)
        {
            if (value < 0.0 || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal readonly struct NumericComparison
    {
        public NumericComparison(double maximumAbsoluteMismatch, double maximumRelativeMismatch)
        {
            MaximumAbsoluteMismatch = maximumAbsoluteMismatch;
            MaximumRelativeMismatch = maximumRelativeMismatch;
        }

        public double MaximumAbsoluteMismatch { get; }

        public double MaximumRelativeMismatch { get; }
    }
}
