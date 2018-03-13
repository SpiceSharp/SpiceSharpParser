using System;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// A representation of expression with double value
    /// </summary>
    public class DoubleExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleExpression"/> class.
        /// </summary>
        /// <param name="expression">An expression value</param>
        /// <param name="setter">An action to run when expression is re-evaluated</param>
        public DoubleExpression(string expression, Action<double> setter)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// Gets an expression value
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets an action to run when expression is re-evaluated
        /// </summary>
        public Action<double> Setter { get; }
    }
}
