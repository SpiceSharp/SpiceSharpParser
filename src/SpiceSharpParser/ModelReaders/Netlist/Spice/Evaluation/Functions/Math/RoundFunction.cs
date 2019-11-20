using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class RoundFunction : Function<double, double>
    {
        public RoundFunction()
        {
            Name = "round";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("round() function expects one argument");
            }

            double x = args[0];
            return System.Math.Round(x);
        }
    }
}
