using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .SPARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SParamControl : ParamBaseControl
    {
        protected override void SetParameter(string parameterName, string parameterExpression, ExpressionContext expressionContext, SpiceNetlistCaseSensitivitySettings caseSettings, IEvaluator evaluator, IReadingContext readingContext)
        {
            var parameters = ExpressionParserHelpers.GetExpressionParameters(parameterExpression, expressionContext, readingContext, false);
            expressionContext.SetCachedParameter(parameterName, parameterExpression, parameters);
        }
    }
}
