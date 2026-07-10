using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Bodies
{
    public class RigidBody2DTests
    {
        [Fact]
        public void ConstantWorldForceThroughCenterMatchesAnalyticSolution()
        {
            BodyError coarse = ConstantForceErrors(0.04);
            BodyError medium = ConstantForceErrors(0.02);
            BodyError fine = ConstantForceErrors(0.01);

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body constant-force relative errors position/velocity: h=0.04 {coarse.Position:R}/{coarse.Velocity:R}, h=0.02 {medium.Position:R}/{medium.Velocity:R}, h=0.01 {fine.Position:R}/{fine.Velocity:R}."));
            Assert.InRange(fine.Velocity, 0.0, 1e-5);
            Assert.InRange(fine.Position, 0.0, 1e-4);
        }

        [Fact]
        public void ConstantTorqueMatchesAnalyticAngularAcceleration()
        {
            BodyError coarse = ConstantTorqueErrors(0.02);
            BodyError medium = ConstantTorqueErrors(0.01);
            BodyError fine = ConstantTorqueErrors(0.005);

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body constant-torque relative errors angle/omega: h=0.02 {coarse.Angle:R}/{coarse.AngularVelocity:R}, h=0.01 {medium.Angle:R}/{medium.AngularVelocity:R}, h=0.005 {fine.Angle:R}/{fine.AngularVelocity:R}."));
            Assert.InRange(fine.AngularVelocity, 0.0, 1e-5);
            Assert.InRange(fine.Angle, 0.0, 1e-4);
        }

        [Fact]
        public void CombinedForceAndTorqueMatchIndependentAnalyticMotion()
        {
            const double mass = 1.5;
            const double inertia = 0.6;
            const double stopTime = 1.2;
            var initialPosition = new Vector2D(-0.4, 0.8);
            var initialVelocity = new Vector2D(0.5, -0.2);
            const double initialAngle = -0.7;
            const double initialAngularVelocity = 0.3;
            var force = new Vector2D(-2.0, 4.0);
            const double torque = -0.9;
            var body = new RigidBody2D(
                "body",
                mass,
                inertia,
                initialPosition,
                initialAngle,
                initialVelocity,
                initialAngularVelocity);
            BodySample actual = RunBody(
                body,
                0.01,
                stopTime,
                new TestRigidBodyLoad2D("load", body.Name, force, torque))
                .FinalTransient;

            Vector2D expectedVelocity = initialVelocity + ((force / mass) * stopTime);
            Vector2D expectedPosition = initialPosition
                + (initialVelocity * stopTime)
                + ((0.5 * stopTime * stopTime / mass) * force);
            double angularAcceleration = torque / inertia;
            double expectedAngularVelocity = initialAngularVelocity
                + (angularAcceleration * stopTime);
            double expectedAngle = initialAngle
                + (initialAngularVelocity * stopTime)
                + (0.5 * angularAcceleration * stopTime * stopTime);
            double positionRelativeError = MaxRelativeError(
                expectedPosition,
                new Vector2D(actual.PositionX, actual.PositionY));
            double velocityRelativeError = MaxRelativeError(
                expectedVelocity,
                new Vector2D(actual.VelocityX, actual.VelocityY));
            double angleRelativeError = RelativeError(expectedAngle, actual.Angle);
            double angularVelocityRelativeError = RelativeError(
                expectedAngularVelocity,
                actual.AngularVelocity);

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body combined-load relative errors position/velocity/angle/omega={positionRelativeError:R}/{velocityRelativeError:R}/{angleRelativeError:R}/{angularVelocityRelativeError:R}."));

            NumericAssert.Equal(expectedPosition.X, actual.PositionX, 1e-4, 1e-4);
            NumericAssert.Equal(expectedPosition.Y, actual.PositionY, 1e-4, 1e-4);
            NumericAssert.Equal(expectedVelocity.X, actual.VelocityX, 1e-5, 1e-5);
            NumericAssert.Equal(expectedVelocity.Y, actual.VelocityY, 1e-5, 1e-5);
            NumericAssert.Equal(expectedAngle, actual.Angle, 1e-4, 1e-4);
            NumericAssert.Equal(
                expectedAngularVelocity,
                actual.AngularVelocity,
                1e-5,
                1e-5);
        }

        [Fact]
        public void NoForceProducesInertialTranslationAndRotation()
        {
            const double stopTime = 2.0;
            var initialPosition = new Vector2D(0.25, -0.5);
            var initialVelocity = new Vector2D(-0.4, 0.75);
            const double initialAngle = 0.6;
            const double initialAngularVelocity = -1.25;
            var body = new RigidBody2D(
                "body",
                2.0,
                0.7,
                initialPosition,
                initialAngle,
                initialVelocity,
                initialAngularVelocity);
            BodySample actual = RunBody(body, 0.04, stopTime).FinalTransient;
            double maximumAbsoluteError = new[]
            {
                Math.Abs(actual.PositionX - (initialPosition.X + (initialVelocity.X * stopTime))),
                Math.Abs(actual.PositionY - (initialPosition.Y + (initialVelocity.Y * stopTime))),
                Math.Abs(actual.VelocityX - initialVelocity.X),
                Math.Abs(actual.VelocityY - initialVelocity.Y),
                Math.Abs(actual.Angle - (initialAngle + (initialAngularVelocity * stopTime))),
                Math.Abs(actual.AngularVelocity - initialAngularVelocity),
            }.Max();

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body inertial-motion maximum absolute error={maximumAbsoluteError:R}."));

            NumericAssert.Equal(
                initialPosition.X + (initialVelocity.X * stopTime),
                actual.PositionX,
                1e-11,
                1e-11);
            NumericAssert.Equal(
                initialPosition.Y + (initialVelocity.Y * stopTime),
                actual.PositionY,
                1e-11,
                1e-11);
            NumericAssert.Equal(initialVelocity.X, actual.VelocityX, 1e-12, 1e-12);
            NumericAssert.Equal(initialVelocity.Y, actual.VelocityY, 1e-12, 1e-12);
            NumericAssert.Equal(
                initialAngle + (initialAngularVelocity * stopTime),
                actual.Angle,
                1e-11,
                1e-11);
            NumericAssert.Equal(
                initialAngularVelocity,
                actual.AngularVelocity,
                1e-12,
                1e-12);
        }

        [Fact]
        public void LocalAndWorldTransformsRoundTrip()
        {
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                new Vector2D(2.0, -1.0),
                initialAngle: 0.73);
            BodyRun result = RunBody(body, 0.01, 0.05);
            IRigidBody2DBehavior behavior = GetBehavior(result, body);
            var localPoint = new Vector2D(-0.35, 1.2);
            var localVector = new Vector2D(0.8, -0.6);
            var worldPoint = new Vector2D(-1.1, 3.4);
            var worldVector = new Vector2D(-2.3, 0.9);

            Vector2D localPointRoundTrip = behavior.WorldPointToLocal(
                behavior.LocalPointToWorld(localPoint));
            Vector2D localVectorRoundTrip = behavior.WorldVectorToLocal(
                behavior.LocalVectorToWorld(localVector));
            Vector2D worldPointRoundTrip = behavior.LocalPointToWorld(
                behavior.WorldPointToLocal(worldPoint));
            Vector2D worldVectorRoundTrip = behavior.LocalVectorToWorld(
                behavior.WorldVectorToLocal(worldVector));
            double maximumError = new[]
            {
                ComponentError(localPoint, localPointRoundTrip),
                ComponentError(localVector, localVectorRoundTrip),
                ComponentError(worldPoint, worldPointRoundTrip),
                ComponentError(worldVector, worldVectorRoundTrip),
            }.Max();

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body transform maximum round-trip absolute error={maximumError:R}."));
            Assert.InRange(maximumError, 0.0, 1e-12);
        }

        [Fact]
        public void PointVelocityIncludesPureAngularMotion()
        {
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialAngle: Math.PI / 2.0,
                initialAngularVelocity: 3.0);
            BodyRun result = RunBody(body, 0.01, 0.0);
            IRigidBody2DBehavior behavior = GetBehavior(result, body);

            Vector2D actual = behavior.GetPointVelocity(new Vector2D(2.0, 0.0));
            NumericAssert.Equal(-6.0, actual.X, 1e-12, 1e-12);
            NumericAssert.Equal(0.0, actual.Y, 1e-12, 1e-12);
        }

        [Fact]
        public void TorqueSignMatchesRightHandedCrossProduct()
        {
            var body = new RigidBody2D("body", 1.0, 1.0);
            BodyRun result = RunBody(body, 0.01, 0.0);
            IRigidBody2DBehavior behavior = GetBehavior(result, body);

            Assert.Equal(
                6.0,
                behavior.ComputeTorque(new Vector2D(2.0, 0.0), new Vector2D(0.0, 3.0)));
            Assert.Equal(
                -6.0,
                behavior.ComputeTorque(new Vector2D(2.0, 0.0), new Vector2D(0.0, -3.0)));
            Assert.Equal(
                -6.0,
                behavior.ComputeTorque(new Vector2D(0.0, 2.0), new Vector2D(3.0, 0.0)));
        }

        [Fact]
        public void TranslationalAndRotationalKineticEnergyAreExported()
        {
            var body = new RigidBody2D(
                "body",
                2.0,
                0.5,
                initialLinearVelocity: new Vector2D(3.0, 4.0),
                initialAngularVelocity: 2.0);
            BodyRun result = RunBody(body, 0.01, 0.05);
            BodySample sample = result.FinalTransient;
            IRigidBody2DBehavior behavior = GetBehavior(result, body);

            Assert.True(result.LinearKineticEnergyExport.IsValid);
            Assert.True(result.AngularKineticEnergyExport.IsValid);
            Assert.True(result.KineticEnergyExport.IsValid);
            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body kinetic energies linear/angular/total={sample.LinearKineticEnergy:R}/{sample.AngularKineticEnergy:R}/{sample.KineticEnergy:R}."));
            NumericAssert.Equal(25.0, sample.LinearKineticEnergy, 1e-12, 1e-12);
            NumericAssert.Equal(1.0, sample.AngularKineticEnergy, 1e-12, 1e-12);
            NumericAssert.Equal(26.0, sample.KineticEnergy, 1e-12, 1e-12);
            NumericAssert.Equal(
                sample.KineticEnergy,
                behavior.KineticEnergy,
                1e-12,
                1e-12);
            Assert.Equal(body.Mass, sample.Mass);
            Assert.Equal(body.Inertia, sample.Inertia);
        }

        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(-1.0, 1.0)]
        [InlineData(double.NaN, 1.0)]
        [InlineData(double.PositiveInfinity, 1.0)]
        [InlineData(1.0, 0.0)]
        [InlineData(1.0, -1.0)]
        [InlineData(1.0, double.NaN)]
        [InlineData(1.0, double.PositiveInfinity)]
        public void NonpositiveOrNonfiniteMassOrInertiaIsRejectedDuringSetup(
            double mass,
            double inertia)
        {
            var body = new RigidBody2D("invalid-body", mass, inertia);
            var simulation = CreateSimulation(0.01, 0.1);
            var circuit = new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                body);

            SpiceSharpException exception = Assert.Throws<SpiceSharpException>(() =>
                simulation.Run(circuit).ToArray());
            Assert.Contains(body.Name, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AngleRemainsUnboundedAndIsNotForciblyWrapped()
        {
            const double initialAngle = 5.0 * Math.PI;
            const double angularVelocity = 4.0;
            const double stopTime = 3.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialAngle: initialAngle,
                initialAngularVelocity: angularVelocity);
            BodySample final = RunBody(body, 0.05, stopTime).FinalTransient;
            double expected = initialAngle + (angularVelocity * stopTime);

            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body unbounded angle expected/measured={expected:R}/{final.Angle:R}."));

            Assert.True(final.Angle > 2.0 * Math.PI);
            NumericAssert.Equal(expected, final.Angle, 1e-10, 1e-11);
        }

        [Fact]
        public void TwoBodiesHaveIndependentSolverVariables()
        {
            var first = new RigidBody2D(
                "first",
                1.0,
                0.4,
                initialLinearVelocity: new Vector2D(1.0, 0.0),
                initialAngularVelocity: 0.5);
            var second = new RigidBody2D(
                "second",
                2.0,
                0.8,
                initialPosition: new Vector2D(3.0, -2.0),
                initialLinearVelocity: new Vector2D(-0.25, 0.75),
                initialAngularVelocity: -1.0);
            Transient simulation = CreateSimulation(0.02, 0.4);
            simulation.Run(new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                first,
                second)).ToArray();
            IRigidBody2DBehavior firstBehavior = simulation.EntityBehaviors[first.Name]
                .GetValue<IRigidBody2DBehavior>();
            IRigidBody2DBehavior secondBehavior = simulation.EntityBehaviors[second.Name]
                .GetValue<IRigidBody2DBehavior>();

            Assert.NotSame(firstBehavior.PositionXVariable, secondBehavior.PositionXVariable);
            Assert.NotSame(firstBehavior.PositionYVariable, secondBehavior.PositionYVariable);
            Assert.NotSame(firstBehavior.AngleVariable, secondBehavior.AngleVariable);
            Assert.NotSame(firstBehavior.VelocityXVariable, secondBehavior.VelocityXVariable);
            Assert.NotSame(firstBehavior.VelocityYVariable, secondBehavior.VelocityYVariable);
            Assert.NotSame(
                firstBehavior.AngularVelocityVariable,
                secondBehavior.AngularVelocityVariable);
            Assert.NotEqual(firstBehavior.Position, secondBehavior.Position);
            Assert.NotEqual(firstBehavior.LinearVelocity, secondBehavior.LinearVelocity);
            Assert.NotEqual(firstBehavior.AngularVelocity, secondBehavior.AngularVelocity);
        }

        [Fact]
        public void RepeatedRunsAreBitwiseDeterministic()
        {
            BodyRun first = RunDeterministicCase();
            BodyRun second = RunDeterministicCase();

            Assert.Equal(first.Samples.Count, second.Samples.Count);
            Console.WriteLine(FormattableString.Invariant(
                $"Rigid-body deterministic comparison sample count={first.Samples.Count}."));
            for (int index = 0; index < first.Samples.Count; index++)
            {
                Assert.Equal(first.Samples[index].ExportType, second.Samples[index].ExportType);
                Assert.Equal(first.Samples[index].Time, second.Samples[index].Time);
                Assert.Equal(first.Samples[index].PositionX, second.Samples[index].PositionX);
                Assert.Equal(first.Samples[index].PositionY, second.Samples[index].PositionY);
                Assert.Equal(first.Samples[index].Angle, second.Samples[index].Angle);
                Assert.Equal(first.Samples[index].VelocityX, second.Samples[index].VelocityX);
                Assert.Equal(first.Samples[index].VelocityY, second.Samples[index].VelocityY);
                Assert.Equal(
                    first.Samples[index].AngularVelocity,
                    second.Samples[index].AngularVelocity);
            }
        }

        private static BodyError ConstantForceErrors(double maximumTimestep)
        {
            const double mass = 2.0;
            const double stopTime = 1.25;
            var force = new Vector2D(3.0, -2.0);
            var initialPosition = new Vector2D(0.5, -0.75);
            var initialVelocity = new Vector2D(0.2, 0.4);
            var body = new RigidBody2D(
                "body",
                mass,
                0.7,
                initialPosition,
                initialAngle: 0.3,
                initialLinearVelocity: initialVelocity,
                initialAngularVelocity: -0.2);
            BodySample actual = RunBody(
                body,
                maximumTimestep,
                stopTime,
                new TestRigidBodyLoad2D("force", body.Name, force, 0.0))
                .FinalTransient;
            Vector2D expectedPosition = initialPosition
                + (initialVelocity * stopTime)
                + ((0.5 * stopTime * stopTime / mass) * force);
            Vector2D expectedVelocity = initialVelocity + ((stopTime / mass) * force);

            return new BodyError(
                MaxRelativeError(expectedPosition, new Vector2D(actual.PositionX, actual.PositionY)),
                MaxRelativeError(expectedVelocity, new Vector2D(actual.VelocityX, actual.VelocityY)),
                0.0,
                0.0);
        }

        private static BodyError ConstantTorqueErrors(double maximumTimestep)
        {
            const double inertia = 0.8;
            const double torque = 1.6;
            const double initialAngle = 0.4;
            const double initialAngularVelocity = -0.3;
            const double stopTime = 1.5;
            var body = new RigidBody2D(
                "body",
                1.0,
                inertia,
                initialAngle: initialAngle,
                initialAngularVelocity: initialAngularVelocity);
            BodySample actual = RunBody(
                body,
                maximumTimestep,
                stopTime,
                new TestRigidBodyLoad2D("torque", body.Name, Vector2D.Zero, torque))
                .FinalTransient;
            double angularAcceleration = torque / inertia;
            double expectedAngle = initialAngle
                + (initialAngularVelocity * stopTime)
                + (0.5 * angularAcceleration * stopTime * stopTime);
            double expectedAngularVelocity = initialAngularVelocity
                + (angularAcceleration * stopTime);

            return new BodyError(
                0.0,
                0.0,
                RelativeError(expectedAngle, actual.Angle),
                RelativeError(expectedAngularVelocity, actual.AngularVelocity));
        }

        private static BodyRun RunDeterministicCase()
        {
            var body = new RigidBody2D(
                "body",
                1.7,
                0.45,
                new Vector2D(-0.2, 0.9),
                initialAngle: 0.6,
                initialLinearVelocity: new Vector2D(0.3, -0.4),
                initialAngularVelocity: 0.8);
            return RunBody(
                body,
                0.015,
                0.75,
                new TestRigidBodyLoad2D(
                    "load",
                    body.Name,
                    new Vector2D(1.2, -0.7),
                    0.35));
        }

        private static IRigidBody2DBehavior GetBehavior(BodyRun run, RigidBody2D body) =>
            run.Simulation.EntityBehaviors[body.Name].GetValue<IRigidBody2DBehavior>();

        private static double ComponentError(Vector2D expected, Vector2D actual) =>
            Math.Max(Math.Abs(actual.X - expected.X), Math.Abs(actual.Y - expected.Y));

        private static double MaxRelativeError(Vector2D expected, Vector2D actual) =>
            Math.Max(
                RelativeError(expected.X, actual.X),
                RelativeError(expected.Y, actual.Y));

        private static double RelativeError(double expected, double actual) =>
            Math.Abs(actual - expected) / Math.Max(Math.Abs(expected), 1e-30);

        private static BodyRun RunBody(
            RigidBody2D body,
            double maximumTimestep,
            double stopTime,
            params SpiceSharp.Entities.IEntity[] connectedEntities)
        {
            Transient simulation = CreateSimulation(maximumTimestep, stopTime);
            var positionXExport = new RealPropertyExport(simulation, body.Name, "positionx");
            var positionYExport = new RealPropertyExport(simulation, body.Name, "positiony");
            var angleExport = new RealPropertyExport(simulation, body.Name, "angle");
            var velocityXExport = new RealPropertyExport(simulation, body.Name, "velocityx");
            var velocityYExport = new RealPropertyExport(simulation, body.Name, "velocityy");
            var angularVelocityExport = new RealPropertyExport(
                simulation,
                body.Name,
                "angularvelocity");
            var massExport = new RealPropertyExport(simulation, body.Name, "mass");
            var inertiaExport = new RealPropertyExport(simulation, body.Name, "inertia");
            var linearKineticEnergyExport = new RealPropertyExport(
                simulation,
                body.Name,
                "linearkineticenergy");
            var angularKineticEnergyExport = new RealPropertyExport(
                simulation,
                body.Name,
                "angularkineticenergy");
            var kineticEnergyExport = new RealPropertyExport(
                simulation,
                body.Name,
                "kineticenergy");
            var entities = new List<SpiceSharp.Entities.IEntity>
            {
                new Resistor("validation-reference", "unused", "0", 1.0),
                body,
            };
            entities.AddRange(connectedEntities);
            var samples = new List<BodySample>();

            foreach (int exportType in simulation.Run(new Circuit(entities.ToArray())))
            {
                if (exportType == Transient.ExportOperatingPoint
                    || exportType == Transient.ExportTransient)
                {
                    samples.Add(new BodySample(
                        exportType,
                        simulation.Time,
                        positionXExport.Value,
                        positionYExport.Value,
                        angleExport.Value,
                        velocityXExport.Value,
                        velocityYExport.Value,
                        angularVelocityExport.Value,
                        massExport.Value,
                        inertiaExport.Value,
                        linearKineticEnergyExport.Value,
                        angularKineticEnergyExport.Value,
                        kineticEnergyExport.Value));
                }
            }

            return new BodyRun(
                simulation,
                linearKineticEnergyExport,
                angularKineticEnergyExport,
                kineticEnergyExport,
                samples);
        }

        private static Transient CreateSimulation(double maximumTimestep, double stopTime)
        {
            var method = new Trapezoidal
            {
                InitialStep = maximumTimestep,
                MaxStep = maximumTimestep,
                StopTime = stopTime,
            };
            return new Transient("tran", method);
        }

        private readonly struct BodyError
        {
            public BodyError(
                double position,
                double velocity,
                double angle,
                double angularVelocity)
            {
                Position = position;
                Velocity = velocity;
                Angle = angle;
                AngularVelocity = angularVelocity;
            }

            public double Position { get; }

            public double Velocity { get; }

            public double Angle { get; }

            public double AngularVelocity { get; }
        }

        private sealed class BodyRun
        {
            public BodyRun(
                Transient simulation,
                RealPropertyExport linearKineticEnergyExport,
                RealPropertyExport angularKineticEnergyExport,
                RealPropertyExport kineticEnergyExport,
                IReadOnlyList<BodySample> samples)
            {
                Simulation = simulation;
                LinearKineticEnergyExport = linearKineticEnergyExport;
                AngularKineticEnergyExport = angularKineticEnergyExport;
                KineticEnergyExport = kineticEnergyExport;
                Samples = samples;
            }

            public Transient Simulation { get; }

            public RealPropertyExport LinearKineticEnergyExport { get; }

            public RealPropertyExport AngularKineticEnergyExport { get; }

            public RealPropertyExport KineticEnergyExport { get; }

            public IReadOnlyList<BodySample> Samples { get; }

            public BodySample FinalTransient => Samples
                .Last(sample => sample.ExportType == Transient.ExportTransient);
        }

        private readonly struct BodySample
        {
            public BodySample(
                int exportType,
                double time,
                double positionX,
                double positionY,
                double angle,
                double velocityX,
                double velocityY,
                double angularVelocity,
                double mass,
                double inertia,
                double linearKineticEnergy,
                double angularKineticEnergy,
                double kineticEnergy)
            {
                ExportType = exportType;
                Time = time;
                PositionX = positionX;
                PositionY = positionY;
                Angle = angle;
                VelocityX = velocityX;
                VelocityY = velocityY;
                AngularVelocity = angularVelocity;
                Mass = mass;
                Inertia = inertia;
                LinearKineticEnergy = linearKineticEnergy;
                AngularKineticEnergy = angularKineticEnergy;
                KineticEnergy = kineticEnergy;
            }

            public int ExportType { get; }

            public double Time { get; }

            public double PositionX { get; }

            public double PositionY { get; }

            public double Angle { get; }

            public double VelocityX { get; }

            public double VelocityY { get; }

            public double AngularVelocity { get; }

            public double Mass { get; }

            public double Inertia { get; }

            public double LinearKineticEnergy { get; }

            public double AngularKineticEnergy { get; }

            public double KineticEnergy { get; }
        }
    }
}
