using System;

namespace SpiceSharpParser.Common
{
    public class Numbers
    {
        public static int[] GetSystemNumber(int value, int[] system)
        {
            var result = new int[system.Length];

            var i = 0;
            while (value != 0)
            {
                var pos = result.Length - i - 1;
                var b = system[pos];

                if (b == 0)
                {
                    throw new Exception($"System at {pos} is 0");
                }

                result[pos] = value % b;
                value /= b;
                i++;
            }

            return result;
        }
    }
}
