using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression
{
    public interface IExpressionParser
    {
        double GetExpressionValue(string expression, EvaluationContext context, bool @throw = true);

        List<string> GetExpressionParameters(string expression, EvaluationContext context, bool @throw = true);

        SimpleDerivativeParser GetDeriveParser(EvaluationContext context, bool @throw = true);

        bool HaveSpiceProperties(string expression, EvaluationContext context);

        bool HaveFunctions(string expression, EvaluationContext context);
    }
}