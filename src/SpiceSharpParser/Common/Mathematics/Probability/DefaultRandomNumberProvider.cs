using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class DefaultRandomNumberProvider: IRandomNumberProvider
    {
        private readonly Random _random;

        public DefaultRandomNumberProvider(Random random)
        {
            _random = random;
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public double NextSignedDouble()
        {
            return _random.NextDouble() * 2.0 - 1.0;
        }

        public int Next()
        {
            return _random.Next();
        }
    }
}
