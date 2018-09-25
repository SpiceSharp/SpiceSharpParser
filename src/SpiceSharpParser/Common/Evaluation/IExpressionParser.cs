using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IExpressionParser
    {
        Dictionary<string, EvaluatorExpression> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="validateParameters">Specifies whether parameter validation is on.</param>
        /// <returns>Returns the result of parse.</returns>
        ExpressionParseResult Parse(string expression, IEvaluator evaluator = null, bool validateParameters = true);
    }
}
