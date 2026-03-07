using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.AnalogBehavioralModeling
{
    public class LaplaceTests : BaseTests
    {
        [Fact]
        public void When_ESourceLaplace_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Laplace E-source test",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6.28e3)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(netlist);
            if (netlist.ValidationResult.HasError)
            {
                var errors = string.Join("; ", netlist.ValidationResult.Errors.Select(e =>
                    e.Message + (e.Exception != null ? $" [{e.Exception.GetType().Name}: {e.Exception.Message}]" : "")));
                Assert.Fail($"Validation errors: {errors}");
            }

            RunSimulations(netlist);
        }

        [Fact]
        public void When_GSourceLaplace_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Laplace G-source test",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "G1 out 0 LAPLACE {V(in)} = {s/(1+s/1e6)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(netlist);
            if (netlist.ValidationResult.HasError)
            {
                var errors = string.Join("; ", netlist.ValidationResult.Errors.Select(e =>
                    e.Message + (e.Exception != null ? $" [{e.Exception.GetType().Name}: {e.Exception.Message}]" : "")));
                Assert.Fail($"Validation errors: {errors}");
            }

            RunSimulations(netlist);
        }

        [Fact]
        public void When_ESourceLaplaceLowerCase_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Laplace case insensitive test",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 laplace {V(in)} = {1/(1+s/6.28e3)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(netlist);
            if (netlist.ValidationResult.HasError)
            {
                var errors = string.Join("; ", netlist.ValidationResult.Errors.Select(e =>
                    e.Message + (e.Exception != null ? $" [{e.Exception.GetType().Name}: {e.Exception.Message}]" : "")));
                Assert.Fail($"Validation errors: {errors}");
            }

            RunSimulations(netlist);
        }

        [Fact]
        public void When_ESourceLaplaceSecondOrder_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Laplace second-order filter test",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/1e3+s*s/1e6)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(netlist);
            if (netlist.ValidationResult.HasError)
            {
                var errors = string.Join("; ", netlist.ValidationResult.Errors.Select(e =>
                    e.Message + (e.Exception != null ? $" [{e.Exception.GetType().Name}: {e.Exception.Message}]" : "")));
                Assert.Fail($"Validation errors: {errors}");
            }

            RunSimulations(netlist);
        }

        [Fact]
        public void When_ESourceLaplaceParsing_Expect_CorrectParameters()
        {
            var model = ParseNetlist(true, true,
                "Laplace parsing test",
                "E1 out 0 LAPLACE {V(in)} = {1/s}",
                ".END");

            Assert.NotNull(model);
            var component = model.Statements.OfType<SpiceSharpParser.Models.Netlist.Spice.Objects.Component>().First();

            // Verify LAPLACE keyword and LaplaceParameter are parsed
            var laplaceWord = component.PinsAndParameters.FirstOrDefault(p => p is WordParameter wp && wp.Value.ToLower() == "laplace");
            Assert.NotNull(laplaceWord);

            var laplaceParam = component.PinsAndParameters.FirstOrDefault(p => p is LaplaceParameter);
            Assert.NotNull(laplaceParam);

            var lp = (LaplaceParameter)laplaceParam;
            Assert.Equal("V(in)", lp.InputExpression);
            Assert.Equal("1/s", lp.TransferFunction);
        }

        [Fact]
        public void When_ESourceLowPass_Expect_UnityDCGain()
        {
            // H(s) = 1/(1+s/w0) with w0 = 2*pi*1000
            // At DC (very low freq), |H| should be 1.0
            var netlist = GetSpiceSharpModel(
                "Laplace low-pass DC gain",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            // At 1 Hz (essentially DC), gain should be ~1.0
            var dcPoint = acData.First(p => Math.Abs(p.Frequency - 1.0) < 0.5);
            Assert.True(Math.Abs(dcPoint.Magnitude - 1.0) < 0.01,
                $"DC gain expected ~1.0, got {dcPoint.Magnitude}");
        }

        [Fact]
        public void When_ESourceLowPass_Expect_Minus3dBAtCutoff()
        {
            // H(s) = 1/(1+s/w0) with w0 = 2*pi*1000 (cutoff at 1 kHz)
            // At cutoff, |H| = 1/sqrt(2) ≈ 0.7071
            var netlist = GetSpiceSharpModel(
                "Laplace low-pass -3dB",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 100 100 10000",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            // Find the point closest to 1 kHz
            var cutoffPoint = acData.OrderBy(p => Math.Abs(p.Frequency - 1000.0)).First();
            double expected = 1.0 / Math.Sqrt(2.0); // 0.7071
            Assert.True(Math.Abs(cutoffPoint.Magnitude - expected) < 0.02,
                $"At cutoff (~{cutoffPoint.Frequency:F1} Hz), expected magnitude ~{expected:F4}, got {cutoffPoint.Magnitude:F4}");
        }

        [Fact]
        public void When_ESourceLowPass_Expect_RolloffAtHighFreq()
        {
            // H(s) = 1/(1+s/w0) with w0 = 2*pi*1000
            // At 10 kHz (10x cutoff), |H| ≈ 1/sqrt(1+100) ≈ 0.0995
            var netlist = GetSpiceSharpModel(
                "Laplace low-pass rolloff",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 100 100 100000",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            // At 10 kHz: |H(j*2*pi*10000)| = 1/sqrt(1+(10000/1000)^2) = 1/sqrt(101) ≈ 0.0995
            var highFreqPoint = acData.OrderBy(p => Math.Abs(p.Frequency - 10000.0)).First();
            double expected = 1.0 / Math.Sqrt(1.0 + Math.Pow(10000.0 / 1000.0, 2));
            Assert.True(Math.Abs(highFreqPoint.Magnitude - expected) < 0.01,
                $"At ~{highFreqPoint.Frequency:F1} Hz, expected magnitude ~{expected:F4}, got {highFreqPoint.Magnitude:F4}");
        }

        [Fact]
        public void When_ESourceUnityGain_Expect_FlatResponse()
        {
            // H(s) = 1 (constant gain) — output should equal input at all frequencies
            var netlist = GetSpiceSharpModel(
                "Laplace unity gain",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            // All points should have magnitude 1.0
            foreach (var point in acData)
            {
                Assert.True(Math.Abs(point.Magnitude - 1.0) < 0.01,
                    $"At {point.Frequency:F1} Hz, expected magnitude 1.0, got {point.Magnitude:F4}");
            }
        }

        [Fact]
        public void When_ESourceGainOf5_Expect_ConstantGain()
        {
            // H(s) = 5 — output magnitude should be 5x input at all frequencies
            var netlist = GetSpiceSharpModel(
                "Laplace constant gain",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {5}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            foreach (var point in acData)
            {
                Assert.True(Math.Abs(point.Magnitude - 5.0) < 0.05,
                    $"At {point.Frequency:F1} Hz, expected magnitude 5.0, got {point.Magnitude:F4}");
            }
        }

        [Fact]
        public void When_GSourceLowPass_Expect_CorrectGain()
        {
            // G-source: transconductance. G1 out 0 LAPLACE {V(in)} = {0.001/(1+s/6283.185)}
            // With R2=1k load, V(out) = G * V(in) * R2 = 0.001 * 1 * 1000 = 1.0 at DC
            var netlist = GetSpiceSharpModel(
                "Laplace G-source gain",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "G1 out 0 LAPLACE {V(in)} = {0.001/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);

            // At DC: G(0) = 0.001, V(out) = 0.001 * 1V * 1kΩ = 1.0V
            var dcPoint = acData.First(p => Math.Abs(p.Frequency - 1.0) < 0.5);
            Assert.True(Math.Abs(dcPoint.Magnitude - 1.0) < 0.02,
                $"G-source DC gain: expected ~1.0V, got {dcPoint.Magnitude:F4}V");

            // At cutoff (1 kHz): should be ~0.707V
            var cutoffPoint = acData.OrderBy(p => Math.Abs(p.Frequency - 1000.0)).First();
            double expected = 1.0 / Math.Sqrt(2.0);
            Assert.True(Math.Abs(cutoffPoint.Magnitude - expected) < 0.02,
                $"G-source at cutoff: expected ~{expected:F4}V, got {cutoffPoint.Magnitude:F4}V");
        }

        [Fact]
        public void When_ESourceLowPass_Expect_MonotonicallyDecreasing()
        {
            // A low-pass filter magnitude should decrease with frequency
            var netlist = GetSpiceSharpModel(
                "Laplace monotonic decrease",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 20 10 100000",
                ".SAVE VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var acData = RunACSimulationAndGetMagnitude(netlist);
            var sorted = acData.OrderBy(p => p.Frequency).ToList();

            for (int i = 1; i < sorted.Count; i++)
            {
                Assert.True(sorted[i].Magnitude <= sorted[i - 1].Magnitude + 1e-6,
                    $"Magnitude should be monotonically decreasing: at {sorted[i].Frequency:F1} Hz got {sorted[i].Magnitude:F6} > {sorted[i - 1].Magnitude:F6} at {sorted[i - 1].Frequency:F1} Hz");
            }
        }

        [Fact]
        public void When_ESourceLowPassMeas_Expect_CorrectMaxGain()
        {
            // Use .MEAS AC to verify max gain is ~1.0 for low-pass filter
            var netlist = GetSpiceSharpModel(
                "Laplace MEAS max gain",
                "V1 in 0 AC 1",
                "R1 in 0 1k",
                "E1 out 0 LAPLACE {V(in)} = {1/(1+s/6283.185)}",
                "R2 out 0 1k",
                ".AC DEC 10 1 1e6",
                ".MEAS AC max_gain MAX VM(out)",
                ".MEAS AC min_gain MIN VM(out)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            RunSimulations(netlist);

            // Max gain should be ~1.0 (at DC)
            AssertMeasurementSuccess(netlist, "max_gain");
            Assert.True(Math.Abs(netlist.Measurements["max_gain"][0].Value - 1.0) < 0.01,
                $"Max gain expected ~1.0, got {netlist.Measurements["max_gain"][0].Value}");

            // Min gain should be very small (at high freq)
            AssertMeasurementSuccess(netlist, "min_gain");
            Assert.True(netlist.Measurements["min_gain"][0].Value < 0.01,
                $"Min gain expected <0.01, got {netlist.Measurements["min_gain"][0].Value}");
        }

        private static List<(double Frequency, double Magnitude)> RunACSimulationAndGetMagnitude(
            SpiceSharpParser.ModelReaders.Netlist.Spice.SpiceSharpModel model)
        {
            var result = new List<(double Frequency, double Magnitude)>();
            var simulation = model.Simulations.First(s => s is AC);
            var ac = (AC)simulation;
            var export = model.Exports.First(e => e.Simulation is AC);

            simulation.EventExportData += (sender, e) =>
            {
                double val = export.Extract();
                result.Add((ac.Frequency, val));
            };

            var codes = ac.Run(model.Circuit, -1);
            codes = simulation.InvokeEvents(codes);
            codes.ToArray();

            return result;
        }
    }
}
