using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class UplimFunction : Function<double, double>
    {
        public UplimFunction()
        {
            Name = "uplim";
            ArgumentsCount = 3;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("uplim() function expects three arguments");
            }

            var x = args[0];
            var limit = args[1];
            var zone = args[2];

            if (zone == 0.0)
            {
                return System.Math.Min(x, limit);
            }

            var width = System.Math.Abs(zone);
            var linearBoundary = limit - width;
            if (x <= linearBoundary)
            {
                return x;
            }

            return limit - (width * System.Math.Exp((linearBoundary - x) / width));
        }
    }
}
