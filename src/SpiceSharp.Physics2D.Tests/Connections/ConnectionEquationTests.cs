using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using System;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Connections
{
    public class ConnectionEquationTests
    {
        private static readonly MechanicalAnchor2D BodyAnchorA =
            MechanicalAnchor2D.Body("a", new Vector2D(0.4, -0.3));
        private static readonly MechanicalAnchor2D BodyAnchorB =
            MechanicalAnchor2D.Body("b", new Vector2D(-0.2, 0.5));

        [Fact]
        public void DistanceConnectionAppliesEqualAndOppositeForces()
        {
            double[] loads = EvaluateDistance(CreateState());
            double errorX = Math.Abs(loads[0] + loads[3]);
            double errorY = Math.Abs(loads[1] + loads[4]);

            Console.WriteLine(FormattableString.Invariant(
                $"Distance connection action/reaction maximum absolute error={Math.Max(errorX, errorY):R} N."));
            Assert.InRange(errorX, 0.0, 1e-11);
            Assert.InRange(errorY, 0.0, 1e-11);
        }

        [Fact]
        public void DistanceConnectionHasZeroNetInternalLinearForce()
        {
            double[] loads = EvaluateDistance(CreateState());
            double residual = Math.Sqrt(
                ((loads[0] + loads[3]) * (loads[0] + loads[3])) +
                ((loads[1] + loads[4]) * (loads[1] + loads[4])));

            Assert.InRange(residual, 0.0, 1e-11);
        }

        [Fact]
        public void DistanceConnectionHasZeroNetWorldOriginTorque()
        {
            double[] state = CreateState();
            double[] loads = EvaluateDistance(state);
            var centerA = new Vector2D(state[0], state[1]);
            var centerB = new Vector2D(state[6], state[7]);
            double residual =
                Vector2D.Cross(centerA, new Vector2D(loads[0], loads[1])) +
                loads[2] +
                Vector2D.Cross(centerB, new Vector2D(loads[3], loads[4])) +
                loads[5];

            Console.WriteLine(FormattableString.Invariant(
                $"Distance connection world-origin internal torque residual={residual:R} N*m."));
            Assert.InRange(Math.Abs(residual), 0.0, 1e-10);
        }

        [Fact]
        public void DistanceConnectionCanAttachOneBodyToWorld()
        {
            var body = new ConnectionBodyState2D(
                new Vector2D(2.0, 0.0),
                0.0,
                Vector2D.Zero,
                0.0);
            var loads = new double[6];
            var jacobian = new double[6, 12];
            DistanceSpringDamper2DEquation.Evaluate(
                MechanicalAnchor2D.World(Vector2D.Zero),
                default,
                MechanicalAnchor2D.Body("body", Vector2D.Zero),
                body,
                1.0,
                10.0,
                0.0,
                1e-12,
                loads,
                jacobian);

            NumericAssert.Equal(-10.0, loads[3], 1e-10, 1e-11);
            NumericAssert.Equal(0.0, loads[4], 1e-12, 1e-12);
            NumericAssert.Equal(0.0, loads[5], 1e-12, 1e-12);
        }

        [Fact]
        public void OffCenterAnchorsProduceMomentArmsAboutBothCenters()
        {
            double[] state = CreateState();
            double[] loads = EvaluateDistance(state);
            Vector2D radiusA = BodyAnchorA.Point.Rotate(state[2]);
            Vector2D radiusB = BodyAnchorB.Point.Rotate(state[8]);
            double expectedA = Vector2D.Cross(radiusA, new Vector2D(loads[0], loads[1]));
            double expectedB = Vector2D.Cross(radiusB, new Vector2D(loads[3], loads[4]));

            NumericAssert.Equal(expectedA, loads[2], 1e-12, 1e-12);
            NumericAssert.Equal(expectedB, loads[5], 1e-12, 1e-12);
            Assert.True(Math.Abs(loads[2]) > 1e-3);
            Assert.True(Math.Abs(loads[5]) > 1e-3);
        }

        [Fact]
        public void RotationalSpringOpposesRelativeAngleError()
        {
            var loads = new double[2];
            var jacobian = new double[2, 4];
            RotationalSpringDamper2DEvaluation result =
                RotationalSpringDamper2DEquation.Evaluate(
                    0.0,
                    0.0,
                    0.25,
                    0.0,
                    0.0,
                    8.0,
                    0.0,
                    loads,
                    jacobian);

            double expected = 8.0 * Math.Sin(0.25);
            NumericAssert.Equal(expected, result.TorqueOnA, 1e-12, 1e-12);
            NumericAssert.Equal(expected, loads[0], 1e-12, 1e-12);
            NumericAssert.Equal(-expected, loads[1], 1e-12, 1e-12);
        }

        [Fact]
        public void RotationalSpringUsesShortestAngleAcrossWrappedSeam()
        {
            var loads = new double[2];
            var jacobian = new double[2, 4];
            RotationalSpringDamper2DEvaluation result =
                RotationalSpringDamper2DEquation.Evaluate(
                    Math.PI - 0.01,
                    0.0,
                    -Math.PI + 0.02,
                    0.0,
                    0.0,
                    10.0,
                    0.0,
                    loads,
                    jacobian);

            NumericAssert.Equal(0.03, result.AngleError, 1e-12, 1e-12);
            NumericAssert.Equal(10.0 * Math.Sin(0.03), loads[0], 1e-12, 1e-12);
        }

        [Fact]
        public void RotationalTorqueAndJacobianAreSmoothAcrossDiagnosticSeam()
        {
            const double stiffness = 7.0;
            const double offset = 1e-7;
            double[] below = EvaluateRotationAtError(Math.PI - offset, stiffness);
            double[] above = EvaluateRotationAtError(Math.PI + offset, stiffness);
            double expectedMagnitude = stiffness * Math.Sin(offset);

            NumericAssert.Equal(expectedMagnitude, below[0], 1e-13, 1e-9);
            NumericAssert.Equal(-expectedMagnitude, above[0], 1e-13, 1e-9);
            NumericAssert.Equal(0.0, below[0] + above[0], 1e-13, 1e-9);

            double[] state = { 0.0, 0.0, Math.PI, 0.0 };
            var loads = new double[2];
            var analytic = new double[2, 4];
            RotationalSpringDamper2DEquation.Evaluate(
                state[0],
                state[1],
                state[2],
                state[3],
                0.0,
                stiffness,
                0.0,
                loads,
                analytic);
            double[,] numerical = FiniteDifferenceJacobian.Calculate(
                values => EvaluateRotationAtError(values[2] - values[0], stiffness),
                state,
                relativeStep: 1e-6,
                minimumStep: 1e-7);

            NumericComparison comparison = NumericAssert.JacobianEqual(
                analytic,
                numerical,
                2e-9,
                5e-6,
                "RotationalSpringDamper2D diagnostic seam");

            Console.WriteLine(FormattableString.Invariant(
                $"Rotational seam Jacobian maximum absolute mismatch={comparison.MaximumAbsoluteMismatch:R}."));
        }

        [Fact]
        public void DistanceAnalyticJacobianMatchesIndependentFiniteDifference()
        {
            double[] state = CreateState();
            var loads = new double[6];
            var analytic = new double[6, 12];
            EvaluateDistance(state, loads, analytic);
            double[,] numerical = FiniteDifferenceJacobian.Calculate(
                EvaluateDistance,
                state,
                relativeStep: 1e-6,
                minimumStep: 1e-7);
            NumericComparison comparison = NumericAssert.JacobianEqual(
                analytic,
                numerical,
                2e-8,
                5e-6,
                "DistanceSpringDamper2D full state");
            double maximumRelativeMismatch = MaximumScaleAwareRelativeMismatch(analytic, numerical);

            Console.WriteLine(FormattableString.Invariant(
                $"Distance connection Jacobian maximum absolute/scale-aware relative mismatch={comparison.MaximumAbsoluteMismatch:R}/{maximumRelativeMismatch:R}."));
            Assert.InRange(maximumRelativeMismatch, 0.0, 5e-6);
        }

        [Fact]
        public void RotationalAnalyticJacobianMatchesIndependentFiniteDifference()
        {
            double[] state = { 0.4, -0.7, -0.2, 1.1 };
            var loads = new double[2];
            var analytic = new double[2, 4];
            EvaluateRotation(state, loads, analytic);
            double[,] numerical = FiniteDifferenceJacobian.Calculate(
                EvaluateRotation,
                state,
                relativeStep: 1e-6,
                minimumStep: 1e-7);
            NumericAssert.JacobianEqual(
                analytic,
                numerical,
                2e-9,
                5e-6,
                "RotationalSpringDamper2D full state");
            double maximumRelativeMismatch = MaximumScaleAwareRelativeMismatch(analytic, numerical);

            Console.WriteLine(FormattableString.Invariant(
                $"Rotational connection Jacobian maximum scale-aware relative mismatch={maximumRelativeMismatch:R}."));
            Assert.InRange(maximumRelativeMismatch, 0.0, 5e-6);
        }

        [Fact]
        public void CoincidentDistanceAnchorsRemainFinite()
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            DistanceSpringDamper2DEquation.Evaluate(
                MechanicalAnchor2D.Body("a", Vector2D.Zero),
                new ConnectionBodyState2D(Vector2D.Zero, 0.3, new Vector2D(1.0, -2.0), 0.7),
                MechanicalAnchor2D.Body("b", Vector2D.Zero),
                new ConnectionBodyState2D(Vector2D.Zero, -0.6, new Vector2D(-3.0, 4.0), -1.2),
                1.0,
                12.0,
                0.8,
                1e-6,
                loads,
                jacobian);

            foreach (double load in loads)
                Assert.True(IsFinite(load));
            foreach (double derivative in jacobian)
                Assert.True(IsFinite(derivative));
        }

        private static double[] CreateState() => new[]
        {
            -0.8, 0.6, 0.37, 0.9, -0.4, 0.75,
            1.4, -0.2, -0.51, -0.3, 1.1, -0.45,
        };

        private static double[] EvaluateDistance(double[] state)
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            EvaluateDistance(state, loads, jacobian);
            return loads;
        }

        private static void EvaluateDistance(double[] state, double[] loads, double[,] jacobian)
        {
            DistanceSpringDamper2DEquation.Evaluate(
                BodyAnchorA,
                ToBodyState(state, 0),
                BodyAnchorB,
                ToBodyState(state, 6),
                1.2,
                17.0,
                0.65,
                1e-5,
                loads,
                jacobian);
        }

        private static double[] EvaluateRotation(double[] state)
        {
            var loads = new double[2];
            var jacobian = new double[2, 4];
            EvaluateRotation(state, loads, jacobian);
            return loads;
        }

        private static void EvaluateRotation(double[] state, double[] loads, double[,] jacobian)
        {
            RotationalSpringDamper2DEquation.Evaluate(
                state[0],
                state[1],
                state[2],
                state[3],
                0.15,
                7.0,
                0.4,
                loads,
                jacobian);
        }

        private static double[] EvaluateRotationAtError(double error, double stiffness)
        {
            var loads = new double[2];
            var jacobian = new double[2, 4];
            RotationalSpringDamper2DEquation.Evaluate(
                0.0,
                0.0,
                error,
                0.0,
                0.0,
                stiffness,
                0.0,
                loads,
                jacobian);
            return loads;
        }

        private static ConnectionBodyState2D ToBodyState(double[] state, int offset) =>
            new ConnectionBodyState2D(
                new Vector2D(state[offset], state[offset + 1]),
                state[offset + 2],
                new Vector2D(state[offset + 3], state[offset + 4]),
                state[offset + 5]);

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

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
