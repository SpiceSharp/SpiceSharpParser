using System;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .PARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ParamControl : ParamBaseControl
    {
        protected override void SetParameter(string parameterName, string parameterExpression, ExpressionContext expressionContext, IEvaluator evaluator, SpiceNetlistCaseSensitivitySettings caseSettings, IReadingContext readingContext)
        {
            var parameters = ExpressionParserHelpers.GetExpressionParameters(parameterExpression, expressionContext, readingContext, caseSettings, @throw: false);

            try
            {
                ExpressionParserHelpers.GetExpressionValue(parameterExpression, expressionContext, evaluator, null, readingContext, true);
            }
            catch (Exception e)
            {
                throw new GeneralReaderException($"Problem with param `{parameterName}` with expression `{parameterExpression}`" ,e);
            }
            

            expressionContext.SetParameter(parameterName, parameterExpression, parameters);
        }
    }
}
