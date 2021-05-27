using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .SPARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SParamControl : ParamBaseControl
    {
        protected override void SetParameter(string parameterName, string parameterExpression, EvaluationContext context)
        {
            context.SetParameter(parameterName, context.Evaluator.EvaluateDouble(parameterExpression));
        }
    }
}