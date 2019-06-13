using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Control
{
    public class DefFunction : Function<object, double>
    {
        public DefFunction()
        {
            Name = "def";
            VirtualParameters = true;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, object[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("def() function expects one argument");
            }

            return context.Parameters.ContainsKey(args[0].ToString()) ? 1 : 0;
        }
    }
}
