using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Analysis
{
    /// <summary>
    /// Provides post-simulation waveform analysis utilities for time-domain and frequency-domain data.
    /// All methods are static and work with generic (x, y) data point lists.
    /// </summary>
    public static class WaveformAnalyzer
    {
        // ────────────────────────────────────────────
        // Time-domain analysis
        // ────────────────────────────────────────────

        /// <summary>
        /// Computes the rise time between two percentage thresholds of the signal range.
        /// </summary>
        /// <param name="data">Time-domain data points (time, value).</param>
        /// <param name="lowPct">Lower threshold as fraction of range (default 0.1 = 10%).</param>
        /// <param name="highPct">Upper threshold as fraction of range (default 0.9 = 90%).</param>
        /// <returns>Rise time in the same units as the time axis, or NaN if not found.</returns>
        public static double RiseTime(List<(double Time, double Value)> data, double lowPct = 0.1, double highPct = 0.9)
        {
            if (data == null || data.Count < 2)
            {
                return double.NaN;
            }

            double min = data.Min(d => d.Value);
            double max = data.Max(d => d.Value);
            double range = max - min;

            if (range <= 0)
            {
                return double.NaN;
            }

            double lowThreshold = min + (lowPct * range);
            double highThreshold = min + (highPct * range);

            double tLow = FindFirstCrossing(data, lowThreshold, rising: true);
            double tHigh = FindFirstCrossing(data, highThreshold, rising: true, startAfter: tLow);

            if (double.IsNaN(tLow) || double.IsNaN(tHigh))
            {
                return double.NaN;
            }

            return tHigh - tLow;
        }

        /// <summary>
        /// Computes the fall time between two percentage thresholds.
        /// </summary>
        public static double FallTime(List<(double Time, double Value)> data, double highPct = 0.9, double lowPct = 0.1)
        {
            if (data == null || data.Count < 2)
            {
                return double.NaN;
            }

            double min = data.Min(d => d.Value);
            double max = data.Max(d => d.Value);
            double range = max - min;

            if (range <= 0)
            {
                return double.NaN;
            }

            double highThreshold = min + (highPct * range);
            double lowThreshold = min + (lowPct * range);

            double tHigh = FindFirstCrossing(data, highThreshold, rising: false);
            double tLow = FindFirstCrossing(data, lowThreshold, rising: false, startAfter: tHigh);

            if (double.IsNaN(tHigh) || double.IsNaN(tLow))
            {
                return double.NaN;
            }

            return tLow - tHigh;
        }

        /// <summary>
        /// Computes the settling time — the time after which the signal stays within
        /// a tolerance band around the final value.
        /// </summary>
        /// <param name="data">Time-domain data points.</param>
        /// <param name="finalValue">The expected steady-state value.</param>
        /// <param name="tolerancePct">Tolerance as fraction of final value (default 0.02 = 2%).</param>
        /// <returns>Settling time, or NaN if the signal never settles.</returns>
        public static double SettlingTime(List<(double Time, double Value)> data, double finalValue, double tolerancePct = 0.02)
        {
            if (data == null || data.Count < 2)
            {
                return double.NaN;
            }

            double band = Math.Abs(finalValue * tolerancePct);
            if (band == 0)
            {
                band = tolerancePct; // absolute tolerance if finalValue is 0
            }

            // Walk backwards from end to find last point outside the band
            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(data[i].Value - finalValue) > band)
                {
                    if (i < data.Count - 1)
                    {
                        return data[i + 1].Time;
                    }

                    return double.NaN; // never settles
                }
            }

            return data[0].Time; // already settled at start
        }

        /// <summary>
        /// Computes the percentage overshoot relative to a final value.
        /// </summary>
        public static double Overshoot(List<(double Time, double Value)> data, double finalValue)
        {
            if (data == null || data.Count == 0 || finalValue == 0)
            {
                return double.NaN;
            }

            double peak = data.Max(d => d.Value);
            if (peak <= finalValue)
            {
                return 0.0;
            }

            return ((peak - finalValue) / Math.Abs(finalValue)) * 100.0;
        }

        /// <summary>
        /// Computes peak-to-peak value within an optional time window.
        /// </summary>
        public static double PeakToPeak(List<(double Time, double Value)> data, double fromTime = double.MinValue, double toTime = double.MaxValue)
        {
            var windowed = GetWindow(data, fromTime, toTime);
            if (windowed.Count == 0)
            {
                return double.NaN;
            }

            return windowed.Max(d => d.Value) - windowed.Min(d => d.Value);
        }

        /// <summary>
        /// Computes the RMS value within an optional time window using trapezoidal integration.
        /// </summary>
        public static double RMS(List<(double Time, double Value)> data, double fromTime = double.MinValue, double toTime = double.MaxValue)
        {
            var windowed = GetWindow(data, fromTime, toTime);
            if (windowed.Count < 2)
            {
                return double.NaN;
            }

            double integral = 0;
            for (int i = 1; i < windowed.Count; i++)
            {
                double dt = windowed[i].Time - windowed[i - 1].Time;
                double v1sq = windowed[i - 1].Value * windowed[i - 1].Value;
                double v2sq = windowed[i].Value * windowed[i].Value;
                integral += (v1sq + v2sq) * 0.5 * dt;
            }

            double totalTime = windowed[windowed.Count - 1].Time - windowed[0].Time;
            if (totalTime <= 0)
            {
                return double.NaN;
            }

            return Math.Sqrt(integral / totalTime);
        }

        /// <summary>
        /// Computes the average value within an optional time window using trapezoidal integration.
        /// </summary>
        public static double Average(List<(double Time, double Value)> data, double fromTime = double.MinValue, double toTime = double.MaxValue)
        {
            var windowed = GetWindow(data, fromTime, toTime);
            if (windowed.Count < 2)
            {
                return double.NaN;
            }

            double integral = 0;
            for (int i = 1; i < windowed.Count; i++)
            {
                double dt = windowed[i].Time - windowed[i - 1].Time;
                integral += (windowed[i - 1].Value + windowed[i].Value) * 0.5 * dt;
            }

            double totalTime = windowed[windowed.Count - 1].Time - windowed[0].Time;
            if (totalTime <= 0)
            {
                return double.NaN;
            }

            return integral / totalTime;
        }

        /// <summary>
        /// Computes the DC offset (average value over the entire waveform).
        /// </summary>
        public static double DCOffset(List<(double Time, double Value)> data)
        {
            return Average(data);
        }

        // ────────────────────────────────────────────
        // Frequency-domain analysis
        // ────────────────────────────────────────────

        /// <summary>
        /// Computes a basic FFT magnitude spectrum from uniformly-sampled time-domain data.
        /// Returns (frequency, magnitude) pairs up to Nyquist.
        /// </summary>
        /// <param name="data">Uniformly-sampled time-domain data.</param>
        /// <returns>List of (frequency, magnitude) pairs.</returns>
        public static List<(double Frequency, double Magnitude)> FFT(List<(double Time, double Value)> data)
        {
            if (data == null || data.Count < 4)
            {
                return new List<(double Frequency, double Magnitude)>();
            }

            // Ensure power-of-2 length
            int n = 1;
            while (n < data.Count)
            {
                n <<= 1;
            }

            // Zero-pad to power of 2
            double[] real = new double[n];
            double[] imag = new double[n];
            for (int i = 0; i < data.Count; i++)
            {
                real[i] = data[i].Value;
            }

            // In-place Cooley-Tukey FFT
            CooleyTukeyFFT(real, imag, false);

            // Compute sample rate
            double totalTime = data[data.Count - 1].Time - data[0].Time;
            double sampleRate = (data.Count - 1) / totalTime;

            var result = new List<(double Frequency, double Magnitude)>();
            int nyquist = n / 2;

            for (int i = 0; i <= nyquist; i++)
            {
                double freq = i * sampleRate / n;
                double mag = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]) * 2.0 / data.Count;
                result.Add((freq, mag));
            }

            // DC component should not be doubled
            if (result.Count > 0)
            {
                result[0] = (result[0].Frequency, result[0].Magnitude / 2.0);
            }

            return result;
        }

        /// <summary>
        /// Computes Total Harmonic Distortion (THD) as a percentage.
        /// </summary>
        /// <param name="data">Uniformly-sampled time-domain data.</param>
        /// <param name="fundamentalFreq">The fundamental frequency to measure THD against.</param>
        /// <param name="numHarmonics">Number of harmonics to include (default 10).</param>
        /// <returns>THD as a percentage, or NaN if fundamental not found.</returns>
        public static double THD(List<(double Time, double Value)> data, double fundamentalFreq, int numHarmonics = 10)
        {
            var spectrum = FFT(data);
            if (spectrum.Count == 0)
            {
                return double.NaN;
            }

            // Find fundamental magnitude (nearest bin)
            double fundamentalMag = FindNearestBinMagnitude(spectrum, fundamentalFreq);
            if (fundamentalMag <= 0)
            {
                return double.NaN;
            }

            // Sum harmonic powers
            double harmonicPowerSum = 0;
            for (int h = 2; h <= numHarmonics + 1; h++)
            {
                double harmonicFreq = fundamentalFreq * h;
                double mag = FindNearestBinMagnitude(spectrum, harmonicFreq);
                harmonicPowerSum += mag * mag;
            }

            return Math.Sqrt(harmonicPowerSum) / fundamentalMag * 100.0;
        }

        /// <summary>
        /// Computes Signal-to-Noise Ratio in dB.
        /// </summary>
        public static double SNR(List<(double Time, double Value)> data, double signalFreq)
        {
            var spectrum = FFT(data);
            if (spectrum.Count == 0)
            {
                return double.NaN;
            }

            double signalMag = FindNearestBinMagnitude(spectrum, signalFreq);
            if (signalMag <= 0)
            {
                return double.NaN;
            }

            double totalPower = spectrum.Sum(s => s.Magnitude * s.Magnitude);
            double noisePower = totalPower - (signalMag * signalMag);

            if (noisePower <= 0)
            {
                return double.PositiveInfinity;
            }

            return 10 * Math.Log10((signalMag * signalMag) / noisePower);
        }

        // ────────────────────────────────────────────
        // AC response analysis
        // ────────────────────────────────────────────

        /// <summary>
        /// Computes the -3dB bandwidth from an AC gain response.
        /// </summary>
        /// <param name="data">AC frequency response data (frequency, gain in dB).</param>
        /// <returns>Bandwidth in Hz, or NaN if -3dB points not found.</returns>
        public static double BandwidthFrom3dBPoints(List<(double Freq, double GainDb)> data)
        {
            if (data == null || data.Count < 2)
            {
                return double.NaN;
            }

            double peakGain = data.Max(d => d.GainDb);
            double threshold = peakGain - 3.0;

            // Find lower -3dB point
            double fLow = double.NaN;
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i - 1].GainDb < threshold && data[i].GainDb >= threshold)
                {
                    fLow = LinearInterpolateX(data[i - 1].Freq, data[i - 1].GainDb, data[i].Freq, data[i].GainDb, threshold);
                    break;
                }
            }

            // Find upper -3dB point
            double fHigh = double.NaN;
            for (int i = data.Count - 2; i >= 0; i--)
            {
                if (data[i].GainDb >= threshold && data[i + 1].GainDb < threshold)
                {
                    fHigh = LinearInterpolateX(data[i].Freq, data[i].GainDb, data[i + 1].Freq, data[i + 1].GainDb, threshold);
                    break;
                }
            }

            if (double.IsNaN(fLow) && !double.IsNaN(fHigh))
            {
                return fHigh; // Low-pass: bandwidth = upper -3dB frequency
            }

            if (!double.IsNaN(fLow) && !double.IsNaN(fHigh))
            {
                return fHigh - fLow; // Band-pass: bandwidth = upper - lower
            }

            return double.NaN;
        }

        /// <summary>
        /// Computes gain and phase margins from AC loop gain data.
        /// </summary>
        /// <param name="gain">Loop gain data (frequency, gain in dB).</param>
        /// <param name="phase">Loop phase data (frequency, phase in degrees).</param>
        /// <returns>Gain margin (dB) and phase margin (degrees). Positive values indicate stability.</returns>
        public static (double GainMarginDb, double PhaseMarginDeg) StabilityMargins(
            List<(double Freq, double GainDb)> gain,
            List<(double Freq, double PhaseDeg)> phase)
        {
            if (gain == null || phase == null || gain.Count < 2 || phase.Count < 2)
            {
                return (double.NaN, double.NaN);
            }

            // Phase margin: phase at 0dB gain crossing + 180°
            double fUnity = FindCrossingInData(gain.Select(g => (g.Freq, g.GainDb)).ToList(), 0.0);
            double phaseMargin = double.NaN;
            if (!double.IsNaN(fUnity))
            {
                double phaseAtUnity = InterpolateAt(phase.Select(p => (p.Freq, p.PhaseDeg)).ToList(), fUnity);
                phaseMargin = phaseAtUnity + 180.0;
            }

            // Gain margin: -gain at -180° phase crossing
            double fPhase180 = FindCrossingInData(phase.Select(p => (p.Freq, p.PhaseDeg)).ToList(), -180.0);
            double gainMargin = double.NaN;
            if (!double.IsNaN(fPhase180))
            {
                double gainAtPhase180 = InterpolateAt(gain.Select(g => (g.Freq, g.GainDb)).ToList(), fPhase180);
                gainMargin = -gainAtPhase180;
            }

            return (gainMargin, phaseMargin);
        }

        /// <summary>
        /// Returns the gain at a specific frequency (interpolated).
        /// </summary>
        public static double GainAt(List<(double Freq, double GainDb)> data, double frequency)
        {
            return InterpolateAt(data.Select(d => (d.Freq, d.GainDb)).ToList(), frequency);
        }

        /// <summary>
        /// Returns the phase at a specific frequency (interpolated).
        /// </summary>
        public static double PhaseAt(List<(double Freq, double PhaseDeg)> data, double frequency)
        {
            return InterpolateAt(data.Select(d => (d.Freq, d.PhaseDeg)).ToList(), frequency);
        }

        /// <summary>
        /// Finds the frequency at which gain equals the specified value.
        /// </summary>
        public static double FrequencyAtGain(List<(double Freq, double GainDb)> data, double gainDb)
        {
            return FindCrossingInData(data.Select(d => (d.Freq, d.GainDb)).ToList(), gainDb);
        }

        // ────────────────────────────────────────────
        // General utilities
        // ────────────────────────────────────────────

        /// <summary>
        /// Interpolates the Y value at a given X point.
        /// </summary>
        public static double InterpolateAt(List<(double X, double Y)> data, double x)
        {
            if (data == null || data.Count == 0)
            {
                return double.NaN;
            }

            if (data.Count == 1)
            {
                return data[0].Y;
            }

            // Find bracketing points
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].X >= x)
                {
                    double x0 = data[i - 1].X;
                    double x1 = data[i].X;
                    double y0 = data[i - 1].Y;
                    double y1 = data[i].Y;

                    if (Math.Abs(x1 - x0) < double.Epsilon)
                    {
                        return y0;
                    }

                    return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
                }
            }

            // Extrapolate from last two points
            int n = data.Count;
            double xA = data[n - 2].X;
            double xB = data[n - 1].X;
            double yA = data[n - 2].Y;
            double yB = data[n - 1].Y;

            if (Math.Abs(xB - xA) < double.Epsilon)
            {
                return yB;
            }

            return yA + (yB - yA) * (x - xA) / (xB - xA);
        }

        /// <summary>
        /// Finds the X value where Y crosses a threshold for the Nth time.
        /// </summary>
        /// <param name="data">Data points (X, Y).</param>
        /// <param name="threshold">The Y threshold to find.</param>
        /// <param name="occurrence">Which occurrence to return (1-based, default 1).</param>
        /// <returns>The interpolated X value at the crossing, or NaN if not found.</returns>
        public static double FindCrossing(List<(double X, double Y)> data, double threshold, int occurrence = 1)
        {
            if (data == null || data.Count < 2 || occurrence < 1)
            {
                return double.NaN;
            }

            int count = 0;
            for (int i = 1; i < data.Count; i++)
            {
                bool crosses = (data[i - 1].Y < threshold && data[i].Y >= threshold)
                            || (data[i - 1].Y > threshold && data[i].Y <= threshold);

                if (crosses)
                {
                    count++;
                    if (count == occurrence)
                    {
                        return LinearInterpolateX(data[i - 1].X, data[i - 1].Y, data[i].X, data[i].Y, threshold);
                    }
                }
            }

            return double.NaN;
        }

        // ────────────────────────────────────────────
        // Private helpers
        // ────────────────────────────────────────────

        private static List<(double Time, double Value)> GetWindow(List<(double Time, double Value)> data, double from, double to)
        {
            if (data == null)
            {
                return new List<(double, double)>();
            }

            return data.Where(d => d.Time >= from && d.Time <= to).ToList();
        }

        private static double FindFirstCrossing(List<(double Time, double Value)> data, double threshold, bool rising, double startAfter = double.MinValue)
        {
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].Time < startAfter)
                {
                    continue;
                }

                if (rising && data[i - 1].Value < threshold && data[i].Value >= threshold)
                {
                    return LinearInterpolateX(data[i - 1].Time, data[i - 1].Value, data[i].Time, data[i].Value, threshold);
                }

                if (!rising && data[i - 1].Value > threshold && data[i].Value <= threshold)
                {
                    return LinearInterpolateX(data[i - 1].Time, data[i - 1].Value, data[i].Time, data[i].Value, threshold);
                }
            }

            return double.NaN;
        }

        private static double FindCrossingInData(List<(double X, double Y)> data, double threshold)
        {
            for (int i = 1; i < data.Count; i++)
            {
                if ((data[i - 1].Y < threshold && data[i].Y >= threshold)
                    || (data[i - 1].Y > threshold && data[i].Y <= threshold))
                {
                    return LinearInterpolateX(data[i - 1].X, data[i - 1].Y, data[i].X, data[i].Y, threshold);
                }
            }

            return double.NaN;
        }

        private static double LinearInterpolateX(double x0, double y0, double x1, double y1, double yTarget)
        {
            if (Math.Abs(y1 - y0) < double.Epsilon)
            {
                return x0;
            }

            return x0 + (x1 - x0) * (yTarget - y0) / (y1 - y0);
        }

        private static double FindNearestBinMagnitude(List<(double Frequency, double Magnitude)> spectrum, double targetFreq)
        {
            if (spectrum.Count == 0)
            {
                return 0;
            }

            double bestDist = double.MaxValue;
            double bestMag = 0;

            foreach (var bin in spectrum)
            {
                double dist = Math.Abs(bin.Frequency - targetFreq);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestMag = bin.Magnitude;
                }
            }

            return bestMag;
        }

        /// <summary>
        /// In-place Cooley-Tukey radix-2 FFT.
        /// </summary>
        private static void CooleyTukeyFFT(double[] real, double[] imag, bool inverse)
        {
            int n = real.Length;

            // Bit-reversal permutation
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                while ((j & bit) != 0)
                {
                    j ^= bit;
                    bit >>= 1;
                }

                j ^= bit;

                if (i < j)
                {
                    double tmpR = real[i];
                    real[i] = real[j];
                    real[j] = tmpR;

                    double tmpI = imag[i];
                    imag[i] = imag[j];
                    imag[j] = tmpI;
                }
            }

            // FFT butterfly
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = 2.0 * Math.PI / len * (inverse ? -1.0 : 1.0);
                double wR = Math.Cos(angle);
                double wI = Math.Sin(angle);

                for (int i = 0; i < n; i += len)
                {
                    double curR = 1.0;
                    double curI = 0.0;

                    for (int j = 0; j < len / 2; j++)
                    {
                        double uR = real[i + j];
                        double uI = imag[i + j];
                        double vR = real[i + j + len / 2] * curR - imag[i + j + len / 2] * curI;
                        double vI = real[i + j + len / 2] * curI + imag[i + j + len / 2] * curR;

                        real[i + j] = uR + vR;
                        imag[i + j] = uI + vI;
                        real[i + j + len / 2] = uR - vR;
                        imag[i + j + len / 2] = uI - vI;

                        double newCurR = curR * wR - curI * wI;
                        curI = curR * wI + curI * wR;
                        curR = newCurR;
                    }
                }
            }

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    real[i] /= n;
                    imag[i] /= n;
                }
            }
        }
    }
}
