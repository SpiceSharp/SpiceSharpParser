using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharpParser.Common.Mathematics.Probability;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class RandomFunction : Function<double, double>
    {
        public RandomFunction()
        {
            Name = "random";
            ArgumentsCount = 0;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 0)
            {
                throw new Exception("random expects no arguments");
            }

            IRandom random = context.Randomizer.GetRandom(context.Seed);
            return random.NextDouble();
        }
    }
}
