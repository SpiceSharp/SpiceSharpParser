using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Combinatorics
{
    public static class CombinationCache
    {
        private static readonly ConcurrentDictionary<string, List<int[]>> Cache = new ConcurrentDictionary<string, List<int[]>>();
        private static readonly CombinationGenerator Generator = new CombinationGenerator();

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
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }

            string key = count + "_" + n;

            if (!Cache.TryGetValue(key, out var combination))
            {
                combination = Generator.Generate(count, n);
                Cache[key] = combination;
            }

            return combination;
        }
    }
}