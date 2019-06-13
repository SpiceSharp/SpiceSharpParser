using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class FloorFunction : Function<double, double>
    {
        public FloorFunction()
        {
            Name = "floor";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("floor() function expects one argument");
            }

            double x = args[0];
            return System.Math.Floor(x);
        }
    }
}
