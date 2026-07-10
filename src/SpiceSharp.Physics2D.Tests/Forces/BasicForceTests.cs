using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Forces
{
    public class BasicForceTests
    {
        [Fact]
        public void FreeFallMatchesAnalyticMotionAndRefines()
        {
            MotionError coarse = FreeFallErrors(0.04);
            MotionError medium = FreeFallErrors(0.02);
            MotionError fine = FreeFallErrors(0.01);

            Console.WriteLine(FormattableString.Invariant(
                $"Free-fall relative errors position/velocity: h=0.04 {coarse.Position:R}/{coarse.Velocity:R}, h=0.02 {medium.Position:R}/{medium.Velocity:R}, h=0.01 {fine.Position:R}/{fine.Velocity:R}."));
            Assert.True(medium.Position < coarse.Position);
            Assert.True(fine.Position < medium.Position);
            Assert.InRange(fine.Position, 0.0, 1e-4);
            Assert.InRange(fine.Velocity, 0.0, 1e-5);
        }

        [Fact]
        public void ProjectileWithoutDragMatchesAnalyticTrajectory()
        {
            const double stopTime = 1.25;
            var initialPosition = new Vector2D(-1.0, 2.0);
            var initialVelocity = new Vector2D(3.5, 6.0);
            var gravity = new Vector2D(0.0, -9.81);
            var body = new RigidBody2D(
                "projectile",
                1.7,
                0.4,
                initialPosition,
                initialLinearVelocity: initialVelocity);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.01,
                stopTime,
                new Gravity2D("gravity", body.Name, gravity))
                .FinalTransient;
            Vector2D expectedPosition = initialPosition
                + (initialVelocity * stopTime)
                + (0.5 * stopTime * stopTime * gravity);
            Vector2D expectedVelocity = initialVelocity + (stopTime * gravity);
            double positionError = MaxRelativeError(expectedPosition, actual.Position);
            double velocityError = MaxRelativeError(expectedVelocity, actual.LinearVelocity);

            Console.WriteLine(FormattableString.Invariant(
                $"Projectile relative errors position/velocity={positionError:R}/{velocityError:R}."));
            Assert.InRange(positionError, 0.0, 1e-4);
            Assert.InRange(velocityError, 0.0, 1e-5);
        }

        [Fact]
        public void LinearDragMatchesExponentialDecayAndRefines()
        {
            DragError coarse = LinearDragErrors(0.02);
            DragError medium = LinearDragErrors(0.01);
            DragError fine = LinearDragErrors(0.005);

            Console.WriteLine(FormattableString.Invariant(
                $"Linear-drag relative errors position/velocity: h=0.02 {coarse.Position:R}/{coarse.Velocity:R}, h=0.01 {medium.Position:R}/{medium.Velocity:R}, h=0.005 {fine.Position:R}/{fine.Velocity:R}."));
            Assert.True(medium.Velocity < coarse.Velocity);
            Assert.True(fine.Velocity < medium.Velocity);
            Assert.InRange(fine.Velocity, 0.0, 2e-4);
            Assert.InRange(fine.Position, 0.0, 2e-4);
        }

        [Fact]
        public void AngularDragMatchesExponentialDecay()
        {
            const double inertia = 0.6;
            const double damping = 0.3;
            const double mediumAngularVelocity = 0.2;
            const double initialAngularVelocity = 2.5;
            const double initialAngle = -0.4;
            const double stopTime = 2.0;
            var body = new RigidBody2D(
                "rotor",
                1.0,
                inertia,
                initialAngle: initialAngle,
                initialAngularVelocity: initialAngularVelocity);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.002,
                stopTime,
                new AngularDrag2D(
                    "drag",
                    body.Name,
                    damping,
                    mediumAngularVelocity))
                .FinalTransient;
            double decay = Math.Exp(-(damping / inertia) * stopTime);
            double expectedAngularVelocity = mediumAngularVelocity
                + ((initialAngularVelocity - mediumAngularVelocity) * decay);
            double expectedAngle = initialAngle
                + (mediumAngularVelocity * stopTime)
                + ((inertia / damping)
                    * (initialAngularVelocity - mediumAngularVelocity)
                    * (1.0 - decay));
            double velocityError = RelativeError(
                expectedAngularVelocity,
                actual.AngularVelocity);
            double angleError = RelativeError(expectedAngle, actual.Angle);

            Console.WriteLine(FormattableString.Invariant(
                $"Angular-drag relative errors angle/omega={angleError:R}/{velocityError:R}."));
            Assert.InRange(velocityError, 0.0, 2e-4);
            Assert.InRange(angleError, 0.0, 2e-4);
            Assert.True(actual.KineticEnergy < 0.5 * inertia
                * initialAngularVelocity * initialAngularVelocity);
        }

        [Fact]
        public void AppliedTorqueMatchesConstantAngularAcceleration()
        {
            const double inertia = 0.8;
            const double torque = 1.2;
            const double stopTime = 1.5;
            const double initialAngle = 0.25;
            const double initialAngularVelocity = -0.2;
            var body = new RigidBody2D(
                "body",
                1.0,
                inertia,
                initialAngle: initialAngle,
                initialAngularVelocity: initialAngularVelocity);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.01,
                stopTime,
                new AppliedTorque2D("torque", body.Name, torque))
                .FinalTransient;
            double acceleration = torque / inertia;
            double expectedOmega = initialAngularVelocity + (acceleration * stopTime);
            double expectedAngle = initialAngle
                + (initialAngularVelocity * stopTime)
                + (0.5 * acceleration * stopTime * stopTime);

            NumericAssert.Equal(expectedOmega, actual.AngularVelocity, 1e-5, 1e-5);
            NumericAssert.Equal(expectedAngle, actual.Angle, 1e-4, 1e-4);
        }

        [Fact]
        public void TimeDependentAppliedForceUsesCurrentIntegrationTime()
        {
            const double mass = 2.0;
            const double stopTime = 1.0;
            var body = new RigidBody2D("body", mass, 1.0);
            var force = new AppliedForce2D(
                "force",
                body.Name,
                time => new Vector2D(2.0 * time, -time));
            BodySample actual = ForceTestSimulation.Run(body, 0.002, stopTime, force)
                .FinalTransient;
            var expectedVelocity = new Vector2D(0.5, -0.25);
            var expectedPosition = new Vector2D(1.0 / 6.0, -1.0 / 12.0);

            Console.WriteLine(FormattableString.Invariant(
                $"Time-force absolute errors position/velocity={MaxAbsoluteError(expectedPosition, actual.Position):R}/{MaxAbsoluteError(expectedVelocity, actual.LinearVelocity):R}."));
            NumericAssert.Equal(expectedVelocity.X, actual.VelocityX, 2e-5, 2e-5);
            NumericAssert.Equal(expectedVelocity.Y, actual.VelocityY, 2e-5, 2e-5);
            NumericAssert.Equal(expectedPosition.X, actual.PositionX, 2e-5, 2e-5);
            NumericAssert.Equal(expectedPosition.Y, actual.PositionY, 2e-5, 2e-5);
        }

        [Fact]
        public void CenterPointForceProducesNoTorque()
        {
            const double initialAngle = 0.7;
            const double initialOmega = -0.4;
            const double stopTime = 1.0;
            var body = new RigidBody2D(
                "body",
                2.0,
                0.6,
                initialAngle: initialAngle,
                initialAngularVelocity: initialOmega);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.01,
                stopTime,
                new PointForce2D(
                    "force",
                    body.Name,
                    Vector2D.Zero,
                    new Vector2D(3.0, -2.0)))
                .FinalTransient;

            NumericAssert.Equal(initialOmega, actual.AngularVelocity, 1e-12, 1e-12);
            NumericAssert.Equal(
                initialAngle + (initialOmega * stopTime),
                actual.Angle,
                1e-11,
                1e-11);
        }

        [Fact]
        public void BodyLocalPointForceRotatesWithBody()
        {
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialAngle: Math.PI / 2.0);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.01,
                1.0,
                new PointForce2D(
                    "force",
                    body.Name,
                    Vector2D.Zero,
                    Vector2D.UnitX,
                    ForceCoordinateSystem2D.BodyLocal))
                .FinalTransient;

            NumericAssert.Equal(0.0, actual.VelocityX, 1e-11, 1e-11);
            NumericAssert.Equal(1.0, actual.VelocityY, 1e-5, 1e-5);
            NumericAssert.Equal(0.0, actual.AngularVelocity, 1e-12, 1e-12);
        }

        [Fact]
        public void OffCenterBodyLocalPointForceProducesExpectedAngularMotion()
        {
            const double inertia = 2.0;
            const double initialAngle = 0.2;
            const double initialOmega = -0.1;
            const double stopTime = 1.0;
            const double torque = 4.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                inertia,
                initialAngle: initialAngle,
                initialAngularVelocity: initialOmega);
            BodySample actual = ForceTestSimulation.Run(
                body,
                0.005,
                stopTime,
                new PointForce2D(
                    "force",
                    body.Name,
                    Vector2D.UnitX,
                    new Vector2D(0.0, torque),
                    ForceCoordinateSystem2D.BodyLocal))
                .FinalTransient;
            double angularAcceleration = torque / inertia;
            double expectedOmega = initialOmega + (angularAcceleration * stopTime);
            double expectedAngle = initialAngle
                + (initialOmega * stopTime)
                + (0.5 * angularAcceleration * stopTime * stopTime);

            NumericAssert.Equal(expectedOmega, actual.AngularVelocity, 1e-5, 1e-5);
            NumericAssert.Equal(expectedAngle, actual.Angle, 1e-4, 1e-4);
        }

        [Fact]
        public void ForcesSuperimposeThroughAdditiveStamps()
        {
            var firstBody = CreateSuperpositionBody();
            var split = ForceTestSimulation.Run(
                firstBody,
                0.01,
                1.2,
                new AppliedForce2D("first-force", firstBody.Name, new Vector2D(3.0, -1.0)),
                new AppliedForce2D("second-force", firstBody.Name, new Vector2D(-2.0, 4.0)))
                .FinalTransient;
            var secondBody = CreateSuperpositionBody();
            var combined = ForceTestSimulation.Run(
                secondBody,
                0.01,
                1.2,
                new AppliedForce2D("combined-force", secondBody.Name, new Vector2D(1.0, 3.0)))
                .FinalTransient;

            AssertBodyStateEqual(combined, split, 1e-12);
        }

        [Fact]
        public void ComponentOrderChangesResultsOnlyAtRoundoffScale()
        {
            var firstBody = CreateOrderedBody();
            BodyRun first = ForceTestSimulation.Run(
                firstBody,
                0.01,
                1.0,
                new Gravity2D("gravity", firstBody.Name, new Vector2D(0.2, -9.81)),
                new AppliedForce2D("force-a", firstBody.Name, new Vector2D(1.1, 2.2)),
                new AppliedForce2D("force-b", firstBody.Name, new Vector2D(-0.7, 0.3)));
            var secondBody = CreateOrderedBody();
            BodyRun second = ForceTestSimulation.Run(
                secondBody,
                0.01,
                1.0,
                new AppliedForce2D("force-b", secondBody.Name, new Vector2D(-0.7, 0.3)),
                new AppliedForce2D("force-a", secondBody.Name, new Vector2D(1.1, 2.2)),
                new Gravity2D("gravity", secondBody.Name, new Vector2D(0.2, -9.81)));
            double maximumDifference = MaximumSeriesDifference(first, second);

            Console.WriteLine(FormattableString.Invariant(
                $"Force-component order maximum state difference={maximumDifference:R}."));
            Assert.InRange(maximumDifference, 0.0, 1e-12);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void InvalidLinearDragIsRejectedDuringSetup(double damping)
        {
            var body = new RigidBody2D("body", 1.0, 1.0);
            AssertSetupFailure(
                body,
                new LinearDrag2D("invalid-drag", body.Name, damping));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void InvalidAngularDragIsRejectedDuringSetup(double damping)
        {
            var body = new RigidBody2D("body", 1.0, 1.0);
            AssertSetupFailure(
                body,
                new AngularDrag2D("invalid-drag", body.Name, damping));
        }

        private static MotionError FreeFallErrors(double maximumTimestep)
        {
            const double stopTime = 1.5;
            var acceleration = new Vector2D(0.0, -9.81);
            var initialPosition = new Vector2D(0.5, 12.0);
            var initialVelocity = new Vector2D(0.2, 0.3);
            var body = new RigidBody2D(
                "body",
                2.0,
                0.7,
                initialPosition,
                initialLinearVelocity: initialVelocity);
            BodySample actual = ForceTestSimulation.Run(
                body,
                maximumTimestep,
                stopTime,
                new Gravity2D("gravity", body.Name, acceleration))
                .FinalTransient;
            Vector2D expectedPosition = initialPosition
                + (initialVelocity * stopTime)
                + (0.5 * stopTime * stopTime * acceleration);
            Vector2D expectedVelocity = initialVelocity + (stopTime * acceleration);
            return new MotionError(
                MaxRelativeError(expectedPosition, actual.Position),
                MaxRelativeError(expectedVelocity, actual.LinearVelocity));
        }

        private static DragError LinearDragErrors(double maximumTimestep)
        {
            const double mass = 2.0;
            const double damping = 0.8;
            const double stopTime = 2.0;
            var mediumVelocity = new Vector2D(0.5, -0.25);
            var initialPosition = new Vector2D(-0.4, 0.8);
            var initialVelocity = new Vector2D(3.0, -2.0);
            var body = new RigidBody2D(
                "body",
                mass,
                0.5,
                initialPosition,
                initialLinearVelocity: initialVelocity);
            BodySample actual = ForceTestSimulation.Run(
                body,
                maximumTimestep,
                stopTime,
                new LinearDrag2D(
                    "drag",
                    body.Name,
                    damping,
                    mediumVelocity))
                .FinalTransient;
            double decay = Math.Exp(-(damping / mass) * stopTime);
            Vector2D relativeInitialVelocity = initialVelocity - mediumVelocity;
            Vector2D expectedVelocity = mediumVelocity + (decay * relativeInitialVelocity);
            Vector2D expectedPosition = initialPosition
                + (mediumVelocity * stopTime)
                + ((mass / damping) * (1.0 - decay) * relativeInitialVelocity);
            return new DragError(
                MaxRelativeError(expectedPosition, actual.Position),
                MaxRelativeError(expectedVelocity, actual.LinearVelocity));
        }

        private static RigidBody2D CreateSuperpositionBody() =>
            new RigidBody2D(
                "body",
                2.0,
                0.5,
                new Vector2D(0.2, -0.3),
                initialLinearVelocity: new Vector2D(-0.1, 0.4));

        private static RigidBody2D CreateOrderedBody() =>
            new RigidBody2D(
                "body",
                1.7,
                0.8,
                new Vector2D(-0.2, 1.1),
                initialLinearVelocity: new Vector2D(0.3, -0.4));

        private static void AssertBodyStateEqual(
            BodySample expected,
            BodySample actual,
            double tolerance)
        {
            NumericAssert.Equal(expected.PositionX, actual.PositionX, tolerance, tolerance);
            NumericAssert.Equal(expected.PositionY, actual.PositionY, tolerance, tolerance);
            NumericAssert.Equal(expected.Angle, actual.Angle, tolerance, tolerance);
            NumericAssert.Equal(expected.VelocityX, actual.VelocityX, tolerance, tolerance);
            NumericAssert.Equal(expected.VelocityY, actual.VelocityY, tolerance, tolerance);
            NumericAssert.Equal(
                expected.AngularVelocity,
                actual.AngularVelocity,
                tolerance,
                tolerance);
        }

        private static double MaximumSeriesDifference(BodyRun first, BodyRun second)
        {
            Assert.Equal(first.Samples.Count, second.Samples.Count);
            double maximum = 0.0;
            for (int index = 0; index < first.Samples.Count; index++)
            {
                BodySample left = first.Samples[index];
                BodySample right = second.Samples[index];
                Assert.Equal(left.ExportType, right.ExportType);
                Assert.Equal(left.Time, right.Time);
                maximum = Math.Max(maximum, MaxAbsoluteError(left.Position, right.Position));
                maximum = Math.Max(
                    maximum,
                    MaxAbsoluteError(left.LinearVelocity, right.LinearVelocity));
                maximum = Math.Max(maximum, Math.Abs(left.Angle - right.Angle));
                maximum = Math.Max(
                    maximum,
                    Math.Abs(left.AngularVelocity - right.AngularVelocity));
            }

            return maximum;
        }

        private static void AssertSetupFailure(
            RigidBody2D body,
            SpiceSharp.Entities.IEntity load)
        {
            var method = new Trapezoidal
            {
                InitialStep = 0.01,
                MaxStep = 0.01,
                StopTime = 0.1,
            };
            var simulation = new Transient("tran", method);
            var circuit = new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                body,
                load);

            SpiceSharpException exception = Assert.Throws<SpiceSharpException>(() =>
                simulation.Run(circuit).ToArray());
            Assert.Contains(load.Name, exception.Message, StringComparison.Ordinal);
        }

        private static double MaxRelativeError(Vector2D expected, Vector2D actual) =>
            Math.Max(
                RelativeError(expected.X, actual.X),
                RelativeError(expected.Y, actual.Y));

        private static double MaxAbsoluteError(Vector2D expected, Vector2D actual) =>
            Math.Max(Math.Abs(expected.X - actual.X), Math.Abs(expected.Y - actual.Y));

        private static double RelativeError(double expected, double actual) =>
            Math.Abs(actual - expected) / Math.Max(Math.Abs(expected), 1e-30);

        private readonly struct MotionError
        {
            public MotionError(double position, double velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public double Position { get; }

            public double Velocity { get; }
        }

        private readonly struct DragError
        {
            public DragError(double position, double velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public double Position { get; }

            public double Velocity { get; }
        }
    }
}
