using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{

    /// <summary>
    /// Reads .SPARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SParamControl : ParamBaseControl
    {
        protected override void SetParameter(string parameterName, string parameterExpression, IExpressionParser expressionParser, ExpressionContext expressionContext, SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            expressionContext.SetCachedParameter(
                            parameterName,
                            parameterExpression,
                            expressionParser.Parse(
                                parameterExpression,
                                new ExpressionParserContext(caseSettings.IsFunctionNameCaseSensitive)
                                {
                                    Functions = expressionContext.Functions
                                }).FoundParameters);
        }
    }
}
