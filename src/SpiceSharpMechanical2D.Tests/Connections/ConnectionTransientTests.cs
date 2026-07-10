using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharpMechanical2D.Tests.Numerics;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Connections
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
        public void OffCenterDistanceStampMatchesIndependentNonlinearReference()
        {
            const double mass = 1.4;
            const double inertia = 0.8;
            const double restLength = 0.9;
            const double stiffness = 6.0;
            const double damping = 0.5;
            const double regularization = 1e-4;
            const double stopTime = 0.3;
            var worldAnchor = new Vector2D(0.1, -0.2);
            var localAnchor = new Vector2D(0.3, -0.4);
            var initialPosition = new Vector2D(1.0, 0.5);
            var initialVelocity = new Vector2D(0.2, -0.1);
            const double initialAngle = 0.35;
            const double initialAngularVelocity = 0.3;
            var body = new RigidBody2D(
                "body",
                mass,
                inertia,
                initialPosition,
                initialAngle,
                initialVelocity,
                initialAngularVelocity);
            var connection = new DistanceSpringDamper2D(
                "off-center",
                MechanicalAnchor2D.World(worldAnchor),
                MechanicalAnchor2D.Body(body.Name, localAnchor),
                restLength,
                stiffness,
                damping,
                regularization);
            ConnectionSample actual = ConnectionTestSimulation.Run(
                body,
                null,
                0.0005,
                stopTime,
                connection).Final;
            double[] expected = IntegrateDistanceReference(
                new[]
                {
                    initialPosition.X,
                    initialPosition.Y,
                    initialAngle,
                    initialVelocity.X,
                    initialVelocity.Y,
                    initialAngularVelocity,
                },
                mass,
                inertia,
                worldAnchor,
                localAnchor,
                restLength,
                stiffness,
                damping,
                regularization,
                stopTime,
                60000);
            double maximumError = 0.0;
            double[] actualState =
            {
                actual.XA,
                actual.YA,
                actual.AngleA,
                actual.VelocityXA,
                actual.VelocityYA,
                actual.AngularVelocityA,
            };
            for (int index = 0; index < expected.Length; index++)
            {
                maximumError = Math.Max(
                    maximumError,
                    Math.Abs(expected[index] - actualState[index]));
            }

            Console.WriteLine(FormattableString.Invariant(
                $"Off-center distance production-stamp maximum state error={maximumError:R}."));

            NumericAssert.Equal(expected[0], actual.XA, 2e-6, 2e-6);
            NumericAssert.Equal(expected[1], actual.YA, 2e-6, 2e-6);
            NumericAssert.Equal(expected[2], actual.AngleA, 2e-6, 2e-6);
            NumericAssert.Equal(expected[3], actual.VelocityXA, 2e-6, 2e-6);
            NumericAssert.Equal(expected[4], actual.VelocityYA, 2e-6, 2e-6);
            NumericAssert.Equal(expected[5], actual.AngularVelocityA, 2e-6, 2e-6);
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
                initialAngle: 0.05);
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

        private static double[] IntegrateDistanceReference(
            double[] initialState,
            double mass,
            double inertia,
            Vector2D worldAnchor,
            Vector2D localAnchor,
            double restLength,
            double stiffness,
            double damping,
            double regularization,
            double duration,
            int steps)
        {
            var state = (double[])initialState.Clone();
            double step = duration / steps;
            for (int index = 0; index < steps; index++)
            {
                double[] derivative1 = EvaluateDistanceReference(state);
                double[] derivative2 = EvaluateDistanceReference(
                    AddScaled(state, derivative1, 0.5 * step));
                double[] derivative3 = EvaluateDistanceReference(
                    AddScaled(state, derivative2, 0.5 * step));
                double[] derivative4 = EvaluateDistanceReference(
                    AddScaled(state, derivative3, step));
                for (int stateIndex = 0; stateIndex < state.Length; stateIndex++)
                {
                    state[stateIndex] += (step / 6.0)
                        * (derivative1[stateIndex]
                            + (2.0 * derivative2[stateIndex])
                            + (2.0 * derivative3[stateIndex])
                            + derivative4[stateIndex]);
                }
            }

            return state;

            double[] EvaluateDistanceReference(double[] current)
            {
                var position = new Vector2D(current[0], current[1]);
                double angle = current[2];
                var velocity = new Vector2D(current[3], current[4]);
                double angularVelocity = current[5];
                Vector2D radius = localAnchor.Rotate(angle);
                Vector2D separation = position + radius - worldAnchor;
                double length = Math.Sqrt(
                    separation.LengthSquared + (regularization * regularization));
                Vector2D direction = separation / length;
                Vector2D pointVelocity = velocity
                    + (radius.Perpendicular() * angularVelocity);
                double normalSpeed = Vector2D.Dot(direction, pointVelocity);
                double forceMagnitude = (stiffness * (length - restLength))
                    + (damping * normalSpeed);
                Vector2D forceOnBody = -direction * forceMagnitude;
                double torque = Vector2D.Cross(radius, forceOnBody);
                return new[]
                {
                    velocity.X,
                    velocity.Y,
                    angularVelocity,
                    forceOnBody.X / mass,
                    forceOnBody.Y / mass,
                    torque / inertia,
                };
            }
        }

        private static double[] AddScaled(double[] state, double[] derivative, double scale)
        {
            var result = new double[state.Length];
            for (int index = 0; index < state.Length; index++)
            {
                result[index] = state[index] + (scale * derivative[index]);
            }

            return result;
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
