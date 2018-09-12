using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class NamedEvaluatorExpression : EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEvaluatorExpression"/> class.
        /// </summary>
        /// <param name="name">A name of expression.</param>
        /// <param name="expression">An expression.</param>
        public NamedEvaluatorExpression(string name, string expression, Func<string, object, EvaluatorExpression, IEvaluator, double> expressionEvaluator, IEvaluator evaluator)
            : base(expression, expressionEvaluator, evaluator)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of expression.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Clones the named expression.
        /// </summary>
        /// <returns></returns>
        public override EvaluatorExpression Clone()
        {
            var result = new NamedEvaluatorExpression(Name, ExpressionString, ExpressionEvaluator, Evaluator);
            return result;
        }
    }
}
