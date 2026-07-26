using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class LTspiceADeviceExampleTests
    {
        [Fact]
        public void ClockDividerExample_DividesByFour()
        {
            SpiceCompilationResult result = CompileAndRun("clock-divider.cir");

            Assert.InRange(Measurement(result, "output_period"), 39.9e-6, 40.1e-6);
            Assert.InRange(Measurement(result, "output_low"), -0.01, 0.05);
        }

        [Fact]
        public void DFlipFlopExample_CapturesDataOnRisingEdges()
        {
            SpiceCompilationResult result = CompileAndRun("edge-capture-dff.cir");

            Assert.InRange(Measurement(result, "q_after_first_edge"), 4.99, 5.01);
            Assert.InRange(Measurement(result, "q_after_second_edge"), -0.01, 0.01);
        }

        [Fact]
        public void SampleAndHoldExample_RetainsSensorSample()
        {
            SpiceCompilationResult result = CompileAndRun("sample-and-hold-sensor.cir");

            Assert.InRange(Measurement(result, "held_early"), 0.99, 1.01);
            Assert.InRange(Measurement(result, "held_late"), 0.99, 1.01);
            Assert.InRange(Measurement(result, "sensor_late"), 3.99, 4.01);
        }

        [Fact]
        public void OtaCurrentLimiterExample_UsesIoutForBothLimits()
        {
            SpiceCompilationResult result = CompileAndRun("ota-current-limiter.cir");

            Assert.InRange(Measurement(result, "source_current"), 99e-6, 101e-6);
            Assert.InRange(Measurement(result, "sink_current"), -101e-6, -99e-6);
        }

        [Fact]
        public void PhaseDetectorExample_ReportsAndClearsReferenceLead()
        {
            SpiceCompilationResult result = CompileAndRun("phase-detector.cir");

            Assert.InRange(Measurement(result, "reference_leads"), 0.99, 1.01);
            Assert.InRange(Measurement(result, "edges_matched"), -0.01, 0.01);
        }

        [Fact]
        public void VaristorExample_ClampsBothPolarities()
        {
            SpiceCompilationResult result = CompileAndRun("controlled-clamp-varistor.cir");

            Assert.InRange(Measurement(result, "positive_clamp"), 2.07, 2.09);
            Assert.InRange(Measurement(result, "negative_clamp"), -2.09, -2.07);
        }

        [Fact]
        public void ModulatorExample_SwitchesFrequencyAndDefaultsUnusedAmplitude()
        {
            SpiceCompilationResult result = CompileAndRun("fm-modulator.cir");

            Assert.InRange(Measurement(result, "space_period"), 0.99e-3, 1.01e-3);
            Assert.InRange(Measurement(result, "mark_period"), 0.49e-3, 0.51e-3);
            Assert.InRange(Measurement(result, "default_amplitude"), 0.99, 1.01);
        }

        [Fact]
        public void NonGroundCommonExample_UsesRelativeLevelsAndDetachedOutput()
        {
            SpiceCompilationResult result = CompileAndRun("non-ground-common-latch.cir");

            Assert.InRange(Measurement(result, "q_after_set"), 4.99, 5.01);
            Assert.InRange(Measurement(result, "q_after_reset"), -0.01, 0.01);
            Assert.InRange(Measurement(result, "absolute_q_after_set"), 5.99, 6.01);
        }

        [LTspiceADeviceCompatibilityGoldenTests.LtspiceFact]
        public void RunnableExamples_WhenRunInNativeLtspice_ProduceDocumentedMeasurements()
        {
            AssertNativeExample(
                "clock-divider.cir",
                ("output_period", 39.9e-6, 40.1e-6),
                ("output_low", -0.01, 0.05));
            AssertNativeExample(
                "edge-capture-dff.cir",
                ("q_after_first_edge", 4.99, 5.01),
                ("q_after_second_edge", -0.01, 0.01));
            AssertNativeExample(
                "sample-and-hold-sensor.cir",
                ("held_early", 0.99, 1.01),
                ("held_late", 0.99, 1.01),
                ("sensor_late", 3.99, 4.01));
            AssertNativeExample(
                "ota-current-limiter.cir",
                ("source_current", 99e-6, 101e-6),
                ("sink_current", -101e-6, -99e-6));
            AssertNativeExample(
                "phase-detector.cir",
                ("reference_leads", 0.99, 1.01),
                ("edges_matched", -0.01, 0.01));
            AssertNativeExample(
                "controlled-clamp-varistor.cir",
                ("positive_clamp", 2.07, 2.09),
                ("negative_clamp", -2.09, -2.07));
            AssertNativeExample(
                "fm-modulator.cir",
                ("space_period", 0.99e-3, 1.01e-3),
                ("mark_period", 0.49e-3, 0.51e-3),
                ("default_amplitude", 0.99, 1.01));
            AssertNativeExample(
                "non-ground-common-latch.cir",
                ("q_after_set", 4.99, 5.01),
                ("q_after_reset", -0.01, 0.01),
                ("absolute_q_after_set", 5.99, 6.01));
        }

        private static SpiceCompilationResult CompileAndRun(string fileName)
        {
            string path = FindRepositoryFile("circuits", "a-devices", fileName);
            var options = new SpiceCompileOptions
            {
                Dialect = SpiceDialect.LTspice,
                ConfigureReader = settings => settings.UseCustomComponents(),
            };
            SpiceCompilationResult result = SpiceCompiler.CompileFile(path, options);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.NotNull(result.Model);
            SpiceSimulationTestHelper.RunSimulations(result.Model);
            Assert.Single(result.Model.XyPlots);
            return result;
        }

        private static double Measurement(
            SpiceCompilationResult result,
            string measurementName)
        {
            return SpiceNetlistAssertions.AssertMeasurementSuccess(
                result.Model,
                measurementName).Value;
        }

        private static void AssertNativeExample(
            string fileName,
            params (string Name, double Minimum, double Maximum)[] expected)
        {
            string path = FindRepositoryFile("circuits", "a-devices", fileName);
            string[] measurementNames = expected.Select(item => item.Name).ToArray();
            IReadOnlyDictionary<string, double> measurements =
                LTspiceADeviceCompatibilityGoldenTests.RunNativeExampleMeasurements(
                    "example_" + Path.GetFileNameWithoutExtension(fileName),
                    File.ReadAllLines(path),
                    measurementNames);

            foreach (var item in expected)
            {
                Assert.InRange(
                    measurements[item.Name],
                    item.Minimum,
                    item.Maximum);
            }
        }

        private static string FindRepositoryFile(params string[] pathSegments)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(
                    directory.FullName,
                    Path.Combine(pathSegments));
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException(
                "Could not locate repository example.",
                Path.Combine(pathSegments));
        }
    }
}
