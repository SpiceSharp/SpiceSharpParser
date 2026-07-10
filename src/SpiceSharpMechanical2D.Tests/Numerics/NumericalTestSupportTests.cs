using System;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Numerics
{
    public class NumericalTestSupportTests
    {
        [Fact]
        public void FiniteDifferenceJacobianMatchesKnownAnalyticJacobian()
        {
            double[] point = { 0.75, -1.25 };
            var analytic = new double[,]
            {
                { 2.0 * point[0], 3.0 },
                { point[1], point[0] },
            };
            double[,] finiteDifference = FiniteDifferenceJacobian.Calculate(
                values => new[]
                {
                    (values[0] * values[0]) + (3.0 * values[1]),
                    values[0] * values[1],
                },
                point);

            NumericComparison comparison = NumericAssert.JacobianEqual(
                analytic,
                finiteDifference,
                1e-10,
                1e-9,
                "Known polynomial");
            Console.WriteLine(FormattableString.Invariant(
                $"Finite-difference support max absolute mismatch={comparison.MaximumAbsoluteMismatch:R}, max relative mismatch={comparison.MaximumRelativeMismatch:R}."));
            Assert.InRange(comparison.MaximumAbsoluteMismatch, 0.0, 1e-9);
        }

        [Fact]
        public void NumericAssertUsesAbsoluteAndRelativeScale()
        {
            NumericAssert.Equal(0.0, 5e-10, 1e-9, 0.0);
            NumericAssert.Equal(1e9, 1e9 + 0.5, 0.0, 1e-9);

            Assert.ThrowsAny<Exception>(() => NumericAssert.Equal(0.0, 2e-9, 1e-9, 0.0));
            Assert.ThrowsAny<Exception>(() => NumericAssert.Equal(1e9, 1e9 + 2.0, 0.0, 1e-9));
        }

        [Fact]
        public void TimeSeriesComparisonInterpolatesActualSeriesAtReferenceTimes()
        {
            var expected = new[]
            {
                new TimeSeriesSample(0.0, 0.0, 1.0),
                new TimeSeriesSample(0.5, 1.0, 0.0),
                new TimeSeriesSample(1.0, 2.0, -1.0),
            };
            var actual = new[]
            {
                new TimeSeriesSample(0.0, 0.0, 1.0),
                new TimeSeriesSample(1.0, 2.0, -1.0),
            };

            TimeSeriesComparisonResult result = TimeSeriesComparison.Compare(
                expected,
                actual,
                normalizationFloor: 1.0);

            Assert.Equal(0.0, result.MaximumAbsoluteError);
            Assert.Equal(0.0, result.NormalizedRootMeanSquareError);
            Assert.Equal(3, result.SampleCount);
            Assert.Equal(6, result.ValueCount);
        }

        [Fact]
        public void TimeSeriesComparisonReportsKnownOffsetError()
        {
            var expected = new[]
            {
                new TimeSeriesSample(0.0, 1.0),
                new TimeSeriesSample(1.0, 1.0),
            };
            var actual = new[]
            {
                new TimeSeriesSample(0.0, 1.25),
                new TimeSeriesSample(1.0, 1.25),
            };

            TimeSeriesComparisonResult result = TimeSeriesComparison.Compare(
                expected,
                actual,
                normalizationFloor: 1.0);

            Assert.Equal(0.25, result.MaximumAbsoluteError);
            Assert.Equal(0.25, result.NormalizedRootMeanSquareError);
        }

        [Fact]
        public void TimeSeriesComparisonRejectsUnorderedOrUncoveredSeries()
        {
            var unordered = new[]
            {
                new TimeSeriesSample(1.0, 1.0),
                new TimeSeriesSample(0.0, 0.0),
            };
            var valid = new[]
            {
                new TimeSeriesSample(0.0, 0.0),
                new TimeSeriesSample(1.0, 1.0),
            };
            var tooShort = new[]
            {
                new TimeSeriesSample(0.0, 0.0),
                new TimeSeriesSample(0.5, 0.5),
            };

            Assert.Throws<ArgumentException>(() => TimeSeriesComparison.Compare(unordered, valid, 1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeSeriesComparison.Compare(valid, tooShort, 1.0));
        }
    }
}
