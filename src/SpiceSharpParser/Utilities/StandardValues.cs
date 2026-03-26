using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Utilities
{
    /// <summary>
    /// Provides standard electronic component value series (E12, E24, E96) and
    /// utilities for snapping calculated values to the nearest standard value.
    /// </summary>
    public static class StandardValues
    {
        /// <summary>
        /// E12 series multipliers (12 values per decade, ±10% tolerance).
        /// </summary>
        public static readonly double[] E12Multipliers = { 1.0, 1.2, 1.5, 1.8, 2.2, 2.7, 3.3, 3.9, 4.7, 5.6, 6.8, 8.2 };

        /// <summary>
        /// E24 series multipliers (24 values per decade, ±5% tolerance).
        /// </summary>
        public static readonly double[] E24Multipliers = { 1.0, 1.1, 1.2, 1.3, 1.5, 1.6, 1.8, 2.0, 2.2, 2.4, 2.7, 3.0, 3.3, 3.6, 3.9, 4.3, 4.7, 5.1, 5.6, 6.2, 6.8, 7.5, 8.2, 9.1 };

        /// <summary>
        /// E96 series multipliers (96 values per decade, ±1% tolerance).
        /// </summary>
        public static readonly double[] E96Multipliers =
        {
            1.00, 1.02, 1.05, 1.07, 1.10, 1.13, 1.15, 1.18, 1.21, 1.24,
            1.27, 1.30, 1.33, 1.37, 1.40, 1.43, 1.47, 1.50, 1.54, 1.58,
            1.62, 1.65, 1.69, 1.74, 1.78, 1.82, 1.87, 1.91, 1.96, 2.00,
            2.05, 2.10, 2.15, 2.21, 2.26, 2.32, 2.37, 2.43, 2.49, 2.55,
            2.61, 2.67, 2.74, 2.80, 2.87, 2.94, 3.01, 3.09, 3.16, 3.24,
            3.32, 3.40, 3.48, 3.57, 3.65, 3.74, 3.83, 3.92, 4.02, 4.12,
            4.22, 4.32, 4.42, 4.53, 4.64, 4.75, 4.87, 4.99, 5.11, 5.23,
            5.36, 5.49, 5.62, 5.76, 5.90, 6.04, 6.19, 6.34, 6.49, 6.65,
            6.81, 6.98, 7.15, 7.32, 7.50, 7.68, 7.87, 8.06, 8.25, 8.45,
            8.66, 8.87, 9.09, 9.31, 9.53, 9.76,
        };

        /// <summary>
        /// Finds the nearest E12 standard value to the given value.
        /// </summary>
        /// <param name="value">The target value (must be positive).</param>
        /// <returns>The nearest E12 standard value.</returns>
        public static double NearestE12(double value)
        {
            return NearestInSeries(value, E12Multipliers);
        }

        /// <summary>
        /// Finds the nearest E24 standard value to the given value.
        /// </summary>
        /// <param name="value">The target value (must be positive).</param>
        /// <returns>The nearest E24 standard value.</returns>
        public static double NearestE24(double value)
        {
            return NearestInSeries(value, E24Multipliers);
        }

        /// <summary>
        /// Finds the nearest E96 standard value to the given value.
        /// </summary>
        /// <param name="value">The target value (must be positive).</param>
        /// <returns>The nearest E96 standard value.</returns>
        public static double NearestE96(double value)
        {
            return NearestInSeries(value, E96Multipliers);
        }

        /// <summary>
        /// Returns the two E24 standard values that bracket the given value (one below, one above).
        /// </summary>
        /// <param name="value">The target value (must be positive).</param>
        /// <returns>A tuple of (below, above) standard values.</returns>
        public static (double Below, double Above) BracketE24(double value)
        {
            return BracketInSeries(value, E24Multipliers);
        }

        /// <summary>
        /// Returns the two E12 standard values that bracket the given value (one below, one above).
        /// </summary>
        /// <param name="value">The target value (must be positive).</param>
        /// <returns>A tuple of (below, above) standard values.</returns>
        public static (double Below, double Above) BracketE12(double value)
        {
            return BracketInSeries(value, E12Multipliers);
        }

        /// <summary>
        /// Generates all standard values in a series within a given range.
        /// </summary>
        /// <param name="minValue">Minimum value (inclusive).</param>
        /// <param name="maxValue">Maximum value (inclusive).</param>
        /// <param name="series">The E-series multipliers to use.</param>
        /// <returns>All standard values within the range, sorted ascending.</returns>
        public static IReadOnlyList<double> GetValuesInRange(double minValue, double maxValue, double[] series)
        {
            if (minValue <= 0 || maxValue <= 0 || minValue > maxValue)
            {
                throw new ArgumentException("Range must be positive with minValue <= maxValue.");
            }

            var results = new List<double>();
            double decade = Math.Pow(10, Math.Floor(Math.Log10(minValue)));

            while (decade * series[0] <= maxValue)
            {
                foreach (double mult in series)
                {
                    double val = decade * mult;
                    if (val >= minValue && val <= maxValue)
                    {
                        results.Add(val);
                    }
                }

                decade *= 10;
            }

            return results;
        }

        private static double NearestInSeries(double value, double[] seriesMultipliers)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(value));
            }

            double logVal = Math.Log10(value);
            double decade = Math.Pow(10, Math.Floor(logVal));
            double normalized = value / decade;

            double bestValue = seriesMultipliers[0] * decade;
            double bestRatio = double.MaxValue;

            // Check current decade and next decade (for wrap-around)
            foreach (double mult in seriesMultipliers)
            {
                double candidate = mult * decade;
                double ratio = Math.Max(value / candidate, candidate / value);
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    bestValue = candidate;
                }
            }

            // Also check first value of next decade
            double nextDecadeFirst = seriesMultipliers[0] * decade * 10;
            double nextRatio = Math.Max(value / nextDecadeFirst, nextDecadeFirst / value);
            if (nextRatio < bestRatio)
            {
                bestValue = nextDecadeFirst;
            }

            // Also check last value of previous decade
            double prevDecadeLast = seriesMultipliers[seriesMultipliers.Length - 1] * decade / 10;
            double prevRatio = Math.Max(value / prevDecadeLast, prevDecadeLast / value);
            if (prevRatio < bestRatio)
            {
                bestValue = prevDecadeLast;
            }

            return bestValue;
        }

        private static (double Below, double Above) BracketInSeries(double value, double[] seriesMultipliers)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(value));
            }

            double logVal = Math.Log10(value);
            double decade = Math.Pow(10, Math.Floor(logVal));

            double below = double.MinValue;
            double above = double.MaxValue;

            // Check previous decade (last value), current decade, and next decade (first value)
            var candidates = new List<double>();
            candidates.Add(seriesMultipliers[seriesMultipliers.Length - 1] * decade / 10);
            foreach (double mult in seriesMultipliers)
            {
                candidates.Add(mult * decade);
            }

            candidates.Add(seriesMultipliers[0] * decade * 10);

            foreach (double candidate in candidates)
            {
                if (candidate <= value && candidate > below)
                {
                    below = candidate;
                }

                if (candidate >= value && candidate < above)
                {
                    above = candidate;
                }
            }

            return (below, above);
        }
    }
}
