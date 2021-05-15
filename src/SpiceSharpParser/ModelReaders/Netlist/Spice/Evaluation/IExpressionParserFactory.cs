using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionParserFactory
    {
        ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true);
    }
}