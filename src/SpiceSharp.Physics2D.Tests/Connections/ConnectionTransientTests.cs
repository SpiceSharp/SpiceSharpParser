using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Connections
{
    public class ConnectionTransientTests
    {
        [Fact]
        public void TwoBodyDistanceOscillatorMatchesReducedMassFrequency()
        {
            const double massA = 1.0;
            const double massB = 3.0;
            const double stiffness = 20.0;
            const double restLength = 1.0;
            var bodyA = new RigidBody2D(
                "a",
                massA,
                1.0,
                initialPosition: new Vector2D(-0.55, 0.0));
            var bodyB = new RigidBody2D(
                "b",
                massB,
                1.0,
                initialPosition: new Vector2D(0.55, 0.0));
            var spring = new DistanceSpringDamper2D(
                "spring",
                MechanicalAnchor2D.Body(bodyA.Name, Vector2D.Zero),
                MechanicalAnchor2D.Body(bodyB.Name, Vector2D.Zero),
                restLength,
                stiffness,
                lengthRegularization: 1e-10);
            ConnectionRun run = ConnectionTestSimulation.Run(
                bodyA,
                bodyB,
                0.001,
                4.0,
                spring);
            double period = EstimatePeriod(
                run.Samples,
                sample => (sample.XB - sample.XA) - restLength);
            double actualFrequency = (2.0 * Math.PI) / period;
            double reducedMass = (massA * massB) / (massA + massB);
            double expectedFrequency = Math.Sqrt(stiffness / reducedMass);
            double relativeError = Math.Abs(actualFrequency - expectedFrequency) / expectedFrequency;

            Console.WriteLine(FormattableString.Invariant(
                $"Reduced-mass oscillator angular frequency expected/actual/relative error={expectedFrequency:R}/{actualFrequency:R}/{relativeError:R}."));
            Assert.InRange(relativeError, 0.0, 3e-3);
        }

        [Fact]
        public void WorldAttachedDistanceDamperMatchesUnderdampedAnalyticMotion()
        {
            const double stiffness = 16.0;
            const double damping = 1.2;
            const double stopTime = 1.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialPosition: new Vector2D(1.0, 0.0));
            var connection = new DistanceSpringDamper2D(
                "spring-damper",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                0.0,
                stiffness,
                damping,
                1e-12);
            ConnectionSample actual = ConnectionTestSimulation.Run(
                body,
                null,
                0.001,
                stopTime,
                connection).Final;
            double decay = damping / 2.0;
            double dampedFrequency = Math.Sqrt(stiffness - (decay * decay));
            double expected = Math.Exp(-decay * stopTime) *
                (Math.Cos(dampedFrequency * stopTime) +
                ((decay / dampedFrequency) * Math.Sin(dampedFrequency * stopTime)));
            double error = Math.Abs(actual.XA - expected);

            Console.WriteLine(FormattableString.Invariant(
                $"Damped world spring final displacement expected/actual/absolute error={expected:R}/{actual.XA:R}/{error:R}."));
            NumericAssert.Equal(expected, actual.XA, 2e-5, 2e-5);
        }

        [Fact]
        public void WorldAttachedRotationalSpringMatchesPureAngularOscillator()
        {
            const double inertia = 0.5;
            const double stiffness = 8.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                inertia,
                initialAngle: 0.25);
            var spring = new RotationalSpringDamper2D(
                "torsion",
                RotationalEndpoint2D.World(),
                RotationalEndpoint2D.Body(body.Name),
                0.0,
                stiffness);
            ConnectionRun run = ConnectionTestSimulation.Run(
                body,
                null,
                0.001,
                3.5,
                spring);
            double period = EstimatePeriod(run.Samples, sample => sample.AngleA);
            double actualFrequency = (2.0 * Math.PI) / period;
            double expectedFrequency = Math.Sqrt(stiffness / inertia);
            double relativeError = Math.Abs(actualFrequency - expectedFrequency) / expectedFrequency;

            Console.WriteLine(FormattableString.Invariant(
                $"Rotational oscillator angular frequency expected/actual/relative error={expectedFrequency:R}/{actualFrequency:R}/{relativeError:R}."));
            Assert.InRange(relativeError, 0.0, 3e-3);
            Assert.InRange(Math.Abs(run.Final.XA), 0.0, 1e-12);
            Assert.InRange(Math.Abs(run.Final.YA), 0.0, 1e-12);
        }

        [Fact]
        public void DistanceSpringTransientConvergesUnderTimestepRefinement()
        {
            double coarseError = WorldSpringFinalError(0.02);
            double mediumError = WorldSpringFinalError(0.01);
            double fineError = WorldSpringFinalError(0.005);

            Console.WriteLine(FormattableString.Invariant(
                $"Distance spring timestep-refinement final-position errors h=0.02/0.01/0.005: {coarseError:R}/{mediumError:R}/{fineError:R}."));
            Assert.True(mediumError < coarseError);
            Assert.True(fineError < mediumError);
            Assert.InRange(fineError, 0.0, 2e-4);
        }

        private static double WorldSpringFinalError(double maximumTimestep)
        {
            const double stiffness = 9.0;
            const double stopTime = 1.0;
            var body = new RigidBody2D(
                "body",
                1.0,
                1.0,
                initialPosition: new Vector2D(0.4, 0.0));
            var spring = new DistanceSpringDamper2D(
                "spring",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                0.0,
                stiffness,
                lengthRegularization: 1e-12);
            double actual = ConnectionTestSimulation.Run(
                body,
                null,
                maximumTimestep,
                stopTime,
                spring).Final.XA;
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
                        samples[index - 1].Time +
                        (fraction * (samples[index].Time - samples[index - 1].Time)));
                }
            }

            Assert.True(crossings.Count >= 2, "At least two positive-to-negative zero crossings are required.");
            return crossings[1] - crossings[0];
        }
    }
}
