using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class AbsFunction : Function<double, double>
    {
        public AbsFunction()
        {
            Name = "abs";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("abs() function expects one argument");
            }

            double x = args[0];
            return System.Math.Abs(x);
        }
    }
}
