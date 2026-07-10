using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using System;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Mathematics
{
    public class SmoothFunctionsTests
    {
        private const double Epsilon = 0.2;

        [Fact]
        public void FunctionsHaveDocumentedValuesAndDerivativesAtZero()
        {
            Assert.Equal(Epsilon / 2.0, SmoothFunctions.PositivePart(0.0, Epsilon));
            Assert.Equal(0.5, SmoothFunctions.PositivePartDerivative(0.0, Epsilon));
            Assert.Equal(-Epsilon / 2.0, SmoothFunctions.NegativePart(0.0, Epsilon));
            Assert.Equal(0.5, SmoothFunctions.NegativePartDerivative(0.0, Epsilon));
            Assert.Equal(Epsilon, SmoothFunctions.Absolute(0.0, Epsilon));
            Assert.Equal(0.0, SmoothFunctions.AbsoluteDerivative(0.0, Epsilon));
            Assert.Equal(0.0, SmoothFunctions.TanhFriction(0.0, Epsilon));
            Assert.Equal(1.0 / Epsilon, SmoothFunctions.TanhFrictionDerivative(0.0, Epsilon));
            Assert.Equal(Epsilon, SmoothFunctions.RegularizedLength(Vector2D.Zero, Epsilon));
            Assert.Equal(Vector2D.Zero, SmoothFunctions.RegularizedLengthGradient(Vector2D.Zero, Epsilon));
        }

        [Fact]
        public void PositiveAndNegativePartsApproachHardLimitsAwayFromZero()
        {
            double magnitude = 1000.0 * Epsilon;
            double smoothingBound = Epsilon * Epsilon / (4.0 * magnitude);

            NumericAssert.Equal(magnitude, SmoothFunctions.PositivePart(magnitude, Epsilon), smoothingBound, 1e-12);
            Assert.InRange(SmoothFunctions.PositivePart(-magnitude, Epsilon), 0.0, smoothingBound * 1.001);
            NumericAssert.Equal(-magnitude, SmoothFunctions.NegativePart(-magnitude, Epsilon), smoothingBound, 1e-12);
            Assert.InRange(SmoothFunctions.NegativePart(magnitude, Epsilon), -smoothingBound * 1.001, 0.0);
            NumericAssert.Equal(magnitude, SmoothFunctions.Absolute(magnitude, Epsilon), smoothingBound * 2.0, 1e-12);
            Assert.True(SmoothFunctions.TanhFriction(magnitude, Epsilon) > 1.0 - 1e-15);
            Assert.True(SmoothFunctions.TanhFriction(-magnitude, Epsilon) < -1.0 + 1e-15);
        }

        [Fact]
        public void StableFormsRemainFiniteAtLargeMagnitude()
        {
            const double magnitude = 1e308;

            Assert.True(double.IsFinite(SmoothFunctions.PositivePart(magnitude, 1.0)));
            Assert.True(double.IsFinite(SmoothFunctions.PositivePart(-magnitude, 1.0)));
            Assert.True(double.IsFinite(SmoothFunctions.NegativePart(-magnitude, 1.0)));
            Assert.True(double.IsFinite(SmoothFunctions.Absolute(magnitude, 1.0)));
            NumericAssert.Equal(
                magnitude,
                SmoothFunctions.PositivePart(magnitude, 1.0),
                0.0,
                1e-15);

            double rootTwo = Math.Sqrt(2.0);
            double expectedPositive = (0.5 * (1.0 + rootTwo)) * magnitude;
            double expectedNegativeInput = (0.5 * (rootTwo - 1.0)) * magnitude;
            NumericAssert.Equal(
                expectedPositive,
                SmoothFunctions.PositivePart(magnitude, magnitude),
                0.0,
                1e-15);
            NumericAssert.Equal(
                expectedNegativeInput,
                SmoothFunctions.PositivePart(-magnitude, magnitude),
                0.0,
                1e-15);
            Assert.True(double.IsFinite(
                SmoothFunctions.RegularizedLength(new Vector2D(3e200, 4e200), 1.0)));
            NumericAssert.Equal(
                5e200,
                SmoothFunctions.RegularizedLength(new Vector2D(3e200, 4e200), 1.0),
                0.0,
                1e-15);
        }

        [Fact]
        public void ScalarAnalyticDerivativesMatchIndependentCentralDifferences()
        {
            double[] points = { -2.0, -0.2, -1e-6, 0.0, 1e-6, 0.2, 2.0 };
            double maximumMismatch = 0.0;

            foreach (double point in points)
            {
                maximumMismatch = Math.Max(
                    maximumMismatch,
                    CheckDerivative(
                        x => SmoothFunctions.PositivePart(x, Epsilon),
                        x => SmoothFunctions.PositivePartDerivative(x, Epsilon),
                        point));
                maximumMismatch = Math.Max(
                    maximumMismatch,
                    CheckDerivative(
                        x => SmoothFunctions.NegativePart(x, Epsilon),
                        x => SmoothFunctions.NegativePartDerivative(x, Epsilon),
                        point));
                maximumMismatch = Math.Max(
                    maximumMismatch,
                    CheckDerivative(
                        x => SmoothFunctions.Absolute(x, Epsilon),
                        x => SmoothFunctions.AbsoluteDerivative(x, Epsilon),
                        point));
                maximumMismatch = Math.Max(
                    maximumMismatch,
                    CheckDerivative(
                        x => SmoothFunctions.TanhFriction(x, Epsilon),
                        x => SmoothFunctions.TanhFrictionDerivative(x, Epsilon),
                        point));
            }

            Console.WriteLine(FormattableString.Invariant(
                $"Maximum scalar smooth-function derivative mismatch={maximumMismatch:R}."));
            Assert.InRange(maximumMismatch, 0.0, 1e-7);
        }

        [Fact]
        public void RegularizedLengthGradientMatchesIndependentJacobian()
        {
            var states = new[]
            {
                new Vector2D(0.0, 0.0),
                new Vector2D(1e-6, -2e-6),
                new Vector2D(0.2, -0.3),
                new Vector2D(3.0, 4.0),
            };
            double maximumAbsoluteMismatch = 0.0;
            double maximumRelativeMismatch = 0.0;

            foreach (Vector2D state in states)
            {
                Vector2D gradient = SmoothFunctions.RegularizedLengthGradient(state, Epsilon);
                var analytic = new double[,] { { gradient.X, gradient.Y } };
                double[,] finiteDifference = FiniteDifferenceJacobian.Calculate(
                    values => new[]
                    {
                        SmoothFunctions.RegularizedLength(
                            new Vector2D(values[0], values[1]),
                            Epsilon),
                    },
                    new[] { state.X, state.Y },
                    relativeStep: 1e-5,
                    minimumStep: 1e-7);
                NumericComparison comparison = NumericAssert.JacobianEqual(
                    analytic,
                    finiteDifference,
                    1e-9,
                    1e-7,
                    "Regularized length gradient");

                maximumAbsoluteMismatch = Math.Max(
                    maximumAbsoluteMismatch,
                    comparison.MaximumAbsoluteMismatch);
                maximumRelativeMismatch = Math.Max(
                    maximumRelativeMismatch,
                    comparison.MaximumRelativeMismatch);
            }

            Console.WriteLine(FormattableString.Invariant(
                $"Regularized-length gradient max absolute mismatch={maximumAbsoluteMismatch:R}, max relative mismatch={maximumRelativeMismatch:R}."));
            Assert.InRange(maximumAbsoluteMismatch, 0.0, 1e-7);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.PositiveInfinity)]
        public void InvalidSmoothingScalesAreRejected(double scale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SmoothFunctions.PositivePart(1.0, scale));
            Assert.Throws<ArgumentOutOfRangeException>(() => SmoothFunctions.TanhFriction(1.0, scale));
            Assert.Throws<ArgumentOutOfRangeException>(() => SmoothFunctions.RegularizedLength(Vector2D.UnitX, scale));
        }

        private static double CheckDerivative(
            Func<double, double> function,
            Func<double, double> derivative,
            double point)
        {
            const double step = 1e-6;
            double expected = derivative(point);
            double actual = FiniteDifferenceJacobian.CentralDerivative(function, point, step);
            NumericAssert.Equal(expected, actual, 1e-9, 1e-7, $"Derivative at {point:R}");
            return Math.Abs(expected - actual);
        }
    }
}
