using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class IntFunction : Function<double, double>
    {
        public IntFunction()
        {
            Name = "int";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("int() function expects one argument");
            }

            double x = args[0];
            return (int)x;
        }
    }
}
