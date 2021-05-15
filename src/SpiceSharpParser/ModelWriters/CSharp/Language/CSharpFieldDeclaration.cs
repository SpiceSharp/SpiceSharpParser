namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpFieldDeclaration : CSharpStatement
    {
        public CSharpFieldDeclaration(string fieldName, string type)
        {
            FieldName = fieldName;
            Type = type;
        }

        public string FieldName { get; }

        public string Type { get; }
    }
}
