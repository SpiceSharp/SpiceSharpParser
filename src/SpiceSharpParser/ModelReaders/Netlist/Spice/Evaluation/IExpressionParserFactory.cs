using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionParserFactory
    {
        ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true);
    }
}