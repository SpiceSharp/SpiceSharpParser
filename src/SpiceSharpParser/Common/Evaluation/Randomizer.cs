using System;
using System.Threading;

namespace SpiceSharpParser.Common.Evaluation
{
    public static class Randomizer
    {
        private static int tickCount = Environment.TickCount;

        public static Random GetRandom(int? randomSeed)
        {
            if (randomSeed.HasValue)
            {
                return new Random(randomSeed.Value);
            }
            else
            {
                int seed = Interlocked.Increment(ref tickCount);
                return new Random(seed);
            }
        }
    }
}
