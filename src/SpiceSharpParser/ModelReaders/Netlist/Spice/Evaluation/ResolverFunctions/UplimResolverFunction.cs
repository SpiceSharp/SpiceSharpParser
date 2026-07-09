using System;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class UplimResolverFunction : DynamicResolverFunction
    {
        public UplimResolverFunction()
        {
            Name = "uplim";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            if (argumentValues.Length != 3)
            {
                throw new ArgumentException("uplim() function expects three arguments");
            }

            var x = argumentValues[0];
            var limit = argumentValues[1];
            var zone = Node.Function("abs", new[] { argumentValues[2] });
            var linearBoundary = limit - zone;

            return Node.Conditional(
                Node.LessThanOrEqual(x, linearBoundary),
                x,
                limit - (zone * Node.Function("exp", new[] { (linearBoundary - x) / zone })));
        }
    }
}
