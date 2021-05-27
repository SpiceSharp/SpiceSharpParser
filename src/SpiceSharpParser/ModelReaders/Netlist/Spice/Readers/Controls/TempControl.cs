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
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters.Count == 0)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "No parameters for .TEMP", statement.LineInfo);
                return;
            }

            if (context.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
            {
                context.SimulationConfiguration.TemperaturesInKelvins.Remove(context.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
            }

            foreach (Parameter param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.SingleParameter
                    && (param is Models.Netlist.Spice.Objects.Parameters.ValueParameter
                        || param is Models.Netlist.Spice.Objects.Parameters.ExpressionParameter))
                {
                    context.SimulationConfiguration.TemperaturesInKelvins.Add(context.Evaluator.EvaluateDouble(param.Value) + Constants.CelsiusKelvin);
                }
                else
                {
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Wrong type of parameter for .TEMP: {param.GetType()}", param.LineInfo);
                }
            }
        }
    }
}