using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression
{
    public interface IExpressionValueProvider
    {
        double GetExpressionValue(string expression, EvaluationContext context, bool @throw = true);
    }
}
