using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class NamedExpression : EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExpression"/> class.
        /// </summary>
        /// <param name="name">A name of expression.</param>
        /// <param name="expression">An expression.</param>
        public NamedExpression(string name, string expression, Func<string, object, EvaluatorExpression, double> expressionEvaluator)
            : base(expression, expressionEvaluator)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of expression.
        /// </summary>
        public string Name { get; }
    }
}
