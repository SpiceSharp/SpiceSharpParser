using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Joints;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharpMechanical2D.Tests.Connections;
using SpiceSharpMechanical2D.Tests.Numerics;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Joints
{
    public class JointTransientTests
    {
        [Fact]
        public void CompliantRevolutePendulumMatchesSmallAnglePeriodAndBoundsAnchorError()
        {
            const double length = 0.6;
            const double mass = 1.2;
            const double inertia = 0.12;
            const double gravity = 9.81;
            const double initialAngle = 0.08;
            Vector2D localPivot = new Vector2D(0.0, length);
            Vector2D initialPosition = -localPivot.Rotate(initialAngle);
            var body = new RigidBody2D(
                "pendulum",
                mass,
                inertia,
                initialPosition,
                initialAngle);
            var joint = new RevoluteJoint2D(
                "pivot",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, localPivot),
                2.0e5,
                120.0);
            var weight = new Gravity2D(
                "gravity",
                body.Name,
                new Vector2D(0.0, -gravity));
            ConnectionRun run = ConnectionTestSimulation.Run(
                body,
                null,
                0.0005,
                3.2,
                joint,
                weight);
            double period = EstimatePeriod(run.Samples, sample => sample.AngleA);
            double expected = 2.0 * Math.PI * Math.Sqrt(
                (inertia + (mass * length * length)) / (mass * gravity * length));
            double relativePeriodError = Math.Abs(period - expected) / expected;
            double maximumAnchorError = 0.0;
            foreach (ConnectionSample sample in run.Samples)
            {
                Vector2D anchor = new Vector2D(sample.XA, sample.YA)
                    + localPivot.Rotate(sample.AngleA);
                maximumAnchorError = Math.Max(maximumAnchorError, anchor.Length);
            }

            Console.WriteLine(FormattableString.Invariant(
                $"Revolute pendulum period expected/actual/relative error={expected:R}/{period:R}/{relativePeriodError:R}; maximum anchor error={maximumAnchorError:R} m."));
            Assert.InRange(relativePeriodError, 0.0, 1.5e-2);
            Assert.InRange(maximumAnchorError / length, 0.0, 1e-3);
        }

        [Fact]
        public void PrismaticJointAllowsFreeAxisTranslation()
        {
            const double stopTime = 0.8;
            var body = new RigidBody2D(
                "slider",
                1.0,
                0.2,
                initialPosition: new Vector2D(-0.3, 0.0),
                initialLinearVelocity: new Vector2D(1.25, 0.0));
            var joint = CreateWorldPrismatic(body, "guide-free");
            ConnectionSample final = ConnectionTestSimulation.Run(
                body,
                null,
                0.002,
                stopTime,
                joint).Final;

            NumericAssert.Equal(-0.3 + (1.25 * stopTime), final.XA, 2e-10, 2e-10);
            NumericAssert.Equal(1.25, final.VelocityXA, 2e-11, 2e-11);
            Assert.InRange(Math.Abs(final.YA), 0.0, 1e-12);
            Assert.InRange(Math.Abs(final.AngleA), 0.0, 1e-12);
        }

        [Fact]
        public void PrismaticJointSuppressesNormalAndAngularMotion()
        {
            var body = new RigidBody2D(
                "slider",
                1.0,
                0.2,
                initialPosition: new Vector2D(0.0, 0.04),
                initialAngle: 0.03,
                initialLinearVelocity: new Vector2D(0.7, 0.0));
            var joint = CreateWorldPrismatic(body, "guide-restoring");
            ConnectionSample final = ConnectionTestSimulation.Run(
                body,
                null,
                0.001,
                0.8,
                joint).Final;

            Assert.InRange(Math.Abs(final.YA), 0.0, 2e-5);
            Assert.InRange(Math.Abs(final.AngleA), 0.0, 2e-5);
            NumericAssert.Equal(0.56, final.XA, 5e-8, 5e-8);
        }

        [Fact]
        public void WeldJointUnderSlowConstantLoadingHasBoundedErrors()
        {
            const double positionStiffness = 5000.0;
            const double angularStiffness = 1200.0;
            var body = new RigidBody2D("fixture", 1.0, 0.3);
            var weld = new WeldJoint2D(
                "weld",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                0.0,
                positionStiffness,
                140.0,
                angularStiffness,
                35.0);
            var force = new AppliedForce2D(
                "load",
                body.Name,
                new Vector2D(2.0, -1.0));
            var torque = new AppliedTorque2D("moment", body.Name, 0.3);
            ConnectionSample final = ConnectionTestSimulation.Run(
                body,
                null,
                0.001,
                1.0,
                weld,
                force,
                torque).Final;
            var expected = new Vector2D(2.0 / positionStiffness, -1.0 / positionStiffness);

            NumericAssert.Equal(expected.X, final.XA, 4e-6, 4e-4);
            NumericAssert.Equal(expected.Y, final.YA, 4e-6, 4e-4);
            NumericAssert.Equal(0.3 / angularStiffness, final.AngleA, 4e-6, 4e-4);
            Assert.InRange(new Vector2D(final.XA, final.YA).Length, 0.0, 1e-3);
        }

        [Fact]
        public void RevoluteJointTransientConvergesUnderTimestepRefinement()
        {
            double coarse = RevoluteOscillatorError(0.02);
            double medium = RevoluteOscillatorError(0.01);
            double fine = RevoluteOscillatorError(0.005);

            Console.WriteLine(FormattableString.Invariant(
                $"Revolute timestep-refinement errors h=0.02/0.01/0.005: {coarse:R}/{medium:R}/{fine:R}."));
            Assert.True(medium < coarse);
            Assert.True(fine < medium);
            Assert.InRange(fine, 0.0, 2e-4);
        }

        private static PrismaticJoint2D CreateWorldPrismatic(RigidBody2D body, string name) =>
            new PrismaticJoint2D(
                name,
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                new Vector2D(1.0, 0.0),
                0.0,
                800.0,
                45.0,
                300.0,
                18.0);

        private static double RevoluteOscillatorError(double maximumTimestep)
        {
            const double stiffness = 9.0;
            const double stopTime = 1.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialPosition: new Vector2D(0.4, 0.0));
            var joint = new RevoluteJoint2D(
                "joint",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                stiffness);
            double actual = ConnectionTestSimulation.Run(
                body,
                null,
                maximumTimestep,
                stopTime,
                joint).Final.XA;
            double expected = 0.4 * Math.Cos(Math.Sqrt(stiffness) * stopTime);
            return Math.Abs(actual - expected);
        }

        private static double EstimatePeriod(
            IReadOnlyList<ConnectionSample> samples,
            Func<ConnectionSample, double> displacement)
        {
            var crossings = new List<double>();
            for (int index = 1; index < samples.Count; index++)
            {
                double previous = displacement(samples[index - 1]);
                double current = displacement(samples[index]);
                if (previous > 0.0 && current <= 0.0)
                {
                    double fraction = previous / (previous - current);
                    crossings.Add(
                        samples[index - 1].Time
                        + (fraction * (samples[index].Time - samples[index - 1].Time)));
                }
            }

            Assert.True(crossings.Count >= 2, "At least two pendulum crossings are required.");
            return crossings[1] - crossings[0];
        }
    }
}
