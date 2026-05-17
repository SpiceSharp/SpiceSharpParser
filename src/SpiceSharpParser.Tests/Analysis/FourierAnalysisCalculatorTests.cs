using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier;
using Xunit;

namespace SpiceSharpParser.Tests.Analysis
{
    public class FourierAnalysisCalculatorTests
    {
        private readonly FourierAnalysisCalculator calculator = new FourierAnalysisCalculator();

        [Fact]
        public void Analyze_PureSine_ReturnsFundamentalAndLowThd()
        {
            var result = this.calculator.Analyze(
                "V(OUT)",
                "tran",
                1000.0,
                GenerateSamples(t => Math.Sin(2.0 * Math.PI * 1000.0 * t)));

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.999, 1.001);
            Assert.InRange(Math.Abs(result.TotalHarmonicDistortionPercent), 0.0, 0.001);
        }

        [Fact]
        public void Analyze_SecondHarmonic_ReturnsExpectedThd()
        {
            var result = this.calculator.Analyze(
                "V(OUT)",
                "tran",
                1000.0,
                GenerateSamples(t =>
                    Math.Sin(2.0 * Math.PI * 1000.0 * t)
                    + (0.1 * Math.Sin(2.0 * Math.PI * 2000.0 * t))));

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.999, 1.001);
            Assert.InRange(Harmonic(result, 2).Magnitude, 0.099, 0.101);
            Assert.InRange(Harmonic(result, 1).NormalizedMagnitude, 0.999, 1.001);
            Assert.InRange(Harmonic(result, 1).NormalizedMagnitudeDecibels, -0.001, 0.001);
            Assert.InRange(Harmonic(result, 2).NormalizedMagnitude, 0.099, 0.101);
            Assert.InRange(Harmonic(result, 2).NormalizedMagnitudeDecibels, -20.1, -19.9);
            Assert.InRange(result.TotalHarmonicDistortionPercent, 9.9, 10.1);
        }

        [Fact]
        public void Analyze_DcOffset_DoesNotCountDcInThd()
        {
            var result = this.calculator.Analyze(
                "V(OUT)",
                "tran",
                1000.0,
                GenerateSamples(t => 2.0 + Math.Sin(2.0 * Math.PI * 1000.0 * t)));

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 0).Magnitude, 1.999, 2.001);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.999, 1.001);
            Assert.InRange(Math.Abs(result.TotalHarmonicDistortionPercent), 0.0, 0.001);
        }

        [Fact]
        public void Analyze_SquareWave_ReturnsOddHarmonicRatios()
        {
            var result = this.calculator.Analyze(
                "V(OUT)",
                "tran",
                1000.0,
                GenerateSamples(t => Math.Sin(2.0 * Math.PI * 1000.0 * t) >= 0.0 ? 1.0 : -1.0, sampleCount: 20001));

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 2).NormalizedMagnitude, 0.0, 0.02);
            Assert.InRange(Harmonic(result, 3).NormalizedMagnitude, 0.31, 0.35);
            Assert.InRange(Harmonic(result, 5).NormalizedMagnitude, 0.18, 0.22);
            Assert.InRange(Harmonic(result, 7).NormalizedMagnitude, 0.12, 0.16);
        }

        [Fact]
        public void Analyze_PhaseUsesCosineReference()
        {
            var cosine = this.calculator.Analyze(
                "V(COS)",
                "tran",
                1000.0,
                GenerateSamples(t => Math.Cos(2.0 * Math.PI * 1000.0 * t)));

            var sine = this.calculator.Analyze(
                "V(SIN)",
                "tran",
                1000.0,
                GenerateSamples(t => Math.Sin(2.0 * Math.PI * 1000.0 * t)));

            Assert.True(cosine.Success);
            Assert.True(sine.Success);
            Assert.InRange(Math.Abs(NormalizePhase(cosine.Harmonics[1].PhaseDegrees)), 0.0, 0.001);
            Assert.InRange(NormalizePhase(sine.Harmonics[1].PhaseDegrees), -90.001, -89.999);
        }

        [Fact]
        public void Analyze_MissingFundamental_ReturnsUndefinedThdAndNormalization()
        {
            var result = this.calculator.Analyze(
                "V(OUT)",
                "tran",
                1000.0,
                GenerateSamples(t => Math.Sin(2.0 * Math.PI * 2000.0 * t)));

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.0, 1e-10);
            Assert.True(double.IsNaN(Harmonic(result, 1).PhaseDegrees));
            Assert.True(double.IsNaN(Harmonic(result, 2).NormalizedMagnitude));
            Assert.True(double.IsNaN(result.TotalHarmonicDistortionPercent));
        }

        [Fact]
        public void Analyze_NonUniformSamples_ResamplesFinalPeriod()
        {
            var samples = GenerateNonUniformSamples(t => Math.Sin(2.0 * Math.PI * 1000.0 * t));

            var result = this.calculator.Analyze("V(OUT)", "tran", 1000.0, samples);

            Assert.True(result.Success);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.99, 1.01);
            Assert.InRange(Math.Abs(result.TotalHarmonicDistortionPercent), 0.0, 0.1);
        }

        [Fact]
        public void Analyze_InsufficientFinalPeriod_ReturnsFailure()
        {
            var samples = new List<(double Time, double Value)>
            {
                (0.0095, 0.0),
                (0.0100, 1.0),
            };

            var result = this.calculator.Analyze("V(OUT)", "tran", 1000.0, samples);

            Assert.False(result.Success);
            Assert.Contains("complete final period", result.ErrorMessage);
        }

        private static FourierHarmonic Harmonic(FourierAnalysisResult result, int harmonic)
        {
            return result.Harmonics.Single(h => h.HarmonicNumber == harmonic);
        }

        private static double NormalizePhase(double phaseDegrees)
        {
            while (phaseDegrees <= -180.0)
            {
                phaseDegrees += 360.0;
            }

            while (phaseDegrees > 180.0)
            {
                phaseDegrees -= 360.0;
            }

            return phaseDegrees;
        }

        private static List<(double Time, double Value)> GenerateSamples(Func<double, double> value, int sampleCount = 10001)
        {
            var samples = new List<(double Time, double Value)>(sampleCount);
            double stopTime = 0.010;
            for (int i = 0; i < sampleCount; i++)
            {
                double time = stopTime * i / (sampleCount - 1);
                samples.Add((time, value(time)));
            }

            return samples;
        }

        private static List<(double Time, double Value)> GenerateNonUniformSamples(Func<double, double> value, int sampleCount = 2500)
        {
            var samples = new List<(double Time, double Value)>(sampleCount);
            double stopTime = 0.010;
            for (int i = 0; i < sampleCount; i++)
            {
                double ratio = (double)i / (sampleCount - 1);
                double time = stopTime * ratio * ratio;
                samples.Add((time, value(time)));
            }

            return samples;
        }
    }
}
