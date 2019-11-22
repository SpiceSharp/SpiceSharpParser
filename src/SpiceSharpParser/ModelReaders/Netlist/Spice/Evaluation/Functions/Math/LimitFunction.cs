using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class LimitFunction : Function<double, double>
    {
        public LimitFunction()
        {
            Name = "limit";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("limit() function expects 3 arguments");
            }

            double x = args[0];
            double xMin = args[1];
            double xMax = args[2];

            if (x < xMin)
            {
                return xMin;
            }

            if (x > xMax)
            {
                return xMax;
            }

            return x;
        }
    }
}
