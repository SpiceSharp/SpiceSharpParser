using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpMechanical2D.ApiProbe;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.ApiProof
{
    public class TransientApiProbeTests
    {
        [Fact]
        public void TransientCreatesProbeBehavior()
        {
            var result = RunProbe(0.05, 0.1);

            Assert.True(result.Simulation.EntityBehaviors.Contains("probe"));
            Assert.NotNull(result.Simulation.EntityBehaviors["probe"].GetValue<ITransientApiProbeBehavior>());
        }

        [Fact]
        public void ProbeOwnsTwoIndependentPrivateSolverVariables()
        {
            var result = RunProbe(0.05, 0.1, 0.25, -0.75);
            var behavior = result.Simulation.EntityBehaviors["probe"].GetValue<ITransientApiProbeBehavior>();

            Assert.NotSame(behavior.AVariable, behavior.BVariable);
            Assert.NotEqual(behavior.AVariable.Name, behavior.BVariable.Name);
            Assert.Equal(behavior.A, behavior.AVariable.Value);
            Assert.Equal(behavior.B, behavior.BVariable.Value);
        }

        [Fact]
        public void OperatingPointAppliesRequestedInitialConditionsToBehaviorState()
        {
            const double initialA = 0.25;
            const double initialB = -0.75;
            var result = RunProbe(0.05, 0.1, initialA, initialB);
            var operatingPoint = Assert.Single(
                result.Samples,
                sample => sample.ExportType == Transient.ExportOperatingPoint);

            Assert.Equal(initialA, operatingPoint.A);
            Assert.Equal(initialB, operatingPoint.B);
        }

        [Fact]
        public void StatePropertiesAreQueryableThroughRealPropertyExportsAtTimeZero()
        {
            const double initialA = -0.125;
            const double initialB = 0.625;
            var result = RunProbe(0.05, 0.1, initialA, initialB);
            var operatingPoint = Assert.Single(
                result.Samples,
                sample => sample.ExportType == Transient.ExportOperatingPoint);

            Assert.True(result.AExport.IsValid);
            Assert.True(result.BExport.IsValid);
            Assert.Equal(0.0, operatingPoint.Time, 15);
            Assert.Equal(initialA, operatingPoint.A);
            Assert.Equal(initialB, operatingPoint.B);
        }

        [Fact]
        public void OscillatorRemainsFiniteForTenPeriods()
        {
            double stopTime = 20.0 * Math.PI;
            var result = RunProbe(0.05, stopTime);
            var transientSamples = result.Samples.Where(sample => sample.ExportType == Transient.ExportTransient).ToArray();
            double maximumRadiusError = transientSamples.Max(sample =>
                Math.Abs(1.0 - Math.Sqrt((sample.A * sample.A) + (sample.B * sample.B))));

            Console.WriteLine(FormattableString.Invariant(
                $"Ten-period samples={transientSamples.Length}, maximum radius error={maximumRadiusError:R}."));

            Assert.NotEmpty(transientSamples);
            Assert.All(transientSamples, sample =>
            {
                Assert.True(double.IsFinite(sample.A));
                Assert.True(double.IsFinite(sample.B));
                Assert.InRange(Math.Sqrt((sample.A * sample.A) + (sample.B * sample.B)), 0.99, 1.01);
            });
        }

        [Fact]
        public void ReducingMaximumTimestepImprovesTenPeriodTrajectoryError()
        {
            double stopTime = 20.0 * Math.PI;
            double coarseError = GetFinalTrajectoryError(RunProbe(0.2, stopTime), stopTime);
            double mediumError = GetFinalTrajectoryError(RunProbe(0.1, stopTime), stopTime);
            double fineError = GetFinalTrajectoryError(RunProbe(0.05, stopTime), stopTime);

            Console.WriteLine(FormattableString.Invariant(
                $"Ten-period endpoint errors: h=0.2 {coarseError:R}, h=0.1 {mediumError:R}, h=0.05 {fineError:R}."));

            Assert.True(mediumError < coarseError, $"Expected {mediumError:R} < {coarseError:R}.");
            Assert.True(fineError < mediumError, $"Expected {fineError:R} < {mediumError:R}.");
        }

        [Fact]
        public void LinkedProbeBehaviorIsResolvedDuringSetup()
        {
            var target = new TransientApiProbe("target");
            var linked = new TransientApiProbe("linked", linkedProbeName: "target");
            var simulation = new Transient("tran", 0.05, 0.1);

            foreach (int ignored in simulation.Run(new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                target,
                linked)))
            {
            }

            var targetBehavior = simulation.EntityBehaviors["target"].GetValue<ITransientApiProbeBehavior>();
            var linkedBehavior = simulation.EntityBehaviors["linked"].GetValue<ITransientApiProbeBehavior>();

            Assert.Same(targetBehavior, linkedBehavior.LinkedBehavior);
        }

        [Fact]
        public void ImplementationSourceDoesNotUsePrivateReflection()
        {
            string sourceRoot = FindSourceRoot();
            string implementationRoot = Path.Combine(sourceRoot, "SpiceSharpMechanical2D");
            var files = Directory
                .EnumerateFiles(implementationRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .ToArray();
            string source = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

            Assert.NotEmpty(files);
            Assert.DoesNotContain("System.Reflection", source, StringComparison.Ordinal);
            Assert.DoesNotContain("BindingFlags", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GetField(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GetMethod(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("NonPublic", source, StringComparison.Ordinal);
        }

        private static ProbeRun RunProbe(
            double maximumTimestep,
            double stopTime,
            double initialA = 1.0,
            double initialB = 0.0)
        {
            var probe = new TransientApiProbe("probe", initialA, initialB);
            var method = new Trapezoidal
            {
                InitialStep = maximumTimestep,
                MaxStep = maximumTimestep,
                StopTime = stopTime,
            };
            var simulation = new Transient("tran", method);
            var aExport = new RealPropertyExport(simulation, "probe", "a");
            var bExport = new RealPropertyExport(simulation, "probe", "b");
            var samples = new List<ProbeSample>();

            foreach (int exportType in simulation.Run(new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                probe)))
            {
                if (exportType == Transient.ExportOperatingPoint || exportType == Transient.ExportTransient)
                {
                    samples.Add(new ProbeSample(exportType, simulation.Time, aExport.Value, bExport.Value));
                }
            }

            return new ProbeRun(simulation, aExport, bExport, samples.ToArray());
        }

        private static double GetFinalTrajectoryError(ProbeRun result, double expectedTime)
        {
            ProbeSample final = result.Samples.Last(sample => sample.ExportType == Transient.ExportTransient);
            Assert.Equal(expectedTime, final.Time, 12);

            double expectedA = Math.Cos(expectedTime);
            double expectedB = -Math.Sin(expectedTime);
            double deltaA = final.A - expectedA;
            double deltaB = final.B - expectedB;
            return Math.Sqrt((deltaA * deltaA) + (deltaB * deltaB));
        }

        private static string FindSourceRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "SpiceSharp-Parser.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not locate the repository source directory.");
        }

        private sealed class ProbeRun
        {
            public ProbeRun(
                Transient simulation,
                RealPropertyExport aExport,
                RealPropertyExport bExport,
                ProbeSample[] samples)
            {
                Simulation = simulation;
                AExport = aExport;
                BExport = bExport;
                Samples = samples;
            }

            public Transient Simulation { get; }

            public RealPropertyExport AExport { get; }

            public RealPropertyExport BExport { get; }

            public ProbeSample[] Samples { get; }
        }

        private readonly struct ProbeSample
        {
            public ProbeSample(int exportType, double time, double a, double b)
            {
                ExportType = exportType;
                Time = time;
                A = a;
                B = b;
            }

            public int ExportType { get; }

            public double Time { get; }

            public double A { get; }

            public double B { get; }
        }
    }
}
