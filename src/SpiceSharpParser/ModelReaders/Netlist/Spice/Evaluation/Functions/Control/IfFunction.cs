using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Control
{
    public class IfFunction : Function<double, double>
    {
        public IfFunction()
        {
            Name = "if";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, ExpressionContext context)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("if() function expects three arguments");
            }

            double x = args[0];
            double y = args[1];
            double z = args[2];

            if (x > 0.5)
            {
                return y;
            }
            else
            {
                return z;
            }
        }
    }
}
