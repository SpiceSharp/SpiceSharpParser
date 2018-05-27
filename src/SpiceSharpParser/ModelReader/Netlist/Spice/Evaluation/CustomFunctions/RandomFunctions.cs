using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions
{
    public class RandomFunctions
    {
        /// <summary>
        /// Create a random() user function.
        /// </summary>
        /// <returns>
        /// A new instance of random spice function.
        /// </returns>
        public static CustomFunction CreateRandom()
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "random";
            function.VirtualParameters = false;
            function.ArgumentsCount = 0;

            function.Logic = (args, simulation) =>
            {
                if (args.Length != 0)
                {
                    throw new Exception("random expects no arguments");
                }
                return randomGenerator.NextDouble();
            };

            return function;
        }
    }
}
