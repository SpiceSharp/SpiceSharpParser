using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Physics2D.Core;
using SpiceSharp.Physics2D.Tests.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Coordinates
{
    public class MechanicalCoordinateTests
    {
        [Fact]
        public void ZeroForceAndZeroVelocityKeepPositionConstant()
        {
            var coordinate = new MechanicalCoordinate("coordinate", 2.0, initialPosition: 0.75);
            CoordinateRun result = RunCoordinate(coordinate, 0.05, 2.0);
            CoordinateSample final = result.FinalTransient;

            NumericAssert.Equal(0.75, final.Position, 1e-12, 1e-12);
            NumericAssert.Equal(0.0, final.Velocity, 1e-12, 1e-12);
        }

        [Fact]
        public void ZeroForceAndNonzeroVelocityProduceConstantVelocityMotion()
        {
            const double initialPosition = 0.25;
            const double initialVelocity = -0.3;
            const double stopTime = 2.0;
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                2.0,
                initialPosition,
                initialVelocity);
            CoordinateRun result = RunCoordinate(coordinate, 0.05, stopTime);
            CoordinateSample final = result.FinalTransient;

            NumericAssert.Equal(
                initialPosition + (initialVelocity * stopTime),
                final.Position,
                1e-11,
                1e-11);
            NumericAssert.Equal(initialVelocity, final.Velocity, 1e-12, 1e-12);
        }

        [Fact]
        public void ConstantForceMatchesAnalyticAcceleration()
        {
            ErrorPair coarse = ConstantForceErrors(0.04);
            ErrorPair medium = ConstantForceErrors(0.02);
            ErrorPair fine = ConstantForceErrors(0.01);

            Console.WriteLine(FormattableString.Invariant(
                $"Constant-force relative errors position/velocity: h=0.04 {coarse.First:R}/{coarse.Second:R}, h=0.02 {medium.First:R}/{medium.Second:R}, h=0.01 {fine.First:R}/{fine.Second:R}."));
            Assert.InRange(fine.First, 0.0, 1e-4);
            Assert.InRange(fine.Second, 0.0, 1e-5);
        }

        [Fact]
        public void LinearDampingMatchesExponentialDecay()
        {
            const double mass = 2.0;
            const double damping = 0.8;
            const double initialPosition = 0.4;
            const double initialVelocity = 3.0;
            const double stopTime = 2.0;
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                mass,
                initialPosition,
                initialVelocity);
            CoordinateRun result = RunCoordinate(
                coordinate,
                0.002,
                stopTime,
                new LinearGeneralizedDamping("damping", coordinate.Name, damping));
            CoordinateSample final = result.FinalTransient;
            double decay = Math.Exp(-(damping / mass) * stopTime);
            double expectedVelocity = initialVelocity * decay;
            double expectedPosition = initialPosition
                + ((mass * initialVelocity / damping) * (1.0 - decay));
            double decayRelativeError = RelativeError(expectedVelocity, final.Velocity);

            Console.WriteLine(FormattableString.Invariant(
                $"Damped-decay relative errors: position={RelativeError(expectedPosition, final.Position):R}, velocity={decayRelativeError:R}."));
            Assert.InRange(decayRelativeError, 0.0, 2e-4);
            NumericAssert.Equal(expectedPosition, final.Position, 1e-8, 2e-4);
        }

        [Fact]
        public void LinearSpringMatchesOscillatorPeriod()
        {
            const double mass = 2.0;
            const double stiffness = 8.0;
            double expectedPeriod = 2.0 * Math.PI * Math.Sqrt(mass / stiffness);
            var coordinate = new MechanicalCoordinate("coordinate", mass, initialPosition: 1.0);
            CoordinateRun result = RunCoordinate(
                coordinate,
                expectedPeriod / 500.0,
                1.6 * expectedPeriod,
                new LinearGeneralizedSpringToReference(
                    "spring",
                    coordinate.Name,
                    stiffness));
            double measuredPeriod = MeasureDownwardCrossingPeriod(result.TransientSamples);
            double relativeError = RelativeError(expectedPeriod, measuredPeriod);

            Console.WriteLine(FormattableString.Invariant(
                $"Undamped oscillator period: expected={expectedPeriod:R}, measured={measuredPeriod:R}, relative error={relativeError:R}."));
            Assert.InRange(relativeError, 0.0, 2e-3);
        }

        [Fact]
        public void DampedOscillatorMatchesDecayEnvelopeAndDampedFrequency()
        {
            const double mass = 1.0;
            const double stiffness = 9.0;
            const double damping = 0.6;
            const double initialPosition = 1.0;
            const double initialVelocity = 0.0;
            double alpha = damping / (2.0 * mass);
            double dampedFrequency = Math.Sqrt((stiffness / mass) - (alpha * alpha));
            double expectedPeriod = 2.0 * Math.PI / dampedFrequency;
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                mass,
                initialPosition,
                initialVelocity);
            CoordinateRun result = RunCoordinate(
                coordinate,
                expectedPeriod / 1000.0,
                2.5 * expectedPeriod,
                new LinearGeneralizedSpringToReference(
                    "spring",
                    coordinate.Name,
                    stiffness),
                new LinearGeneralizedDamping("damping", coordinate.Name, damping));

            TimeSeriesComparisonResult trajectory = CompareDampedTrajectory(
                result.TransientSamples,
                mass,
                stiffness,
                damping,
                initialPosition,
                initialVelocity);
            double measuredPeriod = MeasureDownwardCrossingPeriod(result.TransientSamples);
            double periodRelativeError = RelativeError(expectedPeriod, measuredPeriod);

            Console.WriteLine(FormattableString.Invariant(
                $"Damped oscillator: normalized RMS={trajectory.NormalizedRootMeanSquareError:R}, period relative error={periodRelativeError:R}."));
            Assert.InRange(trajectory.NormalizedRootMeanSquareError, 0.0, 2e-4);
            Assert.InRange(periodRelativeError, 0.0, 2e-3);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void NonpositiveOrNonfiniteMassIsRejectedDuringSetup(double mass)
        {
            var coordinate = new MechanicalCoordinate("invalid-coordinate", mass);
            var simulation = CreateSimulation(0.01, 0.1);
            var circuit = new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                coordinate);

            SpiceSharpException exception = Assert.Throws<SpiceSharpException>(() =>
                simulation.Run(circuit).ToArray());
            Assert.Contains(coordinate.Name, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void InitialConditionModeIsDeterministic()
        {
            Assert.Equal(
                new[] { MechanicalInitialConditionMode.HoldSpecifiedStateDuringOperatingPoint },
                Enum.GetValues(typeof(MechanicalInitialConditionMode))
                    .Cast<MechanicalInitialConditionMode>()
                    .ToArray());

            CoordinateRun first = RunCoordinate(
                new MechanicalCoordinate("coordinate", 1.5, 0.75, -0.25),
                0.01,
                0.5);
            CoordinateRun second = RunCoordinate(
                new MechanicalCoordinate("coordinate", 1.5, 0.75, -0.25),
                0.01,
                0.5);

            Assert.Equal(first.Samples.Count, second.Samples.Count);
            for (int index = 0; index < first.Samples.Count; index++)
            {
                Assert.Equal(first.Samples[index].ExportType, second.Samples[index].ExportType);
                Assert.Equal(first.Samples[index].Time, second.Samples[index].Time);
                Assert.Equal(first.Samples[index].Position, second.Samples[index].Position);
                Assert.Equal(first.Samples[index].Velocity, second.Samples[index].Velocity);
            }

            CoordinateSample operatingPoint = Assert.Single(
                first.Samples,
                sample => sample.ExportType == Transient.ExportOperatingPoint);
            Assert.Equal(0.75, operatingPoint.Position);
            Assert.Equal(-0.25, operatingPoint.Velocity);
        }

        [Fact]
        public void DampedTrajectoryConvergesUnderMaximumTimestepRefinement()
        {
            const double stopTime = 2.0;
            double coarseError = DampedEndpointError(0.2, stopTime);
            double mediumError = DampedEndpointError(0.1, stopTime);
            double fineError = DampedEndpointError(0.05, stopTime);

            Console.WriteLine(FormattableString.Invariant(
                $"Damped refinement endpoint errors: h=0.2 {coarseError:R}, h=0.1 {mediumError:R}, h=0.05 {fineError:R}."));
            Assert.True(mediumError < coarseError, $"Expected {mediumError:R} < {coarseError:R}.");
            Assert.True(fineError < mediumError, $"Expected {fineError:R} < {mediumError:R}.");
        }

        [Fact]
        public void UndampedOscillatorConservesTotalEnergyWithinIntegrationTolerance()
        {
            const double mass = 1.5;
            const double stiffness = 6.0;
            const double initialPosition = 1.0;
            const double initialVelocity = 0.2;
            double period = 2.0 * Math.PI * Math.Sqrt(mass / stiffness);
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                mass,
                initialPosition,
                initialVelocity);
            CoordinateRun result = RunCoordinate(
                coordinate,
                period / 200.0,
                10.0 * period,
                new LinearGeneralizedSpringToReference(
                    "spring",
                    coordinate.Name,
                    stiffness));
            double initialEnergy = (0.5 * mass * initialVelocity * initialVelocity)
                + (0.5 * stiffness * initialPosition * initialPosition);
            double maximumRelativeDrift = result.TransientSamples.Max(sample =>
            {
                double energy = (0.5 * mass * sample.Velocity * sample.Velocity)
                    + (0.5 * stiffness * sample.Position * sample.Position);
                return Math.Abs(energy - initialEnergy) / initialEnergy;
            });

            Console.WriteLine(FormattableString.Invariant(
                $"Undamped oscillator maximum relative energy drift={maximumRelativeDrift:R}."));
            Assert.InRange(maximumRelativeDrift, 0.0, 1e-6);
        }

        [Fact]
        public void ExportsAndBehaviorContractReferToLiveSolverState()
        {
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                1.0,
                initialPosition: 1.0,
                initialVelocity: 0.0);
            CoordinateRun result = RunCoordinate(
                coordinate,
                0.01,
                1.0,
                new ConstantGeneralizedForce("force", coordinate.Name, 1.0));
            IMechanicalCoordinateBehavior behavior = result.Simulation
                .EntityBehaviors[coordinate.Name]
                .GetValue<IMechanicalCoordinateBehavior>();

            Assert.True(result.PositionExport.IsValid);
            Assert.True(result.VelocityExport.IsValid);
            Assert.True(result.GeneralizedMassExport.IsValid);
            Assert.True(result.InitialPositionExport.IsValid);
            Assert.True(result.InitialVelocityExport.IsValid);
            Assert.NotEqual(coordinate.InitialPosition, result.FinalTransient.Position);
            Assert.NotEqual(coordinate.InitialVelocity, result.FinalTransient.Velocity);
            Assert.Equal(behavior.Position, result.FinalTransient.Position);
            Assert.Equal(behavior.Velocity, result.FinalTransient.Velocity);
            Assert.Equal(behavior.PositionVariable.Value, result.FinalTransient.Position);
            Assert.Equal(behavior.VelocityVariable.Value, result.FinalTransient.Velocity);
            NumericAssert.Equal(
                0.5 * coordinate.GeneralizedMass * behavior.Velocity * behavior.Velocity,
                result.FinalTransient.KineticEnergy,
                1e-12,
                1e-12);
            Assert.Equal(coordinate.GeneralizedMass, result.FinalTransient.GeneralizedMass);
            Assert.Equal(coordinate.InitialPosition, result.FinalTransient.InitialPosition);
            Assert.Equal(coordinate.InitialVelocity, result.FinalTransient.InitialVelocity);
            Assert.Equal(1.0, coordinate.InitialPosition);
            Assert.Equal(0.0, coordinate.InitialVelocity);
        }

        private static TimeSeriesComparisonResult CompareDampedTrajectory(
            IReadOnlyList<CoordinateSample> samples,
            double mass,
            double stiffness,
            double damping,
            double initialPosition,
            double initialVelocity)
        {
            var expected = new List<TimeSeriesSample>(samples.Count);
            var actual = new List<TimeSeriesSample>(samples.Count);
            foreach (CoordinateSample sample in samples)
            {
                DampedState exact = EvaluateDampedState(
                    sample.Time,
                    mass,
                    stiffness,
                    damping,
                    initialPosition,
                    initialVelocity);
                expected.Add(new TimeSeriesSample(sample.Time, exact.Position, exact.Velocity));
                actual.Add(new TimeSeriesSample(sample.Time, sample.Position, sample.Velocity));
            }

            return TimeSeriesComparison.Compare(expected, actual, normalizationFloor: 0.1);
        }

        private static ErrorPair ConstantForceErrors(double maximumTimestep)
        {
            const double mass = 2.0;
            const double force = 3.0;
            const double initialPosition = 0.25;
            const double initialVelocity = -0.1;
            const double stopTime = 2.0;
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                mass,
                initialPosition,
                initialVelocity);
            CoordinateSample final = RunCoordinate(
                coordinate,
                maximumTimestep,
                stopTime,
                new ConstantGeneralizedForce("force", coordinate.Name, force))
                .FinalTransient;
            double acceleration = force / mass;
            double expectedPosition = initialPosition
                + (initialVelocity * stopTime)
                + (0.5 * acceleration * stopTime * stopTime);
            double expectedVelocity = initialVelocity + (acceleration * stopTime);
            return new ErrorPair(
                RelativeError(expectedPosition, final.Position),
                RelativeError(expectedVelocity, final.Velocity));
        }

        private static double DampedEndpointError(double maximumTimestep, double stopTime)
        {
            const double mass = 1.0;
            const double stiffness = 4.0;
            const double damping = 0.4;
            const double initialPosition = 1.0;
            const double initialVelocity = 0.25;
            var coordinate = new MechanicalCoordinate(
                "coordinate",
                mass,
                initialPosition,
                initialVelocity);
            CoordinateSample actual = RunCoordinate(
                coordinate,
                maximumTimestep,
                stopTime,
                new LinearGeneralizedSpringToReference(
                    "spring",
                    coordinate.Name,
                    stiffness),
                new LinearGeneralizedDamping("damping", coordinate.Name, damping))
                .FinalTransient;
            DampedState expected = EvaluateDampedState(
                stopTime,
                mass,
                stiffness,
                damping,
                initialPosition,
                initialVelocity);
            double positionError = actual.Position - expected.Position;
            double velocityError = actual.Velocity - expected.Velocity;
            return Math.Sqrt((positionError * positionError) + (velocityError * velocityError));
        }

        private static DampedState EvaluateDampedState(
            double time,
            double mass,
            double stiffness,
            double damping,
            double initialPosition,
            double initialVelocity)
        {
            double alpha = damping / (2.0 * mass);
            double frequency = Math.Sqrt((stiffness / mass) - (alpha * alpha));
            double sineCoefficient = (initialVelocity + (alpha * initialPosition)) / frequency;
            double exponential = Math.Exp(-alpha * time);
            double cosine = Math.Cos(frequency * time);
            double sine = Math.Sin(frequency * time);
            double position = exponential
                * ((initialPosition * cosine) + (sineCoefficient * sine));
            double velocity = exponential
                * ((initialVelocity * cosine)
                    + ((-alpha * sineCoefficient) - (frequency * initialPosition)) * sine);
            return new DampedState(position, velocity);
        }

        private static double MeasureDownwardCrossingPeriod(
            IReadOnlyList<CoordinateSample> samples)
        {
            var crossings = new List<double>();
            for (int index = 1; index < samples.Count; index++)
            {
                CoordinateSample previous = samples[index - 1];
                CoordinateSample current = samples[index];
                if (previous.Position > 0.0 && current.Position <= 0.0)
                {
                    double fraction = previous.Position
                        / (previous.Position - current.Position);
                    crossings.Add(previous.Time + (fraction * (current.Time - previous.Time)));
                }
            }

            Assert.True(crossings.Count >= 2, "Expected at least two downward zero crossings.");
            return crossings[1] - crossings[0];
        }

        private static double RelativeError(double expected, double actual) =>
            Math.Abs(actual - expected) / Math.Max(Math.Abs(expected), 1e-30);

        private static CoordinateRun RunCoordinate(
            MechanicalCoordinate coordinate,
            double maximumTimestep,
            double stopTime,
            params SpiceSharp.Entities.IEntity[] connectedEntities)
        {
            Transient simulation = CreateSimulation(maximumTimestep, stopTime);
            var positionExport = new RealPropertyExport(simulation, coordinate.Name, "position");
            var velocityExport = new RealPropertyExport(simulation, coordinate.Name, "velocity");
            var kineticEnergyExport = new RealPropertyExport(
                simulation,
                coordinate.Name,
                "kineticenergy");
            var generalizedMassExport = new RealPropertyExport(
                simulation,
                coordinate.Name,
                "generalizedmass");
            var initialPositionExport = new RealPropertyExport(
                simulation,
                coordinate.Name,
                "initialposition");
            var initialVelocityExport = new RealPropertyExport(
                simulation,
                coordinate.Name,
                "initialvelocity");
            var samples = new List<CoordinateSample>();
            var entities = new List<SpiceSharp.Entities.IEntity>
            {
                new Resistor("validation-reference", "unused", "0", 1.0),
                coordinate,
            };
            entities.AddRange(connectedEntities);

            foreach (int exportType in simulation.Run(new Circuit(entities.ToArray())))
            {
                if (exportType == Transient.ExportOperatingPoint
                    || exportType == Transient.ExportTransient)
                {
                    samples.Add(new CoordinateSample(
                        exportType,
                        simulation.Time,
                        positionExport.Value,
                        velocityExport.Value,
                        kineticEnergyExport.Value,
                        generalizedMassExport.Value,
                        initialPositionExport.Value,
                        initialVelocityExport.Value));
                }
            }

            return new CoordinateRun(
                simulation,
                positionExport,
                velocityExport,
                kineticEnergyExport,
                generalizedMassExport,
                initialPositionExport,
                initialVelocityExport,
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

        private readonly struct DampedState
        {
            public DampedState(double position, double velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public double Position { get; }

            public double Velocity { get; }
        }

        private readonly struct ErrorPair
        {
            public ErrorPair(double first, double second)
            {
                First = first;
                Second = second;
            }

            public double First { get; }

            public double Second { get; }
        }

        private sealed class CoordinateRun
        {
            public CoordinateRun(
                Transient simulation,
                RealPropertyExport positionExport,
                RealPropertyExport velocityExport,
                RealPropertyExport kineticEnergyExport,
                RealPropertyExport generalizedMassExport,
                RealPropertyExport initialPositionExport,
                RealPropertyExport initialVelocityExport,
                IReadOnlyList<CoordinateSample> samples)
            {
                Simulation = simulation;
                PositionExport = positionExport;
                VelocityExport = velocityExport;
                KineticEnergyExport = kineticEnergyExport;
                GeneralizedMassExport = generalizedMassExport;
                InitialPositionExport = initialPositionExport;
                InitialVelocityExport = initialVelocityExport;
                Samples = samples;
            }

            public Transient Simulation { get; }

            public RealPropertyExport PositionExport { get; }

            public RealPropertyExport VelocityExport { get; }

            public RealPropertyExport KineticEnergyExport { get; }

            public RealPropertyExport GeneralizedMassExport { get; }

            public RealPropertyExport InitialPositionExport { get; }

            public RealPropertyExport InitialVelocityExport { get; }

            public IReadOnlyList<CoordinateSample> Samples { get; }

            public IReadOnlyList<CoordinateSample> TransientSamples => Samples
                .Where(sample => sample.ExportType == Transient.ExportTransient)
                .ToArray();

            public CoordinateSample FinalTransient => TransientSamples.Last();
        }

        private readonly struct CoordinateSample
        {
            public CoordinateSample(
                int exportType,
                double time,
                double position,
                double velocity,
                double kineticEnergy,
                double generalizedMass,
                double initialPosition,
                double initialVelocity)
            {
                ExportType = exportType;
                Time = time;
                Position = position;
                Velocity = velocity;
                KineticEnergy = kineticEnergy;
                GeneralizedMass = generalizedMass;
                InitialPosition = initialPosition;
                InitialVelocity = initialVelocity;
            }

            public int ExportType { get; }

            public double Time { get; }

            public double Position { get; }

            public double Velocity { get; }

            public double KineticEnergy { get; }

            public double GeneralizedMass { get; }

            public double InitialPosition { get; }

            public double InitialVelocity { get; }
        }
    }
}
