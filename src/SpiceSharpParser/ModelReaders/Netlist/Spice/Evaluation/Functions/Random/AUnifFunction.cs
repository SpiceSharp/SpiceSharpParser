using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class AUnifFunction : Function<double, double>
    {
        public AUnifFunction()
        {
            Name = "aunif";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 2)
            {
                throw new Exception("aunif expects two arguments: nominal_val, rel_variation");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();

            double dRand = (2.0 * random.NextDouble()) - 1.0;
            double nominal = args[0];
            double variation = args[1];

            return nominal + (variation * dRand);
        }
    }
}
