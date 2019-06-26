using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class CustomRandomNumberProvider : IRandomNumberProvider
    {
        private readonly IRandomDoubleProvider _baseRandom;
        private readonly Cdf _cdf;

        public CustomRandomNumberProvider(Pdf pdf, IRandomDoubleProvider baseRandom)
        {
            if (pdf.GetFirstPoint().X < -1.0 || pdf.GetLastPoint().X > 1.0)
            {
                throw new ArgumentException(nameof(pdf));
            }

            _cdf = new Cdf(pdf);
            _baseRandom = baseRandom;
        }

        public double NextSignedDouble()
        {
            var p = _baseRandom.NextDouble();

            for (var i = 1; i < _cdf.PointsCount; i++)
            {
                var p1 = _cdf[i - 1];
                var p2 = _cdf[i];

                if (p1.Y <= p && p < p2.Y)
                {
                    return ((p2.X - p1.X) * (p - p1.Y) / (p2.Y - p1.Y)) + p1.X;
                }
            }

            throw new InvalidOperationException("CDF was invalid");
        }

        public int Next()
        {
            // TODO: this is far from perfect, learn and implement something better
            return (int)(NextDouble() * int.MaxValue);
        }

        public double NextDouble()
        {
            return (NextSignedDouble() + 1) / 2.0;
        }
    }
}
