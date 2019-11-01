using SpiceSharpParser.Common.Evaluation;
using System;
using System.Numerics;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PowInfixFunction : Function<double, double>
    {
        public PowInfixFunction(SpiceExpressionMode mode)
        {
            Name = "**";
            ArgumentsCount = 2;
            Infix = true;
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
                    throw new Exception("** is unknown function");

                case SpiceExpressionMode.HSpice:
                    if (x < 0)
                    {
                        return System.Math.Pow(x, (int)y);
                    }
                    else if (x == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return System.Math.Pow(x, y);
                    }

                default:
                    return System.Math.Pow(x, y);
            }
        }
    }
}
