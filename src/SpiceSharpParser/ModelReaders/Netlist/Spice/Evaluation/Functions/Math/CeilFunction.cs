using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class CeilFunction : Function<double, double>
    {
        public CeilFunction()
        {
            Name = "ceil";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("ceil() function expects one argument");
            }

            double x = args[0];
            return System.Math.Ceiling(x);
        }
    }
}
