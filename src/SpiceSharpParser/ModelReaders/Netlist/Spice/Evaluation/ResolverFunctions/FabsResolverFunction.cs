using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class FabsResolverFunction : DynamicResolverFunction
    {
        public FabsResolverFunction()
        {
            Name = "fabs";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return Node.Function("abs", argumentValues);
        }
    }
}
