using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class LimitFunction : Function<double, double>
    {
        public LimitFunction()
        {
            Name = "limit";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 2)
            {
                throw new Exception("limit expects two arguments: nominal_val, abs_variation");
            }

            var random = context.Randomizer.GetRandomDoubleProvider(context.Seed);

            double dRand = (2.0 * random.NextDouble()) - 1.0;
            double nominal = args[0];
            double variation = args[1];

            return nominal + (dRand > 0 ? variation : -1 * variation);
        }
    }
}
