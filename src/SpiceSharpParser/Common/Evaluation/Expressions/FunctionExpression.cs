using System;

namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class FunctionExpression : Expression
    {
        public FunctionExpression(Func<double> val) 
            : base(string.Empty)
        {
            Value = val;
        }

        public Func<double> Value { get; }

        public override bool CanProvideValueDirectly { get; } = true;

        public override Expression Clone()
        {
            return new FunctionExpression(Value);
        }

        public override double GetValue()
        {
            return Value();
        }
    }
}
