namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionResolverFactory
    {
        ExpressionResolver Create(EvaluationContext context, bool throwOnErrors = true);
    }
}