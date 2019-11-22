using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class NintFunction : Function<double, double>
    {
        public NintFunction()
        {
            Name = "nint";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("nint() function expects one argument");
            }

            double x = args[0];
            return System.Math.Round(x);
        }
    }
}