using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class HypotFunction : Function<double, double>
    {
        public HypotFunction()
        {
            Name = "hypot";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("hypot() function expects two arguments");
            }

            double x = args[0];
            double y = args[1];

            return System.Math.Sqrt((x * x) + (y * y));
        }
    }
}
