using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class RandomFunctions
    {
        /// <summary>
        /// Create a gauss() function.
        /// </summary>
        /// <returns>
        /// A new instance of random gauss function.
        /// </returns>
        public static Function CreateGauss()
        {
            Function function = new Function();
            function.Name = "gauss";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new Exception("gauss() expects one argument");
                }

                Random random = Randomizer.GetRandom(evaluator.Seed);

                double p1 = 1 - random.NextDouble();
                double p2 = 1 - random.NextDouble();

                double std = Math.Sqrt(-2.0 * Math.Log(p1)) * Math.Sin(2.0 * Math.PI * p2);
                return (double)args[0] * std;
            };

            return function;
        }

        /// <summary>
        /// Create a random() function. It generates number between 0.0 and 1.0 (uniform distribution).
        /// </summary>
        /// <returns>
        /// A new instance of random function.
        /// </returns>
        public static Function CreateRandom()
        {
            Function function = new Function();
            function.Name = "random";
            function.VirtualParameters = false;
            function.ArgumentsCount = 0;

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 0)
                {
                    throw new Exception("random() expects no arguments");
                }

                Random random = Randomizer.GetRandom(evaluator.Seed);
                return random.NextDouble();
            };

            return function;
        }

        /// <summary>
        /// Create a flat() function. It generates number between -x and +x.
        /// </summary>
        /// <returns>
        /// A new instance of random function.
        /// </returns>
        public static Function CreateFlat()
        {
            Function function = new Function();
            function.Name = "flat";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("flat() function expects one argument");
                }

                Random random = Randomizer.GetRandom(evaluator.Seed);

                double x = (double)args[0];

                return (random.NextDouble() * 2.0 * x) - x;
            };

            return function;
        }
    }
}
