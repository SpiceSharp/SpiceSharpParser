namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class DynamicExpression : Expression
    {
        public DynamicExpression(string expression)
            : base(expression)
        {
        }

        public override bool CanProvideValueDirectly { get; } = false;

        public override Expression Clone()
        {
            return new DynamicExpression(ValueExpression);
        }

        public override double GetValue()
        {
            throw new System.NotImplementedException();
        }
    }
}