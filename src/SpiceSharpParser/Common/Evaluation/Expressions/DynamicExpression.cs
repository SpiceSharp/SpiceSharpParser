namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class DynamicExpression : Expression
    {
        public DynamicExpression(string expression)
            : base(expression)
        {
        }

        public override Expression Clone()
        {
            return new DynamicExpression(ValueExpression);
        }
    }
}
