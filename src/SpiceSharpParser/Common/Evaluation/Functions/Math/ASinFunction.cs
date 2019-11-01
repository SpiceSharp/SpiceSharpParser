using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class ASinFunction : Function<double, double>
    {
        public ASinFunction()
        {
            Name = "asin";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("asin() function expects one argument");
            }

            double d = args[0];
            return System.Math.Asin(d);
        }
    }
}
