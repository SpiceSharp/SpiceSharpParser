using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class UFunction : Function<double, double>
    {
        public UFunction()
        {
            Name = "u";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("u() function expects one argument");
            }

            double x = args[0];
            return x > 0 ? 1 : 0;
        }
    }
}
