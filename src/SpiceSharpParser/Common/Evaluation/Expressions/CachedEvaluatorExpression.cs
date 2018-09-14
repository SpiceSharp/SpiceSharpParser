namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class CachedEvaluatorExpression : EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedEvaluatorExpression"/> class.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        public CachedEvaluatorExpression(string expression, IEvaluator evaluator)
            : base(expression, evaluator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedEvaluatorExpression"/> class.
        /// </summary>
        public CachedEvaluatorExpression(IEvaluator evaluator)
            : this(string.Empty, evaluator)
        {
        }

        /// <summary>
        /// Gets a value indicating whether value of cached expression has been computed.
        /// </summary>
        protected bool IsEvaluated { get; private set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public override double Evaluate()
        {
            if (!IsEvaluated)
            {
                CurrentValue = Evaluator.EvaluateDouble(Expression);
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
