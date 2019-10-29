using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class ExpFunction : Function<double, double>
    {
        public ExpFunction()
        {
            Name = "exp";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("exp() function expects one argument");
            }

            double x = args[0];
            return System.Math.Exp(x);
        }
    }
}
