namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpCallStatement : CSharpStatement
    {
        public CSharpCallStatement(string variableName, string callExpression)
        {
            VariableName = variableName;
            CallExpression = callExpression;
        }

        public string VariableName { get; }

        public string CallExpression { get; }
    }
}
