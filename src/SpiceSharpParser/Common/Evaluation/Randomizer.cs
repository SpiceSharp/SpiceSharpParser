using System;
using System.Threading;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Provider of random number generator.
    /// </summary>
    public static class Randomizer
    {
        private static int _tickCount = Environment.TickCount;

        /// <summary>
        /// Provides a random number generator.
        /// </summary>
        /// <param name="randomSeed">Random generator seed.</param>
        /// <returns>
        /// A new instance of a random number generator.
        /// </returns>
        public static Random GetRandom(int? randomSeed)
        {
            if (randomSeed.HasValue)
            {
                return new Random(randomSeed.Value);
            }
            else
            {
                int seed = Interlocked.Increment(ref _tickCount);
                return new Random(seed);
            }
        }
    }
}
