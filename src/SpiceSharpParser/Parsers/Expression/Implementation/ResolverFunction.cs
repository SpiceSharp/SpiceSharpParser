using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    public class ResolverFunction
    {
        public string Name { get; set; }

        public IReadOnlyList<VariableNode> Arguments { get; set; }

        public Node Body { get; set; }
    }
}
