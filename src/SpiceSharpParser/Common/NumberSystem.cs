using System;

namespace SpiceSharpParser.Common
{
    public class NumberSystem
    {
        /// <summary>
        /// Returns the number in given number system.
        /// </summary>
        /// <param name="value">Number.</param>
        /// <param name="system">System.</param>
        /// <returns>
        /// Array of number system.
        /// </returns>
        public static int[] GetValueInSystem(int value, int[] system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var result = new int[system.Length];

            var i = 0;
            while (value != 0)
            {
                var pos = result.Length - i - 1;
                var b = system[pos];

                if (b == 0)
                {
                    throw new SpiceSharpParserException($"System at {pos} is 0");
                }

                result[pos] = value % b;
                value /= b;
                i++;
            }

            return result;
        }
    }
}