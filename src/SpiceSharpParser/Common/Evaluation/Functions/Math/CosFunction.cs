using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class CosFunction : Function<double, double>
    {
        public CosFunction()
        {
            Name = "cos";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("cos() function expects one argument");
            }

            double d = args[0];
            return System.Math.Cos(d);
        }
    }
}
