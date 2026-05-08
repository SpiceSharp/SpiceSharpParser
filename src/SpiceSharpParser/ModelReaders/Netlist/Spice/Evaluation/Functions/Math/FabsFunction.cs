using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class FabsFunction : Function<double, double>
    {
        public FabsFunction()
        {
            Name = "fabs";
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("fabs() function expects one argument");
            }

            return System.Math.Abs(args[0]);
        }
    }
}
