using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class FlatFunction : Function<double, double>
    {
        public FlatFunction()
        {
            Name = "flat";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("flat function expects one argument");
            }

            var random = context.Randomizer.GetRandomDoubleProvider(context.Seed);

            double x = args[0];
            return x * ((random.NextDouble() * 2.0) - 1.0);
        }
    }
}
