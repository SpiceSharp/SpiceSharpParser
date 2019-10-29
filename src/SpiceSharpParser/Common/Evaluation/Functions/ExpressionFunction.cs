using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
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

            var @value = ExpressionParserHelpers.GetExpressionValue(Expression, childContext, evaluator, simulation, readingContext);
            return @value;
        }

        public Derivatives<Func<double>> Derivative(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
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
                childContext.Arguments.Add(Arguments[i], new ConstantExpression(args[i]));
            }

            var parser = ExpressionParserHelpers.GetDeriveParser(childContext, readingContext, evaluator, simulation);
            var parseResult = parser.Parse(Expression);
            return parseResult;
        }
    }
}
