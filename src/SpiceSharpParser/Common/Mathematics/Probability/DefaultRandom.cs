using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class DefaultRandom : IRandom
    {
        private readonly Random _random;

        public DefaultRandom(Random random)
        {
            _random = random;
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public int Next()
        {
            return _random.Next();
        }
    }
}
