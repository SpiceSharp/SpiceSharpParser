using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal sealed class LaplaceExpressionOptions
    {
        public const double DefaultZeroTolerance = 1e-18;

        public const double DefaultRelativeTolerance = 1e-12;

        public const int DefaultMaxOrder = 10;

        public LaplaceExpressionOptions(
            double zeroTolerance = DefaultZeroTolerance,
            double relativeTolerance = DefaultRelativeTolerance,
            int maxOrder = DefaultMaxOrder)
        {
            if (zeroTolerance < 0.0 || double.IsNaN(zeroTolerance) || double.IsInfinity(zeroTolerance))
            {
                throw new ArgumentOutOfRangeException(nameof(zeroTolerance));
            }

            if (relativeTolerance < 0.0 || double.IsNaN(relativeTolerance) || double.IsInfinity(relativeTolerance))
            {
                throw new ArgumentOutOfRangeException(nameof(relativeTolerance));
            }

            if (maxOrder < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxOrder));
            }

            ZeroTolerance = zeroTolerance;
            RelativeTolerance = relativeTolerance;
            MaxOrder = maxOrder;
        }

        public double ZeroTolerance { get; }

        public double RelativeTolerance { get; }

        public int MaxOrder { get; }
    }
}
