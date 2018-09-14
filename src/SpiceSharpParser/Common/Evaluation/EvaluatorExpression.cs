using System;

namespace SpiceSharpParser.Common
{
    public class EvaluatedArgs : EventArgs
    {
        public double NewValue { get; set; }
    }

    /// <summary>
    /// An evaluator expression.
    /// </summary>
    public class EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluatorExpression"/> class.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <param name="evaluator">Evaluator.</param>
        public EvaluatorExpression(string expression, IEvaluator evaluator)
        {
            Evaluator = evaluator;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Thrown when expression is evaluated.
        /// </summary>
        public event EventHandler<EvaluatedArgs> Evaluated;

        /// <summary>
        /// Gets or sets evalautor for expression.
        /// </summary>
        public IEvaluator Evaluator { get; set; }

        /// <summary>
        /// Gets the expression string.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets or sets the current evaluation value.
        /// </summary>
        public double CurrentValue { get; protected set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public virtual double Evaluate()
        {
            var newValue = Evaluator.EvaluateDouble(Expression);
            CurrentValue = newValue;
            OnEvaluated(newValue);
            return newValue;
        }

        /// <summary>
        /// Invalidates the expression.
        /// </summary>
        public virtual void Invalidate()
        {
            CurrentValue = double.NaN;
        }

        protected void OnEvaluated(double newValue)
        {
            Evaluated?.Invoke(this, new EvaluatedArgs() { NewValue = newValue });
        }

        public virtual EvaluatorExpression Clone()
        {
            var result = new EvaluatorExpression(Expression, Evaluator);
            result.CurrentValue = double.NaN;
            return result;
        }
    }
}
