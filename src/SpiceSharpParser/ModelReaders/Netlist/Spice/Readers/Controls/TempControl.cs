using SpiceSharp;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .TEMP <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class TempControl : BaseControl
    {
        public override string SpiceCommandName => "temp";

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters.Count == 0)
            {
                throw new WrongParametersCountException("No parameters for .TEMP");
            }

            if (context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
            {
                context.Result.SimulationConfiguration.TemperaturesInKelvins.Remove(context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
            }

            foreach (Models.Netlist.Spice.Objects.Parameter param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.SingleParameter s
                    && (param is Models.Netlist.Spice.Objects.Parameters.ValueParameter || param is Models.Netlist.Spice.Objects.Parameters.ExpressionParameter))
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(context.ParseDouble(param.Image) + Circuit.CelsiusKelvin);
                }
                else
                {
                    throw new WrongParameterException("Wrong type of parameter for .temp: " + param.GetType());
                }
            }
        }
    }
}
