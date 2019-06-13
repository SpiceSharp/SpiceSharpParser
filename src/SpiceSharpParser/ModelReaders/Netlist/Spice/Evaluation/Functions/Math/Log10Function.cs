using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class Log10Function : Function<double, double>
    {
        public Log10Function(SpiceExpressionMode mode)
        {
            Name = "log10";
            VirtualParameters = false;
            ArgumentsCount = 1;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("log10() function expects one argument");
            }

            double x = args[0];

            if (Mode == SpiceExpressionMode.HSpice)
            {
                return System.Math.Sign(x) * System.Math.Log10(System.Math.Abs(x));
            }

            return System.Math.Log10(x);
        }
    }
}
