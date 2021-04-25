using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.Parsers.Expression.Implementation.ResolverFunctions
{
    public class MaxResolverFunction : DynamicResolverFunction
    {
        public MaxResolverFunction()
        {
            Name = "max";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return TernaryOperatorNode.Conditional(TernaryOperatorNode.GreaterThan(argumentValues[0], argumentValues[1]), argumentValues[0], argumentValues[1]);
        }
    }
}
