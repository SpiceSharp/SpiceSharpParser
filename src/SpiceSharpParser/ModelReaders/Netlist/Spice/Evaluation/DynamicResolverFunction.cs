using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public abstract class DynamicResolverFunction : ResolverFunction
    {
        public abstract Node GetBody(Node[] argumentValues);
    }
}
