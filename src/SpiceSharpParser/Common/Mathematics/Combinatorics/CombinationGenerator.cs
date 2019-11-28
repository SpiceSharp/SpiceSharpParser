using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Combinatorics
{
    /// <summary>
    /// Helper methods for generating combinations.
    /// </summary>
    public class CombinationGenerator
    {
        /// <summary>
        /// Generates combinations for given n.
        /// </summary>
        /// <param name="count">Number of combinations to generate.</param>
        /// <param name="n">Number of elements.</param>
        /// <returns>
        /// Combinations for given n.
        /// </returns>
        public List<int[]> Generate(int count, int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var result = new List<int[]>();
            result.Add(new int[] { });
            if (result.Count == count)
            {
                return result;
            }

            bool generate = true;
            int k = 1;
            while (generate)
            {
                var combination = GenerateFirstCombination(k);
                result.Add(combination);
                if (result.Count == count)
                {
                    break;
                }

                long combinationCount = GetCombinationsCount(k, n);
                for (var i = 1; i < combinationCount; i++)
                {
                    combination = NextCombination(combination, n);
                    result.Add(combination);

                    if (result.Count == count)
                    {
                        generate = false;
                        break;
                    }
                }

                k++;
            }

            return result;
        }

        /// <summary>
        /// Gets a value of factorial.
        /// </summary>
        /// <param name="n">Factorial number.</param>
        /// <returns>
        /// A value of factorial.
        /// </returns>
        public long GetFactorial(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }

            long result = 1;

            for (var i = 1; i <= n; i++)
            {
                result *= i;
            }

            return result;
        }

        /// <summary>
        /// Gets the number of combinations with repetitions.
        /// </summary>
        /// <param name="k">Size of combination.</param>
        /// <param name="n">Number of elements.</param>
        /// <returns>
        /// The number of combinations with repetitions.
        /// </returns>
        public long GetCombinationsCount(int k, int n)
        {
            long nominator = 1;

            for (var i = n; i <= (n + k - 1); i++)
            {
                nominator *= i;
            }

            return nominator / GetFactorial(k);
        }

        /// <summary>
        /// Generates a first combination where all elements are first element.
        /// </summary>
        /// <param name="k">Size of combination.</param>
        /// <returns>
        /// A first combination for given k.
        /// </returns>
        public int[] GenerateFirstCombination(int k)
        {
            int[] combination = new int[k];
            for (int i = 0; i < k; i++)
            {
                combination[i] = 1;
            }

            return combination;
        }

        /// <summary>
        /// Generate a next combination.
        /// </summary>
        /// <param name="combination">Current combination.</param>
        /// <param name="n">Number of elements.</param>
        /// <returns>
        /// A next combination.
        /// </returns>
        public int[] NextCombination(int[] combination, int n)
        {
            int elementIndexToModify = -1;
            for (int i = combination.Length - 1; i >= 0; i--)
            {
                if (combination[i] != n)
                {
                    elementIndexToModify = i;
                    break;
                }
            }

            if (elementIndexToModify == -1)
            {
                throw new SpiceSharpParserException("There is no next combination");
            }

            int[] newCombination = new int[combination.Length];

            for (var i = 0; i < combination.Length; i++)
            {
                if (i < elementIndexToModify)
                {
                    newCombination[i] = combination[i];
                }
                else
                {
                    if (i == elementIndexToModify)
                    {
                        newCombination[i] = combination[i] + 1;
                    }
                    else
                    {
                        newCombination[i] = newCombination[i - 1];
                    }
                }
            }

            return newCombination;
        }
    }
}