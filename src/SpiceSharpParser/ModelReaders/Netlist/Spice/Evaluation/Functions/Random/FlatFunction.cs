using System;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class FlatFunction : Function<double, double>
    {
        public FlatFunction()
        {
            Name = "flat";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("flat function expects one argument");
            }

            IRandom random = context.Randomizer.GetRandom(context.Seed);

            double x = (double)args[0];

            return x * ((random.NextDouble() * 2.0) - 1.0);
        }
    }
}
