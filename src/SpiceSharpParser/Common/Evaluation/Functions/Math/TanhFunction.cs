using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class TanhFunction : Function<double, double>
    {
        public TanhFunction()
        {
            Name = "tanh";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("tanh() function expects one argument");
            }

            double d = args[0];
            return System.Math.Tanh(d);
        }
    }
}
