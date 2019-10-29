using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PwrFunction : Function<double, double>
    {
        public PwrFunction(SpiceExpressionMode mode)
        {
            Name = "pwr";
            ArgumentsCount = 2;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            double x = args[0];
            double y = args[1];

            switch (Mode)
            {
                case SpiceExpressionMode.LtSpice:
                    return System.Math.Pow(System.Math.Abs(x), y);

                case SpiceExpressionMode.HSpice:
                case SpiceExpressionMode.SmartSpice:
                    return System.Math.Sign(x) * System.Math.Pow(System.Math.Abs(x), y);

                default:
                    return System.Math.Pow(x, y); // TODO: define logic for default
            }
        }
    }
}
