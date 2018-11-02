namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class CachedExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        public CachedExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Gets a value indicating whether value of cached expression has been computed.
        /// </summary>
        protected bool IsEvaluated { get; private set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="evaluator">Evaluator.</param>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public override double Evaluate(IEvaluator evaluator, ExpressionContext context)
        {
            if (!IsEvaluated)
            {
                CurrentValue = evaluator.EvaluateValueExpression(String, context);
                IsEvaluated = true;
            }

            return CurrentValue;
        }

        /// <summary>
        /// Invalidate the expression.
        /// </summary>
        public override void Invalidate()
        {
            IsEvaluated = false;
        }
    }
}
