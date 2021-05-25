using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpStatement
    {
        public CSharpStatementKind Kind { get; set; } = CSharpStatementKind.CreateEntity;

        public Dictionary<string, string> Metadata { get; set; } = new ();

        public bool IncludeInCollection { get; set; } = true;
    }
}
