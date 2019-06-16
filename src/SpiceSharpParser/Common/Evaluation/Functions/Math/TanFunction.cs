using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class TanFunction : Function<double, double>
    {
        public TanFunction()
        {
            Name = "tan";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("tan() function expects one argument");
            }

            double d = args[0];
            return System.Math.Tan(d);
        }
    }
}
