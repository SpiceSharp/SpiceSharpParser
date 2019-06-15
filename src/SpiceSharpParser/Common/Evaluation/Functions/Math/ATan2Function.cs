using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class ATan2Function : Function<double, double>
    {
        public ATan2Function()
        {
            Name = "atan2";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("atan2() function expects two arguments");
            }

            double x = args[0];
            double y = args[1];

            return System.Math.Atan2(x, y);
        }
    }
}
