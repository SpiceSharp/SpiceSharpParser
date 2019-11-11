using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .SPARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SParamControl : ParamBaseControl
    {
        protected override void SetParameter(string parameterName, string parameterExpression, ExpressionContext expressionContext, SpiceNetlistCaseSensitivitySettings caseSettings, IEvaluator evaluator, IReadingContext readingContext)
        {
            var value = readingContext.ReadingEvaluator.Evaluate(new DynamicExpression(parameterExpression), expressionContext, null, readingContext);
            expressionContext.SetParameter(parameterName, value);
        }
    }
}
