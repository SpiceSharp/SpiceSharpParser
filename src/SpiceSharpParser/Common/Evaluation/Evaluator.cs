using System;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Abstract evaluator.
    /// </summary>
    public abstract class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="name">Evaluator name.</param>
        /// <param name="parser">Expression parser.</param>
        public Evaluator(string name, IExpressionParser parser, bool isParameterNameCaseSensitive, bool isFunctionNameCaseSensitive)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            IsFunctionNameCaseSensitive = isFunctionNameCaseSensitive;
            ExpressionParser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Gets or sets the name of the evaluator.
        /// </summary>
        public string Name { get; set; }

        public bool IsParameterNameCaseSensitive { get; }

        public bool IsFunctionNameCaseSensitive { get; }

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        public IExpressionParser ExpressionParser { get; private set; }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        public double EvaluateValueExpression(string expression, ExpressionContext context)
        {
            if (context.Parameters.TryGetValue(expression, out var parameter))
            {
                return parameter.Evaluate(this, context);
            }

            ExpressionParseResult parseResult = ExpressionParser.Parse(expression, new ExpressionParserContext(IsFunctionNameCaseSensitive) { Name = context.Name, Functions = context.Functions });
            double expressionValue = parseResult.Value(new ExpressionEvaluationContext() { ExpressionContext = context, Evaluator = this });

            return expressionValue;
        }

        /// <summary>
        /// Gets value of named expression.
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <returns>
        /// Value of expression.
        /// </returns>
        public double EvaluateNamedExpression(string expressionName, ExpressionContext context)
        {
            return context.ExpressionRegistry.GetExpression(expressionName).Evaluate(this, context);
        }

        /// <summary>
        /// Gets the value of parameter.
        /// </summary>
        /// <param name="id">A parameter identifier.</param>
        /// <param name="context">Context.</param>
        /// <returns>
        /// A value of parameter.
        /// </returns>
        public double EvaluateParameter(string id, ExpressionContext context)
        {
            return context.Parameters[id].Evaluate(this, context);
        }
    }
}
