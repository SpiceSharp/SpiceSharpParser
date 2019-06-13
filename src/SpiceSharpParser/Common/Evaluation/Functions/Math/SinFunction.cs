using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class SinFunction : Function<double, double>
    {
        public SinFunction()
        {
            Name = "sin";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("sin() function expects one argument");
            }

            double d = args[0];
            return System.Math.Sin(d);
        }
    }
}
