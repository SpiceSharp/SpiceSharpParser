using System;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class RandomFunction : Function<double, double>
    {
        public RandomFunction()
        {
            Name = "random";
            ArgumentsCount = 0;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 0)
            {
                throw new SpiceSharpParserException("random expects no arguments");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();
            return random.NextDouble();
        }
    }
}