using SpiceSharpParser.Common.Evaluation;
using System.Numerics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class SqrtFunction : Function<double, double>
    {
        public SqrtFunction(SpiceExpressionMode mode)
        {
            Name = "sqrt";
            ArgumentsCount = 1;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            double x = args[0];

            switch (Mode)
            {
                case SpiceExpressionMode.LtSpice:
                    if (x < 0)
                    {
                        var realResult = Complex.Pow(new Complex(x, 0), new Complex(0.5, 0)).Real;

                        // TODO: remove a hack below, write a good implementation of Complex numbers for C# ...
                        if (System.Math.Abs(realResult) < 1e-15)
                        {
                            return 0;
                        }
                    }

                    return System.Math.Sqrt(x);

                case SpiceExpressionMode.SmartSpice:
                    return System.Math.Sqrt(System.Math.Abs(x));

                case SpiceExpressionMode.HSpice:
                    if (x < 0)
                    {
                        return -System.Math.Sqrt(System.Math.Abs(x));
                    }
                    else
                    {
                        return System.Math.Sqrt(x);
                    }

                default:
                    return System.Math.Sqrt(x);
            }
        }
    }
}
