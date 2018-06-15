using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IExpressionParser
    {
        Dictionary<string, EvaluatorExpression> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; }

        ExpressionParseResult Parse(string expression, object context = null, IEvaluator evaluator = null);
    }
}
