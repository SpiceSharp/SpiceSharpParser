using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class MinFunction : Function<double, double>
    {
        public MinFunction()
        {
            Name = "min";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Min() function expects arguments");
            }

            double min = args[0];

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i] < min)
                {
                    min = args[i];
                }
            }

            return min;
        }
    }
}