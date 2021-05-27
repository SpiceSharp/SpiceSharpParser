using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class RandomResolverFunction : DynamicResolverFunction
    {
        private readonly EvaluationContext _context;

        public RandomResolverFunction(EvaluationContext context)
        {
            Name = "random";
            this._context = context;
        }

        public override Node GetBody(Node[] argumentValues)
        {
            return Node.Constant(_context.Randomizer.GetRandomDoubleProvider().NextDouble());
        }
    }
}
