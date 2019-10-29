using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class NintFunction : Function<double, double>
    {
        public NintFunction()
        {
            Name = "nint";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("nint() function expects one argument");
            }

            double x = args[0];
            return System.Math.Round(x);
        }
    }
}
