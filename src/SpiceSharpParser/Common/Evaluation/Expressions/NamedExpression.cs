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
        public NamedExpression(string name, string expression, Func<string, object, EvaluatorExpression, IEvaluator, double> expressionEvaluator, IEvaluator evaluator)
            : base(expression, expressionEvaluator, evaluator)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of expression.
        /// </summary>
        public string Name { get; }

        public override EvaluatorExpression Clone()
        {
            var result = new NamedExpression(Name, ExpressionString, ExpressionEvaluator, Evaluator);
            return result;
        }
    }
}
