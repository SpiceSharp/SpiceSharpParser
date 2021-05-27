using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class MaxResolverFunction : DynamicResolverFunction
    {
        public MaxResolverFunction()
        {
            Name = "max";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return TernaryOperatorNode.Conditional(Node.GreaterThan(argumentValues[0], argumentValues[1]), argumentValues[0], argumentValues[1]);
        }
    }
}
