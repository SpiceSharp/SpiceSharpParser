using System;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// An evaluator expression.
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluation.Expression"/> class.
        /// </summary>
        /// <param name="expression">Expression.</param>
        protected Expression(string expression)
        {
            ValueExpression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Gets the expression string.
        /// </summary>
        public string ValueExpression { get; }

        /// <summary>
        /// Clones the expression.
        /// </summary>
        /// <returns>
        /// A cloned expression.
        /// </returns>
        public abstract Expression Clone();
    }
}
