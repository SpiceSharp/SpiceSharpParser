using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class RandomFunction : Function<double, double>
    {
        public RandomFunction()
        {
            Name = "random";
            ArgumentsCount = 0;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 0)
            {
                throw new Exception("random expects no arguments");
            }

            var random = context.Randomizer.GetRandomDoubleProvider(context.Seed);
            return random.NextDouble();
        }
    }
}
