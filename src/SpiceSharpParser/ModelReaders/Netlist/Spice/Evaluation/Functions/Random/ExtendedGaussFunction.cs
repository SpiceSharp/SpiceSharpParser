using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class ExtendedGaussFunction : Function<double, double>
    {
        public ExtendedGaussFunction()
        {
            Name = "gauss";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 3)
            {
                throw new SpiceSharpParserException("gauss expects three arguments: nominal_val, rel_variation and sigma");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double normal = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);
            double nominal = args[0];
            double stdDev = args[1];
            double sigma = args[2];

            double stdVar = nominal * stdDev / sigma;

            return nominal + (stdVar * normal);
        }
    }
}