using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions
{
    public class RandomFunctions
    {
        /// <summary>
        /// Create a random() user function. It generates number between 0.0 and 1.0 (uniform distribution).
        /// </summary>
        /// <returns>
        /// A new instance of random custom function.
        /// </returns>
        public static CustomFunction CreateRandom()
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "random";
            function.VirtualParameters = false;
            function.ArgumentsCount = 0;

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 0)
                {
                    throw new Exception("random expects no arguments");
                }
                return randomGenerator.NextDouble();
            };

            return function;
        }

        /// <summary>
        /// Create a flat() user function. It generates number between -x and +x.
        /// </summary>
        /// <returns>
        /// A new instance of random custom function.
        /// </returns>
        public static CustomFunction CreateFlat()
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "flat";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("abs() function expects one argument");
                }

                double x = (double)args[0];

                return (randomGenerator.NextDouble() * 2.0 * x) - x;
            };

            return function;
        }
    }
}
