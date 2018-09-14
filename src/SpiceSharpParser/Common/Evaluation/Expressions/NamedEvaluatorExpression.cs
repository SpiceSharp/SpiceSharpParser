namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class NamedEvaluatorExpression : EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEvaluatorExpression"/> class.
        /// </summary>
        /// <param name="name">A name of expression.</param>
        /// <param name="expression">An expression.</param>
        public NamedEvaluatorExpression(string name, string expression, IEvaluator evaluator)
            : base(expression, evaluator)
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
        /// <returns>
        /// A clone of named expression.
        /// </returns>
        public override EvaluatorExpression Clone()
        {
            var result = new NamedEvaluatorExpression(Name, Expression, Evaluator);
            return result;
        }
    }
}
