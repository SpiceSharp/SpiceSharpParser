using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.Common.Evaluation
{
    //TODO think about it.
    public static class Randomizer
    {
        private static Dictionary<int, Random> generators = new Dictionary<int, Random>();
        private static int startTickCount = Environment.TickCount;
        private static int tickCount = startTickCount;

        public static Random GetRandom(int? randomSeed)
        {
            lock (generators)
            {
                if (randomSeed.HasValue)
                {
                    if (!generators.ContainsKey(randomSeed.Value))
                    {
                        generators[randomSeed.Value] = new Random(randomSeed.Value);
                    }

                    return generators[randomSeed.Value];
                }
                else
                {
                    int seed = Interlocked.Increment(ref tickCount);
                    generators[seed] = new Random(seed);
                    return generators[seed];
                }
            }
        }

        public static void Clear()
        {
            lock (generators)
            {
                generators.Clear();
                tickCount = startTickCount;
            }
        }
    }
}
