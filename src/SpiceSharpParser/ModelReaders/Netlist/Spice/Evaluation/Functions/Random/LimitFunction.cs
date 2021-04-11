using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class LimitFunction : Function<double, double>
    {
        public LimitFunction()
        {
            Name = "limit";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 2)
            {
                throw new SpiceSharpParserException("limit expects two arguments: nominal_val, abs_variation");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();

            double dRand = (2.0 * random.NextDouble()) - 1.0;
            double nominal = args[0];
            double variation = args[1];

            return nominal + (dRand > 0 ? variation : -1 * variation);
        }
    }
}