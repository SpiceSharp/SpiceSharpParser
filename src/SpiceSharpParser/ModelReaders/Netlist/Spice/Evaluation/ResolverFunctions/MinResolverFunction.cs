using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class MinResolverFunction : DynamicResolverFunction
    {
        public MinResolverFunction()
        {
            Name = "min";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return TernaryOperatorNode.Conditional(Node.LessThan(argumentValues[0], argumentValues[1]), argumentValues[0], argumentValues[1]);
        }
    }
}
