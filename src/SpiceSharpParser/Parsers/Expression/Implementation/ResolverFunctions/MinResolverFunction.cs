using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.Parsers.Expression.Implementation.ResolverFunctions
{
    public class MinResolverFunction : DynamicResolverFunction
    {
        public MinResolverFunction()
        {
            Name = "min";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return TernaryOperatorNode.Conditional(TernaryOperatorNode.LessThan(argumentValues[0], argumentValues[1]), argumentValues[0], argumentValues[1]);
        }
    }
}
