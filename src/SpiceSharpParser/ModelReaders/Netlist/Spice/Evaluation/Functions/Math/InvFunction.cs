using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class InvFunction : Function<double, double>
    {
        public InvFunction()
        {
            Name = "inv";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("inv() function expects one argument");
            }

            double x = args[0];

            return x > 0.5 ? 0 : 1;
        }
    }
}
