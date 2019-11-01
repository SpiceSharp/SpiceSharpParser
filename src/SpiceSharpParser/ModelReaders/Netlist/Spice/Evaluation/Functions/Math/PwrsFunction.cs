using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PwrsFunction : Function<double, double>
    {
        public PwrsFunction()
        {
            Name = "pwrs";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            double x = args[0];
            double y = args[1];

            return System.Math.Sign(x) * System.Math.Pow(System.Math.Abs(x), y);
        }
    }
}
