using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression
{
    public interface IExpressionParser
    {
        double GetExpressionValue(string expression, ExpressionContext context, bool @throw = true);

        List<string> GetExpressionParameters(string expression, ExpressionContext context, bool @throw = true);

        SimpleDerivativeParser GetDeriveParser(ExpressionContext context, bool @throw = true);

        bool HaveSpiceProperties(string expression, ExpressionContext context);

        bool HaveFunctions(string expression, ExpressionContext context);
    }
}