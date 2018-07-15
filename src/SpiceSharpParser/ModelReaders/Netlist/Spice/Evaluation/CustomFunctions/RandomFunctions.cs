using System;
using System.Threading;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class RandomFunctions
    {
        private static int tickCount = Environment.TickCount;
        private static ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref tickCount)));

        /// <summary>
        /// Create a gauss() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of random gauss function.
        /// </returns>
        public static CustomFunction CreateGauss()
        {
            var random = threadLocalRandom.Value;

            CustomFunction function = new CustomFunction();
            function.Name = "gauss";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new Exception("gauss() expects one argument");
                }
                double p1 = 1 - random.NextDouble();
                double p2 = 1 - random.NextDouble();

                double std = Math.Sqrt(-2.0 * Math.Log(p1)) * Math.Sin(2.0 * Math.PI * p2);
                return (double)args[0] * std;
            };

            return function;
        }

        /// <summary>
        /// Create a random() custom function. It generates number between 0.0 and 1.0 (uniform distribution).
        /// </summary>
        /// <returns>
        /// A new instance of random custom function.
        /// </returns>
        public static CustomFunction CreateRandom()
        {
            var random = threadLocalRandom.Value;

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
                return random.NextDouble();
            };

            return function;
        }

        /// <summary>
        /// Create a flat() custom function. It generates number between -x and +x.
        /// </summary>
        /// <returns>
        /// A new instance of random custom function.
        /// </returns>
        public static CustomFunction CreateFlat()
        {
            var random = threadLocalRandom.Value;

            CustomFunction function = new CustomFunction();
            function.Name = "flat";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("flat() function expects one argument");
                }

                double x = (double)args[0];

                return (random.NextDouble() * 2.0 * x) - x;
            };

            return function;
        }
    }
}
