using SpiceSharp;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .TEMP <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class TempControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters.Count == 0)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "No parameters for .TEMP", statement.LineInfo));
                return;
            }

            if (context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
            {
                context.Result.SimulationConfiguration.TemperaturesInKelvins.Remove(context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
            }

            foreach (Parameter param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.SingleParameter
                    && (param is Models.Netlist.Spice.Objects.Parameters.ValueParameter
                        || param is Models.Netlist.Spice.Objects.Parameters.ExpressionParameter))
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(context.Evaluator.EvaluateDouble(param.Image) + Constants.CelsiusKelvin);
                }
                else
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Wrong type of parameter for .TEMP: {param.GetType()}", param.LineInfo));
                }
            }
        }
    }
}