using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Custom random number provider.
    /// </summary>
    public class CustomRandomNumberProvider : IRandomNumberProvider
    {
        private readonly IRandomDoubleProvider _baseRandom;
        private readonly Cdf _cdf;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRandomNumberProvider"/> class.
        /// </summary>
        /// <param name="cdf">Cdf.</param>
        /// <param name="baseRandom">Base random provider.</param>
        public CustomRandomNumberProvider(Cdf cdf, IRandomDoubleProvider baseRandom)
        {
            _cdf = cdf;
            _baseRandom = baseRandom;
        }

        /// <summary>
        /// Computes the next random double in range (-1,1) with custom distribution.
        /// </summary>
        /// <returns>
        /// The random double from custom distribution.
        /// </returns>
        public double NextSignedDouble()
        {
            var p = _baseRandom.NextDouble();

            var min = _cdf.GetFirstPoint().X;
            var max = _cdf.GetLastPoint().X;

            var scale = (Math.Max(1, max) - Math.Min(-1, min)) / 2.0;

            for (var i = 1; i < _cdf.PointsCount; i++)
            {
                var p1 = _cdf[i - 1];
                var j = i;

                while (j < _cdf.PointsCount && _cdf[j].Y == p1.Y)
                {
                    j++;
                }

                var p2 = _cdf[j];

                if (p1.Y <= p && p < p2.Y)
                {
                    var result = (((p2.X - p1.X) * (p - p1.Y) / (p2.Y - p1.Y)) + p1.X) / scale;

                    return result;
                }
            }

            throw new SpiceSharpParserException("CDF was invalid");
        }

        /// <summary>
        /// Computes next random integer.
        /// </summary>
        /// <returns>
        /// The random integer from custom distribution.
        /// </returns>
        public int Next()
        {
            // TODO: this is far from perfect, learn and implement something better
            return (int)(NextDouble() * int.MaxValue);
        }

        /// <summary>
        /// Computes next random double.
        /// </summary>
        /// <returns>
        /// The random double from custom distribution.
        /// </returns>
        public double NextDouble()
        {
            return (NextSignedDouble() + 1.0) / 2.0;
        }
    }
}