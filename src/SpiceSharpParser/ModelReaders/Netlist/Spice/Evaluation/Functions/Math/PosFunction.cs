using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PosFunction : Function<double, double>
    {
        public PosFunction()
        {
            Name = "pos";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("pos() function expects two arguments");
            }

            double x = args[0];
            double y = args[1];
            return x <= 0 ? y : x;
        }
    }
}
