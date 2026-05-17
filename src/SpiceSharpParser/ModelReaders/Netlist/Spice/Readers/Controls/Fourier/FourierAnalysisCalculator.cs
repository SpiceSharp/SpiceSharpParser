using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier
{
    /// <summary>
    /// Computes exact-harmonic Fourier rows over the last complete transient period.
    /// </summary>
    public class FourierAnalysisCalculator
    {
        public const int DefaultHighestHarmonic = 9;

        private const double TimeTolerance = 1e-30;
        private const double MagnitudeTolerance = 1e-12;

        public FourierAnalysisResult Analyze(
            string signalName,
            string simulationName,
            double fundamentalFrequency,
            List<(double Time, double Value)> samples,
            int highestHarmonic = DefaultHighestHarmonic)
        {
            var result = new FourierAnalysisResult
            {
                SignalName = signalName,
                SimulationName = simulationName,
                FundamentalFrequency = fundamentalFrequency,
                TotalHarmonicDistortionPercent = double.NaN,
            };

            if (!IsFinite(fundamentalFrequency) || fundamentalFrequency <= 0)
            {
                return Fail(result, "fundamental frequency must be positive and finite");
            }

            if (samples == null || samples.Count < 2)
            {
                return Fail(result, "not enough transient samples");
            }

            var finiteSamples = samples
                .Where(sample => IsFinite(sample.Time) && IsFinite(sample.Value))
                .OrderBy(sample => sample.Time)
                .ToList();

            if (finiteSamples.Count < 2)
            {
                return Fail(result, "not enough finite transient samples");
            }

            double period = 1.0 / fundamentalFrequency;
            double endTime = finiteSamples[finiteSamples.Count - 1].Time;
            double startTime = endTime - period;

            if (startTime < finiteSamples[0].Time)
            {
                return Fail(result, "transient data does not contain one complete final period");
            }

            int originalPointsInsideWindow = finiteSamples.Count(sample => sample.Time >= startTime && sample.Time <= endTime);
            if (originalPointsInsideWindow < 2)
            {
                return Fail(result, "not enough samples in the final period");
            }

            int sampleCount = Math.Max(256, originalPointsInsideWindow);
            var resampled = Resample(finiteSamples, startTime, period, sampleCount);

            double dc = resampled.Sum(point => point.Value) / sampleCount;
            result.Harmonics.Add(new FourierHarmonic
            {
                HarmonicNumber = 0,
                Frequency = 0.0,
                Magnitude = dc,
                PhaseDegrees = double.NaN,
                NormalizedMagnitude = double.NaN,
                NormalizedMagnitudeDecibels = double.NaN,
            });

            for (int harmonic = 1; harmonic <= highestHarmonic; harmonic++)
            {
                double a = 0.0;
                double b = 0.0;

                foreach (var point in resampled)
                {
                    double angle = 2.0 * Math.PI * harmonic * fundamentalFrequency * point.Time;
                    a += point.Value * Math.Cos(angle);
                    b += point.Value * Math.Sin(angle);
                }

                a *= 2.0 / sampleCount;
                b *= 2.0 / sampleCount;

                double magnitude = Math.Sqrt((a * a) + (b * b));

                result.Harmonics.Add(new FourierHarmonic
                {
                    HarmonicNumber = harmonic,
                    Frequency = harmonic * fundamentalFrequency,
                    Magnitude = magnitude,
                    PhaseDegrees = magnitude > MagnitudeTolerance ? Math.Atan2(-b, a) * 180.0 / Math.PI : double.NaN,
                    NormalizedMagnitude = double.NaN,
                    NormalizedMagnitudeDecibels = double.NaN,
                });
            }

            ApplyNormalizationAndThd(result);
            result.Success = true;
            return result;
        }

        private static void ApplyNormalizationAndThd(FourierAnalysisResult result)
        {
            var fundamental = result.Harmonics.FirstOrDefault(h => h.HarmonicNumber == 1);
            if (fundamental == null || fundamental.Magnitude <= MagnitudeTolerance)
            {
                result.TotalHarmonicDistortionPercent = double.NaN;
                return;
            }

            foreach (var harmonic in result.Harmonics.Where(h => h.HarmonicNumber > 0))
            {
                harmonic.NormalizedMagnitude = harmonic.Magnitude > MagnitudeTolerance
                    ? harmonic.Magnitude / fundamental.Magnitude
                    : 0.0;
                harmonic.NormalizedMagnitudeDecibels = harmonic.NormalizedMagnitude > 0.0
                    ? 20.0 * Math.Log10(harmonic.NormalizedMagnitude)
                    : double.NegativeInfinity;
            }

            double distortionPower = result.Harmonics
                .Where(h => h.HarmonicNumber >= 2)
                .Sum(h => h.Magnitude * h.Magnitude);

            result.TotalHarmonicDistortionPercent = 100.0 * Math.Sqrt(distortionPower) / fundamental.Magnitude;
        }

        private static List<(double Time, double Value)> Resample(
            List<(double Time, double Value)> samples,
            double startTime,
            double period,
            int sampleCount)
        {
            var result = new List<(double Time, double Value)>(sampleCount);
            int index = 1;

            for (int i = 0; i < sampleCount; i++)
            {
                double time = startTime + (period * i / sampleCount);

                while (index < samples.Count - 1 && samples[index].Time < time)
                {
                    index++;
                }

                result.Add((time, Interpolate(samples, index, time)));
            }

            return result;
        }

        private static double Interpolate(List<(double Time, double Value)> samples, int index, double time)
        {
            if (time <= samples[0].Time)
            {
                return samples[0].Value;
            }

            if (time >= samples[samples.Count - 1].Time)
            {
                return samples[samples.Count - 1].Value;
            }

            var right = samples[index];
            var left = samples[index - 1];
            double dt = right.Time - left.Time;

            if (Math.Abs(dt) <= TimeTolerance)
            {
                return right.Value;
            }

            double ratio = (time - left.Time) / dt;
            return left.Value + ((right.Value - left.Value) * ratio);
        }

        private static FourierAnalysisResult Fail(FourierAnalysisResult result, string message)
        {
            result.Success = false;
            result.ErrorMessage = message;
            return result;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
