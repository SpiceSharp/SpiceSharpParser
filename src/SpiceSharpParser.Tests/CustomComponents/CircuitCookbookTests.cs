using System;
using System.Collections.Generic;
using System.IO;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.Testing;
using Xunit;
using Xunit.Abstractions;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class CircuitCookbookTests
    {
        private readonly ITestOutputHelper output;

        public CircuitCookbookTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        public static IEnumerable<object[]> Cases
        {
            get
            {
                yield return CookbookCase(
                    "pure-spice",
                    "rectifier-power-supply",
                    "rectifier-power-supply.cir",
                    false,
                    Expected("reservoir_light_avg", 14.0, 17.0),
                    Expected("output_light_avg", 8.7, 9.5),
                    Expected("output_loaded_avg", 8.5, 9.5),
                    Expected("reservoir_loaded_ripple", 0.1, 1.5),
                    Expected("output_loaded_ripple", 0.0, 0.5),
                    Expected("load_regulation", -0.1, 0.5));

                yield return CookbookCase(
                    "pure-spice",
                    "pwm-dac",
                    "pwm-dac.cir",
                    false,
                    Expected("output_average", 1.95, 2.05),
                    Expected("first_pole_ripple", 0.01, 0.2),
                    Expected("output_ripple", 0.0, 0.01),
                    Expected("settling_time", 3e-3, 10e-3));

                yield return CookbookCaseWithFourier(
                    "pure-spice",
                    "bjt-audio-preamplifier",
                    "bjt-audio-preamplifier.cir",
                    false,
                    new FourierExpectation(0.0, 0.5),
                    Expected("collector_bias", 4.5, 6.5),
                    Expected("base_bias", 1.9, 2.3),
                    Expected("emitter_bias", 1.2, 1.7),
                    Expected("gain_20hz", 11.0, 16.0),
                    Expected("gain_1khz", 14.0, 20.0),
                    Expected("gain_20khz", 14.0, 20.0),
                    Expected("gain_1mhz", 13.0, 20.0),
                    Expected("lower_3db", 10.0, 20.0),
                    Expected("upper_3db", 2e6, 8e6),
                    Expected("output_pp", 0.55, 0.80),
                    Expected("output_average", -0.010, 0.010));

                yield return CookbookCase(
                    "custom-components",
                    "ideal-diode-power-or",
                    "ideal-diode-power-or.cir",
                    true,
                    Expected("primary_output_avg", 11.75, 11.9),
                    Expected("backup_output_avg", 11.2, 11.4),
                    Expected("failover_min", 11.2, 11.4),
                    Expected("recovered_output_avg", 11.75, 11.9),
                    Expected("backup_standby_current", -10e-9, 10e-9),
                    Expected("backup_active_current", 0.9, 0.98),
                    Expected("primary_reverse_current", -100e-9, 0.0));

                yield return CookbookCase(
                    "custom-components",
                    "simple-pll",
                    "simple-pll.cir",
                    true,
                    Expected("reference_period", 665e-6, 669e-6),
                    Expected("vco_period", 650e-6, 685e-6),
                    Expected("control_average", 0.45, 0.55),
                    Expected("control_ripple", 0.0, 0.08),
                    Expected("vco_amplitude", 1.95, 2.05));
            }
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void CookbookCircuit_CompilesRunsPlotsAndMeetsDocumentedMeasurements(
            string track,
            string directory,
            string fileName,
            bool useCustomComponents,
            MeasurementExpectation[] expectations,
            FourierExpectation fourierExpectation)
        {
            string path = FindRepositoryFile(
                "circuits",
                "cookbook",
                track,
                directory,
                fileName);
            var options = new SpiceCompileOptions();
            if (useCustomComponents)
            {
                options.Dialect = SpiceDialect.LTspice;
                options.ConfigureReader = settings => settings.UseCustomComponents();
            }

            SpiceCompilationResult result = SpiceCompiler.CompileFile(path, options);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.NotNull(result.Model);
            SpiceSimulationTestHelper.RunSimulations(result.Model);
            Assert.Single(result.Model.XyPlots);

            var failures = new List<string>();
            foreach (MeasurementExpectation expectation in expectations)
            {
                double actual = SpiceNetlistAssertions
                    .AssertMeasurementSuccess(result.Model, expectation.Name)
                    .Value;
                this.output.WriteLine(
                    "{0}/{1}: {2}={3:R}",
                    track,
                    directory,
                    expectation.Name,
                    actual);
                if (actual < expectation.Minimum || actual > expectation.Maximum)
                {
                    failures.Add(string.Format(
                        "{0}={1:R} is outside [{2:R}, {3:R}]",
                        expectation.Name,
                        actual,
                        expectation.Minimum,
                        expectation.Maximum));
                }
            }

            if (fourierExpectation == null)
            {
                Assert.Empty(result.Model.FourierAnalyses);
            }
            else
            {
                var fourier =
                    SpiceNetlistAssertions.AssertSingleSuccessfulFourierResult(result.Model);
                double actualThd = fourier.TotalHarmonicDistortionPercent;
                this.output.WriteLine(
                    "{0}/{1}: fourier_thd_percent={2:R}",
                    track,
                    directory,
                    actualThd);
                if (actualThd < fourierExpectation.MinimumThdPercent ||
                    actualThd > fourierExpectation.MaximumThdPercent)
                {
                    failures.Add(string.Format(
                        "fourier_thd_percent={0:R} is outside [{1:R}, {2:R}]",
                        actualThd,
                        fourierExpectation.MinimumThdPercent,
                        fourierExpectation.MaximumThdPercent));
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private static object[] CookbookCase(
            string track,
            string directory,
            string fileName,
            bool useCustomComponents,
            params MeasurementExpectation[] expectations)
        {
            return new object[]
            {
                track,
                directory,
                fileName,
                useCustomComponents,
                expectations,
                null,
            };
        }

        private static object[] CookbookCaseWithFourier(
            string track,
            string directory,
            string fileName,
            bool useCustomComponents,
            FourierExpectation fourierExpectation,
            params MeasurementExpectation[] expectations)
        {
            return new object[]
            {
                track,
                directory,
                fileName,
                useCustomComponents,
                expectations,
                fourierExpectation,
            };
        }

        private static MeasurementExpectation Expected(
            string name,
            double minimum,
            double maximum)
        {
            return new MeasurementExpectation(name, minimum, maximum);
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
                "Could not locate repository cookbook circuit.",
                Path.Combine(pathSegments));
        }

        public sealed class MeasurementExpectation
        {
            public MeasurementExpectation(string name, double minimum, double maximum)
            {
                this.Name = name;
                this.Minimum = minimum;
                this.Maximum = maximum;
            }

            public string Name { get; }

            public double Minimum { get; }

            public double Maximum { get; }
        }

        public sealed class FourierExpectation
        {
            public FourierExpectation(double minimumThdPercent, double maximumThdPercent)
            {
                this.MinimumThdPercent = minimumThdPercent;
                this.MaximumThdPercent = maximumThdPercent;
            }

            public double MinimumThdPercent { get; }

            public double MaximumThdPercent { get; }
        }
    }
}
