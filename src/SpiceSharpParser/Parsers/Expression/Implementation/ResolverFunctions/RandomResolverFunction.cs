using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression.Implementation.ResolverFunctions
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
