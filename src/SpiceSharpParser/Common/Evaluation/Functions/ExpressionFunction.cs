using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.Common.Evaluation.Functions
{
    public class ExpressionFunction : Function<double, double>, IDerivativeFunction<double, double>
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

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (evaluator == null)
            {
                throw new ArgumentNullException(nameof(evaluator));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var childContext = context.CreateChildContext(string.Empty, false);
            for (var i = 0; i < Arguments.Count; i++)
            {
                childContext.SetParameter(Arguments[i], args[i]);
            }

            var parser = ParsersHelpers.GetSimpleParser(childContext, evaluator);
            var parseResult = parser.Parse(Expression);
            return parseResult;
        }

        public Derivatives<Func<double>> Derivative(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (evaluator == null)
            {
                throw new ArgumentNullException(nameof(evaluator));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var childContext = context.CreateChildContext(string.Empty, false);
            for (var i = 0; i < Arguments.Count; i++)
            {
                childContext.SetParameter(Arguments[i], args[i]);
            }

            var parser = ParsersHelpers.GetDeriveParser(childContext, evaluator, Arguments, args);
            var parseResult = parser.Parse(Expression);
            return parseResult;
        }
    }
}
