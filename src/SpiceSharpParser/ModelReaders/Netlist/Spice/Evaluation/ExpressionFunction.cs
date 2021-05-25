using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionFunction : Function<double, double>
    {
        public ExpressionFunction(string name, List<string> arguments, string expression)
        {
            Name = name;
            ArgumentsCount = arguments.Count;
            Arguments = arguments;
            Expression = expression;
        }

        public List<string> Arguments { get; }

        public string Expression { get; }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var childContext = context.CreateChildContext(string.Empty, false);
            childContext.CircuitContext = context.CircuitContext;
            for (var i = 0; i < Arguments.Count; i++)
            {
                childContext.SetParameter(Arguments[i], args[i]);
            }

            var @value = childContext.Evaluator.EvaluateDouble(Expression);
            return @value;
        }
    }
}