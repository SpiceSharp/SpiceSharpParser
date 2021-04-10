using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Expression
{
    public interface IExpressionParserFactory
    {
        ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true, bool applyVoltage = false);
    }
}