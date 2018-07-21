using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class CachedExpression : EvaluatorExpression
    {
        private double currentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="expressionString">Expression string.</param>
        /// <param name="expressionEvaluator">Expression evaluator.</param>
        public CachedExpression(string expressionString, Func<string, object, EvaluatorExpression, IEvaluator, double> expressionEvaluator, IEvaluator evaluator)
            : base(expressionString, expressionEvaluator, evaluator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="expressionEvaluator">Expression evaluator.</param>
        public CachedExpression(Func<string, object, EvaluatorExpression, IEvaluator, double> expressionEvaluator, IEvaluator evaluator)
            : this(string.Empty, expressionEvaluator, evaluator)
        {
        }

        /// <summary>
        /// Gets a value indicating whether value of cached expression has been computed.
        /// </summary>
        protected bool IsLoaded { get; private set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">The context of evaluation.</param>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public override double Evaluate(object context)
        {
            if (!IsLoaded)
            {
                currentValue = ExpressionEvaluator(ExpressionString, context, this, Evaluator);
                IsLoaded = true;
            }

            return currentValue;
        }

        /// <summary>
        /// Invalidate the expression.
        /// </summary>
        public override void Invalidate()
        {
            IsLoaded = false;
        }
    }
}
