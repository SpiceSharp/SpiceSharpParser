using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class IfResolverFunction : DynamicResolverFunction
    {
        public IfResolverFunction()
        {
            Name = "if";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return TernaryOperatorNode.Conditional(argumentValues[0], argumentValues[1], argumentValues[2]);
        }
    }
}
