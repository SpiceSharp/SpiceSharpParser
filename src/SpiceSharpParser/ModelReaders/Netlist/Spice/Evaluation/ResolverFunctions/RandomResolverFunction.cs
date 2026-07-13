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
            if (argumentValues.Length != 0)
            {
                throw new SpiceSharpParser.Common.SpiceSharpParserException(
                    "random expects no arguments outside LTspice random(x) compatibility lowering");
            }

            return Node.Constant(_context.Randomizer.GetRandomDoubleProvider().NextDouble());
        }
    }
}
