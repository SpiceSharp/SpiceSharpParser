using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    public class StaticResolverFunction : ResolverFunction
    {
        public StaticResolverFunction(string name, Node body, IReadOnlyList<VariableNode> arguments)
        {
            Name = name;
            Body = body;
            Arguments = arguments;
        }

        public IReadOnlyList<VariableNode> Arguments { get; set; }

        public Node Body { get; }

        public Node GetBody()
        {
            return Body;
        }
    }
}
