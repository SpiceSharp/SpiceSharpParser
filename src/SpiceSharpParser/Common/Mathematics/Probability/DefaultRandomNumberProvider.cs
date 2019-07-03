using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Default random number provider.
    /// </summary>
    public class DefaultRandomNumberProvider : IRandomNumberProvider
    {
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRandomNumberProvider"/> class.
        /// </summary>
        /// <param name="random">Random instance.</param>
        public DefaultRandomNumberProvider(Random random)
        {
            _random = random;
        }

        /// <summary>
        /// Computes the next random double in range (0,1) with custom distribution.
        /// </summary>
        /// <returns>
        /// The random double from custom distribution.
        /// </returns>
        public double NextDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// Computes the next random double in range (-1,1) with custom distribution.
        /// </summary>
        /// <returns>
        /// The random double from custom distribution.
        /// </returns>
        public double NextSignedDouble()
        {
            return (_random.NextDouble() * 2.0) - 1.0;
        }

        /// <summary>
        /// Computes next random integer with given maximum.
        /// </summary>
        /// <returns>
        /// The random integer from custom distribution.
        /// </returns>
        public int Next()
        {
            return _random.Next();
        }
    }
}
