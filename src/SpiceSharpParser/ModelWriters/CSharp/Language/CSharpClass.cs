using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpClass
    {
        public CSharpClass(string className, List<CSharpFieldDeclaration> fields, List<CSharpMethod> methods)
        {
            ClassName = className;
            Fields = fields;
            Methods = methods;
        }

        public string ClassName { get; }

        public List<CSharpFieldDeclaration> Fields { get; }

        public List<CSharpMethod> Methods { get; }
    }
}
