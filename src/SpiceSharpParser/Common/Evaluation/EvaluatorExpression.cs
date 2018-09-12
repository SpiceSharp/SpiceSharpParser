using System;

namespace SpiceSharpParser.Common
{
    /// <summary>
    /// An evaluator expression.
    /// </summary>
    public class EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluatorExpression"/> class.
        /// </summary>
        /// <param name="expressionString">A value of expression.</param>
        public EvaluatorExpression(string expressionString, Func<string, object, EvaluatorExpression, IEvaluator, double> expressionEvaluator, IEvaluator evaluator)
        {
            Evaluator = evaluator;
            ExpressionString = expressionString ?? throw new ArgumentNullException(nameof(expressionString));
            ExpressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        }

        /// <summary>
        /// Gets or sets evalautor for expression.
        /// </summary>
        public IEvaluator Evaluator { get; set; }

        /// <summary>
        /// Gets the expression string.
        /// </summary>
        public string ExpressionString { get; }

        /// <summary>
        /// Gets the logic that computes the value of expression.
        /// </summary>
        public Func<string, object, EvaluatorExpression, IEvaluator, double> ExpressionEvaluator { get; }

        /// <summary>
        /// Gets the last evaluation value.
        /// </summary>
        public double? LastValue { get; private set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">The context of evaluation.</param>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public virtual double Evaluate(object context)
        {
            var val = ExpressionEvaluator(ExpressionString, context, this, Evaluator);
            LastValue = val;
            return val;
        }

        /// <summary>
        /// Invalidates the expression.
        /// </summary>
        public virtual void Invalidate()
        {
            LastValue = null;
        }

        public virtual EvaluatorExpression Clone()
        {
            var result = new EvaluatorExpression(ExpressionString, ExpressionEvaluator, Evaluator);
            result.LastValue = null;
            return result;
        }
    }
}
