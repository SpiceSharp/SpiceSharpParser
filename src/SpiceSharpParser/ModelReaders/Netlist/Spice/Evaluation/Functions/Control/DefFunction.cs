using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Control
{
    public class DefFunction : Function<double, double>
    {
        public DefFunction()
        {
            Name = "def";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("def() function expects one argument");
            }

            return !double.IsNaN(args[0]) ? 1.0 : 0.0;
        }
    }
}