using System;

namespace SpiceSharpParser.Common
{
    /// <summary>
    /// A representation of expression.
    /// </summary>
    public class ActionExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionExpression"/> class.
        /// </summary>
        /// <param name="expression">An expression value</param>
        /// <param name="action">An action to run when expression is changed</param>
        public ActionExpression(string expression, Action<double> action)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Gets an expression.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets an action to run when expression is changed.
        /// </summary>
        public Action<double> Action { get; }
    }
}
