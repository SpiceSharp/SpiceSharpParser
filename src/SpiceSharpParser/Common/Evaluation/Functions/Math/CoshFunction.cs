using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class CoshFunction : Function<double, double>
    {
        public CoshFunction()
        {
            Name = "cosh";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("cosh() function expects one argument");
            }

            double d = args[0];
            return System.Math.Cosh(d);
        }
    }
}
