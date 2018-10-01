using System;

namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class NamedExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExpression"/> class.
        /// </summary>
        /// <param name="name">A name of expression.</param>
        /// <param name="expression">An expression.</param>
        /// <param name="evaluator">Evaluator.</param>
        public NamedExpression(string name, string expression, IEvaluator evaluator)
            : base(expression, evaluator)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
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
        public override Expression Clone()
        {
            var result = new NamedExpression(Name, String, Evaluator);
            return result;
        }
    }
}
