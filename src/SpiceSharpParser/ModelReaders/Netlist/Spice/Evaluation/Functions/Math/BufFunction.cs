using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class BufFunction : Function<double, double>
    {
        public BufFunction()
        {
            Name = "buf";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("buf() function expects one argument");
            }

            double x = args[0];

            return x > 0.5 ? 1 : 0;
        }
    }
}
