using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class AGaussFunction : Function<double, double>
    {
        public AGaussFunction()
        {
            Name = "agauss";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 3)
            {
                throw new Exception("agauss expects three arguments: nominal_val, abs_variation and sigma");
            }

            var random = context.Randomizer.GetRandomDoubleProvider(context.Seed);

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double normal = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);
            double nominal = args[0];
            double stdDev = args[1];
            double sigma = args[2];

            double stdVar = stdDev / sigma;

            return nominal + (stdVar * normal);
        }
    }
}
