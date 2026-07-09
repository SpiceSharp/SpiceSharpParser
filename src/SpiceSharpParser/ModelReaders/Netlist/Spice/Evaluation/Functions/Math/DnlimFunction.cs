using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class DnlimFunction : Function<double, double>
    {
        public DnlimFunction()
        {
            Name = "dnlim";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("dnlim() function expects three arguments");
            }

            var x = args[0];
            var limit = args[1];
            var zone = args[2];

            if (zone == 0.0)
            {
                return System.Math.Max(x, limit);
            }

            var width = System.Math.Abs(zone);
            var linearBoundary = limit + width;
            if (x >= linearBoundary)
            {
                return x;
            }

            return limit + (width * System.Math.Exp((x - linearBoundary) / width));
        }
    }
}
