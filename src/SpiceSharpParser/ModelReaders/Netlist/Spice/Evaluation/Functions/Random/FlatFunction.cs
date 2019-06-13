using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class FlatFunction : Function<double, double>
    {
        public FlatFunction()
        {
            Name = "flat";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("flat() function expects one argument");
            }

            System.Random random = context.Randomizer.GetRandom(context.Seed);

            double x = (double)args[0];

            return (random.NextDouble() * 2.0 * x) - x;
        }
    }
}
