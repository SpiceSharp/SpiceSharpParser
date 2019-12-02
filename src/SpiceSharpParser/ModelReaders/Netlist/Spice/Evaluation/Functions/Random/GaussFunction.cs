using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class GaussFunction : Function<double, double>
    {
        public GaussFunction()
        {
            Name = "gauss";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new SpiceSharpParserException("gauss expects one argument - stdDev");
            }

            var random = context.Randomizer.GetRandomDoubleProvider();

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double normal = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);
            double stdDev = args[0];

            return stdDev * normal;
        }
    }
}