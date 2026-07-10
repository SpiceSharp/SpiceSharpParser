using System;
using System.Collections.Generic;

namespace SpiceSharp.Physics2D.Tests.Numerics
{
    internal static class TimeSeriesComparison
    {
        public static TimeSeriesComparisonResult Compare(
            IReadOnlyList<TimeSeriesSample> expected,
            IReadOnlyList<TimeSeriesSample> actual,
            double normalizationFloor,
            double timeTolerance = 1e-12)
        {
            ValidateSeries(expected, nameof(expected));
            ValidateSeries(actual, nameof(actual));
            ValidatePositiveFinite(normalizationFloor, nameof(normalizationFloor));
            ValidateNonnegativeFinite(timeTolerance, nameof(timeTolerance));

            int dimension = expected[0].Values.Count;
            if (actual[0].Values.Count != dimension)
            {
                throw new ArgumentException("Time series dimensions do not match.", nameof(actual));
            }

            double squaredNormalizedError = 0.0;
            double maximumAbsoluteError = 0.0;
            int valueCount = 0;
            int actualIndex = 0;

            for (int expectedIndex = 0; expectedIndex < expected.Count; expectedIndex++)
            {
                TimeSeriesSample expectedSample = expected[expectedIndex];
                while (actualIndex + 1 < actual.Count
                    && actual[actualIndex + 1].Time < expectedSample.Time - timeTolerance)
                {
                    actualIndex++;
                }

                double[] interpolated = Interpolate(
                    actual,
                    actualIndex,
                    expectedSample.Time,
                    dimension,
                    timeTolerance);

                for (int valueIndex = 0; valueIndex < dimension; valueIndex++)
                {
                    double difference = interpolated[valueIndex] - expectedSample.Values[valueIndex];
                    double absoluteDifference = Math.Abs(difference);
                    double scale = Math.Max(
                        normalizationFloor,
                        Math.Abs(expectedSample.Values[valueIndex]));
                    double normalizedDifference = difference / scale;

                    maximumAbsoluteError = Math.Max(maximumAbsoluteError, absoluteDifference);
                    squaredNormalizedError += normalizedDifference * normalizedDifference;
                    valueCount++;
                }
            }

            return new TimeSeriesComparisonResult(
                maximumAbsoluteError,
                Math.Sqrt(squaredNormalizedError / valueCount),
                expected.Count,
                valueCount);
        }

        private static double[] Interpolate(
            IReadOnlyList<TimeSeriesSample> series,
            int lowerIndex,
            double time,
            int dimension,
            double timeTolerance)
        {
            TimeSeriesSample lower = series[lowerIndex];
            if (Math.Abs(time - lower.Time) <= timeTolerance)
            {
                return CopyValues(lower, dimension);
            }

            if (lowerIndex + 1 >= series.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Actual series ends too early.");
            }

            TimeSeriesSample upper = series[lowerIndex + 1];
            if (time < lower.Time - timeTolerance || time > upper.Time + timeTolerance)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Actual series does not cover the requested time.");
            }

            double fraction = (time - lower.Time) / (upper.Time - lower.Time);
            var values = new double[dimension];
            for (int index = 0; index < dimension; index++)
            {
                values[index] = lower.Values[index]
                    + (fraction * (upper.Values[index] - lower.Values[index]));
            }

            return values;
        }

        private static double[] CopyValues(TimeSeriesSample sample, int dimension)
        {
            var values = new double[dimension];
            for (int index = 0; index < dimension; index++)
            {
                values[index] = sample.Values[index];
            }

            return values;
        }

        private static void ValidateSeries(IReadOnlyList<TimeSeriesSample> series, string parameterName)
        {
            if (series == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (series.Count == 0)
            {
                throw new ArgumentException("Time series must not be empty.", parameterName);
            }

            int dimension = series[0].Values.Count;
            if (dimension == 0)
            {
                throw new ArgumentException("Time series samples must contain values.", parameterName);
            }

            for (int index = 0; index < series.Count; index++)
            {
                TimeSeriesSample sample = series[index];
                if (!IsFinite(sample.Time) || sample.Values.Count != dimension)
                {
                    throw new ArgumentException("Time series contains an invalid sample.", parameterName);
                }

                for (int valueIndex = 0; valueIndex < sample.Values.Count; valueIndex++)
                {
                    if (!IsFinite(sample.Values[valueIndex]))
                    {
                        throw new ArgumentException(
                            "Time series contains a non-finite value.",
                            parameterName);
                    }
                }

                if (index > 0 && !(sample.Time > series[index - 1].Time))
                {
                    throw new ArgumentException("Time series times must be strictly increasing.", parameterName);
                }
            }
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (!(value > 0.0) || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateNonnegativeFinite(double value, string parameterName)
        {
            if (value < 0.0 || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal readonly struct TimeSeriesSample
    {
        public TimeSeriesSample(double time, params double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Time = time;
            Values = (double[])values.Clone();
        }

        public double Time { get; }

        public IReadOnlyList<double> Values { get; }
    }

    internal readonly struct TimeSeriesComparisonResult
    {
        public TimeSeriesComparisonResult(
            double maximumAbsoluteError,
            double normalizedRootMeanSquareError,
            int sampleCount,
            int valueCount)
        {
            MaximumAbsoluteError = maximumAbsoluteError;
            NormalizedRootMeanSquareError = normalizedRootMeanSquareError;
            SampleCount = sampleCount;
            ValueCount = valueCount;
        }

        public double MaximumAbsoluteError { get; }

        public double NormalizedRootMeanSquareError { get; }

        public int SampleCount { get; }

        public int ValueCount { get; }
    }
}
