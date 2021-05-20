using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpMethod : CSharpStatement
    {
        public CSharpMethod(bool? isPublic, string methodName, string returnType, string[] argumentNames, string[] defaults, Type[] argumentTypes, List<CSharpStatement> statements, bool optionalArguments)
        {
            IsPublic = isPublic;
            MethodName = methodName;
            ReturnType = returnType;
            ArgumentNames = argumentNames;
            DefaultArgumentValues = defaults;
            ArgumentTypes = argumentTypes;
            Statements = statements;
            OptionalArguments = optionalArguments;
        }

        public bool? IsPublic { get; }

        public bool Local { get; set; } = true;

        public string MethodName { get; }

        public string[] ArgumentNames { get; }

        public string[] DefaultArgumentValues { get; }

        public Type[] ArgumentTypes { get; }

        public string ReturnType { get; }

        public List<CSharpStatement> Statements { get; }

        public bool OptionalArguments { get; }
    }
}
