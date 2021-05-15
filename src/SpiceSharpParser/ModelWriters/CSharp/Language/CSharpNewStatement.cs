namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpNewStatement : CSharpStatement
    {
        public CSharpNewStatement(string variableName, string newExpression)
        {
            VariableName = variableName;
            NewExpression = newExpression;
        }

        public string VariableName { get; }

        public string NewExpression { get; }
    }
}
