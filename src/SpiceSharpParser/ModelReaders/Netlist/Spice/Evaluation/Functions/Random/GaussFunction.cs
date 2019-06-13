using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class GaussFunction : Function<double, double>
    {
        public GaussFunction()
        {
            Name = "gauss";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new Exception("gauss() expects one argument");
            }

            System.Random random = context.Randomizer.GetRandom(context.Seed);

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double std = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);
            return args[0] * std;
        }
    }
}
