using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression
{
    public interface IExpressionParserFactory
    {
        SimpleDerivativeParser Create(EvaluationContext context, bool throwOnErrors = true, bool applyVoltage = false);
    }
}