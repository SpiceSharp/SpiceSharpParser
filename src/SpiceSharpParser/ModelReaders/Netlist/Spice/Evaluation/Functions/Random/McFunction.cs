using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class McFunction : Function<double, double>
    {
        public McFunction()
        {
            Name = "mc";
            VirtualParameters = false;
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 2)
            {
                throw new Exception("mc() expects two arguments");
            }

            System.Random random = context.Randomizer.GetRandom(context.Seed);
            double x = args[0];
            double tol = args[1];

            double min = x - (tol * x);
            double randomChange = random.NextDouble() * 2.0 * tol * x;

            return min + randomChange;
        }
    }
}
