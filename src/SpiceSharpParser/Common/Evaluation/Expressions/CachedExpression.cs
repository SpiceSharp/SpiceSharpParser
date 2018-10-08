namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class CachedExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        /// <param name="evaluator">Evaluator.</param>
        public CachedExpression(string expression, IEvaluator evaluator)
            : base(expression, evaluator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="evaluator">Evaluator.</param>
        public CachedExpression(IEvaluator evaluator)
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
                CurrentValue = Evaluator.EvaluateDouble(String);
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
