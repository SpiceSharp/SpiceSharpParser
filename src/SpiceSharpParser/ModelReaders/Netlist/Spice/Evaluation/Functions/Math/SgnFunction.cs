using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class SgnFunction : Function<double, double>
    {
        public SgnFunction()
        {
            Name = "sgn";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("sgn() function expects one argument");
            }

            double x = args[0];
            return System.Math.Sign(x);
        }
    }
}
