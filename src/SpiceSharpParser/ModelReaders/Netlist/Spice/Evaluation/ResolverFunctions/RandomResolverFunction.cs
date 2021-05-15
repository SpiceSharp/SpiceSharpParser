using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class RandomResolverFunction : DynamicResolverFunction
    {
        private readonly EvaluationContext context;

        public RandomResolverFunction(EvaluationContext context)
        {
            Name = "random";
            this.context = context;
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return Node.Constant(context.Randomizer.GetRandomDoubleProvider().NextDouble());
        }
    }
}
