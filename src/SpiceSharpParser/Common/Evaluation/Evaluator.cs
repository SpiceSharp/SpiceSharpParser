using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Evaluator.
    /// </summary>
    public class Evaluator : IEvaluator
    {
        public Evaluator() : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="name">Evaluator name.</param>
        public Evaluator(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets or sets the name of the evaluator.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <param name="context">Context.</param>
        /// <param name="simulation"></param>
        /// <param name="readingContext"></param>
        /// <returns>
        /// A double value.
        /// </returns>
        public double Evaluate(string expression, ExpressionContext context, Simulation simulation, IReadingContext readingContext)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return ExpressionParserHelpers.GetExpressionValue(expression, context, this, simulation, readingContext, false);
        }

        public double Evaluate(Expression expression, ExpressionContext context, Simulation simulation, IReadingContext readingContext)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (expression is ConstantExpression ce)
            {
                return ce.Value;
            }


            if (expression is FunctionExpression fe)
            {
                return fe.Value();
            }


            return Evaluate(expression.ValueExpression, context, simulation, readingContext);
        }
    }
}
