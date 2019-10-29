using SpiceSharpParser.Common.Evaluation;
using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Control
{
    public class DefFunction : Function<double, double>
    {
        public DefFunction()
        {
            Name = "def";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("def() function expects one argument");
            }

            return !double.IsNaN(args[0]) ? 1.0 : 0.0;
        }
    }
}
