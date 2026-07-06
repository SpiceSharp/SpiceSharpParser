using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class TableFunction : Function<double, double>
    {
        public TableFunction()
        {
            Name = "table";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length < 3 || args.Length % 2 == 0)
            {
                throw new ArgumentException("table() function expects x followed by one or more x/y pairs");
            }

            var x = args[0];
            var firstX = args[1];
            var firstY = args[2];

            if (x <= firstX)
            {
                return firstY;
            }

            for (var i = 3; i < args.Length; i += 2)
            {
                var previousX = args[i - 2];
                var previousY = args[i - 1];
                var nextX = args[i];
                var nextY = args[i + 1];

                if (x <= nextX)
                {
                    if (nextX == previousX)
                    {
                        return nextY;
                    }

                    var fraction = (x - previousX) / (nextX - previousX);
                    return previousY + ((nextY - previousY) * fraction);
                }
            }

            return args[args.Length - 1];
        }
    }
}
