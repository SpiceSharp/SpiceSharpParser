using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class MaxFunction : Function<double, double>
    {
        public MaxFunction()
        {
            Name = "max";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Max() function expects arguments");
            }

            double max = args[0];

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i] > max)
                {
                    max = args[i];
                }
            }

            return max;
        }
    }
}