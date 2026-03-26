using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Analysis;
using Xunit;

namespace SpiceSharpParser.Tests.Analysis
{
    public class WaveformAnalyzerTests
    {
        [Fact]
        public void Average_SineWave_ReturnsOffset()
        {
            // Full cycle of sine with DC offset of 2
            var data = new List<(double Time, double Value)>();
            for (int i = 0; i <= 1000; i++)
            {
                double t = i / 1000.0;
                data.Add((t, 2.0 + Math.Sin(2 * Math.PI * t)));
            }

            double avg = WaveformAnalyzer.Average(data);
            Assert.InRange(avg, 1.99, 2.01);
        }

        [Fact]
        public void RMS_SineWave_ReturnsExpected()
        {
            // RMS of sin(t) = amplitude / sqrt(2)
            double amplitude = 3.0;
            var data = new List<(double Time, double Value)>();
            for (int i = 0; i <= 10000; i++)
            {
                double t = i / 10000.0;
                data.Add((t, amplitude * Math.Sin(2 * Math.PI * t)));
            }

            double rms = WaveformAnalyzer.RMS(data);
            double expected = amplitude / Math.Sqrt(2);
            Assert.InRange(rms, expected * 0.99, expected * 1.01);
        }

        [Fact]
        public void PeakToPeak_ReturnsCorrectRange()
        {
            var data = new List<(double Time, double Value)>
            {
                (0, 1), (1, 5), (2, -3), (3, 2),
            };

            double pp = WaveformAnalyzer.PeakToPeak(data);
            Assert.Equal(8, pp, 6);
        }

        [Fact]
        public void PeakToPeak_WithWindow_FiltersCorrectly()
        {
            var data = new List<(double Time, double Value)>
            {
                (0, 100), (1, 1), (2, 5), (3, 2), (4, -100),
            };

            double pp = WaveformAnalyzer.PeakToPeak(data, fromTime: 0.5, toTime: 3.5);
            Assert.Equal(4, pp, 6); // 5 - 1
        }

        [Fact]
        public void RiseTime_StepResponse_ReturnsCorrectTime()
        {
            // Simulate a step response: 0 to 1 with a linear ramp from t=1 to t=3
            var data = new List<(double Time, double Value)>();
            for (int i = 0; i <= 100; i++)
            {
                double t = i * 0.05; // 0 to 5
                double v = t < 1.0 ? 0.0 : (t > 3.0 ? 1.0 : (t - 1.0) / 2.0);
                data.Add((t, v));
            }

            double rt = WaveformAnalyzer.RiseTime(data);
            // 10% of range = 0.1, 90% = 0.9
            // Time at 0.1 = 1.0 + 0.1*2 = 1.2
            // Time at 0.9 = 1.0 + 0.9*2 = 2.8
            // Rise time = 2.8 - 1.2 = 1.6
            Assert.InRange(rt, 1.5, 1.7);
        }

        [Fact]
        public void SettlingTime_ReturnsCorrectTime()
        {
            // Step response that overshoots then settles at 1.0
            var data = new List<(double Time, double Value)>
            {
                (0, 0), (1, 1.3), (2, 0.95), (3, 1.02), (4, 0.99), (5, 1.0),
            };

            double st = WaveformAnalyzer.SettlingTime(data, 1.0, 0.05); // ±5%
            Assert.InRange(st, 2, 4); // Should settle around t=2-3
        }

        [Fact]
        public void Overshoot_ReturnsCorrectPercentage()
        {
            var data = new List<(double Time, double Value)>
            {
                (0, 0), (1, 1.2), (2, 1.0),
            };

            double os = WaveformAnalyzer.Overshoot(data, 1.0);
            Assert.Equal(20.0, os, 6);
        }

        [Fact]
        public void InterpolateAt_LinearData_ReturnsCorrectValue()
        {
            var data = new List<(double X, double Y)>
            {
                (0, 0), (1, 10), (2, 20),
            };

            Assert.Equal(5, WaveformAnalyzer.InterpolateAt(data, 0.5), 6);
            Assert.Equal(15, WaveformAnalyzer.InterpolateAt(data, 1.5), 6);
        }

        [Fact]
        public void FindCrossing_ReturnsCorrectX()
        {
            var data = new List<(double X, double Y)>
            {
                (0, 0), (1, 2), (2, 4), (3, 2), (4, 0),
            };

            double crossing = WaveformAnalyzer.FindCrossing(data, 3.0, occurrence: 1);
            Assert.InRange(crossing, 1.4, 1.6); // 3.0 is between (1,2) and (2,4)
        }

        [Fact]
        public void BandwidthFrom3dBPoints_LowPassFilter_ReturnsCorrectBW()
        {
            // Simulate a low-pass filter response: flat at 0dB, rolling off at 1kHz
            var data = new List<(double Freq, double GainDb)>();
            for (double f = 1; f <= 100000; f *= 1.1)
            {
                double gainDb = -10 * Math.Log10(1 + Math.Pow(f / 1000, 2));
                data.Add((f, gainDb));
            }

            double bw = WaveformAnalyzer.BandwidthFrom3dBPoints(data);
            Assert.InRange(bw, 900, 1100); // Should be ~1kHz
        }

        [Fact]
        public void FFT_PureSine_HasSinglePeak()
        {
            // Generate a 100Hz sine sampled at 1kHz for 1 second
            var data = new List<(double Time, double Value)>();
            int samples = 1024;
            double sampleRate = 1000.0;
            for (int i = 0; i < samples; i++)
            {
                double t = i / sampleRate;
                data.Add((t, Math.Sin(2 * Math.PI * 100 * t)));
            }

            var spectrum = WaveformAnalyzer.FFT(data);
            Assert.True(spectrum.Count > 0);

            // Find peak (excluding DC)
            var peak = spectrum.Skip(1).OrderByDescending(s => s.Magnitude).First();
            Assert.InRange(peak.Frequency, 95, 105); // Should be near 100Hz
        }

        [Fact]
        public void THD_PureSine_NearZero()
        {
            var data = new List<(double Time, double Value)>();
            int samples = 4096;
            double sampleRate = 10000.0;
            for (int i = 0; i < samples; i++)
            {
                double t = i / sampleRate;
                data.Add((t, Math.Sin(2 * Math.PI * 100 * t)));
            }

            double thd = WaveformAnalyzer.THD(data, 100);
            Assert.InRange(thd, 0, 1.0); // Should be near 0% for pure sine
        }
    }
}
