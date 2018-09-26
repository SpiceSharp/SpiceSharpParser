namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class ConstantEvaluatorExpression : EvaluatorExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantEvaluatorExpression"/> class.
        /// </summary>
        public ConstantEvaluatorExpression(double value)
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
        public override EvaluatorExpression Clone()
        {
            var result = new ConstantEvaluatorExpression(CurrentValue);
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
