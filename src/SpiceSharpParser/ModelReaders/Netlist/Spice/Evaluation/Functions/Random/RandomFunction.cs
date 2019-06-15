using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class RandomFunction : Function<double, double>
    {
        public RandomFunction()
        {
            Name = "random";
            VirtualParameters = false;
            ArgumentsCount = 0;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 0)
            {
                throw new Exception("random expects no arguments");
            }

            System.Random random = context.Randomizer.GetRandom(context.Seed);
            return random.NextDouble();
        }
    }
}
