using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class ACosFunction : Function<double, double>
    {
        public ACosFunction()
        {
            Name = "acos";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("acos() function expects one argument");
            }

            double d = args[0];
            return System.Math.Acos(d);
        }
    }
}
