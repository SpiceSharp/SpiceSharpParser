namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class ConstantExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        public ConstantExpression(double value)
            : base(string.Empty, null)
        {
            CurrentValue = value;
        }

        /// <summary>
        /// Clones the named expression.
        /// </summary>
        /// <returns>
        /// A clone of named expression.
        /// </returns>
        public override Expression Clone()
        {
            var result = new ConstantExpression(CurrentValue);
            return result;
        }

        public override void Invalidate()
        {
        }

        public override double Evaluate()
        {
            return CurrentValue;
        }
    }
}
