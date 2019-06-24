using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class CustomRandom : IRandomDouble
    {
        private readonly IRandom _random;
        private readonly Cdf _cdf;

        public CustomRandom(Pdf pdf, IRandom random)
        {
            if (pdf.GetFirstPoint().X < -1.0 || pdf.GetLastPoint().X > 1.0)
            {
                throw new ArgumentException(nameof(pdf));
            }

            _cdf = new Cdf(pdf);
            _random = random;
        }

        public double NextDouble()
        {
            var p = _random.NextDouble();

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
    }
}
