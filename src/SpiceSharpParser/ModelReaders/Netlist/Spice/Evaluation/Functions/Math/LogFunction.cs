using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class LogFunction : Function<double, double>
    {
        public LogFunction(SpiceExpressionMode mode)
        {
            Name = "log";
            ArgumentsCount = 1;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("log() function expects one argument");
            }

            double x = args[0];

            if (Mode == SpiceExpressionMode.HSpice)
            {
                return System.Math.Sign(x) * System.Math.Log(System.Math.Abs(x));
            }

            return System.Math.Log(x);
        }
    }
}
