using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class McFunction : Function<double, double>
    {
        public McFunction()
        {
            Name = "mc";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 2)
            {
                throw new SpiceSharpParserException("mc expects two arguments");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();

            double x = args[0];
            double tol = args[1];

            double min = x - (tol * x);
            double randomChange = random.NextDouble() * 2.0 * tol * x;

            return min + randomChange;
        }
    }
}