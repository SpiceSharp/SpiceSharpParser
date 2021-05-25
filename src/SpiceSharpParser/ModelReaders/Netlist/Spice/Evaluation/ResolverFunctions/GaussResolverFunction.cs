using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class GaussResolverFunction : DynamicResolverFunction
    {
        private readonly EvaluationContext _context;

        public GaussResolverFunction(EvaluationContext context)
        {
            Name = "gauss";
            this._context = context;
        }

        public override Node GetBody(Node[] argumentValues)
        {
            var random = _context.Randomizer.GetRandomDoubleProvider();

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double normal = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);

            return Node.Multiply(argumentValues[0], normal);
        }
    }
}
