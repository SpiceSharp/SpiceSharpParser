namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpReturnStatement : CSharpStatement
    {
        public CSharpReturnStatement(string variableName)
        {
            VariableName = variableName;
        }

        public string VariableName { get; }
    }
}
