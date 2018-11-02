using System;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// An evaluator expression.
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluation.Expression"/> class.
        /// </summary>
        /// <param name="expression">Expression.</param>
        public Expression(string expression)
        {
            String = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Thrown when expression is evaluated.
        /// </summary>
        public event EventHandler<EvaluatedArgs> Evaluated;

        /// <summary>
        /// Gets the expression string.
        /// </summary>
        public string String { get; }

        /// <summary>
        /// Gets or sets the current evaluation value.
        /// </summary>
        public double CurrentValue { get; protected set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="evaluator">Evaluator.</param>
        /// <returns>
        /// The value of the expression.
        /// </returns>
        public virtual double Evaluate(IEvaluator evaluator, ExpressionContext context)
        {
            var newValue = evaluator.EvaluateValueExpression(String, context);
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

        public virtual Expression Clone()
        {
            var result = new Expression(String);
            result.CurrentValue = double.NaN;
            return result;
        }
    }
}
