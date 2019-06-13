using SpiceSharpParser.Common.Evaluation;
using System.Numerics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PowFunction : Function<double, double>
    {
        public PowFunction(SpiceExpressionMode mode)
        {
            Name = "pow";
            VirtualParameters = false;
            ArgumentsCount = 2;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            double x = args[0];
            double y = args[1];

            switch (Mode)
            {
                case SpiceExpressionMode.LtSpice:
                    if (x < 0)
                    {
                        var realResult = Complex.Pow(new Complex(x, 0), new Complex(y, 0)).Real;

                        // TODO: remove a hack below, write a good implementation of Complex numbers for C# ...
                        if (System.Math.Abs(realResult) < 1e-15)
                        {
                            return 0;
                        }
                    }

                    return System.Math.Pow(x, y);

                case SpiceExpressionMode.SmartSpice:
                    return System.Math.Pow(System.Math.Abs(x), (int)y);

                case SpiceExpressionMode.HSpice:
                    return System.Math.Pow(x, (int)y);

                default:
                    return System.Math.Pow(x, y);
            }
        }
    }
}
