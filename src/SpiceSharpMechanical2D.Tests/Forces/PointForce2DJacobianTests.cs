using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharpMechanical2D.Tests.Numerics;
using System;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Forces
{
    public class PointForce2DJacobianTests
    {
        [Fact]
        public void OffCenterWorldForceProducesExpectedTorque()
        {
            PointForce2DContribution contribution = PointForce2DEquation.Evaluate(
                0.0,
                new Vector2D(2.0, 0.0),
                new Vector2D(0.0, 3.0),
                ForceCoordinateSystem2D.World);
            double error = Math.Abs(contribution.Torque - 6.0);

            Console.WriteLine(FormattableString.Invariant(
                $"Point-force direct torque absolute error={error:R}."));
            Assert.InRange(error, 0.0, 1e-11);
            Assert.Equal(new Vector2D(0.0, 3.0), contribution.WorldForce);
        }

        [Fact]
        public void BodyLocalForceAndPointRotateTogether()
        {
            PointForce2DContribution contribution = PointForce2DEquation.Evaluate(
                Math.PI / 2.0,
                new Vector2D(1.0, 0.0),
                new Vector2D(0.0, 3.0),
                ForceCoordinateSystem2D.BodyLocal);

            NumericAssert.Equal(-3.0, contribution.WorldForce.X, 1e-12, 1e-12);
            NumericAssert.Equal(0.0, contribution.WorldForce.Y, 1e-12, 1e-12);
            NumericAssert.Equal(3.0, contribution.Torque, 1e-12, 1e-12);
            NumericAssert.Equal(
                0.0,
                contribution.TorqueDerivativeByAngle,
                1e-12,
                1e-12);
        }

        [Theory]
        [InlineData(ForceCoordinateSystem2D.World)]
        [InlineData(ForceCoordinateSystem2D.BodyLocal)]
        public void AnalyticAngleJacobianMatchesIndependentFiniteDifference(
            ForceCoordinateSystem2D coordinates)
        {
            const double angle = 0.63;
            var localPoint = new Vector2D(0.7, -1.1);
            var force = new Vector2D(2.3, -0.8);
            PointForce2DContribution contribution = PointForce2DEquation.Evaluate(
                angle,
                localPoint,
                force,
                coordinates);
            var analytic = new double[3, 1]
            {
                { contribution.WorldForceDerivativeByAngle.X },
                { contribution.WorldForceDerivativeByAngle.Y },
                { contribution.TorqueDerivativeByAngle },
            };
            double[,] numerical = FiniteDifferenceJacobian.Calculate(
                state =>
                {
                    PointForce2DContribution value = PointForce2DEquation.Evaluate(
                        state[0],
                        localPoint,
                        force,
                        coordinates);
                    return new[] { value.WorldForce.X, value.WorldForce.Y, value.Torque };
                },
                new[] { angle },
                relativeStep: 1e-6,
                minimumStep: 1e-7);
            NumericComparison comparison = NumericAssert.JacobianEqual(
                analytic,
                numerical,
                2e-9,
                2e-6,
                $"PointForce2D {coordinates}");
            double maximumScaleAwareRelativeMismatch = MaximumScaleAwareRelativeMismatch(
                analytic,
                numerical);

            Console.WriteLine(FormattableString.Invariant(
                $"PointForce2D {coordinates} Jacobian maximum absolute/scale-aware relative mismatch={comparison.MaximumAbsoluteMismatch:R}/{maximumScaleAwareRelativeMismatch:R}."));
            Assert.InRange(maximumScaleAwareRelativeMismatch, 0.0, 2e-6);
        }

        private static double MaximumScaleAwareRelativeMismatch(
            double[,] expected,
            double[,] actual)
        {
            double maximum = 0.0;
            for (int row = 0; row < expected.GetLength(0); row++)
            {
                for (int column = 0; column < expected.GetLength(1); column++)
                {
                    double scale = Math.Max(
                        1.0,
                        Math.Max(Math.Abs(expected[row, column]), Math.Abs(actual[row, column])));
                    maximum = Math.Max(
                        maximum,
                        Math.Abs(expected[row, column] - actual[row, column]) / scale);
                }
            }

            return maximum;
        }
    }
}
