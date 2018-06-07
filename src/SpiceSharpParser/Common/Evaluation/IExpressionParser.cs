using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Common
{
    public interface IExpressionParser
    {
        Dictionary<string, LazyExpression> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; }

        ExpressionParseResult Parse(string expression, object context = null, IEvaluator evaluator = null);
    }
}
