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
        public NamedExpression(string name, string expression)
            : base(expression)
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
            var result = new NamedExpression(Name, String);
            return result;
        }
    }
}
