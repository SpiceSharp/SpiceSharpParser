namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpConditionAssignmentStatement : CSharpStatement
    {
        public CSharpConditionAssignmentStatement(string condition, string left, string valueExpression)
        {
            Condition = condition;
            Left = left;
            ValueExpression = valueExpression;
        }

        public CSharpConditionAssignmentStatement(string condition, string left, string valueExpression, bool @this)
        {
            Condition = condition;
            Left = left;
            ValueExpression = valueExpression;
            This = @this;
        }

        public bool This { get; }

        public string Condition { get; }

        public string Left { get; }

        public string ValueExpression { get; }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + ValueExpression.GetHashCode() + This.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CSharpConditionAssignmentStatement))
            {
                return false;
            }

            var asg = (CSharpConditionAssignmentStatement)obj;

            return
                asg.Condition == this.Condition
                && asg.This == this.This
                && asg.IncludeInCollection == this.IncludeInCollection
                && asg.Left == this.Left
                && asg.ValueExpression == this.ValueExpression;
        }
    }
}
