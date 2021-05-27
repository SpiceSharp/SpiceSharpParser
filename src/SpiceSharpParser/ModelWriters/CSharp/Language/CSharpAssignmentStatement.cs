namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpAssignmentStatement : CSharpStatement
    {
        public CSharpAssignmentStatement(string left, string valueExpression)
        {
            Left = left;
            ValueExpression = valueExpression;
        }

        public CSharpAssignmentStatement(string left, string valueExpression, bool @this)
        {
            Left = left;
            ValueExpression = valueExpression;
            This = @this;
        }

        public bool This { get; }

        public string Left { get; }

        public string ValueExpression { get; }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + ValueExpression.GetHashCode() + This.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CSharpAssignmentStatement))
            {
                return false;
            }

            var asg = (CSharpAssignmentStatement)obj;

            return asg.This == this.This
                && asg.IncludeInCollection == this.IncludeInCollection
                && asg.Left == this.Left
                && asg.ValueExpression == this.ValueExpression;
        }
    }
}
