namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class ConstantExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="value">Value.</param>
        public ConstantExpression(double value) : base(string.Empty)
        {
            Value = value;
        }

        public double Value { get; }

        /// <summary>
        /// Clones the named expression.
        /// </summary>
        /// <returns>
        /// A clone of named expression.
        /// </returns>
        public override Expression Clone()
        {
            return new ConstantExpression(Value);
        }
    }
}
