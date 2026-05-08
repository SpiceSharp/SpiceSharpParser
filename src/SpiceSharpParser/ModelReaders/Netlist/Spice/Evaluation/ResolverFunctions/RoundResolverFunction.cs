using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class RoundResolverFunction : DynamicResolverFunction
    {
        public RoundResolverFunction()
        {
            Name = "round";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            if (argumentValues.Length == 1)
            {
                return Node.Function("nint", argumentValues);
            }

            return Node.Function("round", argumentValues);
        }
    }
}
