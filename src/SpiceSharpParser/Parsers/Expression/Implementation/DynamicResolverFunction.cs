using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    public abstract class DynamicResolverFunction : ResolverFunction
    {
        public abstract Node GetBody(Node[] argumentValues);
    }
}
