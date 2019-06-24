using System;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class UnifFunction : Function<double, double>
    {
        public UnifFunction()
        {
            Name = "unif";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 2)
            {
                throw new Exception("unif expects two arguments: nominal_val, rel_variation");
            }

            IRandom random = context.Randomizer.GetRandom(context.Seed);

            double dRand = (2.0 * random.NextDouble()) - 1.0;
            double nominal = args[0];
            double variation = args[1];

            return nominal + (nominal * variation * dRand);
        }
    }
}
