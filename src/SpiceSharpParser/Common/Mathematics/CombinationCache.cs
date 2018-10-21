using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SpiceSharpParser.Common.Mathematics
{
    public class CombinationCache
    {
        private readonly static ConcurrentDictionary<string, List<int[]>> cache = new ConcurrentDictionary<string, List<int[]>>();
        private readonly static CombinationGenerator generator = new CombinationGenerator();

        /// <summary>
        /// Gets combinations.
        /// </summary>
        /// <param name="count">Count of combinations.</param>
        /// <param name="n">Number of elements of combinations.</param>
        /// <returns>
        ///  List of combinations.
        /// </returns>
        public static List<int[]> GetCombinations(int count, int n)
        {
            string key = count + "_" + n;

            if (!cache.TryGetValue(key, out var combination))
            {
                combination = generator.Generate(count, n);
                cache[key] = combination;
            }

            return combination;
        }
    }
}
