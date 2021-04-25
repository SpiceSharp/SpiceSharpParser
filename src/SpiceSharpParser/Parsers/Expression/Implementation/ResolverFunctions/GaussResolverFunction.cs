using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression.Implementation.ResolverFunctions
{
    public class GaussResolverFunction : DynamicResolverFunction
    {
        private readonly EvaluationContext context;

        public GaussResolverFunction(EvaluationContext context)
        {
            Name = "gauss";
            this.context = context;
        }

        public override Node GetBody(Node[] argumentValues)
        {
            var random = context.Randomizer.GetRandomDoubleProvider();

            double p1 = 1 - random.NextDouble();
            double p2 = 1 - random.NextDouble();

            double normal = System.Math.Sqrt(-2.0 * System.Math.Log(p1)) * System.Math.Sin(2.0 * System.Math.PI * p2);

            return Node.Multiply(argumentValues[0], normal);
        }
    }
}
