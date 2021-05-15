using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpStatement
    {
        public CSharpStatement()
        {
        }

        public CSharpStatementKind Kind { get; set; } = CSharpStatementKind.CreateEntity;

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public bool IncludeInCollection { get; set; } = true;
    }
}
