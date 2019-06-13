using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class ATanFunction : Function<double, double>
    {
        public ATanFunction()
        {
            Name = "atan";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("atan() function expects one argument");
            }

            double d = args[0];
            return System.Math.Atan(d);
        }
    }
}
