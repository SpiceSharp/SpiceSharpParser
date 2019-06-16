using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class FAbsFunction : Function<double, double>
    {
        public FAbsFunction()
        {
            Name = "fabs";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("fabs() function expects one argument");
            }

            double x = args[0];
            return System.Math.Abs(x);
        }
    }
}
