using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class ASinFunction : Function<double, double>
    {
        public ASinFunction()
        {
            Name = "asin";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("asin() function expects one argument");
            }

            double d = args[0];
            return System.Math.Asin(d);
        }
    }
}
