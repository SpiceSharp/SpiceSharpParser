using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class CbrtFunction : Function<double, double>
    {
        public CbrtFunction()
        {
            Name = "cbrt";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("cbrt() function expects one argument");
            }

            double x = args[0];

            return System.Math.Pow(x, 1.0 / 3.0);
        }
    }
}